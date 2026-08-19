using System;
using Fsp.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.World
{
    public sealed class SunscarRuntimeWorldInstaller : MonoBehaviour
    {
        private const string RootName = "SunscarRuntimeWorld";
        private static Material groundMaterial;
        private static Material roadMaterial;
        private static Material rockMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (GameObject.Find(RootName) != null) return;
            new GameObject(RootName).AddComponent<SunscarRuntimeWorldInstaller>();
        }

        private void Awake()
        {
            BuildBaseGround();
            BuildRoadNetwork();
            BuildOpenWorldCover();
#if !UNITY_ANDROID || UNITY_EDITOR
            BuildPoiHosts();
#endif
        }

        private void BuildBaseGround()
        {
            GameObject ground = AndroidSafeMesh.CreateBox("Sunscar_Ground_Base", transform);
            ground.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            ground.transform.localScale = new Vector3(2400f, 1f, 2400f);
            Renderer r = ground.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = GroundMaterial();
            // Add the one required collision surface explicitly; no CreatePrimitive reflection path.
            BoxCollider collider = ground.AddComponent<BoxCollider>();
            collider.size = Vector3.one;
        }

        private void BuildPoiHosts()
        {
            CreatePoiHost<StarterOldCrownEnvironment>("POI_OldCrown", Vector3.zero);
            CreatePoiHost<DryfieldPrototype>("POI_Dryfield", new Vector3(545f, 0f, 428f));
            CreatePoiHost<CopperPortPrototype>("POI_CopperPort", new Vector3(545f, 0f, -400f));
            CreatePoiHost<LanternCoastPrototype>("POI_LanternCoast", new Vector3(-545f, 0f, 415f));
            CreatePoiHost<WhiteQuarryPrototype>("POI_WhiteQuarry", new Vector3(-575f, 0f, -438f));
            CreatePoiHost<RedlineAirstripPrototype>("POI_Redline", new Vector3(-155f, 0f, -680f));
            CreatePoiHost<SaltworksPrototype>("POI_Saltworks", new Vector3(15f, 0f, 635f));
        }

        private T CreatePoiHost<T>(string name, Vector3 position) where T : Component
        {
            GameObject host = new GameObject(name);
            host.transform.SetParent(transform, false);
            host.transform.localPosition = position;
            return host.AddComponent<T>();
        }

        private void BuildRoadNetwork()
        {
            Transform roads = new GameObject("SunscarRoadNetwork").transform;
            roads.SetParent(transform, false);
            Road(roads, "Road_OldCrown_Dryfield", Vector3.zero, new Vector3(650f, 0f, 520f), 18f);
            Road(roads, "Road_OldCrown_CopperPort", Vector3.zero, new Vector3(700f, 0f, -520f), 18f);
            Road(roads, "Road_OldCrown_LanternCoast", Vector3.zero, new Vector3(-700f, 0f, 520f), 17f);
            Road(roads, "Road_OldCrown_WhiteQuarry", Vector3.zero, new Vector3(-700f, 0f, -520f), 17f);
        }

        private static void Road(Transform parent, string name, Vector3 a, Vector3 b, float width)
        {
            Vector3 delta = b - a;
            float length = delta.magnitude;
            if (length < 1f) return;
            GameObject road = AndroidSafeMesh.CreateBox(name, parent);
            road.transform.position = (a + b) * 0.5f + Vector3.up * 0.035f;
            road.transform.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            road.transform.localScale = new Vector3(width, 0.07f, length);
            Renderer renderer = road.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = RoadMaterial();
        }

        private void BuildOpenWorldCover()
        {
            Transform cover = new GameObject("SunscarOpenWorldCover").transform;
            cover.SetParent(transform, false);
            var rng = new System.Random(73021);
            int count = Application.isMobilePlatform ? 24 : 72;
            for (int i = 0; i < count; i++)
            {
                float x = Mathf.Lerp(-900f, 900f, (float)rng.NextDouble());
                float z = Mathf.Lerp(-900f, 900f, (float)rng.NextDouble());
                float sx = Mathf.Lerp(3f, 8f, (float)rng.NextDouble());
                float sy = Mathf.Lerp(2f, 6f, (float)rng.NextDouble());
                float sz = Mathf.Lerp(3f, 8f, (float)rng.NextDouble());
                GameObject rock = AndroidSafeMesh.CreateBox("WorldRock_Cover_" + i.ToString("00"), cover);
                rock.transform.localPosition = new Vector3(x, sy * 0.5f, z);
                rock.transform.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 180f, 0f);
                rock.transform.localScale = new Vector3(sx, sy, sz);
                Renderer renderer = rock.GetComponent<Renderer>();
                if (renderer != null) renderer.sharedMaterial = RockMaterial();
            }
        }

        private static Material MakeMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Unlit/Color") ?? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            if (shader == null) return null;
            Material m = new Material(shader) { name = name, hideFlags = HideFlags.DontSave };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            m.color = color;
            return m;
        }

        private static Material GroundMaterial() => groundMaterial != null ? groundMaterial : groundMaterial = MakeMaterial("SUNSCAR_GROUND", new Color(0.45f, 0.30f, 0.16f, 1f));
        private static Material RoadMaterial() => roadMaterial != null ? roadMaterial : roadMaterial = MakeMaterial("SUNSCAR_ROAD", new Color(0.20f, 0.17f, 0.14f, 1f));
        private static Material RockMaterial() => rockMaterial != null ? rockMaterial : rockMaterial = MakeMaterial("SUNSCAR_ROCK", new Color(0.30f, 0.25f, 0.21f, 1f));
    }
}
