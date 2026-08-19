# Unity CI setup for Fsp

The repository is pinned to Unity `6000.3.17f1` and the Android workflow expects these GitHub Actions secrets for every Unity build:

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

## Google Play release signing

The Android application identifier is fixed by the release pipeline as:

`com.hanygalaleid.fsp`

The first Play release is configured as version `1.0.0` with `versionCode = 1`.

A Google Play AAB is intentionally blocked unless a custom upload keystore is provided. Add these Actions secrets before running an AAB build:

- `FSP_ANDROID_KEYSTORE_BASE64` — base64 text of the upload `.jks` / `.keystore` file.
- `FSP_ANDROID_KEYSTORE_PASSWORD` — keystore password.
- `FSP_ANDROID_KEYALIAS_NAME` — upload-key alias.
- `FSP_ANDROID_KEYALIAS_PASSWORD` — alias/key password.

Example base64 creation on Linux/macOS:

```bash
base64 -w 0 fsp-upload.keystore
```

On macOS, where `-w` is unavailable:

```bash
base64 < fsp-upload.keystore | tr -d '\n'
```

Keep the original keystore and passwords backed up securely. Losing the upload key can complicate future Play updates.

## Running the build

Open `Actions > Unity Android Build > Run workflow`.

- Choose `apk` for the installable test build.
- Choose `aab` only after all four FSP Android signing secrets are configured and the final lobby artwork passes the release gate.

The workflow uses Unity `6000.3.17f1` and the repository build methods under `Fsp.EditorTools.FspBuildCommands`.

Release AAB rules are intentionally strict:

- unique package identifier;
- ARM64 enabled;
- IL2CPP enabled;
- non-development build;
- custom upload keystore required;
- final lobby artwork at least `1280x720`;
- build must finish with `0` Unity warnings.

If any required secret is missing, the workflow intentionally fails with the exact missing value instead of producing a misleading release artifact.

On success, download the `fsp-android-<run number>` artifact from the workflow run.
