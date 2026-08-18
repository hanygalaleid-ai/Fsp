using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.World
{
    /// <summary>
    /// Release-safe world bootstrap for CI-generated Match scenes.
    /// Guarantees a visible styled Sunscar ground before POIs/roads are created.
    /// </summary>
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
            GameObject root = new GameObject(RootName);
            root.AddComponent<SunscarRuntimeWorldInstaller>();
        }

        private void Awake()
        {
            BuildBaseGround();
            BuildPoiHosts();
            BuildRoadNetwork();
            BuildOpenWorldCover();
        }

        private void BuildBaseGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Sunscar_Ground_Base";
            ground.transform.SetParent(transform, false);
            ground.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            ground.transform.localScale = new Vector3(2400f, 1f, 2400f);
            Renderer r = ground.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = GroundMaterial();
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
            Vector3 oldCrown = Vector3.zero;
            Vector3 dryfield = new Vector3(650f, 0f, 520f);
            Vector3 copper = new Vector3(700f, 0f, -520f);
            Vector3 coast = new Vector3(-700f, 0f, 520f);
            Vector3 quarry = new Vector3(-700f, 0f, -520f);
            Vector3 redline = new Vector3(0f, 0f, -800f);
            Vector3 saltworks = new Vector3(0f, 0f, 800f);
            Road(roads, "Road_OldCrown_Dryfield", oldCrown, dryfield, 18f);
            Road(roads, "Road_OldCrown_CopperPort", oldCrown, copper, 18f);
            Road(roads, "Road_OldCrown_LanternCoast", oldCrown, coast, 17f);
            Road(roads, "Road_OldCrown_WhiteQuarry", oldCrown, quarry, 17f);
            Road(roads, "Road_OldCrown_Redline", oldCrown, redline, 20f);
            Road(roads, "Road_OldCrown_Saltworks", oldCrown, saltworks, 20f);
            Road(roads, "Road_North_Coast_Dryfield", coast, dryfield, 14f);
            Road(roads, "Road_South_Quarry_Copper", quarry, copper, 14f);
        }

        private static void Road(Transform parent, string name, Vector3 a, Vector3 b, float width)
        {
            Vector3 delta = b - a;
            float length = delta.magnitude;
            if (length < 1f) return;
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = name;
            road.transform.SetParent(parent, false);
            road.transform.position = (a + b) * 0.5f + Vector3.up * 0.035f;
            road.transform.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            road.transform.localScale = new Vector3(width, 0.07f, length);
            Renderer renderer = road.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = RoadMaterial();
            Collider collider = road.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }

        private void BuildOpenWorldCover()
        {
            Transform cover = new GameObject("SunscarOpenWorldCover").transform;
            cover.SetParent(transform, false);
            var rng = new System.Random(73021);
            int created = 0, attempts = 0;
            while (created < 72 && attempts < 500)
            {
                attempts++;
                float x = Mathf.Lerp(-1020f, 1020f, (float)rng.NextDouble());
                float z = Mathf.Lerp(-1020f, 1020f, (float)rng.NextDouble());
                Vector3 p = new Vector3(x, 0f, z);
                if (InsideReservedPoi(p)) continue;
                float sx = Mathf.Lerp(2.8f, 7.5f, (float)rng.NextDouble());
                float sy = Mathf.Lerp(2.0f, 5.5f, (float)rng.NextDouble());
                float sz = Mathf.Lerp(2.8f, 7.5f, (float)rng.NextDouble());
                GameObject rock = GameObject.CreatePrimitive(created % 3 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere);
                rock.name = "WorldRock_Cover_" + created.ToString("00");
                rock.transform.SetParent(cover, false);
                rock.transform.localPosition = new Vector3(x, sy * 0.42f, z);
                rock.transform.localRotation = Quaternion.Euler((float)rng.NextDouble() * 18f, (float)rng.NextDouble() * 180f, (float)rng.NextDouble() * 12f);
                rock.transform.localScale = new Vector3(sx, sy, sz);
                Renderer renderer = rock.GetComponent<Renderer>();
                if (renderer != null) renderer.sharedMaterial = RockMaterial();
                created++;
            }
        }

        private static bool InsideReservedPoi(Vector3 p)
        {
            Vector2[] centers = { new Vector2(0f,0f), new Vector2(650f,520f), new Vector2(700f,-520f), new Vector2(-700f,520f), new Vector2(-700f,-520f), new Vector2(0f,-800f), new Vector2(0f,800f) };
            Vector2 point = new Vector2(p.x, p.z);
            foreach (Vector2 center in centers) if ((point - center).sqrMagnitude < 150f * 150f) return true;
            return false;
        }

        private static Material MakeMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Simple Lit") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse");
            if (shader == null) return null;
            Material m = new Material(shader) { name = name };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.05f);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.05f);
            return m;
        }

        private static Material GroundMaterial() => groundMaterial != null ? groundMaterial : groundMaterial = MakeMaterial("SUNSCAR_GROUND", new Color(0.45f, 0.30f, 0.16f, 1f));
        private static Material RoadMaterial() => roadMaterial != null ? roadMaterial : roadMaterial = MakeMaterial("SUNSCAR_ROAD", new Color(0.20f, 0.17f, 0.14f, 1f));
        private static Material RockMaterial() => rockMaterial != null ? rockMaterial : rockMaterial = MakeMaterial("SUNSCAR_ROCK", new Color(0.30f, 0.25f, 0.21f, 1f));
    }
}
