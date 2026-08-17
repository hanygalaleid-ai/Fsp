using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class LobbyCharacterShowcase : MonoBehaviour
    {
        [SerializeField] private Transform characterRoot;
        [SerializeField] private Transform cameraRig;
        [SerializeField] private Light keyLight;
        [SerializeField] private Light rimLight;
        [SerializeField] private float idleTurnDegrees = 7f;
        [SerializeField] private float idleTurnSpeed = 0.35f;
        [SerializeField] private float cameraFloat = 0.025f;
        [SerializeField] private float cameraFloatSpeed = 0.6f;

        private Quaternion baseCharacterRotation;
        private Vector3 baseCameraPosition;

        private void Awake()
        {
            if (characterRoot != null) baseCharacterRotation = characterRoot.localRotation;
            if (cameraRig != null) baseCameraPosition = cameraRig.localPosition;
        }

        private void Update()
        {
            float wave = Mathf.Sin(Time.unscaledTime * idleTurnSpeed);
            if (characterRoot != null)
                characterRoot.localRotation = baseCharacterRotation * Quaternion.Euler(0f, wave * idleTurnDegrees, 0f);

            if (cameraRig != null)
            {
                Vector3 p = baseCameraPosition;
                p.y += Mathf.Sin(Time.unscaledTime * cameraFloatSpeed) * cameraFloat;
                cameraRig.localPosition = p;
            }
        }

        public void ApplyTheme(FspVisualTheme theme)
        {
            if (theme == null) return;
            if (keyLight != null)
            {
                keyLight.color = theme.sunColor;
                keyLight.intensity = theme.sunIntensity;
            }
            if (rimLight != null)
            {
                rimLight.color = theme.bronze;
                rimLight.intensity = Mathf.Max(0.35f, theme.sunIntensity * 0.65f);
            }
        }
    }
}
