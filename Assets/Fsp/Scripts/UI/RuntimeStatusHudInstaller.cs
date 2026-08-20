using System;
using Fsp.BattleRoyale;
using Fsp.Inventory;
using Fsp.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fsp.UI
{
    /// <summary>
    /// Non-interactive runtime status HUD for the minimal checked-in Match scene.
    /// It sits below MobileCombatHUD, never receives raycasts, and shows the critical
    /// information a player needs even when no authored BattleRoyaleHud exists.
    /// </summary>
    public sealed class RuntimeStatusHudInstaller : MonoBehaviour
    {
        private PlayerVitals vitals;
        private PlayerInventory inventory;
        private MatchManager matchManager;
        private SafeZoneController safeZone;
        private Transform player;

        private RectTransform healthFill;
        private RectTransform armorFill;
        private Text healthText;
        private Text armorText;
        private Text ammoText;
        private Text aliveText;
        private Text phaseText;
        private Text zoneWarning;

        private float nextResolve;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallAfterSceneLoad()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase)) return;
            if (FindFirstObjectByType<RuntimeStatusHudInstaller>() != null) return;
            new GameObject("Fsp_RuntimeStatusHudInstaller").AddComponent<RuntimeStatusHudInstaller>();
        }

        private void Awake()
        {
            BuildHud();
            ResolveSources();
        }

        private void Update()
        {
            if (!string.Equals(SceneManager.GetActiveScene().name, "Match", StringComparison.OrdinalIgnoreCase))
            {
                Destroy(gameObject);
                return;
            }

            if (Time.unscaledTime >= nextResolve)
            {
                nextResolve = Time.unscaledTime + 0.5f;
                ResolveSources();
            }

            Refresh();
        }

        private void ResolveSources()
        {
            if (player == null)
            {
                foreach (MatchParticipant participant in FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None))
                {
                    if (participant == null || !participant.IsLocalPlayer) continue;
                    player = participant.transform;
                    vitals = participant.GetComponent<PlayerVitals>();
                    inventory = participant.GetComponent<PlayerInventory>();
                    break;
                }
            }

            if (vitals == null && player != null) vitals = player.GetComponent<PlayerVitals>();
            if (inventory == null && player != null) inventory = player.GetComponent<PlayerInventory>();
            if (matchManager == null) matchManager = FindFirstObjectByType<MatchManager>();
            if (safeZone == null) safeZone = FindFirstObjectByType<SafeZoneController>();
        }

        private void BuildHud()
        {
            if (GameObject.Find("RuntimeStatusHUD") != null) return;

            GameObject canvasObject = new GameObject("RuntimeStatusHUD", typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 90;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = canvasObject.GetComponent<RectTransform>();

            GameObject leftPanel = CreatePanel(root, "StatusLeft", new Vector2(0.02f, 0.80f), new Vector2(0.27f, 0.975f));
            healthText = CreateText(leftPanel.transform, "HP 100", 22, new Vector2(0.04f, 0.58f), new Vector2(0.32f, 0.94f), TextAnchor.MiddleLeft);
            healthFill = CreateBar(leftPanel.transform, "HealthBar", new Vector2(0.32f, 0.63f), new Vector2(0.96f, 0.86f), new Color(0.18f, 0.78f, 0.30f, 1f));
            armorText = CreateText(leftPanel.transform, "ARMOR 0", 19, new Vector2(0.04f, 0.13f), new Vector2(0.32f, 0.50f), TextAnchor.MiddleLeft);
            armorFill = CreateBar(leftPanel.transform, "ArmorBar", new Vector2(0.32f, 0.18f), new Vector2(0.96f, 0.41f), new Color(0.20f, 0.58f, 0.95f, 1f));

            GameObject rightPanel = CreatePanel(root, "StatusRight", new Vector2(0.72f, 0.80f), new Vector2(0.98f, 0.975f));
            aliveText = CreateText(rightPanel.transform, "ALIVE --", 22, new Vector2(0.04f, 0.53f), new Vector2(0.48f, 0.93f), TextAnchor.MiddleLeft);
            ammoText = CreateText(rightPanel.transform, "AMMO -- / --", 22, new Vector2(0.48f, 0.53f), new Vector2(0.96f, 0.93f), TextAnchor.MiddleRight);
            phaseText = CreateText(rightPanel.transform, "WAITING", 18, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.48f), TextAnchor.MiddleCenter);

            zoneWarning = CreateText(root, "RETURN TO SAFE ZONE", 24, new Vector2(0.36f, 0.89f), new Vector2(0.64f, 0.95f), TextAnchor.MiddleCenter);
            zoneWarning.color = new Color(1f, 0.34f, 0.10f, 1f);
            zoneWarning.gameObject.SetActive(false);

            Text crosshair = CreateText(root, "+", 30, new Vector2(0.485f, 0.47f), new Vector2(0.515f, 0.53f), TextAnchor.MiddleCenter);
            crosshair.color = new Color(1f, 1f, 1f, 0.86f);
        }

        private void Refresh()
        {
            float hp = vitals != null ? Mathf.Clamp(vitals.Health, 0f, 100f) : 0f;
            float armor = vitals != null ? Mathf.Clamp(vitals.Armor, 0f, 100f) : 0f;
            SetBar(healthFill, hp / 100f);
            SetBar(armorFill, armor / 100f);
            if (healthText != null) healthText.text = "HP " + Mathf.CeilToInt(hp);
            if (armorText != null) armorText.text = "ARMOR " + Mathf.CeilToInt(armor);

            if (inventory != null && inventory.ActiveWeapon != null)
            {
                if (ammoText != null)
                    ammoText.text = "AMMO " + inventory.ActiveWeapon.AmmoInMagazine + " / " + inventory.GetReserveAmmoForActiveWeapon();
            }
            else if (ammoText != null) ammoText.text = "AMMO -- / --";

            if (aliveText != null)
                aliveText.text = "ALIVE " + (matchManager != null ? matchManager.AliveCount.ToString() : "--");

            if (phaseText != null)
            {
                if (matchManager == null) phaseText.text = "LOADING MATCH";
                else if (matchManager.Phase == MatchManager.MatchPhase.Countdown)
                    phaseText.text = "STARTING " + Mathf.CeilToInt(matchManager.CountdownRemaining);
                else phaseText.text = matchManager.Phase.ToString().ToUpperInvariant();
            }

            if (zoneWarning != null)
            {
                bool outside = safeZone != null && player != null && !safeZone.IsInside(player.position);
                zoneWarning.gameObject.SetActive(outside);
            }
        }

        private static GameObject CreatePanel(RectTransform parent, string name, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.02f, 0.04f, 0.07f, 0.72f);
            image.raycastTarget = false;
            return go;
        }

        private static RectTransform CreateBar(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            GameObject bg = new GameObject(name + "Bg", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(parent, false);
            RectTransform br = bg.GetComponent<RectTransform>();
            br.anchorMin = min;
            br.anchorMax = max;
            br.offsetMin = Vector2.zero;
            br.offsetMax = Vector2.zero;
            Image bgImage = bg.GetComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.55f);
            bgImage.raycastTarget = false;

            GameObject fill = new GameObject(name + "Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(bg.transform, false);
            RectTransform fr = fill.GetComponent<RectTransform>();
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = Vector2.one;
            fr.offsetMin = new Vector2(3f, 3f);
            fr.offsetMax = new Vector2(-3f, -3f);
            Image fillImage = fill.GetComponent<Image>();
            fillImage.color = color;
            fillImage.raycastTarget = false;
            return fr;
        }

        private static void SetBar(RectTransform fill, float value)
        {
            if (fill == null) return;
            value = Mathf.Clamp01(value);
            fill.anchorMax = new Vector2(value, 1f);
            fill.offsetMax = new Vector2(value >= 0.01f ? -3f : 0f, -3f);
        }

        private static Text CreateText(Transform parent, string value, int size, Vector2 min, Vector2 max, TextAnchor alignment)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Text text = go.GetComponent<Text>();
            Font font = null;
            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }
    }
}
