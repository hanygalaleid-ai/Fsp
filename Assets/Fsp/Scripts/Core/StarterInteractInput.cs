using Fsp.BattleRoyale;
using Fsp.Vehicles;
using Fsp.World;
using UnityEngine;

namespace Fsp.Core
{
    /// <summary>
    /// One context action for the starter HUD.
    /// Plane: jump. Freefall: open parachute. Ground: open nearby door, otherwise enter/exit vehicle.
    /// </summary>
    public sealed class StarterInteractInput : MonoBehaviour
    {
        [SerializeField] private float doorInteractDistance = 2.6f;

        private DropPlanePassenger passenger;
        private ParachuteController parachute;
        private StarterVehicleInput vehicleInput;

        private void Awake() => Resolve();

        private void Resolve()
        {
            if (passenger == null) passenger = GetComponent<DropPlanePassenger>();
            if (parachute == null) parachute = GetComponent<ParachuteController>();
            if (vehicleInput == null) vehicleInput = GetComponent<StarterVehicleInput>();
        }

        public void Interact()
        {
            Resolve();
            if (passenger != null && passenger.IsAboard)
            {
                passenger.Jump();
                return;
            }

            if (parachute != null && parachute.IsActive && !parachute.IsOpen)
            {
                parachute.OpenParachute();
                return;
            }

            LightweightDoor door = FindNearestDoor();
            if (door != null)
            {
                door.Toggle();
                return;
            }

            vehicleInput?.ToggleVehicleInteraction();
        }

        public bool IsAvailable()
        {
            Resolve();
            if (passenger != null && passenger.IsAboard) return true;
            if (parachute != null && parachute.IsActive && !parachute.IsOpen) return true;
            if (FindNearestDoor() != null) return true;
            return vehicleInput != null && vehicleInput.HasVehicleInRange();
        }

        private LightweightDoor FindNearestDoor()
        {
            LightweightDoor[] doors = Object.FindObjectsOfType<LightweightDoor>();
            LightweightDoor nearest = null;
            float bestSqr = doorInteractDistance * doorInteractDistance;
            foreach (LightweightDoor door in doors)
            {
                if (door == null) continue;
                float sqr = (door.transform.position - transform.position).sqrMagnitude;
                if (sqr > bestSqr) continue;
                bestSqr = sqr;
                nearest = door;
            }
            return nearest;
        }
    }
}
