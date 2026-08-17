using UnityEngine;
using UnityEngine.EventSystems;

namespace Fsp.Input
{
    public sealed class LookDragArea : MonoBehaviour, IDragHandler
    {
        [SerializeField, Min(0.01f)] private float sensitivity = 0.12f;

        public void OnDrag(PointerEventData eventData)
        {
            if (MobileInputBridge.Instance == null) return;
            MobileInputBridge.Instance.AddLook(eventData.delta * sensitivity);
        }
    }
}
