using UnityEngine;
using UnityEngine.EventSystems;

namespace Fsp.Voice
{
    public sealed class PushToTalkButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            SquadVoiceState.Instance?.BeginTalking();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SquadVoiceState.Instance?.EndTalking();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SquadVoiceState.Instance?.EndTalking();
        }

        private void OnDisable()
        {
            SquadVoiceState.Instance?.EndTalking();
        }
    }
}
