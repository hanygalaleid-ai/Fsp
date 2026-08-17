using Fsp.BattleRoyale;
using Fsp.Backend;
using UnityEngine;

namespace Fsp.AI
{
    public sealed class BotDropController : MonoBehaviour
    {
        [SerializeField] private DropPlanePassenger passenger;
        [SerializeField] private ParachuteController parachute;
        [SerializeField] private BotMotor motor;
        [SerializeField] private BotBrain brain;
        [SerializeField] private Transform[] dropTargets;
        [SerializeField] private float jumpTriggerDistance = 170f;
        [SerializeField] private float parachuteOpenHeight = 55f;
        [SerializeField] private LayerMask groundMask = ~0;

        private Vector3 target;
        private bool targetChosen;
        private bool landed;

        private void Awake()
        {
            if (passenger == null) passenger = GetComponent<DropPlanePassenger>();
            if (parachute == null) parachute = GetComponent<ParachuteController>();
            if (motor == null) motor = GetComponent<BotMotor>();
            if (brain == null) brain = GetComponent<BotBrain>();
        }

        private void Start()
        {
            ChooseTarget();
            if (motor != null) motor.enabled = false;
            if (brain != null) brain.enabled = false;
        }

        private void Update()
        {
            if (landed) return;
            if (!targetChosen) ChooseTarget();

            if (passenger != null && passenger.IsAboard)
            {
                Vector3 flat = target - transform.position;
                flat.y = 0f;
                if (flat.sqrMagnitude <= jumpTriggerDistance * jumpTriggerDistance)
                    passenger.Jump();
                return;
            }

            if (parachute != null && parachute.IsActive)
            {
                Vector3 delta = target - transform.position;
                delta.y = 0f;
                Vector2 steer = delta.sqrMagnitude > 1f
                    ? new Vector2(Vector3.Dot(delta.normalized, transform.right), Vector3.Dot(delta.normalized, transform.forward))
                    : Vector2.zero;
                parachute.SetSteer(steer);

                if (!parachute.IsOpen && Physics.Raycast(transform.position, Vector3.down, out _, parachuteOpenHeight, groundMask, QueryTriggerInteraction.Ignore))
                    parachute.OpenParachute();
                return;
            }

            if (passenger != null && passenger.HasJumped)
                FinishLanding();
        }

        private void FinishLanding()
        {
            if (landed) return;
            landed = true;
            if (motor != null) motor.enabled = true;
            if (brain != null) brain.enabled = true;
            enabled = false;
        }

        private void ChooseTarget()
        {
            if (dropTargets == null || dropTargets.Length == 0)
            {
                target = transform.position + transform.forward * 320f;
                targetChosen = true;
                return;
            }

            string seedText = (MatchRoomState.HasMatch ? MatchRoomState.MatchId : "offline") + ":botdrop:" + gameObject.GetInstanceID();
            var random = new System.Random(StableHash(seedText));
            Transform chosen = dropTargets[random.Next(0, dropTargets.Length)];
            if (chosen == null) return;
            float angle = (float)(random.NextDouble() * Mathf.PI * 2f);
            float radius = 12f + (float)random.NextDouble() * 33f;
            target = chosen.position + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            targetChosen = true;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in value ?? string.Empty) hash = hash * 31 + c;
                return hash;
            }
        }
    }
}
