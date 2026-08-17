using Fsp.Combat;
using Fsp.Inventory;
using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class WeaponSocketRig : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private Transform rightHandSocket;
        [SerializeField] private Transform backPrimarySocket;
        [SerializeField] private Transform backSecondarySocket;
        [SerializeField] private Vector3 handLocalPosition;
        [SerializeField] private Vector3 handLocalEuler;

        private HitscanWeapon lastActive;

        private void Awake()
        {
            if (inventory == null) inventory = GetComponentInParent<PlayerInventory>();
        }

        private void LateUpdate()
        {
            if (inventory == null) return;
            HitscanWeapon active = inventory.ActiveWeapon;
            if (active == lastActive) return;
            lastActive = active;
            AttachActive(active);
        }

        private void AttachActive(HitscanWeapon weapon)
        {
            if (weapon == null || rightHandSocket == null) return;
            Transform t = weapon.transform;
            t.SetParent(rightHandSocket, false);
            t.localPosition = handLocalPosition;
            t.localRotation = Quaternion.Euler(handLocalEuler);
        }

        public void StowPrimary(Transform weaponVisual)
        {
            if (weaponVisual == null || backPrimarySocket == null) return;
            weaponVisual.SetParent(backPrimarySocket, false);
            weaponVisual.localPosition = Vector3.zero;
            weaponVisual.localRotation = Quaternion.identity;
        }

        public void StowSecondary(Transform weaponVisual)
        {
            if (weaponVisual == null || backSecondarySocket == null) return;
            weaponVisual.SetParent(backSecondarySocket, false);
            weaponVisual.localPosition = Vector3.zero;
            weaponVisual.localRotation = Quaternion.identity;
        }
    }
}
