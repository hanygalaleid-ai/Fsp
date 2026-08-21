using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>Applies the approved BMG 3D menu icon atlas to the primary Lobby buttons.</summary>
    public sealed class BmgLobby3DIconRuntime : MonoBehaviour
    {
        private static BmgLobby3DIconRuntime instance;
        private Texture2D atlas;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var go = new GameObject("BMG_Lobby3DIconRuntime");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<BmgLobby3DIconRuntime>();
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.ApplyDelayed());
        }

        private void Awake() => atlas = Resources.Load<Texture2D>("BMG/UI/bmg_menu_icons_3d");

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(ApplyDelayed());

        private IEnumerator ApplyDelayed()
        {
            yield return null;
            yield return new WaitForSecondsRealtime(.2f);
            Apply();
            yield return new WaitForSecondsRealtime(.7f);
            Apply();
        }

        private void Apply()
        {
            if (atlas == null) return;
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) return;
            foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (button == null) continue;
                int tile = ResolveTile(button);
                if (tile < 0) continue;
                Attach(button.transform, tile);
            }
        }

        private static int ResolveTile(Button button)
        {
            string n = (button.gameObject.name ?? string.Empty).ToLowerInvariant();
            Text label = button.GetComponentInChildren<Text>(true);
            string t = label != null ? (label.text ?? string.Empty).ToLowerInvariant() : string.Empty;
            string key = n + " " + t;
            if (Has(key, "start", "ابدأ", "بدء")) return 0;
            if (Has(key, "setting", "إعداد", "اعداد")) return 1;
            if (Has(key, "mission", "مهام")) return 2;
            if (Has(key, "team", "squad", "فريق")) return 3;
            if (Has(key, "account", "حساب")) return 4;
            if (Has(key, "loadout", "weapon", "عتاد", "سلاح")) return 5;
            if (Has(key, "store", "shop", "متجر")) return 6;
            if (Has(key, "message", "mail", "رسائل")) return 7;
            if (Has(key, "map", "خريطة")) return 8;
            if (Has(key, "stat", "إحص", "احص")) return 9;
            if (Has(key, "sound", "audio", "صوت")) return 10;
            if (Has(key, "inventory", "backpack", "حقيبة", "مخزن")) return 11;
            return -1;
        }

        private void Attach(Transform parent, int tile)
        {
            Transform existing = parent.Find("BMG_3D_MenuIcon");
            RawImage image;
            if (existing == null)
            {
                var go = new GameObject("BMG_3D_MenuIcon", typeof(RectTransform), typeof(RawImage));
                go.transform.SetParent(parent, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(.02f, .12f);
                rt.anchorMax = new Vector2(.23f, .88f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                image = go.GetComponent<RawImage>();
                image.raycastTarget = false;
            }
            else image = existing.GetComponent<RawImage>();

            image.texture = atlas;
            int col = tile % 4;
            int rowFromTop = tile / 4;
            image.uvRect = new Rect(col * .25f, 1f - (rowFromTop + 1) * .25f, .25f, .25f);
            image.color = Color.white;
        }

        private static bool Has(string value, params string[] keys)
        {
            foreach (string key in keys) if (value.Contains(key)) return true;
            return false;
        }
    }
}
