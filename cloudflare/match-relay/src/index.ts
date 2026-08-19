import { DurableObject } from "cloudflare:workers";

interface Env {
  MATCH_ROOMS: DurableObjectNamespace<MatchRoom>;
  SUPABASE_URL: string;
  SUPABASE_PUBLISHABLE_KEY: string;
}

type SocketAttachment = { playerId: string };
type Envelope = { type: "snapshot" | "fire" | "damage" | "vehicle" | "seat" | "loot_claim" | "loot_claimed" | "appearance" | "match_state" | "elimination"; payload: string };
type PlayerState = { position: Vec3; health: number; armor: number; alive: boolean; updatedAt: number };
type FireState = { origin: Vec3; direction: Vec3; firedAt: number; consumed: boolean };
type Vec3 = { x: number; y: number; z: number };

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
      const currentDriver = await this.ctx.storage.get<string>(`seat:${payload.vehicleId}`);
      if (currentDriver !== attachment.playerId) return;
    } else if (payload.attackerId !== attachment.playerId) {
      return;
    }

    if (envelope.type === "snapshot") {
      await this.handleSnapshot(ws, attachment.playerId, payload, message);
      return;
    }

    if (envelope.type === "damage") {
      await this.handleDamage(ws, attachment.playerId, payload);
      return;
    }

    if (envelope.type === "fire") {
      await this.handleFire(ws, attachment.playerId, payload, message);
      return;
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
      const vehicleId = typeof payload.vehicleId === "string" ? payload.vehicleId.trim() : "";
      if (!vehicleId || vehicleId.length > 64) return;
      const seated = payload.seated === true;
      const key = `seat:${vehicleId}`;
      const existing = await this.ctx.storage.get<string>(key);
      let accepted = false;
      if (seated) {
        accepted = existing == null || existing === attachment.playerId;
        if (accepted) await this.ctx.storage.put(key, attachment.playerId);
      } else {
        accepted = existing == null || existing === attachment.playerId;
        if (existing === attachment.playerId) await this.ctx.storage.delete(key);
      }
      const resultPayload = JSON.stringify({ playerId: attachment.playerId, vehicleId, seated, accepted, timestamp: Date.now() / 1000 });
      this.broadcast(JSON.stringify({ type: "seat", payload: resultPayload }));
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

  private async handleFire(ws: WebSocket, playerId: string, payload: Record<string, unknown>, originalMessage: string): Promise<void> {
    const origin = readVec3(payload.origin);
    const directionRaw = readVec3(payload.direction);
    if (!origin || !directionRaw) return;
    const direction = normalize(directionRaw);
    if (!direction) return;

    const player = await this.ctx.storage.get<PlayerState>(`player:${playerId}`);
    if (!player?.alive) return;
    if (distance(origin, player.position) > 5.5) return;

    const now = Date.now();
    const last = await this.ctx.storage.get<FireState>(`fire:${playerId}`);
    if (last && now - last.firedAt < 55) return;

    await this.ctx.storage.put(`fire:${playerId}`, { origin, direction, firedAt: now, consumed: false } satisfies FireState);
    this.broadcast(originalMessage, ws);
  }

  private async handleSnapshot(ws: WebSocket, playerId: string, payload: Record<string, unknown>, originalMessage: string): Promise<void> {
    const position = readVec3(payload.position);
    const health = Number(payload.health ?? 100);
    const armor = Number(payload.armor ?? 0);
    const alive = payload.alive !== false;
    if (!position || !finiteRange(health, 0, 100) || !finiteRange(armor, 0, 100)) return;

    const now = Date.now();
    const key = `player:${playerId}`;
    const previous = await this.ctx.storage.get<PlayerState>(key);
    if (previous) {
      const dt = Math.max(0.05, (now - previous.updatedAt) / 1000);
      const moved = distance(previous.position, position);
      const speed = moved / dt;
      if (speed > 95 || moved > 140) return;
      if (!previous.alive && alive) return;
      if (health > previous.health + 60) return;
      if (armor > previous.armor + 100) return;
    }

    await this.ctx.storage.put(key, { position, health, armor, alive, updatedAt: now } satisfies PlayerState);
    this.broadcast(originalMessage, ws);

    if (previous?.alive && !alive) {
      const killerId = await this.ctx.storage.get<string>(`last-attacker:${playerId}`) ?? "";
      const eliminationPayload = JSON.stringify({ killerId, victimId: playerId, timestamp: now / 1000 });
      this.broadcast(JSON.stringify({ type: "elimination", payload: eliminationPayload }));
      await this.ctx.storage.delete(`last-attacker:${playerId}`);
    }

    await this.broadcastMatchState();
  }

  private async handleDamage(ws: WebSocket, attackerId: string, payload: Record<string, unknown>): Promise<void> {
    const targetId = typeof payload.targetId === "string" ? payload.targetId.trim() : "";
    const damage = Number(payload.damage ?? 0);
    const hitPoint = readVec3(payload.hitPoint);
    if (!targetId || targetId === attackerId || !hitPoint || !finiteRange(damage, 0.1, 90)) return;

    const attacker = await this.ctx.storage.get<PlayerState>(`player:${attackerId}`);
    const target = await this.ctx.storage.get<PlayerState>(`player:${targetId}`);
    const fire = await this.ctx.storage.get<FireState>(`fire:${attackerId}`);
    if (!attacker?.alive || !target?.alive || !fire || fire.consumed) return;

    const now = Date.now();
    if (now - fire.firedAt > 350) return;
    if (distance(attacker.position, target.position) > 350) return;
    if (distance(hitPoint, target.position) > 4.5) return;
    if (distancePointToRay(target.position, fire.origin, fire.direction) > 4.5) return;
    if (dot(subtract(target.position, fire.origin), fire.direction) < -1) return;

    fire.consumed = true;
    await this.ctx.storage.put(`fire:${attackerId}`, fire);
    await this.ctx.storage.put(`last-attacker:${targetId}`, attackerId);

    const sanitized = JSON.stringify({ attackerId, targetId, damage, hitPoint, timestamp: now / 1000 });
    this.broadcast(JSON.stringify({ type: "damage", payload: sanitized }), ws);
  }

  private async broadcastMatchState(): Promise<void> {
    const states = await this.ctx.storage.list<PlayerState>({ prefix: "player:" });
    const aliveIds: string[] = [];
    for (const [key, value] of states) if (value.alive) aliveIds.push(key.slice("player:".length));
    const winnerId = aliveIds.length === 1 && states.size > 1 ? aliveIds[0] : "";
    const payload = JSON.stringify({ aliveCount: aliveIds.length, totalCount: states.size, winnerId, finished: !!winnerId, timestamp: Date.now() / 1000 });
    this.broadcast(JSON.stringify({ type: "match_state", payload }));
  }

  private broadcast(message: string, except?: WebSocket): void {
    for (const peer of this.ctx.getWebSockets()) if (peer !== except && peer.readyState === WebSocket.OPEN) peer.send(message);
  }

  async webSocketClose(ws: WebSocket, code: number, reason: string): Promise<void> {
    const attachment = ws.deserializeAttachment() as SocketAttachment | null;
    if (attachment?.playerId) {
      const seats = await this.ctx.storage.list<string>({ prefix: "seat:" });
      for (const [key, owner] of seats) if (owner === attachment.playerId) await this.ctx.storage.delete(key);
    }
    ws.close(code, reason);
  }

  async webSocketError(_ws: WebSocket, error: unknown): Promise<void> { console.error("match relay websocket error", error); }
}

function readVec3(value: unknown): Vec3 | null {
  if (!value || typeof value !== "object") return null;
  const v = value as Record<string, unknown>;
  const x = Number(v.x), y = Number(v.y), z = Number(v.z);
  return Number.isFinite(x) && Number.isFinite(y) && Number.isFinite(z) ? { x, y, z } : null;
}

function normalize(v: Vec3): Vec3 | null {
  const m = Math.sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
  if (!Number.isFinite(m) || m < 0.0001) return null;
  return { x: v.x / m, y: v.y / m, z: v.z / m };
}

function subtract(a: Vec3, b: Vec3): Vec3 {
  return { x: a.x - b.x, y: a.y - b.y, z: a.z - b.z };
}

function dot(a: Vec3, b: Vec3): number {
  return a.x * b.x + a.y * b.y + a.z * b.z;
}

function distancePointToRay(point: Vec3, origin: Vec3, direction: Vec3): number {
  const toPoint = subtract(point, origin);
  const t = Math.max(0, dot(toPoint, direction));
  const closest = { x: origin.x + direction.x * t, y: origin.y + direction.y * t, z: origin.z + direction.z * t };
  return distance(point, closest);
}

function distance(a: Vec3, b: Vec3): number {
  const x = a.x - b.x, y = a.y - b.y, z = a.z - b.z;
  return Math.sqrt(x * x + y * y + z * z);
}

function finiteRange(value: number, min: number, max: number): boolean {
  return Number.isFinite(value) && value >= min && value <= max;
}
