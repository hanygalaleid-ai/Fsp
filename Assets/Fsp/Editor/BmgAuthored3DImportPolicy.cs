#if UNITY_EDITOR
using UnityEditor;

namespace Fsp.EditorTools
{
    /// <summary>Android-friendly import defaults for Build 149 BMG authored OBJ meshes.</summary>
    public sealed class BmgAuthored3DImportPolicy : AssetPostprocessor
    {
        private const string Root = "Assets/Fsp/Art/Resources/Models/BMG/";

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(Root, System.StringComparison.OrdinalIgnoreCase)) return;
            if (assetImporter is not ModelImporter importer) return;

            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.addCollider = false;
            importer.importBlendShapes = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
        }
    }
}
#endif
