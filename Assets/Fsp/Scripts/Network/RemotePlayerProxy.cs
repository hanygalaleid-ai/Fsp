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
        private bool eliminated;
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
            eliminated = false;

            NetworkPlayerIdentity identity = GetComponent<NetworkPlayerIdentity>();
            if (identity == null) identity = gameObject.AddComponent<NetworkPlayerIdentity>();
            identity.Initialize(playerId, false);

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
            DropState = snapshot.dropState;

            if (eliminated) return;

            IsAlive = snapshot.alive;
            if (!snapshot.alive)
            {
                MarkEliminated();
                return;
            }

            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (parachuteVisual != null)
                parachuteVisual.SetActive(snapshot.dropState == NetworkDropState.Parachute);
            if (bodyVisual != null) bodyVisual.SetActive(true);
        }

        public void MarkEliminated()
        {
            eliminated = true;
            IsAlive = false;
            Health = 0f;
            seated = false;
            if (parachuteVisual != null) parachuteVisual.SetActive(false);
            if (bodyVisual != null) bodyVisual.SetActive(false);
            gameObject.SetActive(false);
        }

        public void SetVehicleSeat(Transform seatPoint, bool isSeated)
        {
            if (eliminated) return;
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
            if (!initialized || seated || eliminated) return;
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
