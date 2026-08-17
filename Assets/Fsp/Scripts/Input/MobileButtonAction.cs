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
        SwitchWeapon
    }

    public sealed class MobileButtonAction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private MobileButtonActionType action;

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
            }
        }

        public void OnPointerUp(PointerEventData eventData)
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
