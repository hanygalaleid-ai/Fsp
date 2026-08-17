using Fsp.BattleRoyale;
using Fsp.Vehicles;
using UnityEngine;

namespace Fsp.Core
{
    /// <summary>
    /// One context action for the starter HUD.
    /// Plane: jump. Freefall: open parachute. Ground: enter/exit nearby vehicle.
    /// </summary>
    public sealed class StarterInteractInput : MonoBehaviour
    {
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

            vehicleInput?.ToggleVehicleInteraction();
        }

        public bool IsAvailable()
        {
            Resolve();
            if (passenger != null && passenger.IsAboard) return true;
            if (parachute != null && parachute.IsActive && !parachute.IsOpen) return true;
            return vehicleInput != null && vehicleInput.HasVehicleInRange();
        }
    }
}
