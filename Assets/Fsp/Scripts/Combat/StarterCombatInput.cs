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

        private void Awake() => Resolve();

        private void Resolve()
        {
            if (inventory == null) inventory = GetComponent<PlayerInventory>();
            if (passenger == null) passenger = GetComponent<DropPlanePassenger>();
            if (parachute == null) parachute = GetComponent<ParachuteController>();
            if (vehicleInput == null) vehicleInput = GetComponent<StarterVehicleInput>();
        }

        private void Update()
        {
            Resolve();
            if (inventory == null || IsCombatBlocked()) return;
            if (UnityEngine.Input.GetMouseButton(0)) Fire();
            if (UnityEngine.Input.GetKeyDown(KeyCode.R)) Reload();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Q)) SwitchWeapon();
            if (UnityEngine.Input.GetKeyDown(KeyCode.H)) Heal();
        }

        public bool IsCombatBlocked()
        {
            Resolve();
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
