# Fsp MVP — Final Integration Blockers

This file records the remaining items that must be completed before calling the MVP build-ready.

## 1. Unity project shell is incomplete in this repository

Current repository contains `Assets/` gameplay/backend code and service code, but does not currently include a complete Unity project shell (`Packages/`, `ProjectSettings/`, scenes, imported 3D assets, prefabs and their serialized references).

Required before a real Unity compile/build can be claimed:
- Restore/import the original Unity project shell used as the base.
- Confirm Unity editor version and commit `ProjectSettings/ProjectVersion.txt`.
- Commit `Packages/manifest.json` and package lock.
- Wire gameplay scripts to prefabs/scenes and validate serialized references.
- Run an actual Unity compile with zero console errors.
- Produce Android development build, then release AAB.

## 2. Cloudflare Match Relay deployment

Code location: `cloudflare/match-relay`.

Status:
- Wrangler config present.
- Durable Object binding/migration present.
- GitHub Actions validation passes `wrangler check`.
- Deployment workflow is present.

Still required:
- GitHub secret `CLOUDFLARE_API_TOKEN` with Worker/Durable Object deployment permission.
- GitHub secret `CLOUDFLARE_ACCOUNT_ID`.
- Run the `Cloudflare Workers` workflow manually with `deploy=true`.
- Capture the deployed Worker URL and place it into the Unity runtime environment/configuration.
- Smoke-test two clients in the same Match ID.

## 3. Cloudflare Squad Voice deployment

Code location: `cloudflare/voice-worker`.

Status:
- Voice-token Worker source present.
- JWT/Squad membership checks designed server-side.
- GitHub Actions validation passes `wrangler check`.

Still required:
- Create/identify the Cloudflare KV namespace and replace/configure `VOICE_ROOMS` namespace ID.
- Create/identify the RealtimeKit application/preset.
- Configure Cloudflare Account ID / RealtimeKit App ID as deployment values/secrets.
- Configure any RealtimeKit API secret only on the Worker side; never in Unity.
- Deploy Worker and capture its URL.
- Complete the native Unity/WebRTC bridge or chosen RealtimeKit-compatible client path.
- Test Squad mute, push-to-talk, join/leave and token expiry.

## 4. Final gameplay validation

Required end-to-end checks:
- Lobby -> Solo/Squad queue -> Match ID -> gameplay scene.
- Countdown -> plane -> jump -> parachute -> safe landing.
- Loot claim contention between two players.
- Fire/damage/death/kill feed/placement.
- Vehicle enter/exit and remote interpolation.
- Safe-zone phases and outside-zone damage.
- Bot fill to 32 slots without excessive frame-time spikes.
- Match result -> Supabase XP/rank save -> return to Lobby.
- Reconnect/session refresh handling.

## 5. Store-readiness checks

Before Google Play submission:
- Only original or commercially licensed models, textures, audio, fonts and UI assets.
- No PUBG trademarks, copied layouts, maps, skins, icons or promotional material.
- App icon, screenshots and store listing must represent the actual game.
- Privacy policy and data-safety declarations must match Supabase/Auth/voice/network behavior.
- Target SDK / 64-bit / signing / Play integrity requirements must be checked against the current Play Console requirements at release time.
