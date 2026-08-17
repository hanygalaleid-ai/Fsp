using UnityEngine;
using UnityEngine.UI;

namespace Fsp.UI
{
    public sealed class CrosshairController : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image[] lines;
        [SerializeField, Min(0f)] private float normalGap = 10f;
        [SerializeField, Min(0f)] private float firingGap = 20f;
        [SerializeField, Min(0f)] private float smoothing = 14f;

        private float targetGap;
        private float currentGap;

        private void Awake()
        {
            targetGap = currentGap = normalGap;
            ApplyGap(currentGap);
        }

        private void Update()
        {
            currentGap = Mathf.Lerp(currentGap, targetGap, 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime));
            ApplyGap(currentGap);
        }

        public void SetFiring(bool firing)
        {
            targetGap = firing ? firingGap : normalGap;
        }

        public void SetVisible(bool visible)
        {
            if (root != null) root.gameObject.SetActive(visible);
        }

        private void ApplyGap(float gap)
        {
            if (lines == null || lines.Length < 4) return;
            if (lines[0] != null) lines[0].rectTransform.anchoredPosition = new Vector2(-gap, 0f);
            if (lines[1] != null) lines[1].rectTransform.anchoredPosition = new Vector2(gap, 0f);
            if (lines[2] != null) lines[2].rectTransform.anchoredPosition = new Vector2(0f, gap);
            if (lines[3] != null) lines[3].rectTransform.anchoredPosition = new Vector2(0f, -gap);
        }
    }
}
