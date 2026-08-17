using Aeronauts2.Core;
using UnityEngine;

namespace Aeronauts2.Flight
{
    public static class FlightSimulation
    {
        public const float SpeedEpsilon = 0.001f;

        public static void Step(
            ref FlightState state,
            in MovementIntent intent,
            FlightTuning tuning,
            float deltaTime)
        {
            ManeuverIntent noManeuver = ManeuverIntent.None;
            Step(ref state, intent, noManeuver, tuning, deltaTime);
        }

        public static void Step(
            ref FlightState state,
            in MovementIntent intent,
            in ManeuverIntent maneuverIntent,
            FlightTuning tuning,
            float deltaTime)
        {
            if (tuning == null || deltaTime <= 0f)
            {
                return;
            }

            bool airbrakeActive = intent.Airbrake > SpeedEpsilon;
            bool boostPressedThisTick = intent.Boost && !state.BoostInputHeldLastTick;

            UpdateAirbrakeState(ref state, airbrakeActive, tuning, deltaTime);

            float normalizedSpeed = tuning.MaxSpeed > SpeedEpsilon
                ? Mathf.Clamp01(state.Speed / tuning.MaxSpeed)
                : 0f;
            float hookTurnMultiplier = 1f
                + (tuning.AirbrakeTurnMultiplier - 1f) * intent.Airbrake * normalizedSpeed;

            if (state.AutomaticRollDegreesRemaining <= SpeedEpsilon
                && maneuverIntent.AutomaticRollDirection != 0)
            {
                state.AutomaticRollDegreesRemaining = 360f;
                state.AutomaticRollDirection = maneuverIntent.AutomaticRollDirection;
            }

            float rollDelta;
            if (state.AutomaticRollDegreesRemaining > SpeedEpsilon)
            {
                float automaticRollRate = 360f / tuning.AutomaticRollDuration;
                float automaticRollStep = Mathf.Min(
                    state.AutomaticRollDegreesRemaining,
                    automaticRollRate * deltaTime);
                rollDelta = -state.AutomaticRollDirection * automaticRollStep;
                state.AutomaticRollDegreesRemaining -= automaticRollStep;
                if (state.AutomaticRollDegreesRemaining <= SpeedEpsilon)
                {
                    state.AutomaticRollDegreesRemaining = 0f;
                    state.AutomaticRollDirection = 0;
                }
            }
            else
            {
                rollDelta = -intent.Roll * tuning.RollRate * deltaTime;
            }

            Vector3 localRotationDelta = new(
                -intent.Steer.y * tuning.PitchRate * hookTurnMultiplier * deltaTime,
                intent.Steer.x * tuning.YawRate * hookTurnMultiplier * deltaTime,
                rollDelta);
            state.Rotation = Quaternion.Normalize(state.Rotation * Quaternion.Euler(localRotationDelta));

            if (!intent.Boost)
            {
                state.BoostLockedUntilReleased = false;
            }

            bool canBoost = intent.Boost
                && intent.Accelerate > 0f
                && !airbrakeActive
                && !state.BoostLockedUntilReleased
                && state.BoostEnergy > SpeedEpsilon;
            state.IsBoosting = canBoost;

            bool perfectBoostStarted = canBoost
                && boostPressedThisTick
                && state.AirbrakeExitWindowRemaining > SpeedEpsilon;
            if (perfectBoostStarted)
            {
                state.PerfectBoostTimeRemaining = tuning.PerfectBoostDuration;
                state.AirbrakeExitWindowRemaining = 0f;
            }

            if (airbrakeActive || !canBoost)
            {
                state.PerfectBoostTimeRemaining = 0f;
            }

            state.IsPerfectBoosting = canBoost
                && state.PerfectBoostTimeRemaining > SpeedEpsilon;

            float accelerationInput = airbrakeActive ? 0f : intent.Accelerate;
            if (accelerationInput > 0f && state.Speed < tuning.MaxSpeed)
            {
                state.Speed = Mathf.Min(
                    tuning.MaxSpeed,
                    state.Speed + tuning.Acceleration * accelerationInput * deltaTime);
            }

            if (canBoost)
            {
                float boostAcceleration = tuning.BoostAcceleration;
                if (state.IsPerfectBoosting)
                {
                    boostAcceleration += tuning.PerfectBoostAccelerationBonus;
                    state.PerfectBoostTimeRemaining = Mathf.Max(
                        0f,
                        state.PerfectBoostTimeRemaining - deltaTime);
                }

                state.Speed += boostAcceleration * deltaTime;
                state.BoostEnergy = Mathf.Max(
                    0f,
                    state.BoostEnergy - tuning.BoostDrainPerSecond * deltaTime);
                if (state.BoostEnergy <= SpeedEpsilon)
                {
                    state.BoostEnergy = 0f;
                    state.BoostLockedUntilReleased = true;
                }
            }
            else
            {
                state.BoostEnergy = Mathf.Min(
                    tuning.BoostCapacity,
                    state.BoostEnergy + tuning.BoostRegenerationPerSecond * deltaTime);
            }

            state.IsPerfectBoosting = canBoost
                && state.PerfectBoostTimeRemaining > SpeedEpsilon;

            float steeringDemand = Mathf.Clamp01(intent.Steer.magnitude);
            float turnMomentumLoss = tuning.TurnMomentumDeceleration
                * Mathf.Pow(steeringDemand, tuning.TurnMomentumExponent)
                * normalizedSpeed;
            state.Speed = Mathf.MoveTowards(
                state.Speed,
                0f,
                (tuning.AirbrakeDeceleration * intent.Airbrake + turnMomentumLoss) * deltaTime);

            if (!canBoost && state.Speed > tuning.MaxSpeed)
            {
                state.Speed = Mathf.MoveTowards(
                    state.Speed,
                    tuning.MaxSpeed,
                    tuning.OverspeedDeceleration * deltaTime);
            }

            state.Speed = Mathf.Clamp(state.Speed, 0f, tuning.BoostMaxSpeed);
            state.BoostInputHeldLastTick = intent.Boost;
            if (state.Speed <= SpeedEpsilon)
            {
                state.Speed = 0f;
                state.Velocity = Vector3.zero;
                return;
            }

            state.Velocity = state.Rotation * Vector3.forward * state.Speed;
            state.Position += state.Velocity * deltaTime;
        }

        private static void UpdateAirbrakeState(
            ref FlightState state,
            bool airbrakeActive,
            FlightTuning tuning,
            float deltaTime)
        {
            if (airbrakeActive)
            {
                if (!state.IsAirbraking)
                {
                    state.AirbrakeEntrySpeed = state.Speed;
                    state.AirbrakeHoldDuration = 0f;
                }

                state.AirbrakeHoldDuration += deltaTime;
                state.AirbrakeExitWindowRemaining = 0f;
            }
            else if (state.IsAirbraking)
            {
                bool earnedPerfectExit = state.AirbrakeHoldDuration
                        >= tuning.PerfectBoostMinimumAirbrakeDuration
                    && state.AirbrakeEntrySpeed >= tuning.PerfectBoostMinimumEntrySpeed;
                state.AirbrakeExitWindowRemaining = earnedPerfectExit
                    ? tuning.PerfectBoostExitWindow
                    : 0f;
                state.AirbrakeHoldDuration = 0f;
                state.AirbrakeEntrySpeed = 0f;
            }
            else
            {
                state.AirbrakeExitWindowRemaining = Mathf.Max(
                    0f,
                    state.AirbrakeExitWindowRemaining - deltaTime);
            }

            state.IsAirbraking = airbrakeActive;
        }
    }
}
