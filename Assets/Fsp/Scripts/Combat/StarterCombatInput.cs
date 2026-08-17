using Fsp.Inventory;
using UnityEngine;

namespace Fsp.Combat
{
    /// <summary>
    /// Minimal fallback input used only until the final mobile HUD/input prefab is wired.
    /// Desktop: hold left mouse to fire, R to reload, Q to switch weapon, H to heal.
    /// Mobile touch controls can call the public methods directly.
    /// </summary>
    public sealed class StarterCombatInput : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;

        private void Awake()
        {
            if (inventory == null) inventory = GetComponent<PlayerInventory>();
        }

        private void Update()
        {
            if (inventory == null) return;
            if (Input.GetMouseButton(0)) Fire();
            if (Input.GetKeyDown(KeyCode.R)) Reload();
            if (Input.GetKeyDown(KeyCode.Q)) SwitchWeapon();
            if (Input.GetKeyDown(KeyCode.H)) Heal();
        }

        public void Fire() => inventory?.ActiveWeapon?.TryFire();
        public void Reload() => inventory?.TryReloadActiveWeapon();
        public void SwitchWeapon() => inventory?.SwitchWeapon();
        public void Heal() => inventory?.TryUseMedkit();
    }
}
