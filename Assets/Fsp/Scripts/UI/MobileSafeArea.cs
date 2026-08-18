using UnityEngine;

namespace Fsp.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class MobileSafeArea : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Apply();
        }

        private void OnEnable() => Apply();

        private void Update()
        {
            if (Screen.safeArea != lastSafeArea || Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
                Apply();
        }

        private void Apply()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null || Screen.width <= 0 || Screen.height <= 0) return;

            Rect safe = Screen.safeArea;
            if (safe.width <= 1f || safe.height <= 1f)
                safe = new Rect(0f, 0f, Screen.width, Screen.height);

            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x = Mathf.Clamp01(min.x / Screen.width);
            min.y = Mathf.Clamp01(min.y / Screen.height);
            max.x = Mathf.Clamp01(max.x / Screen.width);
            max.y = Mathf.Clamp01(max.y / Screen.height);

            // Never allow a malformed device safe-area report to collapse the gameplay HUD.
            if (max.x - min.x < 0.55f || max.y - min.y < 0.55f)
            {
                min = Vector2.zero;
                max = Vector2.one;
            }

            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            lastSafeArea = safe;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
