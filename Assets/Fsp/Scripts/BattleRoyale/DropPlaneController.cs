using System;
using UnityEngine;

namespace Fsp.BattleRoyale
{
    public sealed class DropPlaneController : MonoBehaviour
    {
        [SerializeField] private Transform routeStart;
        [SerializeField] private Transform routeEnd;
        [SerializeField, Min(1f)] private float speed = 65f;
        [SerializeField] private bool startOnEnable = true;

        public bool IsFlying { get; private set; }
        public float RouteProgress { get; private set; }
        public event Action RouteStarted;
        public event Action RouteFinished;

        private void OnEnable()
        {
            if (startOnEnable) BeginRoute();
        }

        public void ConfigureRoute(Transform start, Transform end, float routeSpeed = 65f, bool beginImmediately = true)
        {
            routeStart = start;
            routeEnd = end;
            speed = Mathf.Max(1f, routeSpeed);
            if (beginImmediately && isActiveAndEnabled) BeginRoute();
        }

        public void BeginRoute()
        {
            if (routeStart == null || routeEnd == null) return;
            transform.SetPositionAndRotation(routeStart.position, routeStart.rotation);
            RouteProgress = 0f;
            IsFlying = true;
            RouteStarted?.Invoke();
        }

        private void Update()
        {
            if (!IsFlying || routeStart == null || routeEnd == null) return;

            Vector3 toEnd = routeEnd.position - transform.position;
            float step = speed * Time.deltaTime;
            if (toEnd.magnitude <= step)
            {
                transform.position = routeEnd.position;
                IsFlying = false;
                RouteProgress = 1f;
                RouteFinished?.Invoke();
                return;
            }

            transform.position += toEnd.normalized * step;
            if (toEnd.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toEnd.normalized, Vector3.up), 6f * Time.deltaTime);

            float total = Vector3.Distance(routeStart.position, routeEnd.position);
            if (total > 0.01f)
                RouteProgress = Mathf.Clamp01(Vector3.Distance(routeStart.position, transform.position) / total);
        }
    }
}
