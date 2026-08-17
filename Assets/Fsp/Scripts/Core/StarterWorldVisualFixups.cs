using Fsp.BattleRoyale;
using Fsp.Presentation;
using Fsp.World;
using UnityEngine;

namespace Fsp.Core
{
    public static class StarterWorldVisualFixups
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Apply()
        {
            DropPlaneController plane = Object.FindObjectOfType<DropPlaneController>();
            if (plane != null && plane.GetComponent<StarterPlaneVisual>() == null)
            {
                Renderer baseRenderer = plane.GetComponent<Renderer>();
                if (baseRenderer != null) baseRenderer.enabled = false;
                plane.gameObject.AddComponent<StarterPlaneVisual>();
            }

            foreach (ParachuteController parachute in Object.FindObjectsOfType<ParachuteController>())
            {
                if (parachute == null || parachute.GetComponent<StarterParachuteVisual>() != null) continue;
                parachute.gameObject.AddComponent<StarterParachuteVisual>();
            }

            if (Object.FindObjectOfType<StarterOldCrownEnvironment>() == null)
            {
                GameObject world = new GameObject("OldCrown_Prototype");
                world.transform.position = Vector3.zero;
                world.AddComponent<StarterOldCrownEnvironment>();
            }
        }
    }
}
