using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class HudAdaptiveVisibility : MonoBehaviour
    {
        [Header("Context groups")]
        [SerializeField] private CanvasGroup interactionGroup;
        [SerializeField] private CanvasGroup backpackGroup;
        [SerializeField] private CanvasGroup vehicleGroup;
        [SerializeField] private CanvasGroup dropGroup;
        [SerializeField] private float fadeSpeed = 9f;

        private bool interactionVisible;
        private bool backpackVisible = true;
        private bool vehicleVisible;
        private bool dropVisible;

        private void Update()
        {
            Fade(interactionGroup, interactionVisible);
            Fade(backpackGroup, backpackVisible);
            Fade(vehicleGroup, vehicleVisible);
            Fade(dropGroup, dropVisible);
        }

        public void SetInteractionAvailable(bool value) => interactionVisible = value;
        public void SetBackpackAvailable(bool value) => backpackVisible = value;
        public void SetVehicleMode(bool value) => vehicleVisible = value;
        public void SetDropMode(bool value) => dropVisible = value;

        private void Fade(CanvasGroup group, bool visible)
        {
            if (group == null) return;
            float target = visible ? 1f : 0f;
            group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.unscaledDeltaTime);
            bool enabled = group.alpha > 0.01f;
            group.interactable = visible && enabled;
            group.blocksRaycasts = visible && enabled;
        }
    }
}
