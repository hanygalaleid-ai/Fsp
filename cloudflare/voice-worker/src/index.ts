import { DurableObject } from "cloudflare:workers";

interface Env {
  VOICE_ROOM_STATE: DurableObjectNamespace<VoiceRoomState>;
  CLOUDFLARE_REALTIME_API_TOKEN: string;
  CLOUDFLARE_ACCOUNT_ID: string;
  REALTIMEKIT_APP_ID: string;
  REALTIMEKIT_PRESET_NAME: string;
  SUPABASE_URL: string;
  SUPABASE_PUBLISHABLE_KEY: string;
}

type TokenRequest = { squadId?: string; displayName?: string };

export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    if (request.method !== "POST") return json({ error: "Method not allowed" }, 405);

    const auth = request.headers.get("Authorization") ?? "";
    if (!auth.startsWith("Bearer ")) return json({ error: "Unauthorized" }, 401);
    const accessToken = auth.slice(7);

    let body: TokenRequest;
    try { body = await request.json<TokenRequest>(); }
    catch { return json({ error: "Invalid JSON" }, 400); }

    const squadId = (body.squadId ?? "").trim();
    if (!squadId) return json({ error: "squadId required" }, 400);

    const user = await getSupabaseUser(env, accessToken);
    if (!user?.id) return json({ error: "Invalid session" }, 401);

    const isMember = await verifySquadMembership(env, accessToken, squadId, user.id);
    if (!isMember) return json({ error: "Not a squad member" }, 403);

    try {
      const meetingId = await getOrCreateMeeting(env, squadId);
      const participant = await addParticipant(
        env,
        meetingId,
        user.id,
        sanitizeName(body.displayName) || "Player"
      );

      if (!participant?.token) return json({ error: "Could not issue voice token" }, 502);

      return json({
        meetingId,
        participantId: participant.id,
        token: participant.token
      });
    } catch (error) {
      console.error("voice token issue failed", error);
      return json({ error: "Voice service unavailable" }, 502);
    }
  }
};

async function getSupabaseUser(env: Env, token: string): Promise<{ id: string } | null> {
  const res = await fetch(`${env.SUPABASE_URL}/auth/v1/user`, {
    headers: {
      apikey: env.SUPABASE_PUBLISHABLE_KEY,
      Authorization: `Bearer ${token}`
    }
  });
  if (!res.ok) return null;
  return await res.json<{ id: string }>();
}

async function verifySquadMembership(env: Env, token: string, squadId: string, userId: string): Promise<boolean> {
  const url = new URL(`${env.SUPABASE_URL}/rest/v1/squad_members`);
  url.searchParams.set("squad_id", `eq.${squadId}`);
  url.searchParams.set("user_id", `eq.${userId}`);
  url.searchParams.set("select", "user_id");
  url.searchParams.set("limit", "1");
  const res = await fetch(url, {
    headers: {
      apikey: env.SUPABASE_PUBLISHABLE_KEY,
      Authorization: `Bearer ${token}`
    }
  });
  if (!res.ok) return false;
  const rows = await res.json<unknown[]>();
  return rows.length === 1;
}

async function getOrCreateMeeting(env: Env, squadId: string): Promise<string> {
  const stub = env.VOICE_ROOM_STATE.getByName(squadId);
  const res = await stub.fetch("https://voice-room.internal/meeting", {
    method: "POST",
    headers: { "x-fsp-squad-id": squadId }
  });
  if (!res.ok) throw new Error(`Voice room state failed: ${await res.text()}`);
  const payload = await res.json<{ meetingId?: string }>();
  if (!payload.meetingId) throw new Error("Meeting ID missing");
  return payload.meetingId;
}

async function addParticipant(env: Env, meetingId: string, userId: string, name: string): Promise<any> {
  const endpoint = `https://api.cloudflare.com/client/v4/accounts/${env.CLOUDFLARE_ACCOUNT_ID}/realtime/kit/${env.REALTIMEKIT_APP_ID}/meetings/${meetingId}/participants`;
  const res = await fetch(endpoint, {
    method: "POST",
    headers: cloudflareHeaders(env),
    body: JSON.stringify({
      name,
      preset_name: env.REALTIMEKIT_PRESET_NAME,
      custom_participant_id: userId
    })
  });
  if (!res.ok) throw new Error(`Participant create failed: ${await res.text()}`);
  const payload: any = await res.json();
  return payload?.data ?? payload?.result;
}

export class VoiceRoomState extends DurableObject<Env> {
  async fetch(request: Request): Promise<Response> {
    if (request.method !== "POST") return new Response("Method not allowed", { status: 405 });
    const squadId = (request.headers.get("x-fsp-squad-id") ?? "").trim();
    if (!squadId) return new Response("Squad ID missing", { status: 400 });

    let meetingId = await this.ctx.storage.get<string>("meetingId");
    if (!meetingId) {
      const endpoint = `https://api.cloudflare.com/client/v4/accounts/${this.env.CLOUDFLARE_ACCOUNT_ID}/realtime/kit/${this.env.REALTIMEKIT_APP_ID}/meetings`;
      const res = await fetch(endpoint, {
        method: "POST",
        headers: cloudflareHeaders(this.env),
        body: JSON.stringify({ title: `Fsp squad ${squadId}` })
      });
      if (!res.ok) return new Response(`Meeting create failed: ${await res.text()}`, { status: 502 });
      const payload: any = await res.json();
      meetingId = payload?.data?.id ?? payload?.result?.id;
      if (!meetingId) return new Response("Meeting ID missing", { status: 502 });
      await this.ctx.storage.put("meetingId", meetingId);
    }

    return json({ meetingId });
  }
}

function cloudflareHeaders(env: Env): HeadersInit {
  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${env.CLOUDFLARE_REALTIME_API_TOKEN}`
  };
}

function sanitizeName(value?: string): string {
  return (value ?? "").replace(/[\r\n\t]/g, " ").trim().slice(0, 32);
}

function json(value: unknown, status = 200): Response {
  return new Response(JSON.stringify(value), {
    status,
    headers: { "Content-Type": "application/json", "Cache-Control": "no-store" }
  });
}
