#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Fsp.EditorTools
{
    public static class FspAndroidIconSetup
    {
        public const string IconPath = "Assets/Fsp/Art/AppIcon/app_icon.png";

        public static void Apply()
        {
            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (icon == null)
                throw new BuildFailedException("Android launcher icon could not be imported: " + IconPath);

            // Legacy/default Android icon. Unity uses this as launcher fallback and for platforms
            // where adaptive icon layers are not configured.
            var sizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Android);
            var icons = new Texture2D[sizes.Length];
            for (int i = 0; i < icons.Length; i++) icons[i] = icon;
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);

            AssetDatabase.SaveAssets();
            Debug.Log($"[FSP] Android launcher icon applied: {IconPath} ({icon.width}x{icon.height})");
        }
    }
}
#endif
