# BMG Production Visual Audit — 2026-08-22

## Audit result
The old mixed FSP / procedural / low-poly presentation is no longer accepted as production art on branch `bmg/build149-authored-3d`.

## Cleaned and approved
- Legacy lobby overlay objects (`BMG_RealisticLogo`, `BMG_RealisticCharacterPreview`) are purged and no longer created by the realistic-art runtime.
- Lobby background is controlled by the BMG clean lobby runtime; the deleted old FSP image can no longer render.
- BMG mobile action icons and joystick are static checked-in assets with no old action-icon fallback.
- Metallic BMG UI surfaces use checked-in static PNG assets; runtime texture-pixel generation is removed.
- Old FSP StoreListing icon and feature graphic were deleted.
- Old world texture families `sand_ground`, `rock_cliff`, `road_dust`, `fortress_wall` and their v2 variants were deleted. BMG v3 textures remain.
- Legacy BMG presentation runtimes that loaded `*_mk1` models are disabled as compatibility shells.
- The old mk1 low-poly model directory was physically removed.
- `BmgProductionVisualController` is the sole production visual authority for live Match 3D.

## Installed Production 3D v1 pack
The following 12 static OBJ assets now exist under:
`Assets/Fsp/Art/Resources/Models/BMG/Production/`

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

These files are generated at source time by `tools/generate_bmg_production_models.py`, committed as normal OBJ assets, and are not generated inside the game at runtime.

## Release protection
`BmgProductionVisualReleaseGuard` now checks both canonical filenames and minimum source-file sizes. A tiny placeholder using a production filename cannot pass the gate, and `_mk1` assets are never accepted.

## Runtime presentation
- Characters use the static Production character mesh path.
- Rifle and SMG use the static Production weapon paths.
- Plane, buggy and environment use the static Production paths.
- Parachute is connected to `ParachuteController.ConfigureVisual` and appears only with parachute state.
- Fixed tactical materials are applied by `BmgProductionVisualController`: tactical olive character, gunmetal weapons, military slate plane, desert vehicle, orange parachute and desert environment.
- Compatibility colliders/gameplay systems may remain, but their renderers are hidden.

## Quality status — important
Production v1 is a **clean static mid-poly replacement layer**, substantially denser than the removed mk1 placeholder meshes. It is not being labeled as final photorealistic AAA art.

Still required for the final realism target:
- rigged/skinned male and female character meshes;
- walk/run/jump/crouch/aim/reload/fire animations;
- proper UVs and PBR materials (albedo/normal/roughness/metallic);
- separate vehicle materials, wheels and suspension presentation;
- higher-detail authored terrain/POI kit with LODs;
- final weapon materials, attachments and first/third-person alignment;
- final lobby 3D character preview using the same production character set.

## Build rule
A technical test build may be used to validate import, scale, camera alignment and gameplay wiring of Production v1. Do not call that build the final photorealistic release until the rigging/PBR/animation list above is completed.
