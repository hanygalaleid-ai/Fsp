using Fsp.Vehicles;
using Fsp.Presentation;
using UnityEngine;

namespace Fsp.World
{
    public sealed class StarterVehicleDistribution : MonoBehaviour
    {
        private static readonly Vector3[] Spots =
        {
            new(-48, 1, 18), new(55, 1, -42), new(91, 1, 73), new(-104, 1, -58),
            new(128, 1, -91), new(-5, 1, 132), new(-128, 1, 78)
        };

        private void Start()
        {
            SimpleVehicleController[] existing = Object.FindObjectsByType<SimpleVehicleController>(FindObjectsSortMode.None);
            if (existing.Length >= 4) return;

            for (int i = existing.Length; i < 4; i++)
            {
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.name = "ScoutVehicle_" + i;
                g.transform.position = Spots[i * 2 % Spots.Length];
                g.transform.localScale = new Vector3(2.2f, 1.1f, 4.1f);
                Renderer rootRenderer = g.GetComponent<Renderer>();
                if (rootRenderer != null) rootRenderer.enabled = false;
                g.transform.localScale = Vector3.one;
                BoxCollider bodyCollider = g.GetComponent<BoxCollider>();
                if (bodyCollider != null)
                {
                    bodyCollider.size = new Vector3(2.2f, 1.4f, 4.1f);
                    bodyCollider.center = new Vector3(0f, .7f, 0f);
                }

                Rigidbody rb = g.AddComponent<Rigidbody>();
                rb.mass = 950f;
                rb.linearDamping = 0.25f;
                rb.angularDamping = 2f;
                SimpleVehicleController vehicle = g.AddComponent<SimpleVehicleController>();
                g.AddComponent<StarterProceduralVehicleVisual>();

                Transform seat = new GameObject("DriverSeat").transform;
                seat.SetParent(g.transform, false);
                seat.localPosition = new Vector3(-.45f, 1.15f, -.2f);
                Transform exit = new GameObject("DriverExit").transform;
                exit.SetParent(g.transform, false);
                exit.localPosition = new Vector3(-1.8f, .9f, 0f);
                VehicleSeat vehicleSeat = g.AddComponent<VehicleSeat>();
                vehicleSeat.Configure(vehicle, seat, exit);
            }
        }
    }
}
