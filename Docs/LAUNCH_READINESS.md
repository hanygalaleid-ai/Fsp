# FSP Launch Readiness

Local review date: 20 August 2026.

## Local implementation completed

- Responsive 16:9 lobby inside Android Safe Area; the START control is no longer baked into oversized artwork.
- Responsive match HUD, mobile controls, minimap, result screen and voice panel placement.
- Low/Medium/High graphics presets, mobile lighting/fog and runtime recovery for unsupported magenta materials.
- Procedural original music, ambience and gameplay/UI effects with independent persistent music/SFX settings.
- Solo/offline flow, plane route, forced jump, free fall, automatic/manual parachute, bots, loot, combat, vehicles, safe zone and mission progress.
- Account creation/sign-in/sign-out/profile synchronization and permanent in-app account deletion client flow.
- Google OAuth button, Android deep-link callback, Supabase session verification and server-backed sign-out client flow.
- A six-slot original starter wardrobe (head, face, torso, legs, backpack and parachute), local persistence, account persistence and real procedural-character application.
- Team creation/invites/accept/ready/leave/squad matchmaking client flow.
- Loadout selection and match-applied starter weapon statistics.
- English, Arabic, Hindi, Turkish, Brazilian Portuguese and Indonesian text, language art, RTL Arabic shaping and Android OS font fallback.
- Android release pipeline: `com.hanygalaleid.fsp`, ARM64, IL2CPP, API 36, INTERNET enforcement, launcher icon and mandatory upload signing for AAB.
- Google Play icon and 1024×500 feature graphic.

## Hard blockers before a release AAB

- Supabase `privacy` is deployed ACTIVE without JWT. Verify its public URL from an unrestricted phone/browser.
- Set `FSP_SUPPORT_EMAIL`, then verify the privacy and external account-deletion pages on a phone.
- Supabase `delete-account` is deployed ACTIVE with JWT. Test it with a disposable account and confirm all associated rows plus Auth are removed.
- Deploy the Cloudflare match relay and set `app_runtime_config.match_relay_ws_url` to its production `wss://` endpoint.
- Deploy the Cloudflare voice worker and set `app_runtime_config.voice_token_endpoint` to its production HTTPS endpoint.
- Configure Google provider credentials in Supabase Auth and add `com.hanygalaleid.fsp://auth-callback` to the redirect allow list.
- Open the latest local source with Unity 6000.3.17f1 and confirm zero game-script compile errors after the newest changes.
- Pass the complete physical-device checklist in `Docs/RELEASE_TEST_CHECKLIST.md`, including two-phone online/voice tests and Arabic/Hindi tests on two manufacturers.
- Generate a signed candidate AAB only after the above gates pass, upload it to Play internal testing, and verify 16 KB native-library compatibility in Play Console.

## Build decision

Do not create the Google Play release AAB yet. After every hard blocker above is closed, first make one test APK for the final phone smoke test. If that passes, increment `FSP_ANDROID_VERSION_CODE` above the latest Play version and run `Fsp/Build/Android/Build AAB (Google Play)` with the upload-keystore environment variables configured.
