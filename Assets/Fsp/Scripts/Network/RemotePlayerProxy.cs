using System.Collections.Generic;
using UnityEngine;

namespace Fsp.Networking
{
    public sealed class RemotePlayerProxy : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float positionLerp = 12f;
        [SerializeField, Min(1f)] private float rotationLerp = 14f;
        [SerializeField] private GameObject parachuteVisual;
        [SerializeField] private GameObject bodyVisual;

        private static readonly Dictionary<string, RemotePlayerProxy> registry = new();
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private bool initialized;
        private bool seated;
        private Transform worldParent;

        public string PlayerId { get; private set; }
        public float Health { get; private set; } = 100f;
        public float Armor { get; private set; }
        public bool IsAlive { get; private set; } = true;
        public NetworkDropState DropState { get; private set; } = NetworkDropState.Grounded;

        public static bool TryFind(string playerId, out RemotePlayerProxy proxy) => registry.TryGetValue(playerId, out proxy) && proxy != null;

        public void Initialize(string playerId)
        {
            PlayerId = playerId;
            targetPosition = transform.position;
            targetRotation = transform.rotation;
            worldParent = transform.parent;
            initialized = true;
            if (parachuteVisual != null) parachuteVisual.SetActive(false);
            if (!string.IsNullOrWhiteSpace(playerId)) registry[playerId] = this;
        }

        public void Apply(NetworkPlayerSnapshot snapshot)
        {
            if (!initialized) Initialize(snapshot.playerId);
            targetPosition = snapshot.position;
            targetRotation = snapshot.rotation;
            Health = snapshot.health;
            Armor = snapshot.armor;
            IsAlive = snapshot.alive;
            DropState = snapshot.dropState;
            gameObject.SetActive(snapshot.alive);

            if (parachuteVisual != null)
                parachuteVisual.SetActive(snapshot.alive && snapshot.dropState == NetworkDropState.Parachute);

            if (bodyVisual != null)
                bodyVisual.SetActive(snapshot.alive);
        }

        public void SetVehicleSeat(Transform seatPoint, bool isSeated)
        {
            seated = isSeated && seatPoint != null;
            if (seated)
            {
                transform.SetParent(seatPoint, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.SetParent(worldParent, true);
            }
        }

        private void Update()
        {
            if (!initialized || seated) return;
            transform.position = Vector3.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-positionLerp * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-rotationLerp * Time.deltaTime));
        }

        private void OnDestroy()
        {
            if (!string.IsNullOrWhiteSpace(PlayerId) && registry.TryGetValue(PlayerId, out var current) && current == this)
                registry.Remove(PlayerId);
        }
    }
}
