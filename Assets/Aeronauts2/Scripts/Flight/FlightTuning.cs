using UnityEngine;

namespace Aeronauts2.Flight
{
    [CreateAssetMenu(fileName = "FlightTuning", menuName = "Aeronauts 2/Flight Tuning")]
    public sealed class FlightTuning : ScriptableObject
    {
        [Header("Speed")]
        [SerializeField, Min(0.1f)] private float maxSpeed = 45f;
        [SerializeField, Min(0f)] private float acceleration = 32f;
        [SerializeField, Min(0f)] private float airbrakeDeceleration = 55f;

        [Header("Momentum")]
        [SerializeField, Min(0f)] private float turnMomentumDeceleration = 38f;
        [SerializeField, Min(1f)] private float turnMomentumExponent = 2f;

        [Header("Boost")]
        [SerializeField, Min(0.1f)] private float boostMaxSpeed = 75f;
        [SerializeField, Min(0f)] private float boostAcceleration = 48f;
        [SerializeField, Min(0f)] private float overspeedDeceleration = 14f;
        [SerializeField, Min(0.01f)] private float boostCapacity = 1f;
        [SerializeField, Min(0f)] private float boostDrainPerSecond = 0.32f;
        [SerializeField, Min(0f)] private float boostRegenerationPerSecond = 0.16f;

        [Header("Body rotation (degrees/second)")]
        [SerializeField, Min(0f)] private float pitchRate = 130f;
        [SerializeField, Min(0f)] private float yawRate = 82f;
        [SerializeField, Min(0f)] private float rollRate = 200f;
        [SerializeField, Min(1f)] private float airbrakeTurnMultiplier = 2.15f;

        [Header("Skill maneuvers")]
        [SerializeField, Min(0.01f)] private float automaticRollDuration = 0.4f;
        [SerializeField, Min(0f)] private float perfectBoostExitWindow = 0.15f;
        [SerializeField, Min(0f)] private float perfectBoostMinimumAirbrakeDuration = 0.18f;
        [SerializeField, Min(0f)] private float perfectBoostMinimumEntrySpeed = 18f;
        [SerializeField, Min(0f)] private float perfectBoostDuration = 0.22f;
        [SerializeField, Min(0f)] private float perfectBoostAccelerationBonus = 34f;

        public float MaxSpeed => maxSpeed;
        public float Acceleration => acceleration;
        public float AirbrakeDeceleration => airbrakeDeceleration;
        public float TurnMomentumDeceleration => turnMomentumDeceleration;
        public float TurnMomentumExponent => turnMomentumExponent;
        public float BoostMaxSpeed => Mathf.Max(maxSpeed, boostMaxSpeed);
        public float BoostAcceleration => boostAcceleration;
        public float OverspeedDeceleration => overspeedDeceleration;
        public float BoostCapacity => boostCapacity;
        public float BoostDrainPerSecond => boostDrainPerSecond;
        public float BoostRegenerationPerSecond => boostRegenerationPerSecond;
        public float PitchRate => pitchRate;
        public float YawRate => yawRate;
        public float RollRate => rollRate;
        public float AirbrakeTurnMultiplier => airbrakeTurnMultiplier;
        public float AutomaticRollDuration => automaticRollDuration;
        public float PerfectBoostExitWindow => perfectBoostExitWindow;
        public float PerfectBoostMinimumAirbrakeDuration => perfectBoostMinimumAirbrakeDuration;
        public float PerfectBoostMinimumEntrySpeed => perfectBoostMinimumEntrySpeed;
        public float PerfectBoostDuration => perfectBoostDuration;
        public float PerfectBoostAccelerationBonus => perfectBoostAccelerationBonus;
    }
}
