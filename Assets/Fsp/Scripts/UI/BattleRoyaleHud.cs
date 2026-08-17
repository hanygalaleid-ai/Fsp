using Fsp.BattleRoyale;
using Fsp.Combat;
using Fsp.Inventory;
using Fsp.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Fsp.UI
{
    public sealed class BattleRoyaleHud : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private PlayerVitals vitals;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private MatchManager matchManager;
        [SerializeField] private SafeZoneController safeZone;
        [SerializeField] private Transform trackedPlayer;

        [Header("Bars")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider armorBar;

        [Header("Labels")]
        [SerializeField] private Text ammoText;
        [SerializeField] private Text aliveText;
        [SerializeField] private Text medkitText;
        [SerializeField] private Text zoneWarningText;

        [Header("Buttons")]
        [SerializeField] private Button reloadButton;
        [SerializeField] private Button healButton;

        private HitscanWeapon observedWeapon;
        private bool subscribed;

        public void ConfigureSources(PlayerVitals playerVitals, PlayerInventory playerInventory, MatchManager manager, SafeZoneController zone, Transform playerTransform)
        {
            Unsubscribe();
            vitals = playerVitals;
            inventory = playerInventory;
            matchManager = manager;
            safeZone = zone;
            trackedPlayer = playerTransform;
            if (isActiveAndEnabled) Subscribe();
            RefreshAll();
        }

        private void OnEnable()
        {
            Subscribe();
            RefreshAll();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (subscribed) return;
            if (vitals != null) vitals.Changed += OnVitalsChanged;
            if (inventory != null) inventory.InventoryChanged += RefreshInventory;
            if (matchManager != null) matchManager.AliveCountChanged += OnAliveChanged;
            if (reloadButton != null) reloadButton.onClick.AddListener(Reload);
            if (healButton != null) healButton.onClick.AddListener(Heal);
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed) return;
            if (vitals != null) vitals.Changed -= OnVitalsChanged;
            if (inventory != null) inventory.InventoryChanged -= RefreshInventory;
            if (matchManager != null) matchManager.AliveCountChanged -= OnAliveChanged;
            if (observedWeapon != null) observedWeapon.AmmoChanged -= OnAmmoChanged;
            if (reloadButton != null) reloadButton.onClick.RemoveListener(Reload);
            if (healButton != null) healButton.onClick.RemoveListener(Heal);
            observedWeapon = null;
            subscribed = false;
        }

        private void Update()
        {
            if (zoneWarningText == null || safeZone == null || trackedPlayer == null) return;
            zoneWarningText.gameObject.SetActive(!safeZone.IsInside(trackedPlayer.position));
        }

        private void RefreshAll()
        {
            if (vitals != null) OnVitalsChanged(vitals.Health, vitals.Armor);
            RefreshInventory();
            if (matchManager != null) OnAliveChanged(matchManager.AliveCount);
            if (zoneWarningText != null) zoneWarningText.text = "ارجع إلى المنطقة الآمنة";
        }

        private void OnVitalsChanged(float health, float armor)
        {
            if (healthBar != null) healthBar.value = health / 100f;
            if (armorBar != null) armorBar.value = armor / 100f;
        }

        private void RefreshInventory()
        {
            if (inventory == null) return;

            if (observedWeapon != inventory.ActiveWeapon)
            {
                if (observedWeapon != null) observedWeapon.AmmoChanged -= OnAmmoChanged;
                observedWeapon = inventory.ActiveWeapon;
                if (observedWeapon != null) observedWeapon.AmmoChanged += OnAmmoChanged;
            }

            RefreshAmmoText();
            if (medkitText != null) medkitText.text = inventory.Medkits.ToString();
            if (healButton != null) healButton.interactable = inventory.Medkits > 0 && vitals != null && vitals.Health < 99.9f;
        }

        private void OnAmmoChanged(int value) => RefreshAmmoText();

        private void RefreshAmmoText()
        {
            if (ammoText == null || inventory == null) return;
            int mag = inventory.ActiveWeapon != null ? inventory.ActiveWeapon.AmmoInMagazine : 0;
            ammoText.text = mag + " / " + inventory.GetReserveAmmoForActiveWeapon();
        }

        private void OnAliveChanged(int count)
        {
            if (aliveText != null) aliveText.text = count.ToString();
        }

        private void Reload()
        {
            inventory?.TryReloadActiveWeapon();
            RefreshInventory();
        }

        private void Heal()
        {
            inventory?.TryUseMedkit();
            RefreshInventory();
        }
    }
}
