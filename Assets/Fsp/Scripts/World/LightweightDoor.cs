using UnityEngine;

namespace Fsp.World
{
    public sealed class LightweightDoor : MonoBehaviour
    {
        [SerializeField] private float openAngle = 95f;
        [SerializeField] private float speed = 260f;
        [SerializeField] private bool startsOpen;

        private Quaternion closedRotation;
        private Quaternion openRotation;
        private bool open;

        public bool IsOpen => open;

        private void Awake()
        {
            closedRotation = transform.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
            open = startsOpen;
            if (open) transform.localRotation = openRotation;
        }

        private void Update()
        {
            Quaternion target = open ? openRotation : closedRotation;
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, target, speed * Time.deltaTime);
        }

        public void Toggle() => open = !open;
    }
}
