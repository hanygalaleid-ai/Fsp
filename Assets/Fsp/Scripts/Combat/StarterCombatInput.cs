using Fsp.BattleRoyale;
using Fsp.Inventory;
using Fsp.Vehicles;
using UnityEngine;

namespace Fsp.Combat
{
    /// <summary>
    /// Minimal fallback input used only until the final mobile HUD/input prefab is wired.
    /// Desktop: hold left mouse to fire, R to reload, Q to switch weapon, H to heal.
    /// Combat is disabled while aboard the drop plane, parachuting, or driving.
    /// </summary>
    public sealed class StarterCombatInput : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        private DropPlanePassenger passenger;
        private ParachuteController parachute;
        private StarterVehicleInput vehicleInput;

        private void Awake()
        {
            if (inventory == null) inventory = GetComponent<PlayerInventory>();
            passenger = GetComponent<DropPlanePassenger>();
            parachute = GetComponent<ParachuteController>();
            vehicleInput = GetComponent<StarterVehicleInput>();
        }

        private void Update()
        {
            if (inventory == null || IsCombatBlocked()) return;
            if (Input.GetMouseButton(0)) Fire();
            if (Input.GetKeyDown(KeyCode.R)) Reload();
            if (Input.GetKeyDown(KeyCode.Q)) SwitchWeapon();
            if (Input.GetKeyDown(KeyCode.H)) Heal();
        }

        public bool IsCombatBlocked()
        {
            if (passenger != null && passenger.IsAboard) return true;
            if (parachute != null && parachute.IsActive) return true;
            if (vehicleInput != null && vehicleInput.IsDriving) return true;
            return false;
        }

        public void Fire()
        {
            if (!IsCombatBlocked()) inventory?.ActiveWeapon?.TryFire();
        }

        public void Reload()
        {
            if (!IsCombatBlocked()) inventory?.TryReloadActiveWeapon();
        }

        public void SwitchWeapon()
        {
            if (!IsCombatBlocked()) inventory?.SwitchWeapon();
        }

        public void Heal()
        {
            if (!IsCombatBlocked()) inventory?.TryUseMedkit();
        }
    }
}
