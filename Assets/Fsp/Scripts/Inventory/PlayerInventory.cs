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

        public int PrimaryAmmo { get; private set; }
        public int SecondaryAmmo { get; private set; }
        public HitscanWeapon ActiveWeapon { get; private set; }

        public event Action InventoryChanged;

        private void Awake()
        {
            if (vitals == null) vitals = GetComponent<PlayerVitals>();
            SetActiveWeapon(primaryWeapon != null ? primaryWeapon : secondaryWeapon);
        }

        public bool TryPickup(InventoryItem item)
        {
            if (item == null) return false;

            switch (item.type)
            {
                case InventoryItemType.Ammo:
                    AddAmmo(item.ammoAmount);
                    break;
                case InventoryItemType.Medkit:
                    vitals?.Heal(item.healAmount);
                    break;
                case InventoryItemType.Armor:
                    vitals?.AddArmor(item.armorAmount);
                    break;
                case InventoryItemType.Weapon:
                    return false; // weapon pickup is wired by prefab/reference when the full Unity base is imported.
            }

            InventoryChanged?.Invoke();
            return true;
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

        public bool TryConsumeAmmo(int amount = 1)
        {
            if (ActiveWeapon == null || amount <= 0) return false;

            if (ActiveWeapon == secondaryWeapon)
            {
                if (SecondaryAmmo < amount) return false;
                SecondaryAmmo -= amount;
            }
            else
            {
                if (PrimaryAmmo < amount) return false;
                PrimaryAmmo -= amount;
            }

            InventoryChanged?.Invoke();
            return true;
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
