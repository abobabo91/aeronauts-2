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
        [SerializeField] private CombatantHealth playerHealth;
        [SerializeField] private CombatantHealth botHealth;
        [SerializeField] private BotFlightController botFlight;
        [SerializeField] private BotWeaponController botWeapon;
        [SerializeField] private DuelMatchController match;

        private readonly BrowserTelemetrySnapshot snapshot = new();
        private float timeUntilNextReport;

        public void Configure(
            FlightMotor motor,
            AimCameraController cameraController,
            PlayerWeaponController weaponController,
            CombatantHealth playerCombatant,
            CombatantHealth botCombatant,
            BotFlightController botFlightController,
            BotWeaponController botWeaponController,
            DuelMatchController duelMatch)
        {
            flightMotor = motor;
            aimCamera = cameraController;
            weapon = weaponController;
            playerHealth = playerCombatant;
            botHealth = botCombatant;
            botFlight = botFlightController;
            botWeapon = botWeaponController;
            match = duelMatch;
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
            snapshot.eliminations = weapon.Eliminations;
            snapshot.railgunAmmo = weaponState.RailgunAmmo;
            snapshot.activeProjectiles = weapon.ActiveProjectileCount;
            snapshot.lastTargetShield = weapon.LastTargetShield;
            snapshot.lastTargetHull = weapon.LastTargetHull;
            snapshot.lastHitBrokeShield = weapon.LastHitBrokeShield;
            snapshot.lastHitBypassedShield = weapon.LastHitBypassedShield;
            snapshot.lastHitEliminated = weapon.LastHitEliminated;
            snapshot.muzzleBlocked = weapon.MuzzleBlocked;
            snapshot.lastShotWasBlocked = weapon.LastShotWasBlocked;
            snapshot.playerShield = playerHealth != null ? playerHealth.Shield : 0f;
            snapshot.playerHull = playerHealth != null ? playerHealth.Hull : 0f;
            snapshot.playerEliminated = playerHealth != null && playerHealth.IsEliminated;
            snapshot.botShield = botHealth != null ? botHealth.Shield : 0f;
            snapshot.botHull = botHealth != null ? botHealth.Hull : 0f;
            snapshot.botEliminated = botHealth != null && botHealth.IsEliminated;
            snapshot.botSpeed = botFlight != null ? botFlight.State.Speed : 0f;
            snapshot.botAlignmentError = botFlight != null
                ? botFlight.VelocityAlignmentErrorDegrees
                : 0f;
            snapshot.botShots = botWeapon != null ? botWeapon.PrimaryShotsFired : 0;
            snapshot.botHits = botWeapon != null ? botWeapon.Hits : 0;
            snapshot.playerScore = match != null ? match.PlayerScore : 0;
            snapshot.botScore = match != null ? match.BotScore : 0;
            snapshot.matchComplete = match != null && match.MatchComplete;
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
            public int eliminations;
            public int railgunAmmo;
            public int activeProjectiles;
            public float lastTargetShield;
            public float lastTargetHull;
            public bool lastHitBrokeShield;
            public bool lastHitBypassedShield;
            public bool lastHitEliminated;
            public bool muzzleBlocked;
            public bool lastShotWasBlocked;
            public float playerShield;
            public float playerHull;
            public bool playerEliminated;
            public float botShield;
            public float botHull;
            public bool botEliminated;
            public float botSpeed;
            public float botAlignmentError;
            public int botShots;
            public int botHits;
            public int playerScore;
            public int botScore;
            public bool matchComplete;
        }
    }
}
