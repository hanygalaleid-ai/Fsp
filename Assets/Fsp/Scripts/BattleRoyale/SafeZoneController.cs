using System;
using Fsp.Backend;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class SafeZoneController : MonoBehaviour
    {
        public enum PhaseState { Waiting, Shrinking, Complete }
        [SerializeField] private Transform zoneVisual;
        [SerializeField] private SafeZonePlan plan;
        [SerializeField] private float playableHalfExtent = 1200f;
        public Vector3 Center { get; private set; }
        public float CurrentRadius { get; private set; }
        public Vector3 TargetCenter { get; private set; }
        public float TargetRadius { get; private set; }
        public int PhaseIndex { get; private set; } = -1;
        public PhaseState State { get; private set; }
        public float StateTimeRemaining { get; private set; }
        public float CurrentDamagePerSecond { get; private set; }
        public event Action<float> RadiusChanged;
        public event Action ZoneChanged;
        public event Action<int, PhaseState, float> PhaseChanged;
        private Vector3 phaseStartCenter;
        private float phaseStartRadius;
        private float stateDuration;
        private bool initialized;
        private void OnEnable() { TryInitialize(); }
        public void ConfigurePlan(SafeZonePlan value, Transform visual = null) { plan = value; if (visual != null) zoneVisual = visual; if (isActiveAndEnabled) TryInitialize(true); }
        private void TryInitialize(bool force = false) { if (plan == null || (initialized && !force)) return; initialized = true; PhaseIndex = -1; Center = Flatten(transform.position); CurrentRadius = plan.initialRadius; TargetCenter = Center; TargetRadius = CurrentRadius; State = PhaseState.Waiting; RefreshVisual(); StartNextPhase(); }
        private void Update()
        {
            if (!initialized || plan == null || State == PhaseState.Complete) return;
            StateTimeRemaining = Mathf.Max(0f, StateTimeRemaining - Time.deltaTime);
            if (State == PhaseState.Shrinking && stateDuration > 0f) { float t = 1f - StateTimeRemaining / stateDuration; float eased = Mathf.SmoothStep(0f, 1f, t); Center = Vector3.Lerp(phaseStartCenter, TargetCenter, eased); CurrentRadius = Mathf.Lerp(phaseStartRadius, TargetRadius, eased); transform.position = Center; RefreshVisual(); RadiusChanged?.Invoke(CurrentRadius); ZoneChanged?.Invoke(); }
            if (StateTimeRemaining > 0f) return;
            if (State == PhaseState.Waiting) BeginShrink(); else { Center = TargetCenter; CurrentRadius = TargetRadius; transform.position = Center; RefreshVisual(); RadiusChanged?.Invoke(CurrentRadius); ZoneChanged?.Invoke(); StartNextPhase(); }
        }
        public bool IsInside(Vector3 worldPosition) { Vector2 delta = new(worldPosition.x - Center.x, worldPosition.z - Center.z); return delta.sqrMagnitude <= CurrentRadius * CurrentRadius; }
        public float OutsideDamagePerSecond(Vector3 worldPosition) => IsInside(worldPosition) ? 0f : CurrentDamagePerSecond;
        private void StartNextPhase()
        {
            PhaseIndex++;
            if (plan.phases == null || PhaseIndex >= plan.phases.Length) { State = PhaseState.Complete; StateTimeRemaining = 0f; PhaseChanged?.Invoke(PhaseIndex, State, 0f); return; }
            SafeZonePlan.Phase phase = plan.phases[PhaseIndex]; phaseStartCenter = Center; phaseStartRadius = CurrentRadius; TargetRadius = Mathf.Max(12f, CurrentRadius * Mathf.Clamp(phase.radiusFactor, 0.05f, 1f)); TargetCenter = PickNextCenter(Center, CurrentRadius, TargetRadius, phase.centerShiftFactor, PhaseIndex); CurrentDamagePerSecond = phase.damagePerSecond; State = PhaseState.Waiting; stateDuration = Mathf.Max(0f, phase.waitSeconds); StateTimeRemaining = stateDuration; PhaseChanged?.Invoke(PhaseIndex, State, StateTimeRemaining); ZoneChanged?.Invoke();
        }
        private void BeginShrink() { SafeZonePlan.Phase phase = plan.phases[PhaseIndex]; phaseStartCenter = Center; phaseStartRadius = CurrentRadius; State = PhaseState.Shrinking; stateDuration = Mathf.Max(1f, phase.shrinkSeconds); StateTimeRemaining = stateDuration; PhaseChanged?.Invoke(PhaseIndex, State, StateTimeRemaining); }
        private Vector3 PickNextCenter(Vector3 current, float currentRadius, float nextRadius, float shiftFactor, int phaseIndex)
        {
            string matchId = MatchRoomState.HasMatch ? MatchRoomState.MatchId : "offline";
            var random = new System.Random(StableHash(matchId + ":zone:" + phaseIndex)); double angle = random.NextDouble() * Math.PI * 2.0; float maxShift = Mathf.Max(0f, currentRadius - nextRadius) * Mathf.Clamp01(shiftFactor); float distance = maxShift * (0.35f + 0.65f * (float)random.NextDouble()); Vector3 offset = new(Mathf.Cos((float)angle) * distance, 0f, Mathf.Sin((float)angle) * distance); Vector3 candidate = Flatten(current + offset); candidate.x = Mathf.Clamp(candidate.x, -playableHalfExtent + nextRadius, playableHalfExtent - nextRadius); candidate.z = Mathf.Clamp(candidate.z, -playableHalfExtent + nextRadius, playableHalfExtent - nextRadius); return candidate;
        }
        private void RefreshVisual() { if (zoneVisual == null) return; float diameter = CurrentRadius * 2f; zoneVisual.position = Center; zoneVisual.localScale = new Vector3(diameter, zoneVisual.localScale.y, diameter); }
        private static Vector3 Flatten(Vector3 v) => new(v.x, 0f, v.z);
        private static int StableHash(string value) { unchecked { int hash = 23; foreach (char c in value ?? string.Empty) hash = hash * 31 + c; return hash; } }
    }
}
