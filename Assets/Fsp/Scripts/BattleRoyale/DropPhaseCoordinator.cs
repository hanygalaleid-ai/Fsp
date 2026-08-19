using Fsp.Backend;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class DropPhaseCoordinator : MonoBehaviour
    {
        [SerializeField] private DropPlaneController plane;
        [SerializeField] private DropPlanePassenger localPassenger;
        [SerializeField] private bool autoBoardOnStart = true;
        [SerializeField] private bool forceJumpAtRouteEnd = true;

        private void Awake()
        {
            if (plane == null) plane = FindFirstObjectByType<DropPlaneController>();
            if (localPassenger == null) localPassenger = FindFirstObjectByType<DropPlanePassenger>();
        }

        private void OnEnable()
        {
            if (plane != null) plane.RouteFinished += HandleRouteFinished;
        }

        private void Start()
        {
            if (!MatchRoomState.HasMatch) return;
            if (autoBoardOnStart && localPassenger != null && !localPassenger.HasJumped)
                localPassenger.Board();
        }

        private void HandleRouteFinished()
        {
            if (!forceJumpAtRouteEnd || localPassenger == null) return;
            if (localPassenger.IsAboard && !localPassenger.HasJumped)
                localPassenger.Jump();
        }

        private void OnDisable()
        {
            if (plane != null) plane.RouteFinished -= HandleRouteFinished;
        }
    }
}
