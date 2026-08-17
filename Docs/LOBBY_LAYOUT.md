# Fsp Lobby Layout

## Composition
Landscape mobile-first lobby with an original layout.

- Center-left: full-height 3D player character on a shallow stone platform.
- Background: fictional coastal/desert staging camp with distant fort silhouette, cloth canopies, antenna mast and dust haze. No copied map/location.
- Top-left: player name, level/rank badge and compact profile access.
- Top-center: currency/status strip, kept narrow so the character remains dominant.
- Left edge: small vertical navigation for Loadout, Appearance and Career.
- Right: Squad panel with up to four portrait slots, invite affordance and Ready state.
- Bottom-right: large bronze-accent START button.
- Directly above START: mode selector (Solo / Squad) and map/match card.
- Bottom-left: settings/audio and region/ping indicators.

## Visual rules
- Deep navy translucent panels; avoid large opaque rectangles.
- Bronze only for primary action, selected state and premium highlight.
- Warm-white typography, sand secondary labels.
- Touch targets >= 56 px reference size.
- Maintain Safe Area on all edges.
- Use subtle depth: 3D scene behind UI, soft vignette, restrained dust particles.
- Do not imitate PUBG menu geometry, icon shapes, fonts, wording, lobby pose, or screen composition.

## Character presentation
- Neutral confident idle, weapon lowered or stowed; not an existing game's signature pose.
- Slow 7-degree idle turn and subtle camera float.
- Warm key light + bronze rim light.
- Cosmetics preview without reloading the scene.

## Performance
- Lobby character can use highest available LOD because only one hero is visible.
- Background buildings use LOD1/2 and baked light.
- Dust uses a small pooled particle budget.
- Disable expensive post effects on Low tier.
