using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class LobbyBackdropMotion : MonoBehaviour
    {
        [SerializeField] private Transform[] distantLayers;
        [SerializeField] private float driftSpeed = 0.12f;
        [SerializeField] private float driftAmount = 0.35f;
        [SerializeField] private ParticleSystem dust;

        private Vector3[] origins;

        private void Awake()
        {
            origins = new Vector3[distantLayers != null ? distantLayers.Length : 0];
            for (int i = 0; i < origins.Length; i++)
                if (distantLayers[i] != null) origins[i] = distantLayers[i].localPosition;
        }

        private void Update()
        {
            for (int i = 0; i < origins.Length; i++)
            {
                Transform layer = distantLayers[i];
                if (layer == null) continue;
                Vector3 p = origins[i];
                p.x += Mathf.Sin(Time.unscaledTime * driftSpeed + i * 0.8f) * driftAmount * (i + 1f) / origins.Length;
                layer.localPosition = p;
            }
        }

        public void SetLowPower(bool lowPower)
        {
            if (dust == null) return;
            var emission = dust.emission;
            emission.rateOverTimeMultiplier = lowPower ? 0.35f : 1f;
        }
    }
}
