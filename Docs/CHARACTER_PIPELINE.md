# Fsp Character Pipeline

## Goal
One original stylized-realistic humanoid base optimized for mobile, with modular cosmetics instead of full duplicated character meshes.

## Base character
- Original proportions and facial design; no copied silhouettes or costumes.
- Shared humanoid skeleton across all playable bodies where possible.
- LOD0: 25–35k triangles total character budget.
- LOD1: 12–18k triangles.
- LOD2: 5–8k triangles.
- 2K textures for hero/lobby tier, 1K runtime fallback on lower quality tiers.
- PBR materials kept simple: skin, cloth, hard-surface, hair/face accessory.

## Default visual identity
Mediterranean / North-African tactical survivor:
- Sand/stone field jacket with asymmetrical utility straps.
- Deep navy undershirt details.
- Functional trousers with wrapped knee protection.
- Compact modular backpack.
- Neutral boots/gloves without real-world logos or military unit insignia.
- Subtle bronze hardware accents, not decorative gold.

## Cosmetic slots
- Head
- Face
- Torso
- Legs
- Backpack
- Parachute

All cosmetic items must be original or properly licensed and must declare which body regions they hide to prevent mesh clipping.

## Skeleton and sockets
Required sockets/bones:
- RightHandWeapon
- LeftHandSupport
- BackPrimary
- BackSecondary
- BackpackRoot
- HeadAccessory
- FaceAccessory
- ParachuteRoot

## Animation compatibility
Every modular part must skin to the same humanoid avatar and work with:
Idle, walk, run, sprint, jump, freefall, parachute, aim, fire, reload, hit reaction, death, enter/drive/exit vehicle.

## Mobile rules
- Merge compatible materials per cosmetic where possible.
- Prefer atlases over many small textures.
- Disable hidden base-body renderers under jackets/trousers.
- Limit alpha hair/cards and transparent materials.
- Use LODGroup on character root and backpack.
