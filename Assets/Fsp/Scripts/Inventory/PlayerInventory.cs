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
                    return false;
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
