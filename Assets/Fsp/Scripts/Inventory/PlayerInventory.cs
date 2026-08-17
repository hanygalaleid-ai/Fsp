using System;
using Fsp.Combat;
using Fsp.Player;
using UnityEngine;

namespace Fsp.Inventory
{
    public sealed class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private HitscanWeapon primaryWeapon;
        [SerializeField] private HitscanWeapon secondaryWeapon;
        [SerializeField] private PlayerVitals vitals;
        [SerializeField] private float medkitHealAmount = 45f;
        [SerializeField] private int maxMedkits = 5;

        public int PrimaryAmmo { get; private set; }
        public int SecondaryAmmo { get; private set; }
        public int Medkits { get; private set; }
        public HitscanWeapon ActiveWeapon { get; private set; }

        public event Action InventoryChanged;

        private void Awake()
        {
            if (vitals == null) vitals = GetComponent<PlayerVitals>();
            SetActiveWeapon(primaryWeapon != null ? primaryWeapon : secondaryWeapon);
        }

        public bool CanPickup(InventoryItem item)
        {
            if (item == null) return false;
            switch (item.type)
            {
                case InventoryItemType.Medkit: return Medkits < maxMedkits;
                case InventoryItemType.Weapon: return false;
                case InventoryItemType.Ammo:
                case InventoryItemType.Armor:
                    return true;
                default: return false;
            }
        }

        public bool TryPickup(InventoryItem item)
        {
            if (!CanPickup(item)) return false;

            switch (item.type)
            {
                case InventoryItemType.Ammo:
                    AddAmmo(item.ammoAmount);
                    break;
                case InventoryItemType.Medkit:
                    Medkits++;
                    break;
                case InventoryItemType.Armor:
                    vitals?.AddArmor(item.armorAmount);
                    break;
                case InventoryItemType.Weapon:
                    return false;
            }

            InventoryChanged?.Invoke();
            return true;
        }

        public DeathInventorySnapshot DrainForDeath()
        {
            var snapshot = new DeathInventorySnapshot
            {
                primaryAmmo = PrimaryAmmo,
                secondaryAmmo = SecondaryAmmo,
                medkits = Medkits
            };

            PrimaryAmmo = 0;
            SecondaryAmmo = 0;
            Medkits = 0;
            InventoryChanged?.Invoke();
            return snapshot;
        }

        public void AddDeathLoot(DeathInventorySnapshot snapshot)
        {
            PrimaryAmmo += Mathf.Max(0, snapshot.primaryAmmo);
            SecondaryAmmo += Mathf.Max(0, snapshot.secondaryAmmo);
            Medkits = Mathf.Min(maxMedkits, Medkits + Mathf.Max(0, snapshot.medkits));
            InventoryChanged?.Invoke();
        }

        public void AddAmmo(int amount)
        {
            if (amount <= 0) return;
            if (ActiveWeapon == secondaryWeapon) SecondaryAmmo += amount;
            else PrimaryAmmo += amount;
            InventoryChanged?.Invoke();
        }

        public void SwitchWeapon()
        {
            if (primaryWeapon == null && secondaryWeapon == null) return;
            SetActiveWeapon(ActiveWeapon == primaryWeapon ? secondaryWeapon : primaryWeapon);
        }

        public bool TryUseMedkit()
        {
            if (Medkits <= 0 || vitals == null || !vitals.IsAlive) return false;
            if (vitals.Health >= 99.9f) return false;

            Medkits--;
            vitals.Heal(medkitHealAmount);
            InventoryChanged?.Invoke();
            return true;
        }

        public bool TryReloadActiveWeapon()
        {
            return ActiveWeapon != null && ActiveWeapon.ReloadInstant();
        }

        public int GetReserveAmmoForActiveWeapon()
        {
            if (ActiveWeapon == secondaryWeapon) return SecondaryAmmo;
            return PrimaryAmmo;
        }

        public int ConsumeReserveAmmoFor(HitscanWeapon weapon, int requested)
        {
            if (weapon == null || requested <= 0) return 0;

            int taken;
            if (weapon == secondaryWeapon)
            {
                taken = Mathf.Min(SecondaryAmmo, requested);
                SecondaryAmmo -= taken;
            }
            else
            {
                taken = Mathf.Min(PrimaryAmmo, requested);
                PrimaryAmmo -= taken;
            }

            if (taken > 0) InventoryChanged?.Invoke();
            return taken;
        }

        private void SetActiveWeapon(HitscanWeapon weapon)
        {
            ActiveWeapon = weapon;
            if (primaryWeapon != null) primaryWeapon.gameObject.SetActive(primaryWeapon == weapon);
            if (secondaryWeapon != null) secondaryWeapon.gameObject.SetActive(secondaryWeapon == weapon);
            InventoryChanged?.Invoke();
        }
    }
}
