using UnityEngine;
using UnityEngine.UI;

namespace Fsp.Presentation
{
    public sealed class AppearanceItemCard : MonoBehaviour
    {
        [SerializeField] private Image previewImage;
        [SerializeField] private GameObject ownedBadge;
        [SerializeField] private GameObject lockedBadge;
        [SerializeField] private GameObject equippedBadge;
        [SerializeField] private Button button;

        public CosmeticItemDefinition Item { get; private set; }

        public void Bind(CosmeticItemDefinition item, Sprite preview, bool owned, bool equipped, System.Action<CosmeticItemDefinition> onPressed)
        {
            Item = item;
            if (previewImage != null)
            {
                previewImage.sprite = preview;
                previewImage.enabled = preview != null;
            }
            if (ownedBadge != null) ownedBadge.SetActive(owned && !equipped);
            if (lockedBadge != null) lockedBadge.SetActive(!owned);
            if (equippedBadge != null) equippedBadge.SetActive(equipped);
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onPressed?.Invoke(Item));
            }
        }
    }
}
