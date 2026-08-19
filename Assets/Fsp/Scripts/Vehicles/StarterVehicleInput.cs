using Fsp.Backend;
using Fsp.Networking;
using Fsp.Player;
using UnityEngine;

namespace Fsp.Vehicles
{
    /// <summary>
    /// Fallback vehicle interaction for the starter scene.
    /// Desktop: E enters/exits the nearest vehicle, WASD drives, Space brakes.
    /// Mobile HUD can call ToggleVehicleInteraction().
    /// </summary>
    public sealed class StarterVehicleInput : MonoBehaviour
    {
        [SerializeField] private float enterDistance = 4f;

        private SimpleVehicleController activeVehicle;
        private NetworkVehicleSync activeSync;
        private ThirdPersonMotor motor;
        private CharacterController characterController;
        private Renderer[] renderers;
        private Transform originalParent;
        private bool enterPending;

        public bool IsDriving => activeVehicle != null;

        private void Awake()
        {
            motor = GetComponent<ThirdPersonMotor>();
            characterController = GetComponent<CharacterController>();
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
                ToggleVehicleInteraction();
#endif

            if (activeVehicle == null) return;

            float throttle = 0f;
            float steering = 0f;
#if ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow)) throttle += 1f;
            if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow)) throttle -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow)) steering -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow)) steering += 1f;
            bool brake = UnityEngine.Input.GetKey(KeyCode.Space);
#else
            bool brake = false;
#endif
            activeVehicle.SetInput(throttle, steering, brake);
        }

        public void ToggleVehicleInteraction()
        {
            if (enterPending) return;
            if (activeVehicle == null) TryEnterNearestVehicle();
            else ExitVehicle();
        }

        public bool HasVehicleInRange()
        {
            if (activeVehicle != null) return true;
            float maxSqr = enterDistance * enterDistance;
            foreach (SimpleVehicleController vehicle in FindObjectsByType<SimpleVehicleController>(FindObjectsSortMode.None))
            {
                if (vehicle == null || vehicle.DriverPresent) continue;
                NetworkVehicleSync sync = vehicle.GetComponent<NetworkVehicleSync>();
                if (sync != null && sync.RemotelyOccupied) continue;
                if ((vehicle.transform.position - transform.position).sqrMagnitude <= maxSqr)
                    return true;
            }
            return false;
        }

        private void TryEnterNearestVehicle()
        {
            SimpleVehicleController[] vehicles = FindObjectsByType<SimpleVehicleController>(FindObjectsSortMode.None);
            SimpleVehicleController best = null;
            float bestSqr = enterDistance * enterDistance;

            foreach (SimpleVehicleController vehicle in vehicles)
            {
                if (vehicle == null || vehicle.DriverPresent) continue;
                NetworkVehicleSync sync = vehicle.GetComponent<NetworkVehicleSync>();
                if (sync != null && sync.RemotelyOccupied) continue;
                float sqr = (vehicle.transform.position - transform.position).sqrMagnitude;
                if (sqr > bestSqr) continue;
                bestSqr = sqr;
                best = vehicle;
            }

            if (best == null) return;

            bool onlineMatch = SupabaseSession.IsSignedIn && MatchRoomState.HasMatch;
            NetworkVehicleSync networkSync = best.GetComponent<NetworkVehicleSync>();
            if (onlineMatch)
            {
                if (networkSync == null) networkSync = best.gameObject.AddComponent<NetworkVehicleSync>();
                enterPending = true;
                bool requested = networkSync.RequestDriverSeat(accepted =>
                {
                    enterPending = false;
                    if (accepted) CompleteEnter(best, networkSync);
                });
                if (!requested) enterPending = false;
                return;
            }

            CompleteEnter(best, networkSync);
        }

        private void CompleteEnter(SimpleVehicleController vehicle, NetworkVehicleSync networkSync)
        {
            if (vehicle == null || activeVehicle != null) return;
            activeVehicle = vehicle;
            activeSync = networkSync;
            if (activeSync != null) activeSync.MarkLocalDriverActive();
            else activeVehicle.SetDriverPresent(true);

            originalParent = transform.parent;
            if (motor != null) motor.enabled = false;
            if (characterController != null) characterController.enabled = false;
            SetRenderers(false);

            transform.SetParent(activeVehicle.transform, false);
            transform.localPosition = new Vector3(0f, 0.9f, 0f);
            transform.localRotation = Quaternion.identity;
        }

        private void ExitVehicle()
        {
            if (activeVehicle == null) return;
            SimpleVehicleController vehicle = activeVehicle;
            NetworkVehicleSync sync = activeSync;

            vehicle.SetInput(0f, 0f, true);
            if (sync != null) sync.ReleaseDriverSeat();
            else vehicle.SetDriverPresent(false);

            activeVehicle = null;
            activeSync = null;

            transform.SetParent(originalParent, true);
            transform.position = vehicle.transform.position + vehicle.transform.right * 2.4f + Vector3.up * 0.2f;
            SetRenderers(true);
            if (characterController != null) characterController.enabled = true;
            if (motor != null) motor.enabled = true;
        }

        private void SetRenderers(bool visible)
        {
            if (renderers == null) return;
            foreach (Renderer renderer in renderers)
                if (renderer != null) renderer.enabled = visible;
        }
    }
}
