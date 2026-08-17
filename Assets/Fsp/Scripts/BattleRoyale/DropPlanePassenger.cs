using Fsp.Player;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class DropPlanePassenger : MonoBehaviour
    {
        [SerializeField] private DropPlaneController plane;
        [SerializeField] private Transform cabinAnchor;
        [SerializeField] private ParachuteController parachute;
        [SerializeField] private ThirdPersonMotor motor;

        private Transform originalParent;
        private bool aboard;
        private bool jumped;

        public bool IsAboard => aboard;
        public bool HasJumped => jumped;

        private void Awake()
        {
            if (parachute == null) parachute = GetComponent<ParachuteController>();
            if (motor == null) motor = GetComponent<ThirdPersonMotor>();
        }

        public void Configure(DropPlaneController value, Transform cabin)
        {
            plane = value;
            cabinAnchor = cabin;
            if (parachute == null) parachute = GetComponent<ParachuteController>();
            if (motor == null) motor = GetComponent<ThirdPersonMotor>();
        }

        public void Board()
        {
            if (cabinAnchor == null || jumped) return;
            originalParent = transform.parent;
            transform.SetParent(cabinAnchor, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            aboard = true;
            if (motor != null) motor.enabled = false;
        }

        public void Jump()
        {
            if (!aboard || jumped) return;
            transform.SetParent(originalParent, true);
            aboard = false;
            jumped = true;
            if (motor != null) motor.enabled = false;
            parachute?.BeginDrop();
        }

        private void Update()
        {
            if (!jumped || parachute == null || parachute.IsActive) return;
            if (motor != null) motor.enabled = true;
            enabled = false;
        }
    }
}
