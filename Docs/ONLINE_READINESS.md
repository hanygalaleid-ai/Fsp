# Fsp Online Readiness

Verified 2026-08-19.

## Confirmed working foundation

- Supabase project `Fsp` is active.
- Supabase Edge Function `matchmake` is deployed and active with JWT verification enabled.
- Required tables exist: `matchmaking_tickets`, `match_rooms`, `match_room_members`, `squad_members`.
- RLS is enabled on all four online/matchmaking tables.
- Supabase security advisors currently report no security lints.
- Unity has matchmaking and match-room clients.
- Unity has `NetworkSessionManager` plus Cloudflare WebSocket transport.
- Network session now auto-discovers the local player and transport at runtime where possible.
- Cloudflare voice worker validates Supabase session and squad membership before issuing a RealtimeKit participant token.

## Remaining blockers before a real two-device online match

1. Deploy `cloudflare/match-relay` and set its real `wss://.../ws` URL in the Match scene transport. Placeholder relay URLs are now rejected.
2. Ensure the Match scene contains or runtime-installs the network transport/session components.
3. Assign an authored `remotePlayerPrefab` so remote snapshots have a visible player representation.
4. Complete the actual RealtimeKit Unity audio runtime/bridge. Receiving a voice token alone is not treated as an active voice connection anymore.
5. Run a two-client smoke test: sign in, queue, match into the same room, connect relay, exchange snapshots, verify damage/events, verify reconnect/leave.

## Build protection

The project validator now blocks online builds when required Match scene/network wiring is missing or the relay URL is still a placeholder.
