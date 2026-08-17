using Fsp.Backend;
using UnityEngine;

namespace Fsp.Networking
{
    public sealed class RemoteFireEffects : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour transportBehaviour;
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private GameObject tracerPrefab;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip remoteShotClip;
        [SerializeField, Min(0.1f)] private float tracerLength = 18f;
        [SerializeField, Min(0.01f)] private float effectLifetime = 0.2f;

        private INetworkTransport transport;

        private void Awake()
        {
            transport = transportBehaviour as INetworkTransport;
        }

        private void OnEnable()
        {
            if (transport != null) transport.FireReceived += HandleFire;
        }

        private void OnDisable()
        {
            if (transport != null) transport.FireReceived -= HandleFire;
        }

        private void HandleFire(NetworkFireEvent fireEvent)
        {
            if (fireEvent == null || fireEvent.playerId == SupabaseSession.UserId) return;

            Vector3 direction = fireEvent.direction.sqrMagnitude > 0.001f ? fireEvent.direction.normalized : Vector3.forward;
            if (muzzleFlashPrefab != null)
            {
                var flash = Instantiate(muzzleFlashPrefab, fireEvent.origin, Quaternion.LookRotation(direction));
                Destroy(flash, effectLifetime);
            }

            if (tracerPrefab != null)
            {
                var tracer = Instantiate(tracerPrefab, fireEvent.origin + direction * (tracerLength * 0.5f), Quaternion.LookRotation(direction));
                Vector3 scale = tracer.transform.localScale;
                scale.z = tracerLength;
                tracer.transform.localScale = scale;
                Destroy(tracer, effectLifetime);
            }

            if (audioSource != null && remoteShotClip != null)
            {
                audioSource.transform.position = fireEvent.origin;
                audioSource.PlayOneShot(remoteShotClip);
            }
        }
    }
}
