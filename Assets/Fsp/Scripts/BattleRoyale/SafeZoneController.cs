using System;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class SafeZoneController : MonoBehaviour
    {
        [SerializeField] private Transform zoneVisual;
        [SerializeField] private float startRadius = 500f;
        [SerializeField] private float endRadius = 35f;
        [SerializeField] private float shrinkDelay = 90f;
        [SerializeField] private float shrinkDuration = 720f;
        [SerializeField] private float damagePerSecondOutside = 4f;

        private float startedAt;
        private float currentRadius;

        public float CurrentRadius => currentRadius;
        public Vector3 Center => transform.position;
        public event Action<float> RadiusChanged;

        private void OnEnable()
        {
            startedAt = Time.time;
            currentRadius = startRadius;
            RefreshVisual();
        }

        private void Update()
        {
            float elapsed = Time.time - startedAt;
            if (elapsed <= shrinkDelay) return;

            float t = Mathf.Clamp01((elapsed - shrinkDelay) / Mathf.Max(1f, shrinkDuration));
            float nextRadius = Mathf.Lerp(startRadius, endRadius, t);
            if (Mathf.Abs(nextRadius - currentRadius) < 0.05f) return;

            currentRadius = nextRadius;
            RefreshVisual();
            RadiusChanged?.Invoke(currentRadius);
        }

        public bool IsInside(Vector3 worldPosition)
        {
            Vector2 delta = new Vector2(worldPosition.x - Center.x, worldPosition.z - Center.z);
            return delta.sqrMagnitude <= currentRadius * currentRadius;
        }

        public float OutsideDamagePerSecond(Vector3 worldPosition)
        {
            return IsInside(worldPosition) ? 0f : damagePerSecondOutside;
        }

        private void RefreshVisual()
        {
            if (zoneVisual == null) return;
            float diameter = currentRadius * 2f;
            zoneVisual.localScale = new Vector3(diameter, zoneVisual.localScale.y, diameter);
        }
    }
}
