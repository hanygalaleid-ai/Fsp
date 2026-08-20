using System;
using Fsp.BattleRoyale;
using Fsp.Networking;
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
        private MatchManager matchManager;
        private Transform target;
        private float nextShot;
        private float nextScan;
        private float verticalVelocity;

        private static bool localDropReleased;
        private static float nextDropCheck;
        private static float nextSharedTargetRefresh;
        private static MatchParticipant[] sharedParticipants = Array.Empty<MatchParticipant>();
        private static RemotePlayerProxy[] sharedRemotes = Array.Empty<RemotePlayerProxy>();

        public event Action<FallbackBotAgent, string, float, Vector3> NetworkPlayerHit;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStartupGate() => ResetForNewMatch();

        public static void ResetForNewMatch()
        {
            localDropReleased = false;
            nextDropCheck = 0f;
            nextSharedTargetRefresh = 0f;
            sharedParticipants = Array.Empty<MatchParticipant>();
            sharedRemotes = Array.Empty<RemotePlayerProxy>();
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            vitals = GetComponent<PlayerVitals>();
            matchManager = FindFirstObjectByType<MatchManager>();
        }

        private void Update()
        {
            if (vitals != null && !vitals.IsAlive) return;
            if (!CombatMayStart()) return;

            if (Time.time >= nextScan)
            {
                nextScan = Time.time + 0.45f + UnityEngine.Random.Range(0f, 0.18f);
                AcquireTarget();
            }

            Vector3 planar = Vector3.zero;
            if (target != null)
            {
                if (!TargetIsAlive(target))
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
                        if (UnityEngine.Random.value <= 0.72f)
                            ApplyAttack(target);
                    }
                }
            }

            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity -= 22f * Time.deltaTime;
            controller.Move((planar * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        private void ApplyAttack(Transform victim)
        {
            if (victim == null) return;

            NetworkPlayerIdentity identity = victim.GetComponentInParent<NetworkPlayerIdentity>();
            if (identity != null && !string.IsNullOrWhiteSpace(identity.PlayerId) && NetworkPlayerHit != null)
            {
                NetworkPlayerHit.Invoke(this, identity.PlayerId, damage, victim.position + Vector3.up);
                return;
            }

            PlayerDamageable damageable = victim.GetComponent<PlayerDamageable>();
            if (damageable != null)
                damageable.ApplyDamage(damage, victim.position + Vector3.up, Vector3.up, gameObject);
        }

        private static bool TargetIsAlive(Transform candidate)
        {
            if (candidate == null || !candidate.gameObject.activeInHierarchy) return false;

            MatchParticipant participant = candidate.GetComponent<MatchParticipant>();
            if (participant != null) return participant.IsAlive;

            RemotePlayerProxy remote = candidate.GetComponent<RemotePlayerProxy>();
            return remote != null && remote.IsAlive;
        }

        private bool CombatMayStart()
        {
            if (matchManager == null) matchManager = FindFirstObjectByType<MatchManager>();
            if (matchManager == null || matchManager.Phase != MatchManager.MatchPhase.Active) return false;
            if (localDropReleased) return true;

            if (Time.unscaledTime < nextDropCheck) return false;
            nextDropCheck = Time.unscaledTime + 0.25f;

            RefreshSharedTargetsIfNeeded();
            foreach (MatchParticipant participant in sharedParticipants)
            {
                if (participant == null || !participant.IsLocalPlayer) continue;
                DropPlanePassenger passenger = participant.GetComponent<DropPlanePassenger>();
                if (passenger != null && passenger.IsAboard) return false;
                localDropReleased = true;
                return true;
            }

            return false;
        }

        private void AcquireTarget()
        {
            RefreshSharedTargetsIfNeeded();
            float best = detectionRange * detectionRange;
            Transform bestTarget = null;

            foreach (MatchParticipant participant in sharedParticipants)
            {
                if (participant == null || participant.gameObject == gameObject || !participant.IsAlive || participant.IsBot) continue;
                float sqr = (participant.transform.position - transform.position).sqrMagnitude;
                if (sqr >= best) continue;
                best = sqr;
                bestTarget = participant.transform;
            }

            foreach (RemotePlayerProxy remote in sharedRemotes)
            {
                if (remote == null || !remote.IsAlive || !remote.gameObject.activeInHierarchy) continue;
                float sqr = (remote.transform.position - transform.position).sqrMagnitude;
                if (sqr >= best) continue;
                best = sqr;
                bestTarget = remote.transform;
            }

            target = bestTarget;
        }

        private static void RefreshSharedTargetsIfNeeded()
        {
            if (Time.unscaledTime < nextSharedTargetRefresh) return;
            nextSharedTargetRefresh = Time.unscaledTime + 0.35f;
            sharedParticipants = FindObjectsByType<MatchParticipant>(FindObjectsSortMode.None);
            sharedRemotes = FindObjectsByType<RemotePlayerProxy>(FindObjectsSortMode.None);
        }
    }
}
