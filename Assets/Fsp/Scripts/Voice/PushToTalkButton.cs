using UnityEngine;
using UnityEngine.EventSystems;
using Fsp.Audio;

namespace Fsp.Voice
{
    public sealed class PushToTalkButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            FspAudioRuntime.PlayActionTap();
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
