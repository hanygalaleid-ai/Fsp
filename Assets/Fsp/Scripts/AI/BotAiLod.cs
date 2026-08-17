using UnityEngine;

namespace Fsp.AI
{
    public sealed class BotAiLod : MonoBehaviour
    {
        [SerializeField] private Transform viewer;
        [SerializeField] private BotBrain brain;
        [SerializeField] private BotPerception perception;
        [SerializeField] private float fullAiDistance = 90f;
        [SerializeField] private float reducedAiDistance = 180f;
        [SerializeField] private float checkInterval = 1f;

        private float nextCheck;

        private void Awake()
        {
            if (brain == null) brain = GetComponent<BotBrain>();
            if (perception == null) perception = GetComponent<BotPerception>();
            if (viewer == null && Camera.main != null) viewer = Camera.main.transform;
        }

        private void Update()
        {
            if (Time.time < nextCheck) return;
            nextCheck = Time.time + checkInterval + Random.Range(0f, 0.2f);
            if (viewer == null) return;

            float distance = Vector3.Distance(transform.position, viewer.position);
            bool full = distance <= fullAiDistance;
            bool reduced = distance <= reducedAiDistance;

            if (brain != null) brain.enabled = reduced;
            if (perception != null) perception.enabled = full;
        }
    }
}
