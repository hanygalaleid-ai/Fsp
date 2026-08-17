using UnityEngine;
using UnityEngine.EventSystems;

namespace Fsp.Input
{
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField, Range(0.1f, 1f)] private float handleRange = 0.65f;

        private Camera uiCamera;

        private void Awake()
        {
            if (background == null) background = transform as RectTransform;
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                uiCamera = canvas.worldCamera;
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || MobileInputBridge.Instance == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, uiCamera, out Vector2 local))
                return;

            Vector2 half = background.rect.size * 0.5f;
            Vector2 normalized = new Vector2(
                half.x > 0f ? local.x / half.x : 0f,
                half.y > 0f ? local.y / half.y : 0f);
            normalized = Vector2.ClampMagnitude(normalized, 1f);

            MobileInputBridge.Instance.SetMove(normalized);
            if (handle != null)
                handle.anchoredPosition = new Vector2(normalized.x * half.x, normalized.y * half.y) * handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            MobileInputBridge.Instance?.SetMove(Vector2.zero);
            if (handle != null) handle.anchoredPosition = Vector2.zero;
        }
    }
}
