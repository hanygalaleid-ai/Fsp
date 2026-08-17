# Fsp Match HUD — Original Mobile Layout

## Goal
Readable, premium mobile HUD with minimal obstruction and an original composition distinct from competing battle-royale games.

## Layout
- Top-left: compact squad stack (portrait, name, health, voice state). Solo mode collapses this to the local profile strip.
- Top-center: slim compass with heading ticks; safe-zone countdown appears directly beneath it only when relevant.
- Top-right: circular minimap in an original navy/bronze frame; alive count and kills sit as two small chips below it.
- Bottom-left: movement joystick with sprint ring; crouch/prone/jump form a shallow arc above/right of movement instead of a copied vertical stack.
- Bottom-center: compact health + armor ribbon, medkit quick slot and interaction prompt.
- Bottom-right: primary fire as the largest action; aim, reload and weapon swap orbit it with generous spacing.
- Weapon/ammo card sits inward from the right edge rather than directly copying common shooter layouts.
- Backpack and interact are contextual and fade when unavailable.

## Visual identity
- Deep navy translucent glass panels.
- Bronze for selected/primary action only.
- Warm-white text and icons; sand for secondary information.
- Danger state uses muted red, not neon.
- Icons must be original/licensed and use one consistent rounded tactical line language.
- No competitor fonts, icon silhouettes, button arrangements, crosshair assets, map frames or copied HUD geometry.

## Touch and accessibility
- Reference touch target >= 56 px, with extra invisible hit padding where necessary.
- Safe-area aware on all four sides.
- Fire/aim can be mirrored in settings for left-handed players later.
- HUD scale target 80–120% later through settings.
- Important state is conveyed by shape/text as well as color.

## Performance
- Avoid animated blur; use static translucent sprites/materials.
- No continuously animating UI unless gameplay-relevant.
- Pool kill-feed rows and damage indicators.
- Minimap refresh can be throttled on Low tier.
