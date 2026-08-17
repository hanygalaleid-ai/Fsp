# Unity CI setup for Fsp

The repository is pinned to Unity `6000.3.17f1` and the Android workflow expects these GitHub Actions secrets:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

## Unity Personal

Unity Personal must be activated through Unity Hub. Do not use the old browser/manual `.alf -> .ulf` activation flow for a Personal license.

1. On any Windows, macOS or Linux computer that can run Unity Hub, sign in to the Unity account that will be used for CI.
2. In Unity Hub open `Preferences > Licenses > Add` and activate a free Unity Personal license.
3. Locate the generated Unity license file:
   - Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
   - macOS: `/Library/Application Support/Unity/Unity_lic.ulf`
   - Linux: `~/.local/share/unity3d/Unity/Unity_lic.ulf`
4. Open the `Fsp` repository on GitHub, then go to `Settings > Secrets and variables > Actions`.
5. Add `UNITY_LICENSE` and paste the complete text content of `Unity_lic.ulf` as its value.
6. Add `UNITY_EMAIL` with the Unity account email.
7. Add `UNITY_PASSWORD` with the Unity account password.

Never commit the `.ulf` file, email or password to the repository.

## Running the build

Open `Actions > Unity Android Build > Run workflow`.

- Choose `apk` for the installable test build.
- Choose `aab` only after the final package identifier and signing configuration are ready.

The workflow uses Unity `6000.3.17f1` and the repository build methods under `Fsp.EditorTools.FspBuildCommands`.

If any Unity secret is missing, the workflow intentionally fails at `Require Unity CI secrets` with the exact missing secret name. This prevents a skipped build from looking like a successful build.

On success, download the `fsp-android-<run number>` artifact from the workflow run.
