# Fsp MVP Scope

## Core Gameplay
- Third-person character controller
- Aim / fire / reload
- Weapon switching
- Health and armor
- Loot pickups
- Backpack / inventory
- Safe Zone shrink phases
- Airdrop
- Death / winner flow

## Modes
- Solo
- Squad
- 32 total match slots initially
- Bots fill missing players

## Map
- One launch map
- Mixed open terrain + compounds + roads
- Mobile-first optimization
- Streaming/LOD strategy from the start

## Vehicles
- One light vehicle class initially
- Driver/passenger seats
- Damage and destruction rules

## Online / Backend
- Supabase: profile, inventory, rank, progression, match results
- Cloudflare: API edge, protection, squad voice
- Game server/networking kept separate from database traffic

## Voice
- Squad-only voice
- Push-to-talk
- Mute self / mute teammate

## Platforms
- Android
- iOS
- Windows PC

## Performance Targets
- Scalable quality presets: Low / Medium / High
- Favor stable frame pacing over maximum visual effects
- Mobile build size target: keep initial downloadable package as small as practical, with optional content downloadable later

## Deferred Features
Not required for first public version:
- Full replay system
- World voice chat
- Global text chat
- Multiple launch maps
- 100-player matches
- Advanced spectator tools

## Visual Direction
Realistic lightweight military/survival style with original identity. Inspired by the battle-royale genre, not copied from PUBG assets, UI, names, maps, or branding.
