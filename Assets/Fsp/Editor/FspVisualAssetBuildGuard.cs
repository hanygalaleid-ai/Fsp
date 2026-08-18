#if UNITY_EDITOR
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    /// <summary>
    /// Prevents a release/test APK from silently falling back to an empty visual prototype when
    /// required shipped art has been removed or not checked into source control.
    /// </summary>
    public sealed class FspVisualAssetBuildGuard : IPreprocessBuildWithReport
    {
        private const string LobbyArt = "Assets/Fsp/Art/Resources/Lobby/lobby_reference.jpg";
        public int callbackOrder => -900;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!File.Exists(LobbyArt))
                throw new BuildFailedException("Required fixed FSP lobby art is missing: " + LobbyArt);

            FileInfo info = new FileInfo(LobbyArt);
            // The checked-in optimized lobby reference is intentionally compressed. A 4 KB minimum
            // catches missing/empty files without rejecting the valid shipped artwork.
            if (info.Length < 4 * 1024)
                throw new BuildFailedException("FSP lobby art looks invalid/empty: " + LobbyArt);

            Debug.Log($"FSP fixed visual asset guard OK: {LobbyArt} ({info.Length / 1024f:0.0} KB)");
        }
    }
}
#endif
