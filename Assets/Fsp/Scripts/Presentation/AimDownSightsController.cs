using Fsp.Input;
using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class AimDownSightsController : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Transform cameraRig;
        [SerializeField] private WeaponPresentation presentation;
        [SerializeField] private float hipFov = FspFixedTheme.MatchFieldOfView;
        [SerializeField] private float aimFov = 52f;
        [SerializeField] private Vector3 hipLocalPosition;
        [SerializeField] private Vector3 aimLocalPosition = new(0.18f, 0.02f, 0.18f);
        [SerializeField, Min(1f)] private float blendSpeed = 12f;

        private bool aiming;

        private void Awake()
        {
            if (gameplayCamera == null) gameplayCamera = Camera.main;
            if (cameraRig != null && hipLocalPosition == Vector3.zero) hipLocalPosition = cameraRig.localPosition;
        }

        private void Update()
        {
            if (gameplayCamera == null) gameplayCamera = Camera.main;

            MobileInputBridge input = MobileInputBridge.Instance;
            if (input != null) SetAim(input.AimHeld);

            float t = 1f - Mathf.Exp(-blendSpeed * Time.deltaTime);
            if (gameplayCamera != null) gameplayCamera.fieldOfView = Mathf.Lerp(gameplayCamera.fieldOfView, aiming ? aimFov : hipFov, t);
            if (cameraRig != null) cameraRig.localPosition = Vector3.Lerp(cameraRig.localPosition, aiming ? aimLocalPosition : hipLocalPosition, t);
        }

        public void SetAim(bool value)
        {
            if (aiming == value) return;
            aiming = value;
            presentation?.SetAim(value);
        }

        public bool IsAiming => aiming;
    }
}
