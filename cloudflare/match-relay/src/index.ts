import { DurableObject } from "cloudflare:workers";

interface Env {
  MATCH_ROOMS: DurableObjectNamespace<MatchRoom>;
  SUPABASE_URL: string;
  SUPABASE_PUBLISHABLE_KEY: string;
}

type SocketAttachment = { playerId: string };
type Vec3 = { x: number; y: number; z: number };
type PlayerState = { position: Vec3; health: number; armor: number; alive: boolean; dropState: number; updatedAt: number };
type FireState = { origin: Vec3; direction: Vec3; firedAt: number; consumed: boolean };
type Envelope = { type: string; payload: string };
type ZonePhase = { wait: number; shrink: number; factor: number; shift: number; dps: number };

const STARTER_DAMAGE_CAP = 35;
const COUNTDOWN_SECONDS = 8;
const INITIAL_ZONE_RADIUS = 1100;
const PLAYABLE_HALF_EXTENT = 1200;
const DROP_GROUNDED = 0;
const DROP_ABOARD = 1;
const DROP_FREEFALL = 2;
const DROP_PARACHUTE = 3;
const ZONE_PHASES: ZonePhase[] = [
  { wait: 90, shrink: 70, factor: 0.72, shift: 0.35, dps: 1 },
  { wait: 65, shrink: 55, factor: 0.55, shift: 0.45, dps: 2 },
  { wait: 50, shrink: 45, factor: 0.42, shift: 0.55, dps: 4 },
  { wait: 35, shrink: 35, factor: 0.30, shift: 0.65, dps: 7 },
  { wait: 25, shrink: 28, factor: 0.18, shift: 0.75, dps: 11 },
  { wait: 15, shrink: 22, factor: 0.08, shift: 0.85, dps: 16 },
];
const CLIENT_TYPES = new Set(["snapshot", "bot_snapshot", "fire", "damage", "bot_damage", "zone_probe", "vehicle", "seat", "loot_claim", "appearance"]);

async function authenticate(request: Request, env: Env): Promise<string | null> {
  const auth = request.headers.get("Authorization") ?? "";
  if (!auth.startsWith("Bearer ")) return null;
  const res = await fetch(`${env.SUPABASE_URL}/auth/v1/user`, { headers: { Authorization: auth, apikey: env.SUPABASE_PUBLISHABLE_KEY } });
  if (!res.ok) return null;
  return (await res.json<{ id?: string }>()).id ?? null;
}

async function isMatchMember(request: Request, env: Env, matchId: string, userId: string): Promise<boolean> {
  const auth = request.headers.get("Authorization") ?? "";
  const url = `${env.SUPABASE_URL}/rest/v1/match_room_members?match_id=eq.${encodeURIComponent(matchId)}&user_id=eq.${encodeURIComponent(userId)}&select=user_id&limit=1`;
  const res = await fetch(url, { headers: { Authorization: auth, apikey: env.SUPABASE_PUBLISHABLE_KEY } });
  if (!res.ok) return false;
  return (await res.json<unknown[]>()).length === 1;
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
    const headers = new Headers({ Upgrade: "websocket", "x-fsp-player-id": userId, "x-fsp-match-id": matchId });
    return env.MATCH_ROOMS.getByName(matchId).fetch(new Request(request, { headers }));
  }
};

export class MatchRoom extends DurableObject<Env> {
  async fetch(request: Request): Promise<Response> {
    if (request.headers.get("Upgrade")?.toLowerCase() !== "websocket") return new Response("Upgrade required", { status: 426 });
    const playerId = request.headers.get("x-fsp-player-id")?.trim();
    const matchId = request.headers.get("x-fsp-match-id")?.trim();
    if (!playerId || !matchId) return new Response("Missing match identity", { status: 400 });

    if (!(await this.ctx.storage.get<string>("match-id"))) await this.ctx.storage.put("match-id", matchId);
    let startedAt = await this.ctx.storage.get<number>("world-started-at");
    if (!startedAt) { startedAt = Date.now() / 1000; await this.ctx.storage.put("world-started-at", startedAt); }

    const pair = new WebSocketPair();
    const [client, server] = Object.values(pair);
    server.serializeAttachment({ playerId } satisfies SocketAttachment);
    this.ctx.acceptWebSocket(server, [`player:${playerId}`]);
    await this.ensureBotAuthority(playerId);
    server.send(JSON.stringify({ type: "world_state", payload: JSON.stringify({ startedAt, serverNow: Date.now() / 1000, timestamp: Date.now() / 1000, countdownSeconds: COUNTDOWN_SECONDS }) }));
    return new Response(null, { status: 101, webSocket: client });
  }

  async webSocketMessage(ws: WebSocket, message: string | ArrayBuffer): Promise<void> {
    if (typeof message !== "string" || message.length > 16 * 1024) return;
    let envelope: Envelope;
    try { envelope = JSON.parse(message) as Envelope; } catch { return; }
    if (!envelope || !CLIENT_TYPES.has(envelope.type) || typeof envelope.payload !== "string") return;

    const attachment = ws.deserializeAttachment() as SocketAttachment | null;
    if (!attachment?.playerId) return;
    let payload: Record<string, unknown>;
    try { payload = JSON.parse(envelope.payload) as Record<string, unknown>; } catch { return; }

    if (envelope.type === "bot_snapshot") { await this.handleBotSnapshot(ws, attachment.playerId, payload); return; }
    if (envelope.type === "bot_damage") { await this.handleBotDamage(ws, attachment.playerId, payload); return; }
    if (envelope.type === "zone_probe") { await this.handleZoneProbe(attachment.playerId, payload); return; }

    if (["snapshot", "fire", "seat", "loot_claim", "appearance"].includes(envelope.type) && payload.playerId !== attachment.playerId) return;
    if (envelope.type === "damage" && payload.attackerId !== attachment.playerId) return;

    if (envelope.type === "vehicle") {
      if (payload.driverId !== attachment.playerId) return;
      const vehicleId = cleanString(payload.vehicleId, 64);
      if (!vehicleId || (await this.ctx.storage.get<string>(`seat:${vehicleId}`)) !== attachment.playerId) return;
      this.broadcast(message, ws);
      return;
    }

    if (envelope.type === "snapshot") { await this.handleSnapshot(ws, attachment.playerId, payload, message); return; }
    if (envelope.type === "fire") { await this.handleFire(ws, attachment.playerId, payload, message); return; }
    if (envelope.type === "damage") { await this.handleDamage(ws, attachment.playerId, payload); return; }

    if (envelope.type === "appearance") {
      const loadout = payload.loadout as Record<string, unknown> | undefined;
      if (!loadout) return;
      for (const key of ["headItemId", "faceItemId", "torsoItemId", "legsItemId", "backpackItemId", "parachuteItemId"])
        if (!cleanString(loadout[key], 80)) return;
      this.broadcast(message, ws);
      return;
    }

    if (envelope.type === "seat") { await this.handleSeat(attachment.playerId, payload); return; }
    if (envelope.type === "loot_claim") await this.handleLoot(attachment.playerId, payload);
  }

  private async handleZoneProbe(playerId: string, payload: Record<string, unknown>): Promise<void> {
    if (payload.playerId !== playerId) return;
    const player = await this.ctx.storage.get<PlayerState>(`player:${playerId}`);
    if (!player?.alive || player.dropState === DROP_ABOARD) return;
    const nowMs = Date.now();
    const last = await this.ctx.storage.get<number>(`zone-probe:${playerId}`) ?? 0;
    if (nowMs - last < 400) return;
    await this.ctx.storage.put(`zone-probe:${playerId}`, nowMs);

    const startedAt = await this.ctx.storage.get<number>("world-started-at");
    const matchId = await this.ctx.storage.get<string>("match-id");
    if (!startedAt || !matchId) return;
    const gameplayElapsed = Math.max(0, nowMs / 1000 - startedAt - COUNTDOWN_SECONDS);
    const zone = zoneAt(gameplayElapsed, matchId);
    if (distance2D(player.position, zone.center) <= zone.radius) return;

    const damage = Math.max(0, zone.dps * 0.5);
    if (damage <= 0) return;
    const sanitized = JSON.stringify({ attackerId: "zone", targetId: playerId, damage, hitPoint: player.position, timestamp: nowMs / 1000 });
    this.broadcast(JSON.stringify({ type: "damage", payload: sanitized }));
    await this.ctx.storage.put(`last-attacker:${playerId}`, "zone");
  }

  private async handleBotSnapshot(ws: WebSocket, senderId: string, payload: Record<string, unknown>): Promise<void> {
    const authority = await this.ctx.storage.get<string>("bot-authority");
    if (authority !== senderId) return;
    const botId = cleanString(payload.playerId, 48);
    if (!botId || !botId.startsWith("bot:")) return;
    payload.dropState = DROP_GROUNDED;
    const canonical = JSON.stringify({ ...payload, playerId: botId, dropState: DROP_GROUNDED });
    await this.handleSnapshot(ws, botId, payload, JSON.stringify({ type: "snapshot", payload: canonical }));
  }

  private async handleBotDamage(ws: WebSocket, senderId: string, payload: Record<string, unknown>): Promise<void> {
    const authority = await this.ctx.storage.get<string>("bot-authority");
    if (authority !== senderId) return;
    const attackerId = cleanString(payload.attackerId, 48);
    const targetId = cleanString(payload.targetId, 128);
    const damage = Number(payload.damage ?? 0);
    const hitPoint = readVec3(payload.hitPoint);
    if (!attackerId.startsWith("bot:") || !targetId || !hitPoint || !finiteRange(damage, 0.1, STARTER_DAMAGE_CAP)) return;
    const bot = await this.ctx.storage.get<PlayerState>(`player:${attackerId}`);
    const target = await this.ctx.storage.get<PlayerState>(`player:${targetId}`);
    if (!bot?.alive || !target?.alive) return;
    if (distance(bot.position, target.position) > 350 || distance(hitPoint, target.position) > 4.5) return;
    const now = Date.now();
    const key = `bot-damage:${attackerId}:${targetId}`;
    const last = await this.ctx.storage.get<number>(key) ?? 0;
    if (now - last < 55) return;
    await this.ctx.storage.put(key, now);
    await this.ctx.storage.put(`last-attacker:${targetId}`, attackerId);
    const sanitized = JSON.stringify({ attackerId, targetId, damage, hitPoint, timestamp: now / 1000 });
    this.broadcast(JSON.stringify({ type: "damage", payload: sanitized }), ws);
  }

  private async handleSnapshot(ws: WebSocket, playerId: string, payload: Record<string, unknown>, originalMessage: string): Promise<void> {
    const position = readVec3(payload.position);
    const health = Number(payload.health ?? 100);
    const armor = Number(payload.armor ?? 0);
    const alive = payload.alive !== false;
    const dropState = Number(payload.dropState ?? DROP_GROUNDED);
    if (!position || !finiteRange(health, 0, 100) || !finiteRange(armor, 0, 100) || !Number.isInteger(dropState) || dropState < DROP_GROUNDED || dropState > DROP_PARACHUTE) return;

    const now = Date.now();
    const startedAt = await this.ctx.storage.get<number>("world-started-at");
    const gameplayStarted = !!startedAt && now / 1000 >= startedAt + COUNTDOWN_SECONDS;
    const key = `player:${playerId}`;
    const previous = await this.ctx.storage.get<PlayerState>(key);

    if (playerId.startsWith("bot:") && dropState !== DROP_GROUNDED) return;
    if (!playerId.startsWith("bot:") && !isValidDropTransition(previous?.dropState, dropState, gameplayStarted)) return;

    if (previous) {
      const dt = Math.max(0.05, (now - previous.updatedAt) / 1000);
      const moved = distance(previous.position, position);
      const airborne = dropState === DROP_FREEFALL || dropState === DROP_PARACHUTE;
      const maxSpeed = airborne ? 125 : 95;
      if (moved / dt > maxSpeed || moved > 160 || (!previous.alive && alive) || health > previous.health + 60 || armor > previous.armor + 100) return;
    }

    await this.ctx.storage.put(key, { position, health, armor, alive, dropState, updatedAt: now } satisfies PlayerState);
    this.broadcast(originalMessage, ws);
    if (previous?.alive && !alive) {
      const killerId = await this.ctx.storage.get<string>(`last-attacker:${playerId}`) ?? "";
      this.broadcast(JSON.stringify({ type: "elimination", payload: JSON.stringify({ killerId, victimId: playerId, timestamp: now / 1000 }) }));
      await this.ctx.storage.delete(`last-attacker:${playerId}`);
    }
    await this.broadcastMatchState();
  }

  private async handleFire(ws: WebSocket, playerId: string, payload: Record<string, unknown>, originalMessage: string): Promise<void> {
    const origin = readVec3(payload.origin);
    const rawDirection = readVec3(payload.direction);
    const direction = rawDirection ? normalize(rawDirection) : null;
    if (!origin || !direction) return;
    const player = await this.ctx.storage.get<PlayerState>(`player:${playerId}`);
    if (!player?.alive || player.dropState === DROP_ABOARD || distance(origin, player.position) > 5.5) return;
    const now = Date.now();
    const last = await this.ctx.storage.get<FireState>(`fire:${playerId}`);
    if (last && now - last.firedAt < 55) return;
    await this.ctx.storage.put(`fire:${playerId}`, { origin, direction, firedAt: now, consumed: false } satisfies FireState);
    this.broadcast(originalMessage, ws);
  }

  private async handleDamage(ws: WebSocket, attackerId: string, payload: Record<string, unknown>): Promise<void> {
    const targetId = cleanString(payload.targetId, 128);
    const damage = Number(payload.damage ?? 0);
    const hitPoint = readVec3(payload.hitPoint);
    if (!targetId || targetId === attackerId || !hitPoint || !finiteRange(damage, 0.1, STARTER_DAMAGE_CAP)) return;
    const attacker = await this.ctx.storage.get<PlayerState>(`player:${attackerId}`);
    const target = await this.ctx.storage.get<PlayerState>(`player:${targetId}`);
    const fire = await this.ctx.storage.get<FireState>(`fire:${attackerId}`);
    if (!attacker?.alive || !target?.alive || !fire || fire.consumed || attacker.dropState === DROP_ABOARD) return;
    const now = Date.now();
    if (now - fire.firedAt > 350 || distance(attacker.position, target.position) > 350 || distance(hitPoint, target.position) > 4.5 || distancePointToRay(target.position, fire.origin, fire.direction) > 4.5 || dot(subtract(target.position, fire.origin), fire.direction) < -1) return;
    fire.consumed = true;
    await this.ctx.storage.put(`fire:${attackerId}`, fire);
    await this.ctx.storage.put(`last-attacker:${targetId}`, attackerId);
    const sanitized = JSON.stringify({ attackerId, targetId, damage, hitPoint, timestamp: now / 1000 });
    this.broadcast(JSON.stringify({ type: "damage", payload: sanitized }), ws);
  }

  private async handleSeat(playerId: string, payload: Record<string, unknown>): Promise<void> {
    const vehicleId = cleanString(payload.vehicleId, 64);
    if (!vehicleId) return;
    const seated = payload.seated === true;
    const key = `seat:${vehicleId}`;
    const existing = await this.ctx.storage.get<string>(key);
    let accepted = false;
    if (seated) { accepted = existing == null || existing === playerId; if (accepted) await this.ctx.storage.put(key, playerId); }
    else { accepted = existing == null || existing === playerId; if (existing === playerId) await this.ctx.storage.delete(key); }
    this.broadcast(JSON.stringify({ type: "seat", payload: JSON.stringify({ playerId, vehicleId, seated, accepted, timestamp: Date.now() / 1000 }) }));
  }

  private async handleLoot(playerId: string, payload: Record<string, unknown>): Promise<void> {
    const lootId = cleanString(payload.lootId, 96);
    if (!lootId) return;
    const key = `loot:${lootId}`;
    const existing = await this.ctx.storage.get<string>(key);
    const accepted = existing == null;
    if (accepted) await this.ctx.storage.put(key, playerId);
    this.broadcast(JSON.stringify({ type: "loot_claimed", payload: JSON.stringify({ playerId: accepted ? playerId : existing, lootId, accepted, timestamp: Date.now() / 1000 }) }));
  }

  private async ensureBotAuthority(preferredPlayerId?: string): Promise<void> {
    const current = await this.ctx.storage.get<string>("bot-authority");
    if (current && this.isPlayerConnected(current)) { this.broadcastBotAuthority(current); return; }
    let next = preferredPlayerId && this.isPlayerConnected(preferredPlayerId) ? preferredPlayerId : "";
    if (!next) for (const socket of this.ctx.getWebSockets()) {
      const attachment = socket.deserializeAttachment() as SocketAttachment | null;
      if (attachment?.playerId && socket.readyState === WebSocket.OPEN) { next = attachment.playerId; break; }
    }
    if (next) await this.ctx.storage.put("bot-authority", next); else await this.ctx.storage.delete("bot-authority");
    this.broadcastBotAuthority(next);
  }

  private isPlayerConnected(playerId: string): boolean {
    for (const socket of this.ctx.getWebSockets()) {
      const attachment = socket.deserializeAttachment() as SocketAttachment | null;
      if (attachment?.playerId === playerId && socket.readyState === WebSocket.OPEN) return true;
    }
    return false;
  }

  private broadcastBotAuthority(playerId: string): void {
    this.broadcast(JSON.stringify({ type: "bot_authority", payload: JSON.stringify({ playerId, timestamp: Date.now() / 1000 }) }));
  }

  private async broadcastMatchState(): Promise<void> {
    const states = await this.ctx.storage.list<PlayerState>({ prefix: "player:" });
    const aliveIds: string[] = [];
    for (const [key, value] of states) if (value.alive) aliveIds.push(key.slice("player:".length));
    const winnerId = aliveIds.length === 1 && states.size > 1 ? aliveIds[0] : "";
    this.broadcast(JSON.stringify({ type: "match_state", payload: JSON.stringify({ aliveCount: aliveIds.length, totalCount: states.size, winnerId, finished: !!winnerId, timestamp: Date.now() / 1000 }) }));
  }

  private broadcast(message: string, except?: WebSocket): void {
    for (const peer of this.ctx.getWebSockets()) if (peer !== except && peer.readyState === WebSocket.OPEN) peer.send(message);
  }

  async webSocketClose(ws: WebSocket, code: number, reason: string): Promise<void> {
    const attachment = ws.deserializeAttachment() as SocketAttachment | null;
    if (attachment?.playerId) {
      const seats = await this.ctx.storage.list<string>({ prefix: "seat:" });
      for (const [key, owner] of seats) if (owner === attachment.playerId) await this.ctx.storage.delete(key);
      const authority = await this.ctx.storage.get<string>("bot-authority");
      if (authority === attachment.playerId) { await this.ctx.storage.delete("bot-authority"); await this.ensureBotAuthority(); }
    }
    ws.close(code, reason);
  }

  async webSocketError(_ws: WebSocket, error: unknown): Promise<void> { console.error("match relay websocket error", error); }
}

function isValidDropTransition(previous: number | undefined, next: number, gameplayStarted: boolean): boolean {
  if (previous == null) {
    if (!gameplayStarted) return next === DROP_GROUNDED || next === DROP_ABOARD;
    return next === DROP_GROUNDED || next === DROP_ABOARD || next === DROP_FREEFALL;
  }
  if (previous === next) return true;
  if (previous === DROP_GROUNDED) return !gameplayStarted && next === DROP_ABOARD;
  if (previous === DROP_ABOARD) return gameplayStarted && next === DROP_FREEFALL;
  if (previous === DROP_FREEFALL) return next === DROP_PARACHUTE || next === DROP_GROUNDED;
  if (previous === DROP_PARACHUTE) return next === DROP_GROUNDED;
  return false;
}

function zoneAt(elapsed: number, matchId: string): { center: Vec3; radius: number; dps: number } {
  let remaining = Math.max(0, elapsed);
  let center: Vec3 = { x: 0, y: 0, z: 0 };
  let radius = INITIAL_ZONE_RADIUS;
  let dps = 0;
  for (let i = 0; i < ZONE_PHASES.length; i++) {
    const p = ZONE_PHASES[i];
    const nextRadius = Math.max(12, radius * p.factor);
    const nextCenter = pickZoneCenter(center, radius, nextRadius, p.shift, i, matchId);
    dps = p.dps;
    if (remaining < p.wait) return { center, radius, dps };
    remaining -= p.wait;
    if (remaining < p.shrink) {
      const t = clamp01(remaining / Math.max(1, p.shrink));
      const e = t * t * (3 - 2 * t);
      return { center: lerp3(center, nextCenter, e), radius: radius + (nextRadius - radius) * e, dps };
    }
    remaining -= p.shrink;
    center = nextCenter;
    radius = nextRadius;
  }
  return { center, radius, dps };
}

function pickZoneCenter(current: Vec3, currentRadius: number, nextRadius: number, shift: number, phase: number, matchId: string): Vec3 {
  const angle = hash01(`${matchId}:zone:${phase}:a`) * Math.PI * 2;
  const maxShift = Math.max(0, currentRadius - nextRadius) * clamp01(shift);
  const dist = maxShift * (0.35 + 0.65 * hash01(`${matchId}:zone:${phase}:d`));
  const min = -PLAYABLE_HALF_EXTENT + nextRadius;
  const max = PLAYABLE_HALF_EXTENT - nextRadius;
  return { x: clamp(current.x + Math.cos(angle) * dist, min, max), y: 0, z: clamp(current.z + Math.sin(angle) * dist, min, max) };
}

function hash01(value: string): number {
  let hash = 23 >>> 0;
  for (let i = 0; i < value.length; i++) hash = (Math.imul(hash, 31) + value.charCodeAt(i)) >>> 0;
  return (hash & 0x00ffffff) / 16777215;
}

function cleanString(value: unknown, max: number): string { return typeof value === "string" ? value.trim().slice(0, max) : ""; }
function readVec3(value: unknown): Vec3 | null { if (!value || typeof value !== "object") return null; const v = value as Record<string, unknown>; const x = Number(v.x), y = Number(v.y), z = Number(v.z); return Number.isFinite(x) && Number.isFinite(y) && Number.isFinite(z) ? { x, y, z } : null; }
function normalize(v: Vec3): Vec3 | null { const m = Math.sqrt(v.x * v.x + v.y * v.y + v.z * v.z); return Number.isFinite(m) && m >= 0.0001 ? { x: v.x / m, y: v.y / m, z: v.z / m } : null; }
function subtract(a: Vec3, b: Vec3): Vec3 { return { x: a.x - b.x, y: a.y - b.y, z: a.z - b.z }; }
function dot(a: Vec3, b: Vec3): number { return a.x * b.x + a.y * b.y + a.z * b.z; }
function distancePointToRay(point: Vec3, origin: Vec3, direction: Vec3): number { const toPoint = subtract(point, origin); const t = Math.max(0, dot(toPoint, direction)); const closest = { x: origin.x + direction.x * t, y: origin.y + direction.y * t, z: origin.z + direction.z * t }; return distance(point, closest); }
function distance(a: Vec3, b: Vec3): number { const x = a.x - b.x, y = a.y - b.y, z = a.z - b.z; return Math.sqrt(x * x + y * y + z * z); }
function distance2D(a: Vec3, b: Vec3): number { const x = a.x - b.x, z = a.z - b.z; return Math.sqrt(x * x + z * z); }
function lerp3(a: Vec3, b: Vec3, t: number): Vec3 { return { x: a.x + (b.x - a.x) * t, y: a.y + (b.y - a.y) * t, z: a.z + (b.z - a.z) * t }; }
function clamp01(v: number): number { return Math.max(0, Math.min(1, v)); }
function clamp(v: number, min: number, max: number): number { return Math.max(min, Math.min(max, v)); }
function finiteRange(value: number, min: number, max: number): boolean { return Number.isFinite(value) && value >= min && value <= max; }