#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    /// <summary>
    /// Visual release gate. Test APKs may still be produced for device validation, but a Play AAB
    /// must never ship with missing, invalid, or low-resolution lobby artwork.
    ///
    /// Important: Cloud Build can run this pre-build hook before AssetDatabase has imported a newly
    /// checked-out image. Therefore APK validation must not fail merely because LoadAssetAtPath returns
    /// null. We validate the JPEG bytes/dimensions directly and only use AssetDatabase as an optional
    /// secondary check.
    /// </summary>
    public sealed class FspVisualAssetBuildGuard : IPreprocessBuildWithReport
    {
        private const string LobbyArt = "Assets/Fsp/Art/Resources/Lobby/lobby_reference.jpg";
        private const int ReleaseMinWidth = 1280;
        private const int ReleaseMinHeight = 720;
        public int callbackOrder => -900;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!File.Exists(LobbyArt))
                throw new BuildFailedException("Required fixed FSP lobby art is missing: " + LobbyArt);

            FileInfo info = new FileInfo(LobbyArt);
            if (info.Length < 4 * 1024)
                throw new BuildFailedException("FSP lobby art looks invalid/empty: " + LobbyArt);

            if (!TryReadJpegSize(LobbyArt, out int width, out int height))
                throw new BuildFailedException("FSP lobby art is not a readable JPEG: " + LobbyArt);

            // AssetDatabase import is useful when available, but it is not a valid reason to fail a
            // test APK during Cloud Build startup. The file itself has already been validated above.
            Texture2D imported = AssetDatabase.LoadAssetAtPath<Texture2D>(LobbyArt);
            if (imported != null && imported.width > 0 && imported.height > 0)
            {
                width = imported.width;
                height = imported.height;
            }
            else
            {
                Debug.LogWarning("FSP lobby art has not been imported into AssetDatabase yet; using validated JPEG dimensions for this pre-build check.");
            }

            bool lowResolution = width < ReleaseMinWidth || height < ReleaseMinHeight;
            if (lowResolution)
            {
                string message = $"FSP lobby art is only {width}x{height}. Play release requires at least {ReleaseMinWidth}x{ReleaseMinHeight}.";
                if (EditorUserBuildSettings.buildAppBundle)
                    throw new BuildFailedException(message + " Replace the lobby art before building the release AAB.");

                Debug.LogWarning(message + " APK is allowed only for device testing; do not publish it.");
            }

            Debug.Log($"FSP visual asset guard OK for current build type: {LobbyArt} ({width}x{height}, {info.Length / 1024f:0.0} KB)");
        }

        private static bool TryReadJpegSize(string path, out int width, out int height)
        {
            width = 0;
            height = 0;

            try
            {
                using (FileStream stream = File.OpenRead(path))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    if (reader.ReadByte() != 0xFF || reader.ReadByte() != 0xD8)
                        return false;

                    while (stream.Position + 4 < stream.Length)
                    {
                        byte prefix = reader.ReadByte();
                        if (prefix != 0xFF)
                            continue;

                        byte marker;
                        do { marker = reader.ReadByte(); } while (marker == 0xFF && stream.Position < stream.Length);

                        // Standalone markers have no segment length.
                        if (marker == 0xD8 || marker == 0xD9 || (marker >= 0xD0 && marker <= 0xD7))
                            continue;

                        if (stream.Position + 2 > stream.Length)
                            return false;

                        int segmentLength = ReadBigEndianUInt16(reader);
                        if (segmentLength < 2 || stream.Position + segmentLength - 2 > stream.Length)
                            return false;

                        // SOF markers that carry image dimensions (excluding DHT/JPG/DAC markers).
                        bool isStartOfFrame = marker >= 0xC0 && marker <= 0xCF && marker != 0xC4 && marker != 0xC8 && marker != 0xCC;
                        if (isStartOfFrame)
                        {
                            if (segmentLength < 7)
                                return false;

                            reader.ReadByte(); // sample precision
                            height = ReadBigEndianUInt16(reader);
                            width = ReadBigEndianUInt16(reader);
                            return width > 0 && height > 0;
                        }

                        stream.Seek(segmentLength - 2, SeekOrigin.Current);
                    }
                }
            }
            catch (IOException)
            {
                return false;
            }

            return false;
        }

        private static int ReadBigEndianUInt16(BinaryReader reader)
        {
            int high = reader.ReadByte();
            int low = reader.ReadByte();
            return (high << 8) | low;
        }
    }
}
#endif
