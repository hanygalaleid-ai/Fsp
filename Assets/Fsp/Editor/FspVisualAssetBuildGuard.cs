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
    /// must never ship with missing or low-resolution lobby artwork.
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

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(LobbyArt);
            if (texture == null)
                throw new BuildFailedException("Unity could not import required FSP lobby art: " + LobbyArt);

            bool lowResolution = texture.width < ReleaseMinWidth || texture.height < ReleaseMinHeight;
            if (lowResolution)
            {
                string message = $"FSP lobby art is only {texture.width}x{texture.height}. Play release requires at least {ReleaseMinWidth}x{ReleaseMinHeight}.";
                if (EditorUserBuildSettings.buildAppBundle)
                    throw new BuildFailedException(message + " Replace the lobby art before building the release AAB.");

                Debug.LogWarning(message + " APK is allowed only for device testing; do not publish it.");
            }

            Debug.Log($"FSP visual asset guard OK for current build type: {LobbyArt} ({texture.width}x{texture.height}, {info.Length / 1024f:0.0} KB)");
        }
    }
}
#endif
