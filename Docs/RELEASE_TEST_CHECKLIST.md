# FSP Release Test Checklist

No release is approved until every applicable item is checked on a physical Android phone.

## Import and compile

- [ ] Open with Unity 6000.3.17f1 and complete a clean asset import.
- [ ] Confirm game scripts have zero compile errors and zero script warnings; review Unity/package toolchain warnings separately.
- [ ] Run the FSP project validator and all art guards.
- [ ] Confirm Lobby and Match are the only enabled release scenes.

## Lobby and responsive UI

- [ ] Test 16:9, 18:9, 19.5:9, 20:9 and a notched device in landscape.
- [ ] Confirm artwork is not stretched and START remains fully visible and clickable.
- [ ] Open Settings, Account & Privacy, Team, Loadout and Missions without overlap.
- [ ] Create an account from the responsive Account window, verify email, sign in, restore the session after restart, then sign out.
- [ ] Enable Google in Supabase Auth, allow `com.hanygalaleid.fsp://auth-callback`, then test CONTINUE WITH GOOGLE on both phones.
- [ ] Change the player name and character, restart, and confirm profile plus player-directory values are synchronized.
- [ ] Verify Music, SFX and Graphics settings persist after restart.
- [ ] Create a squad, invite a real account, ready, leave and start squad matchmaking.
- [ ] On the invited phone, use CHECK INVITES, accept the invite and confirm the member list refreshes.
- [ ] Cycle Assault, Scout and Heavy loadouts; confirm the selected weapon stats change in Match.
- [ ] Cycle all six wardrobe slots, equip each starter variant, restart, and confirm the exact selection is restored locally and from the signed-in account.
- [ ] Finish a match and confirm mission counters update exactly once.

## Match HUD and controls

- [ ] Verify joystick, look area, fire, aim, jump, reload, heal, use, swap and sprint.
- [ ] Confirm HP, armor, ammo, alive count, phase and zone warning update correctly.
- [ ] Confirm the minimap follows player heading, shows terrain in the plane and on the ground, and never overlaps the title or safe-zone warning.
- [ ] Confirm controls remain inside Safe Area on every tested phone.
- [ ] Confirm result screen placement, kills and return-to-lobby flow.
- [ ] Let the plane reach route end without manual input; confirm forced jump still occurs above collidable island terrain.
- [ ] Run every safe-zone phase and confirm its center/radius never leave the playable island.

## Drop, characters and vehicles

- [ ] Confirm the local player boards the transport plane, can jump, free-fall, open the parachute and land without falling through the world.
- [ ] Confirm automatic parachute opening occurs near the configured safety height rather than immediately after jumping.
- [ ] Confirm local and bot character visuals appear with no capsule/cube placeholder visible.
- [ ] Confirm SOLDIER 01/02/03 changes the actual in-match character identity and that head, visor, uniform, trousers, backpack and parachute selections affect the real model.
- [ ] Enter, drive, brake and exit every scout vehicle; confirm the player exits beside the vehicle and does not remain parented to it.
- [ ] Confirm procedural character movement stays still aboard the plane and animates only during ground locomotion.

## Graphics and performance

- [ ] Confirm no magenta/pink renderer in Lobby, Match, players, bots, vehicles or loot.
- [ ] Confirm sand, roads, rocks and walls use the checked-in 512px textures.
- [ ] Verify sun, ambient light and distance fog on Low, Medium and High.
- [ ] Test frame pacing and temperature for at least 15 minutes on a low-memory phone.
- [ ] Confirm Low targets 30 FPS, Medium 45 FPS and High 60 FPS.

## Audio and voice

- [ ] Hear lobby/match music, desert ambience, UI click, rifle and reload effects.
- [ ] Confirm Music OFF does not mute SFX; SFX OFF does not mute music.
- [ ] Confirm audio preferences persist after restart.
- [ ] Set `voice_token_endpoint` in Supabase to the deployed HTTPS voice service.
- [ ] Test two signed-in squad accounts: microphone permission, join, hold-to-talk, mute, remote audio, leave and reconnect.
- [ ] Confirm voice never captures before permission and remains muted until hold-to-talk.
- [ ] Confirm the voice panel sits below the top-right status card and never covers FIRE/AIM/RELOAD.

## Languages and fonts

- [ ] Test English, Arabic, Hindi, Turkish, Portuguese (Brazil) and Indonesian.
- [ ] Verify Arabic joining/RTL and Hindi glyph shaping on two Android manufacturers.
- [ ] Confirm every HUD, result, settings, team, loadout, mission and voice label switches language.
- [ ] Confirm language icons match the selected language.

## Accounts, privacy and backend

- [ ] Test sign-up, email verification, sign-in, session restore and offline guest play.
- [ ] Test Google OAuth, server-backed sign-out, session restore and account deletion with a disposable Google account.
- [ ] Deploy and test the privacy and delete-account Supabase functions.
- [ ] Delete a disposable account and verify Auth plus associated rows are removed.
- [ ] Publish an external account-deletion request page and add its URL to Play Console.
- [ ] Confirm Data Safety declarations match Auth, profile, matchmaking, squad and microphone behavior.

## Google Play package

- [ ] Set a versionCode higher than every previous Play release.
- [ ] Confirm application ID, ARM64, IL2CPP, target SDK and upload signing.
- [ ] Confirm Android manifest contains INTERNET and RECORD_AUDIO permissions and microphone permission is requested only when squad voice starts.
- [ ] Inspect the generated AAB in Play Console and confirm every native library supports 16 KB memory pages.
- [ ] Produce the AAB only after this checklist passes and the owner explicitly requests Build.
- [ ] Install through Play internal testing and repeat the startup/match/return smoke test.
