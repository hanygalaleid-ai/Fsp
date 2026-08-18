using System;
using Fsp.Player;
using UnityEngine;

namespace Fsp.Vehicles
{
    public sealed class VehicleSeat : MonoBehaviour
    {
        [SerializeField] private SimpleVehicleController vehicle;
        [SerializeField] private Transform seatPoint;
        [SerializeField] private Transform exitPoint;

        private ThirdPersonMotor currentDriver;
        private Transform originalParent;

        public bool Occupied => currentDriver != null;
        public SimpleVehicleController Vehicle => vehicle;
        public Transform SeatPoint => seatPoint;
        public event Action<ThirdPersonMotor> DriverEntered;
        public event Action<ThirdPersonMotor> DriverExited;
        public bool IsDriver(ThirdPersonMotor driver) => currentDriver == driver;

        public void Configure(SimpleVehicleController targetVehicle, Transform targetSeatPoint, Transform targetExitPoint)
        {
            vehicle = targetVehicle;
            seatPoint = targetSeatPoint;
            exitPoint = targetExitPoint;
        }

        public bool TryEnter(ThirdPersonMotor driver)
        {
            if (driver == null || Occupied || vehicle == null || seatPoint == null) return false;

            currentDriver = driver;
            originalParent = driver.transform.parent;
            driver.enabled = false;
            driver.transform.SetParent(seatPoint, false);
            driver.transform.localPosition = Vector3.zero;
            driver.transform.localRotation = Quaternion.identity;
            vehicle.SetDriverPresent(true);
            DriverEntered?.Invoke(driver);
            return true;
        }

        public void Exit()
        {
            if (currentDriver == null) return;

            ThirdPersonMotor leavingDriver = currentDriver;
            Transform driverTransform = leavingDriver.transform;
            driverTransform.SetParent(originalParent, true);
            if (exitPoint != null)
            {
                driverTransform.position = exitPoint.position;
                driverTransform.rotation = exitPoint.rotation;
            }

            leavingDriver.enabled = true;
            vehicle.SetDriverPresent(false);
            currentDriver = null;
            originalParent = null;
            DriverExited?.Invoke(leavingDriver);
        }
    }
}
