using UnityEngine;

namespace Fsp.Networking
{
    public sealed class RemotePlayerProxy : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float positionLerp = 12f;
        [SerializeField, Min(1f)] private float rotationLerp = 14f;

        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private bool initialized;

        public string PlayerId { get; private set; }
        public float Health { get; private set; } = 100f;
        public float Armor { get; private set; }
        public bool IsAlive { get; private set; } = true;

        public void Initialize(string playerId)
        {
            PlayerId = playerId;
            targetPosition = transform.position;
            targetRotation = transform.rotation;
            initialized = true;
        }

        public void Apply(NetworkPlayerSnapshot snapshot)
        {
            if (!initialized) Initialize(snapshot.playerId);
            targetPosition = snapshot.position;
            targetRotation = snapshot.rotation;
            Health = snapshot.health;
            Armor = snapshot.armor;
            IsAlive = snapshot.alive;
            gameObject.SetActive(snapshot.alive);
        }

        private void Update()
        {
            if (!initialized) return;
            transform.position = Vector3.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-positionLerp * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-rotationLerp * Time.deltaTime));
        }
    }
}
