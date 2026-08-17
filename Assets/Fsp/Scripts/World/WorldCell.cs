using UnityEngine;

namespace Fsp.World
{
    public sealed class WorldCell : MonoBehaviour
    {
        [SerializeField] private Vector2Int coordinates;
        [SerializeField] private GameObject highDetailRoot;
        [SerializeField] private GameObject lowDetailRoot;
        [SerializeField] private Collider[] gameplayColliders;

        public Vector2Int Coordinates => coordinates;

        public void SetState(bool active, bool preloaded)
        {
            if (highDetailRoot != null) highDetailRoot.SetActive(active);
            if (lowDetailRoot != null) lowDetailRoot.SetActive(!active && preloaded);
            if (gameplayColliders != null)
            {
                for (int i = 0; i < gameplayColliders.Length; i++)
                    if (gameplayColliders[i] != null) gameplayColliders[i].enabled = active;
            }
            gameObject.SetActive(active || preloaded);
        }
    }
}
