#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace Fsp.EditorTools
{
    public static class FspAndroidIconSetup
    {
        // Opaque BMG launcher artwork prevents Android launchers from filling transparent
        // corners with white and keeps the emblem safe under adaptive icon masks.
        public const string IconPath = "Assets/Fsp/Art/Resources/UI/bmg_app_icon.png";
        public const string AdaptiveForegroundPath = "Assets/Fsp/Art/Resources/UI/bmg_adaptive_foreground.png";
        public const string AdaptiveBackgroundPath = "Assets/Fsp/Art/Resources/UI/bmg_adaptive_background.png";

        public static void Apply()
        {
            if (!File.Exists(IconPath))
            {
                throw new BuildFailedException("Required Android launcher icon was not found: " + IconPath);
            }

            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (icon == null || icon.width <= 0 || icon.height <= 0)
                throw new BuildFailedException("Android launcher icon exists but could not be imported: " + IconPath);

            AssetDatabase.ImportAsset(AdaptiveForegroundPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(AdaptiveBackgroundPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            Texture2D adaptiveForeground = AssetDatabase.LoadAssetAtPath<Texture2D>(AdaptiveForegroundPath);
            Texture2D adaptiveBackground = AssetDatabase.LoadAssetAtPath<Texture2D>(AdaptiveBackgroundPath);
            if (adaptiveForeground == null || adaptiveBackground == null)
                throw new BuildFailedException("BMG Android adaptive icon foreground/background layers are missing or invalid.");

            int[] sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Android, IconKind.Application);
            Texture2D[] icons = new Texture2D[sizes.Length];
            for (int i = 0; i < icons.Length; i++) icons[i] = icon;
            PlayerSettings.SetIcons(NamedBuildTarget.Android, icons, IconKind.Application);

            PlatformIcon[] adaptiveIcons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, AndroidPlatformIconKind.Adaptive);
            for (int i = 0; i < adaptiveIcons.Length; i++)
                adaptiveIcons[i].SetTextures(adaptiveForeground, adaptiveBackground);
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, AndroidPlatformIconKind.Adaptive, adaptiveIcons);

            AssetDatabase.SaveAssets();
            Debug.Log($"[BMG] Android legacy and adaptive launcher icons applied ({icon.width}x{icon.height}).");
        }
    }
}
#endif
