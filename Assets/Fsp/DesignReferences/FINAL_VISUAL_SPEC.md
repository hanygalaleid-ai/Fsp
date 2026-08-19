# FSP Final Visual Specification

This file is the release visual source-of-truth for the FSP project.

## Approved visual target

The approved design board created on 2026-08-18 defines the intended look:

- Lobby: FSP OPERATIVE, masked tactical operative centered in a cinematic desert-fortress environment, player profile and daily missions on the left, team/map/mode panel on the right, gold primary Start button, dark charcoal UI with gold accents.
- Match HUD: third-person tactical shooter view, alive/kill counters, compass, minimap, left virtual joystick, right-side aim/action controls, bottom weapon/ammo HUD.
- World: Sunscar Island / desert-fortress environment with roads, ruins, palms, rocks, military props and warm cinematic lighting.
- Drop phase: transport plane, jump control, parachute descent and speed indicator.
- Core palette: charcoal/black panels, warm gold primary actions, white text, muted gray secondary UI, green ready-state.

## Release rules

1. Checked-in authored Scenes and art are authoritative.
2. Runtime code must never rebuild or replace the approved visual presentation with Unity primitives, fallback geometry, placeholder HUD, generated roads, generated POIs, generated characters, generated weapons, generated vehicles or generated scene backgrounds.
3. Build-time Editor code must validate required art; it must not generate replacement art or scenes.
4. If required release assets are missing, the build must fail rather than silently create a prototype replacement.
5. `Lobby.unity` and `Match.unity` are the only release scenes.

## Current repository gap found during audit

The approved design board shows production character, weapon, prop, HUD and world assets, but the current GitHub repository does not contain those authored 3D production assets. The checked-in `Assets/Fsp/Art` tree currently contains the fixed lobby image, two small UI textures and four world textures. `Match.unity` is therefore not yet a complete authored production scene.

Until real authored Match assets are added, the release validator intentionally blocks an APK/AAB that would otherwise open into an empty or prototype Match scene.
