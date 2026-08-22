#if UNITY_EDITOR
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Fsp.EditorTools
{
    public sealed class BmgNoProceduralPresentationGuard : IPreprocessBuildWithReport
    {
        private static readonly string[] FilesToAudit =
        {
            "Assets/Fsp/Scripts/Presentation/StarterProceduralCharacterVisual.cs",
            "Assets/Fsp/Scripts/Presentation/StarterProceduralVehicleVisual.cs",
            "Assets/Fsp/Scripts/Presentation/StarterPlaneVisual.cs",
            "Assets/Fsp/Scripts/Presentation/StarterProceduralWeaponVisual.cs"
        };

        public int callbackOrder => -1000;
        public void OnPreprocessBuild(BuildReport report)
        {
            foreach (var path in FilesToAudit)
            {
                if (!File.Exists(path)) continue;
                string text = File.ReadAllText(path);
                if (text.Contains("AndroidSafeMesh.Create") || text.Contains("GameObject.CreatePrimitive"))
                    throw new BuildFailedException("BMG authored-only violation: procedural visual generation found in " + path);
            }
        }
    }
}
#endif
