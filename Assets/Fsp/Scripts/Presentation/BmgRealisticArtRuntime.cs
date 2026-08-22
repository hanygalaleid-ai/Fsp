using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fsp.Lobby;

namespace Fsp.Presentation
{
    /// <summary>
    /// Approved BMG presentation art only. Uses the six selected character renders and five selected weapon renders.
    /// The live controls remain functional above the modern backgrounds.
    /// </summary>
    public sealed class BmgRealisticArtRuntime : MonoBehaviour
    {
        private static BmgRealisticArtRuntime instance;
        private Texture2D logoTexture;
        private Texture2D characterAtlas;
        private Texture2D weaponAtlas;
        private Texture2D loadoutTexture;
        private RawImage logoImage;
        private RawImage characterImage;
        private RawImage weaponImage;
        private RawImage loadoutBackgroundImage;
        private LobbyState boundLobbyState;
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
            logoTexture = Resources.Load<Texture2D>("UI/bmg_app_icon");
            characterAtlas = Resources.Load<Texture2D>("BMG/Characters/bmg_characters_6_atlas");
            weaponAtlas = Resources.Load<Texture2D>("BMG/Weapons/bmg_weapons_5_atlas");
            loadoutTexture = Resources.Load<Texture2D>("BMG/UI/bmg_loadout_modern");
        }

        private void OnDestroy()
        {
            if (boundLobbyState != null) boundLobbyState.Changed -= RefreshCharacter;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this) instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (boundLobbyState != null)
            {
                boundLobbyState.Changed -= RefreshCharacter;
                boundLobbyState = null;
            }
            logoImage = null;
            characterImage = null;
            weaponImage = null;
            loadoutBackgroundImage = null;
            StartCoroutine(RefreshDelayed());
        }

        private IEnumerator RefreshDelayed()
        {
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(.25f);
            RefreshSceneArt();
            yield return new WaitForSecondsRealtime(.75f);
            RefreshSceneArt();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .35f;
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) return;
            BindLobbyState();
            RefreshWeaponPreview();
        }

        private void RefreshSceneArt()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Lobby", System.StringComparison.OrdinalIgnoreCase)) return;
            if (characterAtlas == null) characterAtlas = Resources.Load<Texture2D>("BMG/Characters/bmg_characters_6_atlas");
            if (weaponAtlas == null) weaponAtlas = Resources.Load<Texture2D>("BMG/Weapons/bmg_weapons_5_atlas");
            if (loadoutTexture == null) loadoutTexture = Resources.Load<Texture2D>("BMG/UI/bmg_loadout_modern");
            CreateLobbyLogo();
            CreateCharacterPreview();
            BindLobbyState();
            RefreshCharacter();
            RefreshWeaponPreview();
        }

        private static RectTransform FindLobbyRoot()
        {
            GameObject safe = GameObject.Find("SafeRoot");
            if (safe != null) return safe.GetComponent<RectTransform>();
            GameObject canvas = GameObject.Find("ProductionLobbyCanvas");
            return canvas != null ? canvas.GetComponent<RectTransform>() : null;
        }

        private void CreateLobbyLogo()
        {
            if (logoTexture == null || logoImage != null) return;
            RectTransform root = FindLobbyRoot();
            if (root == null) return;
            GameObject existing = GameObject.Find("BMG_RealisticLogo");
            if (existing != null) { logoImage = existing.GetComponent<RawImage>(); return; }

            GameObject go = new("BMG_RealisticLogo", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(root, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(.015f, .785f);
            rt.anchorMax = new Vector2(.16f, .985f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            logoImage = go.GetComponent<RawImage>();
            logoImage.texture = logoTexture;
            logoImage.color = Color.white;
            logoImage.raycastTarget = false;
            go.transform.SetAsLastSibling();
        }

        private void CreateCharacterPreview()
        {
            if (characterAtlas == null || characterImage != null) return;
            RectTransform root = FindLobbyRoot();
            if (root == null) return;
            GameObject existing = GameObject.Find("BMG_RealisticCharacterPreview");
            if (existing != null)
            {
                characterImage = existing.GetComponent<RawImage>();
                if (characterImage != null) characterImage.texture = characterAtlas;
                return;
            }

            GameObject go = new("BMG_RealisticCharacterPreview", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
            go.transform.SetParent(root, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(.31f, .145f);
            rt.anchorMax = new Vector2(.70f, .93f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            AspectRatioFitter fit = go.GetComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fit.aspectRatio = 1f;
            characterImage = go.GetComponent<RawImage>();
            characterImage.texture = characterAtlas;
            characterImage.color = Color.white;
            characterImage.raycastTarget = false;
            go.transform.SetSiblingIndex(Mathf.Min(2, root.childCount - 1));
        }

        private void BindLobbyState()
        {
            LobbyState state = LobbyState.Instance;
            if (state == null || state == boundLobbyState) return;
            if (boundLobbyState != null) boundLobbyState.Changed -= RefreshCharacter;
            boundLobbyState = state;
            boundLobbyState.Changed += RefreshCharacter;
            RefreshCharacter();
        }

        private void RefreshCharacter()
        {
            if (characterImage == null || characterAtlas == null) return;
            string id = LobbyState.Instance != null ? LobbyState.Instance.SelectedCharacterId : "soldier_01";
            characterImage.uvRect = AtlasRect(CharacterTile(id), 3, 2);
        }

        private static int CharacterTile(string id)
        {
            switch ((id ?? string.Empty).ToLowerInvariant())
            {
                case "soldier_01": return 0; // male 1
                case "soldier_02": return 1; // male 2
                case "soldier_03": return 2; // male 3
                case "soldier_04": return 3; // female 1
                case "soldier_05": return 4; // female 2
                case "soldier_06": return 5; // female 3
                default: return 0;
            }
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
                fit.aspectRatio = 5f;
                weaponImage = go.GetComponent<RawImage>();
                weaponImage.texture = weaponAtlas;
                weaponImage.color = Color.white;
                weaponImage.raycastTarget = false;
            }

            weaponImage.gameObject.SetActive(true);
            weaponImage.uvRect = AtlasRect(WeaponTile(LobbyGameplayProgress.LoadoutName), 5, 1);
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
