using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    /// <summary>
    /// BMG lobby art helper. Runtime creation of lobby logo/character overlays is intentionally disabled.
    /// The lobby background and authored UI own the main screen. This component only decorates the loadout modal.
    /// </summary>
    public sealed class BmgRealisticArtRuntime : MonoBehaviour
    {
        private static BmgRealisticArtRuntime instance;
        private Texture2D weaponAtlas;
        private Texture2D loadoutTexture;
        private RawImage weaponImage;
        private RawImage loadoutBackgroundImage;
        private float nextRefresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null) return;
            var go = new GameObject("BMG_RealisticArtRuntime");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<BmgRealisticArtRuntime>();
            SceneManager.sceneLoaded += instance.OnSceneLoaded;
            instance.StartCoroutine(instance.RefreshDelayed());
        }

        private void Awake()
        {
            weaponAtlas = Resources.Load<Texture2D>("BMG/Weapons/bmg_weapons_5_atlas");
            loadoutTexture = Resources.Load<Texture2D>("BMG/UI/bmg_loadout_modern");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this) instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            weaponImage = null;
            loadoutBackgroundImage = null;
            StartCoroutine(RefreshDelayed());
        }

        private IEnumerator RefreshDelayed()
        {
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(.25f);
            RefreshLoadoutArt();
            yield return new WaitForSecondsRealtime(.75f);
            RefreshLoadoutArt();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .35f;
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) return;
            RefreshLoadoutArt();
        }

        private void RefreshLoadoutArt()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) return;
            if (weaponAtlas == null) weaponAtlas = Resources.Load<Texture2D>("BMG/Weapons/bmg_weapons_5_atlas");
            if (loadoutTexture == null) loadoutTexture = Resources.Load<Texture2D>("BMG/UI/bmg_loadout_modern");
            RefreshWeaponPreview();
        }

        private void RefreshWeaponPreview()
        {
            GameObject modal = GameObject.Find("LobbySectionModal");
            bool loadoutOpen = modal != null && modal.activeInHierarchy && ContainsLoadoutTitle(modal);
            if (!loadoutOpen)
            {
                if (weaponImage != null) weaponImage.gameObject.SetActive(false);
                if (loadoutBackgroundImage != null) loadoutBackgroundImage.gameObject.SetActive(false);
                return;
            }

            RectTransform panel = modal.GetComponent<RectTransform>();
            if (panel == null) return;

            if (loadoutTexture != null && loadoutBackgroundImage == null)
            {
                GameObject bg = new("BMG_ModernLoadoutBackground", typeof(RectTransform), typeof(RawImage));
                bg.transform.SetParent(panel, false);
                RectTransform brt = bg.GetComponent<RectTransform>();
                brt.anchorMin = Vector2.zero;
                brt.anchorMax = Vector2.one;
                brt.offsetMin = Vector2.zero;
                brt.offsetMax = Vector2.zero;
                loadoutBackgroundImage = bg.GetComponent<RawImage>();
                loadoutBackgroundImage.texture = loadoutTexture;
                loadoutBackgroundImage.color = Color.white;
                loadoutBackgroundImage.raycastTarget = false;
                bg.transform.SetAsFirstSibling();
            }
            if (loadoutBackgroundImage != null) loadoutBackgroundImage.gameObject.SetActive(true);

            if (weaponAtlas == null) return;
            if (weaponImage == null)
            {
                GameObject go = new("BMG_RealisticWeaponPreview", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
                go.transform.SetParent(panel, false);
                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(.48f, .40f);
                rt.anchorMax = new Vector2(.91f, .68f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                AspectRatioFitter fit = go.GetComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                fit.aspectRatio = 1.6f;
                weaponImage = go.GetComponent<RawImage>();
                weaponImage.texture = weaponAtlas;
                weaponImage.color = Color.white;
                weaponImage.raycastTarget = false;
            }

            weaponImage.gameObject.SetActive(true);
            weaponImage.texture = weaponAtlas;
            weaponImage.uvRect = AtlasRect(WeaponTile(Fsp.Lobby.LobbyGameplayProgress.LoadoutName), 5, 1);
            weaponImage.transform.SetAsLastSibling();
        }

        private static bool ContainsLoadoutTitle(GameObject modal)
        {
            foreach (Text t in modal.GetComponentsInChildren<Text>(true))
            {
                string v = (t.text ?? string.Empty).ToLowerInvariant();
                if (v.Contains("loadout") || v.Contains("عتاد") || v.Contains("تجهيز")) return true;
            }
            return false;
        }

        private static int WeaponTile(string loadout)
        {
            string v = (loadout ?? string.Empty).ToLowerInvariant();
            if (v.Contains("smg") || v.Contains("scout")) return 3;
            if (v.Contains("sniper")) return 4;
            if (v.Contains("marksman") || v.Contains("dmr")) return 2;
            if (v.Contains("heavy") || v.Contains("shotgun")) return 1;
            return 0;
        }

        private static Rect AtlasRect(int tile, int columns, int rows)
        {
            int col = Mathf.Clamp(tile % columns, 0, columns - 1);
            int rowFromTop = Mathf.Clamp(tile / columns, 0, rows - 1);
            float w = 1f / columns;
            float h = 1f / rows;
            float y = 1f - (rowFromTop + 1) * h;
            return new Rect(col * w, y, w, h);
        }
    }
}
