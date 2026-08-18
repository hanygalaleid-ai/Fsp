using UnityEngine;

namespace Fsp.Input
{
    public sealed class MobileInputBridge : MonoBehaviour
    {
        public static MobileInputBridge Instance { get; private set; }

        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool FireHeld { get; private set; }
        public bool AimHeld { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool SwitchWeaponPressed { get; private set; }
        public bool ReloadPressed { get; private set; }
        public bool HealPressed { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void LateUpdate()
        {
            JumpPressed = false;
            InteractPressed = false;
            SwitchWeaponPressed = false;
            ReloadPressed = false;
            HealPressed = false;
            Look = Vector2.zero;
        }

        public void SetMove(Vector2 value) => Move = Vector2.ClampMagnitude(value, 1f);
        public void AddLook(Vector2 delta) => Look += delta;
        public void SetFire(bool value) => FireHeld = value;
        public void SetAim(bool value) => AimHeld = value;
        public void SetSprint(bool value) => SprintHeld = value;
        public void PressJump() => JumpPressed = true;
        public void PressInteract() => InteractPressed = true;
        public void PressSwitchWeapon() => SwitchWeaponPressed = true;
        public void PressReload() => ReloadPressed = true;
        public void PressHeal() => HealPressed = true;
    }
}
