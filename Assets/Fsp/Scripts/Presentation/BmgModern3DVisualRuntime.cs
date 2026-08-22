using System.Collections;
using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fsp.Presentation
{
    /// <summary>
    /// High-detail runtime fallback for BMG visuals. It replaces the blocky MK1/procedural character
    /// and transport aircraft with smoother multi-part 3D geometry and PBR-like materials created
    /// entirely at runtime, so no legacy renderers remain visible.
    /// </summary>
    public sealed class BmgModern3DVisualRuntime : MonoBehaviour
    {
        private static BmgModern3DVisualRuntime instance;
        private static Material tacticalBlack, tacticalOlive, skin, metal, glass, orange;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var host = new GameObject("BMG_Modern3DVisualRuntime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BmgModern3DVisualRuntime>();
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.ApplyDelayed());
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(ApplyDelayed());

        private IEnumerator ApplyDelayed()
        {
            EnsureMaterials();
            for (int pass = 0; pass < 10; pass++)
            {
                yield return pass == 0 ? null : new WaitForSeconds(.3f);
                ReplaceCharacters();
                ReplacePlanes();
            }
        }

        private static void EnsureMaterials()
        {
            if (tacticalBlack != null) return;
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Fsp/MobileSafeLit");
            tacticalBlack = Make(shader, new Color(.035f,.04f,.045f), .45f, .72f);
            tacticalOlive = Make(shader, new Color(.10f,.13f,.09f), .20f, .48f);
            skin = Make(shader, new Color(.34f,.20f,.14f), .05f, .35f);
            metal = Make(shader, new Color(.09f,.095f,.10f), .82f, .70f);
            glass = Make(shader, new Color(.035f,.09f,.12f), .55f, .90f);
            orange = Make(shader, new Color(.95f,.22f,.015f), .45f, .78f);
        }

        private static Material Make(Shader shader, Color color, float metallic, float smoothness)
        {
            var m = new Material(shader) { color = color, hideFlags = HideFlags.DontSave };
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            return m;
        }

        private static void ReplaceCharacters()
        {
            foreach (var character in FindObjectsByType<StarterProceduralCharacterVisual>(FindObjectsSortMode.None))
            {
                if (character == null || character.transform.Find("BMG_ModernCharacter") != null) continue;
                var oldRoot = character.transform.Find("FSP_CharacterVisual");
                if (oldRoot != null) DisableRenderers(oldRoot);
                var authored = character.transform.Find("BMG_Character_Authored");
                if (authored != null) DisableRenderers(authored);

                var root = new GameObject("BMG_ModernCharacter").transform;
                root.SetParent(character.transform, false);
                root.localPosition = Vector3.zero;
                root.localRotation = Quaternion.identity;

                // Tactical body silhouette.
                Part(root, PrimitiveType.Capsule, "Torso", new Vector3(0,1.22f,0), new Vector3(.58f,.72f,.34f), Quaternion.identity, tacticalBlack);
                Part(root, PrimitiveType.Cube, "Vest", new Vector3(0,1.28f,.16f), new Vector3(.64f,.55f,.18f), Quaternion.identity, tacticalOlive);
                Part(root, PrimitiveType.Sphere, "Head", new Vector3(0,1.92f,0), new Vector3(.34f,.38f,.34f), Quaternion.identity, skin);
                Part(root, PrimitiveType.Sphere, "Helmet", new Vector3(0,2.03f,-.01f), new Vector3(.40f,.28f,.40f), Quaternion.identity, tacticalBlack);
                Part(root, PrimitiveType.Cube, "Visor", new Vector3(0,1.98f,.25f), new Vector3(.34f,.09f,.045f), Quaternion.identity, glass);

                Limb(root,"LeftArm",new Vector3(-.48f,1.34f,0),new Vector3(.16f,.54f,.16f),Quaternion.Euler(0,0,-13));
                Limb(root,"RightArm",new Vector3(.48f,1.34f,0),new Vector3(.16f,.54f,.16f),Quaternion.Euler(0,0,13));
                Limb(root,"LeftLeg",new Vector3(-.20f,.56f,0),new Vector3(.19f,.62f,.19f),Quaternion.identity);
                Limb(root,"RightLeg",new Vector3(.20f,.56f,0),new Vector3(.19f,.62f,.19f),Quaternion.identity);

                Part(root,PrimitiveType.Cube,"LeftBoot",new Vector3(-.20f,.05f,.10f),new Vector3(.23f,.16f,.38f),Quaternion.identity,tacticalBlack);
                Part(root,PrimitiveType.Cube,"RightBoot",new Vector3(.20f,.05f,.10f),new Vector3(.23f,.16f,.38f),Quaternion.identity,tacticalBlack);
                Part(root,PrimitiveType.Cube,"Backpack",new Vector3(0,1.30f,-.31f),new Vector3(.52f,.58f,.22f),Quaternion.identity,tacticalOlive);

                // Chest pouches and armor detail.
                for (int i=-2;i<=2;i++) Part(root,PrimitiveType.Cube,"Pouch"+i,new Vector3(i*.115f,1.13f,.285f),new Vector3(.095f,.17f,.07f),Quaternion.identity,tacticalBlack);
                Part(root,PrimitiveType.Cube,"ShoulderL",new Vector3(-.43f,1.57f,.02f),new Vector3(.22f,.16f,.28f),Quaternion.Euler(0,0,-10),tacticalOlive);
                Part(root,PrimitiveType.Cube,"ShoulderR",new Vector3(.43f,1.57f,.02f),new Vector3(.22f,.16f,.28f),Quaternion.Euler(0,0,10),tacticalOlive);

                // Rifle with layered body, stock, optic and barrel.
                var rifle = new GameObject("BMG_ModernRifle").transform;
                rifle.SetParent(root,false);
                rifle.localPosition = new Vector3(.19f,1.30f,.38f);
                rifle.localRotation = Quaternion.Euler(10,-10,-34);
                Part(rifle,PrimitiveType.Cube,"Receiver",Vector3.zero,new Vector3(.16f,.12f,.62f),Quaternion.identity,metal);
                Part(rifle,PrimitiveType.Cube,"Stock",new Vector3(0,0,-.44f),new Vector3(.14f,.13f,.28f),Quaternion.identity,tacticalBlack);
                Part(rifle,PrimitiveType.Cylinder,"Barrel",new Vector3(0,0,.50f),new Vector3(.035f,.31f,.035f),Quaternion.Euler(90,0,0),metal);
                Part(rifle,PrimitiveType.Cube,"Optic",new Vector3(0,.11f,.03f),new Vector3(.10f,.09f,.18f),Quaternion.identity,tacticalBlack);
                Part(rifle,PrimitiveType.Cube,"Magazine",new Vector3(0,-.16f,.05f),new Vector3(.11f,.25f,.12f),Quaternion.Euler(10,0,0),metal);

                // BMG orange accent strip.
                Part(root,PrimitiveType.Cube,"BMGAccent",new Vector3(0,1.58f,.305f),new Vector3(.30f,.035f,.025f),Quaternion.identity,orange);
            }
        }

        private static void Limb(Transform parent, string name, Vector3 pos, Vector3 scale, Quaternion rot)
        {
            Part(parent, PrimitiveType.Capsule, name, pos, scale, rot, tacticalBlack);
        }

        private static void ReplacePlanes()
        {
            foreach (var plane in FindObjectsByType<StarterPlaneVisual>(FindObjectsSortMode.None))
            {
                if (plane == null || plane.transform.Find("BMG_ModernTransportPlane") != null) continue;
                var old = plane.transform.Find("FSP_TransportPlaneVisual");
                if (old != null) DisableRenderers(old);
                var authored = plane.transform.Find("BMG_TransportPlane_Authored");
                if (authored != null) DisableRenderers(authored);

                var root = new GameObject("BMG_ModernTransportPlane").transform;
                root.SetParent(plane.transform,false);
                root.localPosition = Vector3.zero;
                root.localRotation = Quaternion.identity;

                Part(root,PrimitiveType.Capsule,"Fuselage",Vector3.zero,new Vector3(.72f,2.8f,.72f),Quaternion.Euler(90,0,0),metal);
                Part(root,PrimitiveType.Cube,"Wing",new Vector3(0,.03f,0),new Vector3(5.8f,.12f,1.15f),Quaternion.Euler(0,0,0),metal);
                Part(root,PrimitiveType.Cube,"TailWing",new Vector3(0,.25f,-2.10f),new Vector3(2.25f,.09f,.52f),Quaternion.identity,metal);
                Part(root,PrimitiveType.Cube,"TailFin",new Vector3(0,.75f,-2.15f),new Vector3(.10f,1.15f,.60f),Quaternion.Euler(-12,0,0),tacticalOlive);
                Part(root,PrimitiveType.Sphere,"Nose",new Vector3(0,0,2.43f),new Vector3(.76f,.68f,.90f),Quaternion.identity,metal);
                Part(root,PrimitiveType.Cube,"Cockpit",new Vector3(0,.30f,1.80f),new Vector3(.48f,.18f,.50f),Quaternion.Euler(-12,0,0),glass);

                for(int side=-1;side<=1;side+=2)
                {
                    Part(root,PrimitiveType.Cylinder,"Engine"+side,new Vector3(side*1.55f,-.10f,.35f),new Vector3(.30f,.55f,.30f),Quaternion.Euler(90,0,0),tacticalBlack);
                    Part(root,PrimitiveType.Cylinder,"EngineRing"+side,new Vector3(side*1.55f,-.10f,.83f),new Vector3(.34f,.06f,.34f),Quaternion.Euler(90,0,0),orange);
                }
            }
        }

        private static GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Quaternion rot, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent,false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            go.transform.localScale = scale;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            return go;
        }

        private static void DisableRenderers(Transform root)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                if (renderer != null) renderer.enabled = false;
        }
    }
}
