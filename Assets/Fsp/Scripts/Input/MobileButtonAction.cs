using UnityEngine;
using UnityEngine.EventSystems;

namespace Fsp.Input
{
    public enum MobileButtonActionType
    {
        Fire,
        Aim,
        Sprint,
        Jump,
        Interact,
        SwitchWeapon,
        Reload,
        Heal
    }

    public sealed class MobileButtonAction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private MobileButtonActionType action;

        public void Configure(MobileButtonActionType value) => action = value;

        public void OnPointerDown(PointerEventData eventData)
        {
            var input = MobileInputBridge.Instance;
            if (input == null) return;

            switch (action)
            {
                case MobileButtonActionType.Fire: input.SetFire(true); break;
                case MobileButtonActionType.Aim: input.SetAim(true); break;
                case MobileButtonActionType.Sprint: input.SetSprint(true); break;
                case MobileButtonActionType.Jump: input.PressJump(); break;
                case MobileButtonActionType.Interact: input.PressInteract(); break;
                case MobileButtonActionType.SwitchWeapon: input.PressSwitchWeapon(); break;
                case MobileButtonActionType.Reload: input.PressReload(); break;
                case MobileButtonActionType.Heal: input.PressHeal(); break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ReleaseHeldAction();
        }

        private void OnDisable()
        {
            // MobileInputBridge survives scene loads. If a HUD/button is destroyed while a finger is
            // held down, PointerUp may never arrive; always release continuous actions here.
            ReleaseHeldAction();
        }

        private void ReleaseHeldAction()
        {
            var input = MobileInputBridge.Instance;
            if (input == null) return;

            switch (action)
            {
                case MobileButtonActionType.Fire: input.SetFire(false); break;
                case MobileButtonActionType.Aim: input.SetAim(false); break;
                case MobileButtonActionType.Sprint: input.SetSprint(false); break;
            }
        }
    }
}
