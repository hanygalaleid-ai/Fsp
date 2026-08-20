# FSP Release Gate Status

Review date: 20 August 2026. No build was started.

## Completed and verified

- Supabase project `izjdvlkwuqgtawwbksun` is ACTIVE_HEALTHY.
- `privacy` Edge Function is ACTIVE, version 1, public (`verify_jwt=false`).
- `delete-account` Edge Function is ACTIVE, version 1, authenticated (`verify_jwt=true`).
- `profiles`, `player_cosmetics`, and `app_runtime_config` use RLS. Supabase security advisors report zero findings.
- Wardrobe RLS accepts only the 18 original starter items in their correct slots; weaker duplicate matchmaking policies were removed.
- Missing foreign-key indexes were added for room membership, matchmaking, invites, and squad leaders.
- Character selection now changes the real procedural match character identity.
- Clothing selection now changes head, visor, torso, legs, backpack, and parachute visuals.
- Clothing is persisted locally and patched into the signed-in account profile.
- Network appearance messages carry character and clothing IDs and the relay validates the allowed catalog.
- The responsive Loadout page exposes wardrobe slot/item controls and an Equip & Save action.
- The account panel includes Continue with Google, a custom Android callback, token verification, session save/restore, server-backed sign-out, and existing deletion flow.
- Android release builds explicitly use Unity Activity (one entry point) so the OAuth callback manifest targets the activity actually launched by the store build.
- New Google/wardrobe/status UI text is present in English, Arabic, Hindi, Turkish, Brazilian Portuguese, and Indonesian.
- Static source checks: no whitespace errors, no merge markers, no duplicate localization keys, no missing direct localization keys, and no C# delimiter mismatches.
- Cloudflare Match Relay and Voice Worker TypeScript syntax checks pass. Voice sessions are bound to their owning user before sync, renegotiation, or leave.
- Unity project version is pinned to `6000.3.17f1`.
- Cloud Build 137 finished SUCCESS on Unity 6000.3.17f1, but it predates the newest wardrobe/OAuth/security changes and therefore is not accepted as their compile proof.
- Device screenshots from build 137 confirmed that its baked START lobby and material fallback are obsolete. Current source disables the legacy world-space lobby art, requires the responsive 16:9 safe-area canvas, and retries canvas creation if it is missing.
- Android material recovery now classifies meshes by full hierarchy and bounds, restores the checked-in sand/road/rock/wall textures, and uses themed non-white fallbacks. The island polish creates materials with the checked-in mobile-safe shader directly.
- Pre-build validation now blocks a release when the current lobby art, mobile-safe shader, or any required world texture is missing.

## External gates still open

- Supply and configure a real public `FSP_SUPPORT_EMAIL` for the privacy/deletion page.
- Deploy both Cloudflare Workers. The current environment has no Cloudflare API token and no Realtime SFU secrets; the Cloudflare dashboard also presented a repeated human-verification challenge.
- After deployment, write the real `wss://.../ws` and HTTPS URLs to `match_relay_ws_url` and `voice_token_endpoint`. Both database values are intentionally still empty rather than placeholders.
- Configure Google OAuth in Supabase with the Google Web client ID/secret and allow `com.hanygalaleid.fsp://auth-callback`.
- Open/import in Unity 6000.3.17f1 and obtain a zero-error compile audit. Unity is not installed in the current execution environment.
- Run the two-phone account/team/voice/wardrobe/language/aspect-ratio checklist on physical Android devices.
- Only after those tests, create the signed candidate AAB, upload it to Play internal testing, and inspect its native libraries for 16 KB page-size compatibility.

## Decision

Do not build yet. Build is approved only after every external gate above is evidenced as passed.
