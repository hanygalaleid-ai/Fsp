using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class HudScaleController : MonoBehaviour
    {
        [SerializeField] private RectTransform hudRoot;
        [SerializeField, Range(0.8f, 1.2f)] private float scale = 1f;
        private const string PrefKey = "fsp.hud.scale";

        private void Awake()
        {
            scale = Mathf.Clamp(PlayerPrefs.GetFloat(PrefKey, scale), 0.8f, 1.2f);
            Apply();
        }

        public void SetScale(float value)
        {
            scale = Mathf.Clamp(value, 0.8f, 1.2f);
            PlayerPrefs.SetFloat(PrefKey, scale);
            Apply();
        }

        public float GetScale() => scale;

        private void Apply()
        {
            if (hudRoot != null) hudRoot.localScale = Vector3.one * scale;
        }
    }
}
