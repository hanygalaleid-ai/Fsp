# BMG / FSP release visual audit — 2026-08-20

This audit follows the player path from launch through match completion. It records what is
checked in and what still requires a physical Android run after Unity Cloud Build.

## 1. Android launch

- BMG legacy icon: checked in, opaque navy background.
- BMG adaptive foreground/background: checked in and assigned during Android build.
- Landscape full-screen and safe-area handling: configured.
- Device verification: pending next approved build.

## 2. Lobby

- 16:9 artwork is fitted inside the safe viewport without stretching.
- Profile, language, wallet, settings, mode, team, loadout, missions, account and Start controls
  are built above the backdrop and have an EventSystem/GraphicRaycaster.
- Character and wardrobe navigation uses clear left/right arrow controls.
- Google sign-in uses the official checked-in Google G asset.
- Physical tap verification: pending next approved build.

## 3. Match loading and drop plane

- Match scene intentionally contains Camera and Sun; the runtime assembler installs the gameplay
  player, drop route, match manager, HUD, world, audio, networking and result flow.
- Drop camera now tracks the plane from an external chase view instead of sitting inside the
  fuselage.
- Plane visual has visible body panels, cockpit glass, windows, four engines and orange markings.
- Combat-only controls, crosshair and safe-zone warning stay hidden while aboard.
- Sunscar horizon panorama and the sea/island geometry remain visible through the full route.

## 4. Jump and parachute

- Player visual is hidden while aboard and restored immediately after jumping.
- Camera snaps to a wider parachute view on the state transition.
- Movement and jump/parachute controls remain visible; firearm controls stay hidden until landing.
- Parachute canopy visual is bound to the real ParachuteController state.

## 5. Ground gameplay

- Island base, sea, ridges, vegetation, rocks and roads use checked-in mobile-safe textures.
- Old Crown, White Quarry, Redline Airstrip and Saltworks generators are active.
- Previously empty Copper Port, Dryfield and Lantern Coast components now construct their complete
  collision-backed areas.
- Player visual has brighter mobile-safe materials plus helmet, mask, vest, pouches, backpack,
  knee protection, boots and rifle details.
- Mobile shader minimum lighting was raised so characters, aircraft and props do not become black
  silhouettes on Android.
- Joystick uses a checked-in transparent directional pad; action buttons use the eight-icon atlas.
- Minimap renders the world from the plane through ground combat.

## 6. Localization

- English, Arabic, Hindi, Turkish, Brazilian Portuguese and Indonesian remain selectable.
- Dynamic Arabic HUD values are shaped in LateUpdate after health/ammo/phase updates, preventing
  the intermittent reversed/disconnected match text shown in the device screenshots.
- Physical font verification across all six languages: pending next approved build.

## 7. Match completion

- Match completion canvas is independent of the gameplay HUD.
- Placement, kills and XP remain visible after gameplay input is disabled.
- Return-to-lobby button has its own raycaster and non-blocking label.
- Full offline win/loss run and online two-device run: pending next approved build.

## Static checks completed

- Every checked-in PNG/JPEG under `Assets/Fsp/Art/Resources` decodes successfully.
- New sky and joystick assets are included in both project and pre-build validators.
- New sky/POI runtime code is included in the required-file validator.
- Git whitespace check and C# brace-balance audit pass.

## Mandatory next verification

Do not treat static checks as a substitute for Unity/Android execution. The next approved cloud
build must be installed and exercised through: Lobby -> Account -> Start -> Plane -> Jump ->
Parachute -> Land -> Move/Aim/Fire/Reload/Heal -> Elimination/Victory -> Return to Lobby.
