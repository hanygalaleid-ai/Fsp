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
        [SerializeField] private Vector3 primaryBackEuler = new(8f, 18f, 42f);
        [SerializeField] private Vector3 secondaryBackEuler = new(-5f, -22f, -38f);

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
            RefreshSockets(active);
        }

        private void RefreshSockets(HitscanWeapon active)
        {
            HitscanWeapon primary = inventory.PrimaryWeapon;
            HitscanWeapon secondary = inventory.SecondaryWeapon;

            if (primary != null)
            {
                primary.gameObject.SetActive(true);
                if (primary == active) AttachToHand(primary.transform);
                else AttachToBack(primary.transform, backPrimarySocket, primaryBackEuler);
            }

            if (secondary != null)
            {
                secondary.gameObject.SetActive(true);
                if (secondary == active) AttachToHand(secondary.transform);
                else AttachToBack(secondary.transform, backSecondarySocket, secondaryBackEuler);
            }
        }

        private void AttachToHand(Transform weapon)
        {
            if (weapon == null || rightHandSocket == null) return;
            weapon.SetParent(rightHandSocket, false);
            weapon.localPosition = handLocalPosition;
            weapon.localRotation = Quaternion.Euler(handLocalEuler);
        }

        private static void AttachToBack(Transform weapon, Transform socket, Vector3 euler)
        {
            if (weapon == null || socket == null) return;
            weapon.SetParent(socket, false);
            weapon.localPosition = Vector3.zero;
            weapon.localRotation = Quaternion.Euler(euler);
        }
    }
}
