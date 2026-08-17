using UnityEngine;

namespace Aeronauts2.Flight
{
    public struct FlightState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public float Speed;
        public float BoostEnergy;
        public bool IsBoosting;
        public bool BoostLockedUntilReleased;
        public bool IsAirbraking;
        public float AirbrakeHoldDuration;
        public float AirbrakeEntrySpeed;
        public float AirbrakeExitWindowRemaining;
        public bool BoostInputHeldLastTick;
        public bool IsPerfectBoosting;
        public float PerfectBoostTimeRemaining;
        public float AutomaticRollDegreesRemaining;
        public int AutomaticRollDirection;

        public static FlightState Create(Vector3 position, Quaternion rotation, float boostEnergy)
        {
            return new FlightState
            {
                Position = position,
                Rotation = rotation,
                Velocity = Vector3.zero,
                Speed = 0f,
                BoostEnergy = boostEnergy,
                IsBoosting = false,
                BoostLockedUntilReleased = false,
                IsAirbraking = false,
                AirbrakeHoldDuration = 0f,
                AirbrakeEntrySpeed = 0f,
                AirbrakeExitWindowRemaining = 0f,
                BoostInputHeldLastTick = false,
                IsPerfectBoosting = false,
                PerfectBoostTimeRemaining = 0f,
                AutomaticRollDegreesRemaining = 0f,
                AutomaticRollDirection = 0,
            };
        }
    }
}
