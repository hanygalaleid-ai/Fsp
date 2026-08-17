using Fsp.Bots;
using Fsp.Lobby;
using Fsp.Player;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    /// <summary>
    /// Creates a lightweight playable fallback when final art prefabs are not assigned yet.
    /// Final prefabs can replace these objects without changing the gameplay systems.
    /// </summary>
    public sealed class MatchSceneAssembler : MonoBehaviour
    {
        [SerializeField] private bool createFallbackPlayer = true;
        [SerializeField] private bool createFallbackPopulation = true;
        [SerializeField] private bool createFallbackSafeZone = true;

        private void Awake()
        {
            EnsureMatchManager();
            if (createFallbackSafeZone) EnsureSafeZone();
            if (createFallbackPlayer) EnsureLocalPlayer();
            if (createFallbackPopulation) EnsurePopulation();
        }

        private static void EnsureMatchManager()
        {
            if (FindObjectOfType<MatchManager>() != null) return;
            new GameObject("MatchManager").AddComponent<MatchManager>();
        }

        private static void EnsureLocalPlayer()
        {
            MatchParticipant[] participants = FindObjectsOfType<MatchParticipant>();
            foreach (MatchParticipant participant in participants)
                if (participant != null && participant.IsLocalPlayer) return;

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "LocalPlayer_Placeholder";
            player.transform.position = new Vector3(0f, 1.05f, 0f);

            Collider primitiveCollider = player.GetComponent<Collider>();
            if (primitiveCollider != null) Destroy(primitiveCollider);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            player.AddComponent<PlayerVitals>();
            var participant = player.AddComponent<MatchParticipant>();
            string displayName = LobbyState.Instance != null ? LobbyState.Instance.DisplayName : "Player";
            participant.ConfigureAsLocalPlayer(displayName);
            player.AddComponent<ThirdPersonMotor>();
        }

        private static void EnsurePopulation()
        {
            BotSpawner spawner = FindObjectOfType<BotSpawner>();
            if (spawner == null)
            {
                var go = new GameObject("BotPopulation");
                spawner = go.AddComponent<BotSpawner>();
                go.AddComponent<MatchPopulationBootstrap>();
            }
            else if (spawner.GetComponent<MatchPopulationBootstrap>() == null)
            {
                spawner.gameObject.AddComponent<MatchPopulationBootstrap>();
            }
        }

        private static void EnsureSafeZone()
        {
            if (FindObjectOfType<SafeZoneController>() != null) return;

            var root = new GameObject("SafeZone");
            root.transform.position = Vector3.zero;
            var controller = root.AddComponent<SafeZoneController>();

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "SafeZoneVisual_Placeholder";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            visual.transform.localScale = new Vector3(1f, 0.01f, 1f);
            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null) Destroy(visualCollider);

            var plan = ScriptableObject.CreateInstance<SafeZonePlan>();
            plan.hideFlags = HideFlags.DontSave;
            controller.ConfigurePlan(plan, visual.transform);
        }
    }
}
