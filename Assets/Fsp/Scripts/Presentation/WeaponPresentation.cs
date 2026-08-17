using Fsp.Combat;
using UnityEngine;

namespace Fsp.Presentation
{
    public sealed class WeaponPresentation : MonoBehaviour
    {
        [SerializeField] private HitscanWeapon weapon;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private ParticleSystem shellEject;
        [SerializeField] private AudioSource shotAudio;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform recoilRoot;
        [SerializeField] private Vector3 recoilPosition = new(0f, 0f, -0.035f);
        [SerializeField] private Vector3 recoilEuler = new(-2.2f, 0.6f, 0f);
        [SerializeField, Min(1f)] private float recoilReturn = 18f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private Vector3 recoilOffset;
        private Vector3 recoilAngles;

        private static readonly int Fire = Animator.StringToHash("Fire");
        private static readonly int Reload = Animator.StringToHash("Reload");
        private static readonly int Aim = Animator.StringToHash("Aim");

        private void Awake()
        {
            if (weapon == null) weapon = GetComponentInParent<HitscanWeapon>();
            if (recoilRoot != null)
            {
                baseLocalPosition = recoilRoot.localPosition;
                baseLocalRotation = recoilRoot.localRotation;
            }
        }

        private void OnEnable()
        {
            if (weapon == null) return;
            weapon.ShotFired += HandleShot;
            weapon.ReloadStarted += HandleReloadStarted;
        }

        private void OnDisable()
        {
            if (weapon == null) return;
            weapon.ShotFired -= HandleShot;
            weapon.ReloadStarted -= HandleReloadStarted;
        }

        private void LateUpdate()
        {
            recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, 1f - Mathf.Exp(-recoilReturn * Time.deltaTime));
            recoilAngles = Vector3.Lerp(recoilAngles, Vector3.zero, 1f - Mathf.Exp(-recoilReturn * Time.deltaTime));
            if (recoilRoot != null)
            {
                recoilRoot.localPosition = baseLocalPosition + recoilOffset;
                recoilRoot.localRotation = baseLocalRotation * Quaternion.Euler(recoilAngles);
            }
        }

        public void SetAim(bool aiming)
        {
            if (animator != null) animator.SetBool(Aim, aiming);
        }

        private void HandleReloadStarted()
        {
            if (animator != null) animator.SetTrigger(Reload);
        }

        private void HandleShot(Vector3 origin, Vector3 direction)
        {
            if (muzzleFlash != null) muzzleFlash.Play(true);
            if (shellEject != null) shellEject.Emit(1);
            if (shotAudio != null) shotAudio.Play();
            if (animator != null) animator.SetTrigger(Fire);
            recoilOffset += recoilPosition;
            recoilAngles += new Vector3(recoilEuler.x, Random.Range(-Mathf.Abs(recoilEuler.y), Mathf.Abs(recoilEuler.y)), recoilEuler.z);
        }
    }
}
