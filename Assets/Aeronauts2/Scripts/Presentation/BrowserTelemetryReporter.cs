using System;
using System.Runtime.InteropServices;
using Aeronauts2.CameraSystem;
using Aeronauts2.Combat;
using Aeronauts2.Flight;
using UnityEngine;

namespace Aeronauts2.Presentation
{
    public sealed class BrowserTelemetryReporter : MonoBehaviour
    {
        private const float ReportInterval = 0.1f;

        [SerializeField] private FlightMotor flightMotor;
        [SerializeField] private AimCameraController aimCamera;
        [SerializeField] private PlayerWeaponController weapon;

        private readonly BrowserTelemetrySnapshot snapshot = new();
        private float timeUntilNextReport;

        public void Configure(
            FlightMotor motor,
            AimCameraController cameraController,
            PlayerWeaponController weaponController)
        {
            flightMotor = motor;
            aimCamera = cameraController;
            weapon = weaponController;
        }

        private void Update()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (flightMotor == null || weapon == null)
            {
                return;
            }

            timeUntilNextReport -= Time.unscaledDeltaTime;
            if (timeUntilNextReport > 0f)
            {
                return;
            }

            timeUntilNextReport = ReportInterval;
            FlightState flight = flightMotor.State;
            WeaponState weaponState = weapon.State;
            snapshot.speed = flight.Speed;
            snapshot.boostEnergy = flight.BoostEnergy;
            snapshot.flightMode = GetFlightMode(flight);
            snapshot.aimBodyAngle = aimCamera != null ? aimCamera.BodyAimAngleDegrees : 0f;
            snapshot.cameraViewMode = aimCamera != null
                ? aimCamera.ActiveViewMode.ToString()
                : "Unavailable";
            snapshot.aimLimitMode = aimCamera != null
                ? aimCamera.ActiveLimitMode.ToString()
                : "Unavailable";
            snapshot.aimHorizontalLimit = aimCamera != null
                ? aimCamera.ActiveHorizontalLimitDegrees
                : 0f;
            snapshot.aimUpLimit = aimCamera != null ? aimCamera.ActiveUpLimitDegrees : 0f;
            snapshot.aimDownLimit = aimCamera != null ? aimCamera.ActiveDownLimitDegrees : 0f;
            snapshot.alignmentError = flightMotor.VelocityAlignmentErrorDegrees;
            snapshot.primaryShots = weapon.PrimaryShotsFired;
            snapshot.railgunShots = weapon.RailgunShotsFired;
            snapshot.hits = weapon.Hits;
            snapshot.criticalHits = weapon.CriticalHits;
            snapshot.railgunAmmo = weaponState.RailgunAmmo;
            snapshot.activeProjectiles = weapon.ActiveProjectileCount;
            snapshot.muzzleBlocked = weapon.MuzzleBlocked;
            snapshot.lastShotWasBlocked = weapon.LastShotWasBlocked;
            Aeronauts2_ReportTelemetry(JsonUtility.ToJson(snapshot));
#endif
        }

        private static string GetFlightMode(in FlightState state)
        {
            if (state.IsAirbraking)
            {
                return "AIRBRAKE";
            }

            if (state.IsPerfectBoosting)
            {
                return "SLINGSHOT";
            }

            if (state.IsBoosting)
            {
                return "BOOST";
            }

            return state.AutomaticRollDegreesRemaining > 0f ? "AUTO ROLL" : "CRUISE";
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void Aeronauts2_ReportTelemetry(string json);
#endif

        [Serializable]
        private sealed class BrowserTelemetrySnapshot
        {
            public float speed;
            public float boostEnergy;
            public string flightMode;
            public float aimBodyAngle;
            public string cameraViewMode;
            public string aimLimitMode;
            public float aimHorizontalLimit;
            public float aimUpLimit;
            public float aimDownLimit;
            public float alignmentError;
            public int primaryShots;
            public int railgunShots;
            public int hits;
            public int criticalHits;
            public int railgunAmmo;
            public int activeProjectiles;
            public bool muzzleBlocked;
            public bool lastShotWasBlocked;
        }
    }
}
