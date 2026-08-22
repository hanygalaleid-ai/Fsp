# BMG Production Visual Audit — 2026-08-22

## Audit result
The old mixed FSP / procedural / low-poly presentation is no longer accepted as production art on branch `bmg/build149-authored-3d`.

## Cleaned and approved
- Legacy lobby overlay objects (`BMG_RealisticLogo`, `BMG_RealisticCharacterPreview`) are purged and no longer created by the realistic-art runtime.
- Lobby background is controlled by the BMG clean lobby runtime.
- BMG mobile action icons and joystick are static checked-in assets with no old action-icon fallback.
- Metallic BMG UI surfaces now use checked-in static PNG assets; runtime texture pixel generation has been removed.
- Old FSP StoreListing icon and feature graphic were deleted.
- Old world texture families `sand_ground`, `rock_cliff`, `road_dust`, `fortress_wall` and their v2 variants were deleted. BMG v3 textures remain.
- Legacy BMG presentation runtimes that loaded `*_mk1` models are disabled as compatibility shells: authored character/weapon/vehicle runtime, residual replacements, environment replacement, POI landmarks, utility props, weapon refresh and vehicle lighting.
- The old `Assets/Fsp/Art/Resources/Models/BMG` mk1 low-poly model directory was physically removed.
- `BmgProductionVisualController` is the sole production visual authority for live Match 3D.
- `BmgProductionVisualReleaseGuard` blocks release builds until genuine production models are present.

## Production 3D models required before any release build
Place genuine production-quality imported model assets under:
`Assets/Fsp/Art/Resources/Models/BMG/Production/`

Canonical required names:
1. `bmg_sunscar_environment`
2. `bmg_transport_plane`
3. `bmg_parachute`
4. `bmg_buggy`
5. `bmg_assault_rifle`
6. `bmg_smg`
7. `bmg_character_01`
8. `bmg_character_02`
9. `bmg_character_03`
10. `bmg_character_04`
11. `bmg_character_05`
12. `bmg_character_06`

## Important release rule
Do **not** build or publish an APK as the final visual version until all 12 production models above exist. The strict build guard is intentionally expected to fail while they are missing. This prevents another APK from accidentally shipping primitive Capsule/Box/Cylinder or old `mk1` visuals.

## Gameplay compatibility
Legacy gameplay scripts and colliders may remain where required for movement, collision, loot, doors, networking, bots and match flow. Their renderers are not accepted as production presentation; the production visual controller owns what is allowed to render.
