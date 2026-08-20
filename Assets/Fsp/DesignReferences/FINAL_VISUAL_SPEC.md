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

1. Checked-in art, shaders, and release scenes are authoritative.
2. The minimal Match scene is completed by deterministic runtime assemblies that use only checked-in textures and mobile-safe shaders; they must always produce the full island, POIs, drop plane, player visual, HUD and collision surfaces.
3. Build-time Editor code validates required art; it must not download or generate replacement art or scenes.
4. If required release assets are missing, the build must fail rather than silently omit a visual system.
5. `Lobby.unity` and `Match.unity` are the only release scenes.

## Current repository gap found during audit

`Match.unity` intentionally stays small (camera and light). `StarterWorldGameplayInstaller`, the Sunscar POI builders and the presentation components are release-critical and assemble the complete mobile-safe match from checked-in textures, shaders and deterministic mesh definitions. The release validator verifies those inputs before APK/AAB creation.
