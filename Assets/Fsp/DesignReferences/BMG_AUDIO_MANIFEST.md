# BMG original audio manifest

All files in `Assets/Fsp/Art/Resources/Audio` were synthesized specifically for this project from generated waveforms and filtered noise. They contain no sampled third-party recordings or copyrighted music.

## Runtime coverage

- Lobby theme and match ambience
- Menu click, confirm and back actions
- Rifle fire, reload and empty magazine
- Damage, footsteps, jump and landing
- Item pickup, weapon switch and healing
- Plane engine, parachute wind/open and vehicle engine
- Safe-zone warning, victory and defeat
- Push-to-talk and all mobile action buttons

`FspAudioAssetBuildGuard` blocks a release build if any required WAV file is missing, truncated or cannot be imported by Unity.
