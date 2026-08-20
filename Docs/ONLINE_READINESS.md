# Fsp Online Readiness

Reviewed locally 2026-08-20. Deployment state must be re-verified before release.

## Confirmed working foundation

- Supabase project `Fsp` is active.
- Supabase Edge Function `matchmake` is deployed and active with JWT verification enabled.
- Required tables exist: `matchmaking_tickets`, `match_rooms`, `match_room_members`, `squad_members`.
- RLS is enabled on all four online/matchmaking tables.
- Supabase security advisors currently report no security lints.
- Unity has matchmaking and match-room clients.
- Unity has `NetworkSessionManager` plus Cloudflare WebSocket transport.
- Network session now auto-discovers the local player and transport at runtime where possible.
- Cloudflare voice worker validates the Supabase session and squad membership before joining the Cloudflare Realtime SFU.
- Unity WebRTC microphone capture, offer/answer signaling, remote audio, mute and hold-to-talk paths are implemented.

## Remaining blockers before a real two-device online match

1. Deploy `cloudflare/match-relay` and store its real `wss://.../ws` URL in Supabase `app_runtime_config.match_relay_ws_url`. Placeholder/empty URLs fall back to offline play.
2. Deploy `cloudflare/voice-worker`, configure its Cloudflare SFU and Supabase secrets, and store its HTTPS URL in `app_runtime_config.voice_token_endpoint`.
3. Verify both runtime settings can be read by authenticated clients under RLS.
4. Run a two-client smoke test: sign in, form/restore a squad, ready, queue, enter the same match, exchange player/combat/vehicle snapshots, verify reconnect/leave.
5. On the same two devices, verify microphone permission, muted-by-default behavior, hold-to-talk and remote audio.

## Build protection

The project validator checks the required local network and voice source files. External deployment and physical two-device tests remain release gates and cannot be proven by a Unity compile alone.
