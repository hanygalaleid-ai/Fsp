# Map 01 — Sunscar Island

Original fictional 32-player battle royale map designed for mobile readability and short rotations.

## Scale target
- Playable footprint: roughly 2.4 km x 2.4 km for MVP.
- 32-player matches with bots filling empty slots.
- Travel between adjacent major POIs should usually stay under ~75 seconds on foot.
- Road loop supports lightweight vehicles without making them mandatory.

## Major regions

### 1. Old Crown
Hilltop limestone fort and compact old-town alleys.
- Vertical close/medium-range combat.
- Rooftop sightlines but limited long sniping lanes.
- Landmark: broken bronze signal tower.

### 2. Copper Port
Harbor warehouses, containers, fish-market roofs and a dry dock.
- SMG/shotgun-heavy interior fights.
- Vehicle spawn on inland road, not inside dense container maze.

### 3. Dryfield
Low stone walls, olive-like fictional groves and irrigation ruins.
- Open rotations with intermittent hard cover.
- Good mid-range rifle fights.

### 4. White Quarry
Stepped stone quarry, cranes, ramps and equipment sheds.
- Strong elevation changes.
- Marksman lanes broken by machinery and rock shelves.

### 5. Redline Airstrip
Short abandoned airstrip with hangars and weather station.
- High-risk central loot location.
- Long sightline across runway but flank routes behind hangars.

### 6. Saltworks
Shallow salt flats, processing sheds and raised pipe lanes.
- Distinct bright terrain silhouette.
- Sparse cover, fast vehicle rotation route.

### 7. Lantern Coast
Small coastal settlement, cliff road and lighthouse-like original beacon structure.
- Balanced final-circle location with indoor/outdoor transitions.

## Minor compounds
Scatter 12–16 small compounds between major POIs so players are never forced into empty 300m runs. Use 2–4 building clusters, roadside workshops, ruined farms and lookout shelters.

## Art direction
- Warm limestone, dusty soil, faded painted doors, canvas shade, oxidized metal and deep navy signage accents.
- No real-world brands or copied architecture from another game's map.
- Each POI has a unique silhouette visible from distance.

## Mobile performance
- Modular buildings with shared atlases.
- Interiors only where gameplay-relevant; avoid fully furnished decorative rooms.
- HLOD/LOD for distant compounds.
- Occlusion areas in dense Old Crown / Copper Port.
- Vegetation sparse and clustered instead of blanket foliage.

## Zone design
Safe-zone centers should be weighted across the entire island, with safeguards against repeatedly ending in inaccessible water/cliff areas. Final circle candidates should include both urban and open terrain presets.
