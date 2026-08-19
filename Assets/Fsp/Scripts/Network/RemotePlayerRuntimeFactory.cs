using UnityEngine;

namespace Fsp.Networking
{
    /// <summary>
    /// Test/early-production fallback that reuses the authored local character visuals when
    /// a dedicated remote-player prefab has not been assigned yet. Gameplay/input/network
    /// behaviours are disabled on the visual clone before it is activated.
    /// </summary>
    public static class RemotePlayerRuntimeFactory
    {
        public static RemotePlayerProxy CreateFromLocalVisual(Transform localPlayer, Vector3 position, Quaternion rotation)
        {
            if (localPlayer == null) return null;

            var holder = new GameObject("RemotePlayer_Runtime");
            holder.SetActive(false);
            holder.transform.SetPositionAndRotation(position, rotation);

            GameObject visual = Object.Instantiate(localPlayer.gameObject, holder.transform, false);
            visual.name = "Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            foreach (MonoBehaviour behaviour in visual.GetComponentsInChildren<MonoBehaviour>(true))
                behaviour.enabled = false;

            foreach (Camera camera in visual.GetComponentsInChildren<Camera>(true))
                camera.enabled = false;

            foreach (AudioListener listener in visual.GetComponentsInChildren<AudioListener>(true))
                listener.enabled = false;

            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;

            foreach (CharacterController controller in visual.GetComponentsInChildren<CharacterController>(true))
                controller.enabled = false;

            foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>(true))
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
                body.detectCollisions = false;
            }

            foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>(true))
                renderer.enabled = true;

            // The visual clone stays non-physical, but the remote root gets one stable hit volume
            // so local hitscan weapons can resolve NetworkPlayerIdentity and report network damage.
            CapsuleCollider hitCapsule = holder.AddComponent<CapsuleCollider>();
            hitCapsule.center = new Vector3(0f, 0.9f, 0f);
            hitCapsule.height = 1.8f;
            hitCapsule.radius = 0.38f;
            hitCapsule.direction = 1;
            hitCapsule.isTrigger = false;

            var proxy = holder.AddComponent<RemotePlayerProxy>();
            holder.SetActive(true);
            return proxy;
        }
    }
}
