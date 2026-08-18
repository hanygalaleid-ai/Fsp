using Fsp.BattleRoyale;
using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class MinimapZonePresenter : MonoBehaviour
    {
        [SerializeField] private SafeZoneController zone;
        [SerializeField] private RectTransform mapRect;
        [SerializeField] private RectTransform currentCircle;
        [SerializeField] private RectTransform nextCircle;
        [SerializeField] private float worldHalfExtent = 1200f;

        private void Awake()
        {
            if (zone == null) zone = FindFirstObjectByType<SafeZoneController>();
        }

        private void OnEnable()
        {
            if (zone != null) zone.ZoneChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (zone != null) zone.ZoneChanged -= Refresh;
        }

        private void Refresh()
        {
            if (zone == null || mapRect == null) return;
            UpdateCircle(currentCircle, zone.Center, zone.CurrentRadius);
            UpdateCircle(nextCircle, zone.TargetCenter, zone.TargetRadius);
        }

        private void UpdateCircle(RectTransform circle, Vector3 center, float radius)
        {
            if (circle == null) return;
            float mapSize = Mathf.Min(mapRect.rect.width, mapRect.rect.height);
            Vector2 normalized = new Vector2(center.x / worldHalfExtent, center.z / worldHalfExtent) * 0.5f;
            circle.anchoredPosition = new Vector2(normalized.x * mapSize, normalized.y * mapSize);
            float diameter = Mathf.Max(2f, radius / worldHalfExtent * mapSize);
            circle.sizeDelta = new Vector2(diameter, diameter);
        }
    }
}
