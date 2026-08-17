import { DurableObject } from "cloudflare:workers";

interface Env {
  MATCH_ROOMS: DurableObjectNamespace<MatchRoom>;
  SUPABASE_URL: string;
  SUPABASE_PUBLISHABLE_KEY: string;
}

type SocketAttachment = { playerId: string };
type Envelope = { type: "snapshot" | "fire" | "damage" | "vehicle" | "seat" | "loot_claim" | "loot_claimed" | "appearance"; payload: string };

async function authenticate(request: Request, env: Env): Promise<string | null> {
  const auth = request.headers.get("Authorization") ?? "";
  if (!auth.startsWith("Bearer ")) return null;
  const res = await fetch(`${env.SUPABASE_URL}/auth/v1/user`, { headers: { Authorization: auth, apikey: env.SUPABASE_PUBLISHABLE_KEY } });
  if (!res.ok) return null;
  const user = await res.json<{ id?: string }>();
  return user.id ?? null;
}

async function isMatchMember(request: Request, env: Env, matchId: string, userId: string): Promise<boolean> {
  const auth = request.headers.get("Authorization") ?? "";
  const url = `${env.SUPABASE_URL}/rest/v1/match_room_members?match_id=eq.${encodeURIComponent(matchId)}&user_id=eq.${encodeURIComponent(userId)}&select=user_id&limit=1`;
  const res = await fetch(url, { headers: { Authorization: auth, apikey: env.SUPABASE_PUBLISHABLE_KEY } });
  if (!res.ok) return false;
  const rows = await res.json<unknown[]>();
  return rows.length === 1;
}

export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    const url = new URL(request.url);
    if (url.pathname !== "/ws") return new Response("Not found", { status: 404 });
    if (request.headers.get("Upgrade")?.toLowerCase() !== "websocket") return new Response("Upgrade required", { status: 426 });
    const matchId = url.searchParams.get("matchId")?.trim();
    if (!matchId) return new Response("matchId required", { status: 400 });
    const userId = await authenticate(request, env);
    if (!userId) return new Response("Unauthorized", { status: 401 });
    if (!(await isMatchMember(request, env, matchId, userId))) return new Response("Forbidden", { status: 403 });
    return env.MATCH_ROOMS.getByName(matchId).fetch(new Request(request, { headers: new Headers({ Upgrade: "websocket", "x-fsp-player-id": userId }) }));
  },
};

export class MatchRoom extends DurableObject<Env> {
  async fetch(request: Request): Promise<Response> {
    if (request.headers.get("Upgrade")?.toLowerCase() !== "websocket") return new Response("Upgrade required", { status: 426 });
    const playerId = request.headers.get("x-fsp-player-id");
    if (!playerId) return new Response("Missing player identity", { status: 400 });
    const pair = new WebSocketPair();
    const [client, server] = Object.values(pair);
    server.serializeAttachment({ playerId } satisfies SocketAttachment);
    this.ctx.acceptWebSocket(server, [`player:${playerId}`]);
    return new Response(null, { status: 101, webSocket: client });
  }

  async webSocketMessage(ws: WebSocket, message: string | ArrayBuffer): Promise<void> {
    if (typeof message !== "string" || message.length > 16 * 1024) return;
    let envelope: Envelope;
    try { envelope = JSON.parse(message) as Envelope; } catch { return; }
    if (!envelope || !["snapshot", "fire", "damage", "vehicle", "seat", "loot_claim", "appearance"].includes(envelope.type) || typeof envelope.payload !== "string") return;

    const attachment = ws.deserializeAttachment() as SocketAttachment | null;
    if (!attachment?.playerId) return;
    let payload: Record<string, unknown>;
    try { payload = JSON.parse(envelope.payload) as Record<string, unknown>; } catch { return; }

    if (["snapshot", "fire", "seat", "loot_claim", "appearance"].includes(envelope.type)) {
      if (payload.playerId !== attachment.playerId) return;
    } else if (envelope.type === "vehicle") {
      if (payload.driverId !== attachment.playerId) return;
      if (typeof payload.vehicleId !== "string" || payload.vehicleId.length < 1 || payload.vehicleId.length > 64) return;
    } else {
      if (payload.attackerId !== attachment.playerId) return;
      const damage = Number(payload.damage ?? 0);
      if (!Number.isFinite(damage) || damage <= 0 || damage > 200) return;
    }

    if (envelope.type === "appearance") {
      const loadout = payload.loadout as Record<string, unknown> | undefined;
      if (!loadout) return;
      for (const key of ["headItemId", "faceItemId", "torsoItemId", "legsItemId", "backpackItemId", "parachuteItemId"]) {
        const value = loadout[key];
        if (typeof value !== "string" || value.length < 1 || value.length > 80) return;
      }
      this.broadcast(message, ws);
      return;
    }

    if (envelope.type === "seat") {
      if (typeof payload.vehicleId !== "string" || payload.vehicleId.length < 1 || payload.vehicleId.length > 64) return;
      this.broadcast(message, ws);
      return;
    }

    if (envelope.type === "loot_claim") {
      const lootId = typeof payload.lootId === "string" ? payload.lootId.trim() : "";
      if (!lootId || lootId.length > 96) return;
      const key = `loot:${lootId}`;
      const existing = await this.ctx.storage.get<string>(key);
      const accepted = existing == null;
      if (accepted) await this.ctx.storage.put(key, attachment.playerId);
      const resultPayload = JSON.stringify({ playerId: accepted ? attachment.playerId : existing, lootId, accepted, timestamp: Date.now() / 1000 });
      this.broadcast(JSON.stringify({ type: "loot_claimed", payload: resultPayload }));
      return;
    }

    this.broadcast(message, ws);
  }

  private broadcast(message: string, except?: WebSocket): void {
    for (const peer of this.ctx.getWebSockets()) if (peer !== except && peer.readyState === WebSocket.OPEN) peer.send(message);
  }

  async webSocketClose(ws: WebSocket, code: number, reason: string): Promise<void> { ws.close(code, reason); }
  async webSocketError(_ws: WebSocket, error: unknown): Promise<void> { console.error("match relay websocket error", error); }
}
