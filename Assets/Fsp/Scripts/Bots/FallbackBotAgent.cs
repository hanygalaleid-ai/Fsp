using Fsp.BattleRoyale;
using Fsp.Player;
using UnityEngine;

namespace Fsp.Bots
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FallbackBotAgent : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.8f;
        [SerializeField] private float attackRange = 18f;
        [SerializeField] private float detectionRange = 65f;
        [SerializeField] private float damage = 12f;
        [SerializeField] private float shotsPerSecond = 1.8f;
        [SerializeField] private float turnSpeed = 8f;

        private CharacterController controller;
        private PlayerVitals vitals;
        private Transform target;
        private float nextShot;
        private float nextScan;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            vitals = GetComponent<PlayerVitals>();
        }

        private void Update()
        {
            if (vitals != null && !vitals.IsAlive) return;

            if (Time.time >= nextScan)
            {
                nextScan = Time.time + 0.4f + Random.Range(0f, 0.12f);
                AcquireTarget();
            }

            Vector3 planar = Vector3.zero;
            if (target != null)
            {
                PlayerVitals targetVitals = target.GetComponent<PlayerVitals>();
                if (targetVitals == null || !targetVitals.IsAlive)
                {
                    target = null;
                }
                else
                {
                    Vector3 delta = target.position - transform.position;
                    delta.y = 0f;
                    float distance = delta.magnitude;
                    if (distance > 0.1f)
                    {
                        Quaternion desired = Quaternion.LookRotation(delta.normalized, Vector3.up);
                        transform.rotation = Quaternion.Slerp(transform.rotation, desired, turnSpeed * Time.deltaTime);
                    }

                    if (distance > attackRange)
                    {
                        planar = delta.normalized;
                    }
                    else if (Time.time >= nextShot)
                    {
                        nextShot = Time.time + 1f / Mathf.Max(0.1f, shotsPerSecond);
                        if (Random.value <= 0.72f)
                        {
                            PlayerDamageable damageable = target.GetComponent<PlayerDamageable>();
                            if (damageable != null)
                                damageable.ApplyDamage(damage, target.position + Vector3.up, Vector3.up, gameObject);
                        }
                    }
                }
            }

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity -= 22f * Time.deltaTime;
            controller.Move((planar * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        private void AcquireTarget()
        {
            MatchParticipant[] participants = FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None);
            float best = detectionRange * detectionRange;
            Transform bestTarget = null;

            foreach (MatchParticipant participant in participants)
            {
                if (participant == null || participant.gameObject == gameObject || !participant.IsAlive) continue;
                float sqr = (participant.transform.position - transform.position).sqrMagnitude;
                if (sqr >= best) continue;
                best = sqr;
                bestTarget = participant.transform;
            }

            target = bestTarget;
        }
    }
}
