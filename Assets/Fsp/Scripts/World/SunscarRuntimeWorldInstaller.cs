using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.World
{
    /// <summary>
    /// Release-safe world bootstrap for CI-generated Match scenes. The repository currently ships the
    /// Sunscar POIs as runtime prototype components rather than serialized scene objects, so this installer
    /// guarantees those authored gameplay areas are actually present in every Android build.
    /// </summary>
    public sealed class SunscarRuntimeWorldInstaller : MonoBehaviour
    {
        private const string RootName = "SunscarRuntimeWorld";

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
            BuildPoiHosts();
            BuildRoadNetwork();
            BuildOpenWorldCover();
        }

        private void BuildPoiHosts()
        {
            // Target centers are spread across the official 2400x2400 Sunscar map. Several legacy POI
            // components contain their own local origin offsets, so host positions compensate for those.
            CreatePoiHost<StarterOldCrownEnvironment>("POI_OldCrown", new Vector3(0f, 0f, 0f));

            // Dryfield internal origin = (105, 92) -> target center about (650, 520).
            CreatePoiHost<DryfieldPrototype>("POI_Dryfield", new Vector3(545f, 0f, 428f));

            // Copper Port internal origin = (155, -120) -> target center about (700, -520).
            CreatePoiHost<CopperPortPrototype>("POI_CopperPort", new Vector3(545f, 0f, -400f));

            // Lantern Coast internal origin = (-155, 105) -> target center about (-700, 520).
            CreatePoiHost<LanternCoastPrototype>("POI_LanternCoast", new Vector3(-545f, 0f, 415f));

            // White Quarry internal origin = (-125, -82) -> target center about (-700, -520).
            CreatePoiHost<WhiteQuarryPrototype>("POI_WhiteQuarry", new Vector3(-575f, 0f, -438f));

            // Redline internal origin = (155, -120) -> target center about (0, -800).
            CreatePoiHost<RedlineAirstripPrototype>("POI_Redline", new Vector3(-155f, 0f, -680f));

            // Saltworks internal origin = (-15, 165) -> target center about (0, 800).
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

            // Outer cross-links prevent every rotation from forcing players through the center.
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

            Collider collider = road.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }

        private void BuildOpenWorldCover()
        {
            Transform cover = new GameObject("SunscarOpenWorldCover").transform;
            cover.SetParent(transform, false);

            // Deterministic cover in the large traversal spaces between POIs. Keep the central Old Crown
            // and each POI footprint relatively clear while giving players hard cover on long rotations.
            var rng = new System.Random(73021);
            int created = 0;
            int attempts = 0;
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
                rock.transform.localRotation = Quaternion.Euler(
                    (float)rng.NextDouble() * 18f,
                    (float)rng.NextDouble() * 180f,
                    (float)rng.NextDouble() * 12f);
                rock.transform.localScale = new Vector3(sx, sy, sz);
                created++;
            }
        }

        private static bool InsideReservedPoi(Vector3 p)
        {
            Vector2[] centers =
            {
                new Vector2(0f, 0f),
                new Vector2(650f, 520f),
                new Vector2(700f, -520f),
                new Vector2(-700f, 520f),
                new Vector2(-700f, -520f),
                new Vector2(0f, -800f),
                new Vector2(0f, 800f)
            };

            Vector2 point = new Vector2(p.x, p.z);
            foreach (Vector2 center in centers)
            {
                if ((point - center).sqrMagnitude < 150f * 150f)
                    return true;
            }
            return false;
        }
    }
}
