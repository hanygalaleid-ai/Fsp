using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fsp.Lobby;

namespace Fsp.Presentation
{
    /// <summary>
    /// Uses the approved realistic 2D BMG artwork in menus while preserving the live 3D match.
    /// Character and weapon artwork are packed into compact atlases for Android.
    /// </summary>
    public sealed class BmgRealisticArtRuntime : MonoBehaviour
    {
        private static BmgRealisticArtRuntime instance;
        private Texture2D logoTexture;
        private Texture2D characterAtlas;
        private Texture2D weaponAtlas;
        private RawImage logoImage;
        private RawImage characterImage;
        private RawImage weaponImage;
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
            logoTexture = Resources.Load<Texture2D>("BMG/Brand/bmg_logo");
            characterAtlas = Resources.Load<Texture2D>("BMG/Atlases/bmg_characters_atlas");
            weaponAtlas = Resources.Load<Texture2D>("BMG/Atlases/bmg_weapons_atlas");
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
            if (existing != null) { characterImage = existing.GetComponent<RawImage>(); return; }

            GameObject go = new("BMG_RealisticCharacterPreview", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
            go.transform.SetParent(root, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(.30f, .145f);
            rt.anchorMax = new Vector2(.71f, .94f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            AspectRatioFitter fit = go.GetComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fit.aspectRatio = 1f;
            characterImage = go.GetComponent<RawImage>();
            characterImage.texture = characterAtlas;
            characterImage.color = Color.white;
            characterImage.raycastTarget = false;
            // Keep menus/buttons above the portrait.
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
            int tile = CharacterTile(id);
            characterImage.uvRect = AtlasRect(tile, 4, 2);
        }

        private static int CharacterTile(string id)
        {
            switch ((id ?? string.Empty).ToLowerInvariant())
            {
                case "soldier_01": return 1; // approved desert operator
                case "soldier_02": return 2;
                case "soldier_03": return 4;
                case "soldier_04": return 5;
                case "soldier_05": return 0;
                case "soldier_06": return 6;
                default: return 1;
            }
        }

        private void RefreshWeaponPreview()
        {
            if (weaponAtlas == null) return;
            GameObject modal = GameObject.Find("LobbySectionModal");
            bool loadoutOpen = modal != null && modal.activeInHierarchy && ContainsLoadoutTitle(modal);
            if (!loadoutOpen)
            {
                if (weaponImage != null) weaponImage.gameObject.SetActive(false);
                return;
            }

            if (weaponImage == null)
            {
                RectTransform panel = modal.GetComponent<RectTransform>();
                GameObject go = new("BMG_RealisticWeaponPreview", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
                go.transform.SetParent(panel, false);
                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(.24f, .39f);
                rt.anchorMax = new Vector2(.76f, .68f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                AspectRatioFitter fit = go.GetComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                fit.aspectRatio = 1f;
                weaponImage = go.GetComponent<RawImage>();
                weaponImage.texture = weaponAtlas;
                weaponImage.raycastTarget = false;
            }

            weaponImage.gameObject.SetActive(true);
            weaponImage.uvRect = AtlasRect(WeaponTile(LobbyGameplayProgress.LoadoutName), 3, 2);
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
            if (v.Contains("smg") || v.Contains("scout")) return 2;
            if (v.Contains("sniper")) return 3;
            if (v.Contains("marksman") || v.Contains("dmr")) return 4;
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
