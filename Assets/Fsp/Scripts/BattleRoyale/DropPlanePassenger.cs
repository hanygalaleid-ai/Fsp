using Fsp.Backend;
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
        public DropPlaneController Plane => plane;

        private void Awake()
        {
            if (parachute == null) parachute = GetComponent<ParachuteController>();
            if (motor == null) motor = GetComponent<ThirdPersonMotor>();
        }

        private void OnDisable() => UnsubscribePlane();

        public void Configure(DropPlaneController value, Transform cabin)
        {
            UnsubscribePlane();
            plane = value;
            cabinAnchor = cabin;
            if (parachute == null) parachute = GetComponent<ParachuteController>();
            if (motor == null) motor = GetComponent<ThirdPersonMotor>();
            SubscribePlane();
            if (plane != null && plane.IsFlying && !aboard && !jumped) Board();
        }

        public void Board()
        {
            if (cabinAnchor == null || jumped || aboard) return;
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
            if (SupabaseSession.IsSignedIn && MatchRoomState.HasMatch)
            {
                MatchManager manager = MatchManager.Instance;
                if (manager == null || manager.Phase != MatchManager.MatchPhase.Active) return;
            }

            transform.SetParent(originalParent, true);
            aboard = false;
            jumped = true;
            if (motor != null) motor.enabled = false;
            parachute?.BeginDrop();
        }

        private void Update()
        {
            // Online configuration can finish after the local plane reaches its route end.
            // Retry the forced jump once the authoritative match becomes Active instead of
            // leaving the player permanently parented to a stopped plane.
            if (aboard && !jumped && plane != null && !plane.IsFlying)
            {
                Jump();
                if (aboard) return;
            }

            if (!jumped || parachute == null || parachute.IsActive) return;
            if (motor != null) motor.enabled = true;
            enabled = false;
        }

        private void HandlePlaneFinished()
        {
            if (aboard && !jumped) Jump();
        }

        private void SubscribePlane()
        {
            if (plane != null) plane.RouteFinished += HandlePlaneFinished;
        }

        private void UnsubscribePlane()
        {
            if (plane != null) plane.RouteFinished -= HandlePlaneFinished;
        }
    }
}
