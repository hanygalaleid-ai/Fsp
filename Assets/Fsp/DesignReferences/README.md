# Approved FSP visual references

This folder contains the approved visual references that define the target presentation for the release build.

- `fsp_lobby_reference_full.png` — approved Lobby composition and visual hierarchy.
- `fsp_match_hud_reference.png` — approved in-match HUD composition.
- `fsp_sunscar_map_reference.png` — approved Sunscar map reference.

These files are design references, not runtime fallback art. Runtime or build-time code must not substitute them as screenshots in place of real gameplay, characters, world geometry, or functional UI.

Implementation rule: match the references with authored Unity UI, prefabs, materials and scenes. If production assets are missing, fail validation instead of generating primitives/placeholders.
