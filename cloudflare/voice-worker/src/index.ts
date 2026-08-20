import { DurableObject } from "cloudflare:workers";

interface Env {
  VOICE_ROOM_STATE: DurableObjectNamespace<VoiceRoomState>;
  REALTIME_SFU_APP_ID: string;
  REALTIME_SFU_APP_SECRET: string;
  SUPABASE_URL: string;
  SUPABASE_PUBLISHABLE_KEY: string;
}

type JoinRequest = { squadId?: string; sdp?: string };
type SyncRequest = { squadId?: string; sessionId?: string };
type RenegotiateRequest = { squadId?: string; sessionId?: string; sdp?: string };
type LeaveRequest = { squadId?: string; sessionId?: string };
type RoomPeer = { userId: string; sessionId: string; trackName: string };
type PendingResult = { peers: RoomPeer[]; keys: string[] };

type SfuDescription = { type?: string; sdp?: string };
type SfuResponse = {
  sessionId?: string;
  sessionDescription?: SfuDescription;
  tracks?: Array<{ trackName?: string; sessionId?: string; location?: string; kind?: string }>;
};

export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    const url = new URL(request.url);
    if (request.method !== "POST") return json({ error: "Method not allowed" }, 405);

    const auth = request.headers.get("Authorization") ?? "";
    if (!auth.startsWith("Bearer ")) return json({ error: "Unauthorized" }, 401);
    const accessToken = auth.slice(7);
    const user = await getSupabaseUser(env, accessToken);
    if (!user?.id) return json({ error: "Invalid session" }, 401);

    try {
      if (url.pathname === "/join") return handleJoin(request, env, accessToken, user.id);
      if (url.pathname === "/sync") return handleSync(request, env, accessToken, user.id);
      if (url.pathname === "/renegotiate") return handleRenegotiate(request, env, accessToken, user.id);
      if (url.pathname === "/leave") return handleLeave(request, env, accessToken, user.id);
      return json({ error: "Not found" }, 404);
    } catch (error) {
      console.error("voice SFU signaling failed", error);
      return json({ error: "Voice service unavailable" }, 502);
    }
  }
};

async function handleJoin(request: Request, env: Env, accessToken: string, userId: string): Promise<Response> {
  const body = await readJson<JoinRequest>(request);
  const squadId = clean(body.squadId, 80);
  const sdp = body.sdp?.trim() ?? "";
  if (!squadId || !sdp) return json({ error: "squadId and sdp required" }, 400);
  if (!(await verifySquadMembership(env, accessToken, squadId, userId))) return json({ error: "Not a squad member" }, 403);

  const created = await sfu(env, "/sessions/new", "POST", {});
  const sessionId = created.sessionId;
  if (!sessionId) throw new Error("SFU session ID missing");

  const trackName = `mic-${userId}`;
  const published = await sfu(env, `/sessions/${encodeURIComponent(sessionId)}/tracks/new`, "POST", {
    sessionDescription: { type: "offer", sdp },
    tracks: [{ location: "local", trackName, kind: "audio" }]
  });

  const resolvedTrackName = published.tracks?.find(t => t.location === "local")?.trackName ?? trackName;
  await roomCall(env, squadId, "/join", { userId, sessionId, trackName: resolvedTrackName });

  return json({
    sessionId,
    sdp: published.sessionDescription?.sdp ?? "",
    sdpType: published.sessionDescription?.type ?? "answer",
    publishedTrackName: resolvedTrackName
  });
}

async function handleSync(request: Request, env: Env, accessToken: string, userId: string): Promise<Response> {
  const body = await readJson<SyncRequest>(request);
  const squadId = clean(body.squadId, 80);
  const sessionId = clean(body.sessionId, 128);
  if (!squadId || !sessionId) return json({ error: "squadId and sessionId required" }, 400);
  if (!(await verifySquadMembership(env, accessToken, squadId, userId))) return json({ error: "Not a squad member" }, 403);
  if (!(await ownsVoiceSession(env, squadId, userId, sessionId))) return json({ error: "Invalid voice session" }, 403);

  const pending = await roomCall<PendingResult>(env, squadId, "/pending", { userId, subscriberSessionId: sessionId });
  const remotes = pending.peers ?? [];
  if (remotes.length === 0) return json({ changed: false, sdp: "", sdpType: "" });

  const pulled = await sfu(env, `/sessions/${encodeURIComponent(sessionId)}/tracks/new`, "POST", {
    tracks: remotes.map(p => ({ location: "remote", sessionId: p.sessionId, trackName: p.trackName }))
  });

  await roomCall(env, squadId, "/mark-subscribed", {
    userId,
    subscriberSessionId: sessionId,
    keys: pending.keys ?? []
  });

  return json({
    changed: !!pulled.sessionDescription?.sdp,
    sdp: pulled.sessionDescription?.sdp ?? "",
    sdpType: pulled.sessionDescription?.type ?? "offer"
  });
}

async function handleRenegotiate(request: Request, env: Env, accessToken: string, userId: string): Promise<Response> {
  const body = await readJson<RenegotiateRequest>(request);
  const squadId = clean(body.squadId, 80);
  const sessionId = clean(body.sessionId, 128);
  const sdp = body.sdp?.trim() ?? "";
  if (!squadId || !sessionId || !sdp) return json({ error: "squadId, sessionId and sdp required" }, 400);
  if (!(await verifySquadMembership(env, accessToken, squadId, userId))) return json({ error: "Not a squad member" }, 403);
  if (!(await ownsVoiceSession(env, squadId, userId, sessionId))) return json({ error: "Invalid voice session" }, 403);

  await sfu(env, `/sessions/${encodeURIComponent(sessionId)}/renegotiate`, "PUT", {
    sessionDescription: { type: "answer", sdp }
  });
  return json({ ok: true });
}

async function handleLeave(request: Request, env: Env, accessToken: string, userId: string): Promise<Response> {
  const body = await readJson<LeaveRequest>(request);
  const squadId = clean(body.squadId, 80);
  const sessionId = clean(body.sessionId, 128);
  if (!squadId || !sessionId) return json({ error: "squadId and sessionId required" }, 400);
  if (!(await verifySquadMembership(env, accessToken, squadId, userId))) return json({ error: "Not a squad member" }, 403);
  if (!(await ownsVoiceSession(env, squadId, userId, sessionId))) return json({ error: "Invalid voice session" }, 403);
  await roomCall(env, squadId, "/leave", { userId, sessionId });
  return json({ ok: true });
}

async function sfu(env: Env, path: string, method: "POST" | "PUT", body: unknown): Promise<SfuResponse> {
  if (!env.REALTIME_SFU_APP_ID || !env.REALTIME_SFU_APP_SECRET) throw new Error("Realtime SFU secrets missing");
  const endpoint = `https://rtc.live.cloudflare.com/v1/apps/${encodeURIComponent(env.REALTIME_SFU_APP_ID)}${path}`;
  const res = await fetch(endpoint, {
    method,
    headers: {
      Authorization: `Bearer ${env.REALTIME_SFU_APP_SECRET}`,
      "Content-Type": "application/json"
    },
    body: JSON.stringify(body)
  });
  const text = await res.text();
  if (!res.ok) throw new Error(`SFU ${method} ${path} failed (${res.status}): ${text.slice(0, 500)}`);
  return text ? JSON.parse(text) as SfuResponse : {};
}

async function roomCall<T = Record<string, unknown>>(env: Env, squadId: string, path: string, payload: unknown): Promise<T> {
  const stub = env.VOICE_ROOM_STATE.getByName(squadId);
  const res = await stub.fetch(`https://voice-room.internal${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });
  if (!res.ok) throw new Error(`Voice room state failed: ${await res.text()}`);
  return await res.json<T>();
}

async function getSupabaseUser(env: Env, token: string): Promise<{ id: string } | null> {
  const res = await fetch(`${env.SUPABASE_URL}/auth/v1/user`, {
    headers: { apikey: env.SUPABASE_PUBLISHABLE_KEY, Authorization: `Bearer ${token}` }
  });
  if (!res.ok) return null;
  return await res.json<{ id: string }>();
}

async function ownsVoiceSession(env: Env, squadId: string, userId: string, sessionId: string): Promise<boolean> {
  const result = await roomCall<{ owned?: boolean }>(env, squadId, "/owns-session", { userId, sessionId });
  return result.owned === true;
}

async function verifySquadMembership(env: Env, token: string, squadId: string, userId: string): Promise<boolean> {
  const url = new URL(`${env.SUPABASE_URL}/rest/v1/squad_members`);
  url.searchParams.set("squad_id", `eq.${squadId}`);
  url.searchParams.set("user_id", `eq.${userId}`);
  url.searchParams.set("select", "user_id");
  url.searchParams.set("limit", "1");
  const res = await fetch(url, {
    headers: { apikey: env.SUPABASE_PUBLISHABLE_KEY, Authorization: `Bearer ${token}` }
  });
  if (!res.ok) return false;
  const rows = await res.json<unknown[]>();
  return rows.length === 1;
}

export class VoiceRoomState extends DurableObject<Env> {
  async fetch(request: Request): Promise<Response> {
    if (request.method !== "POST") return json({ error: "Method not allowed" }, 405);
    const url = new URL(request.url);
    const body = await readJson<any>(request);

    if (url.pathname === "/join") {
      const userId = clean(body.userId, 128);
      const sessionId = clean(body.sessionId, 128);
      const trackName = clean(body.trackName, 160);
      if (!userId || !sessionId || !trackName) return json({ error: "Invalid peer" }, 400);
      await this.ctx.storage.put(`peer:${userId}`, { userId, sessionId, trackName, touchedAt: Date.now() });
      return json({ ok: true });
    }

    if (url.pathname === "/pending") {
      const userId = clean(body.userId, 128);
      const subscriberSessionId = clean(body.subscriberSessionId, 128);
      if (!userId || !subscriberSessionId) return json({ error: "Invalid subscriber" }, 400);

      const stored = await this.ctx.storage.list<RoomPeer & { touchedAt?: number }>({ prefix: "peer:" });
      const subscribedKey = `subs:${userId}:${subscriberSessionId}`;
      const subscribed = new Set((await this.ctx.storage.get<string[]>(subscribedKey)) ?? []);
      const now = Date.now();
      const peers: RoomPeer[] = [];
      const keys: string[] = [];

      for (const [key, value] of stored) {
        if (value.touchedAt && now - value.touchedAt > 6 * 60 * 60 * 1000) {
          await this.ctx.storage.delete(key);
          continue;
        }
        if (value.userId === userId) continue;
        const remoteKey = `${value.sessionId}:${value.trackName}`;
        if (subscribed.has(remoteKey)) continue;
        peers.push({ userId: value.userId, sessionId: value.sessionId, trackName: value.trackName });
        keys.push(remoteKey);
      }
      return json({ peers, keys });
    }

    if (url.pathname === "/owns-session") {
      const userId = clean(body.userId, 128);
      const sessionId = clean(body.sessionId, 128);
      if (!userId || !sessionId) return json({ owned: false });
      const peer = await this.ctx.storage.get<RoomPeer>(`peer:${userId}`);
      return json({ owned: peer?.sessionId === sessionId });
    }

    if (url.pathname === "/mark-subscribed") {
      const userId = clean(body.userId, 128);
      const subscriberSessionId = clean(body.subscriberSessionId, 128);
      const keys = Array.isArray(body.keys) ? body.keys.filter((v: unknown) => typeof v === "string").slice(0, 16) : [];
      if (!userId || !subscriberSessionId) return json({ error: "Invalid subscriber" }, 400);
      const storageKey = `subs:${userId}:${subscriberSessionId}`;
      const current = new Set((await this.ctx.storage.get<string[]>(storageKey)) ?? []);
      for (const value of keys) current.add(String(value).slice(0, 320));
      await this.ctx.storage.put(storageKey, Array.from(current).slice(-64));
      return json({ ok: true });
    }

    if (url.pathname === "/leave") {
      const userId = clean(body.userId, 128);
      if (userId) await this.ctx.storage.delete(`peer:${userId}`);
      return json({ ok: true });
    }

    return json({ error: "Not found" }, 404);
  }
}

async function readJson<T>(request: Request): Promise<T> {
  try { return await request.json<T>(); }
  catch { throw new Error("Invalid JSON"); }
}

function clean(value: unknown, max: number): string {
  return typeof value === "string" ? value.trim().slice(0, max) : "";
}

function json(value: unknown, status = 200): Response {
  return new Response(JSON.stringify(value), {
    status,
    headers: { "Content-Type": "application/json", "Cache-Control": "no-store" }
  });
}
