using UnityEngine;

namespace Fsp.UI
{
    public sealed class MiniMapFollower : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float height = 60f;
        [SerializeField] private bool rotateWithTarget = false;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 p = target.position;
            transform.position = new Vector3(p.x, p.y + height, p.z);

            float yaw = rotateWithTarget ? target.eulerAngles.y : 0f;
            transform.rotation = Quaternion.Euler(90f, yaw, 0f);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
