using Aeronauts2.Core;
using Aeronauts2.Flight;
using NUnit.Framework;
using UnityEngine;

namespace Aeronauts2.Tests
{
    public sealed class FlightSimulationTests
    {
        private FlightTuning tuning;

        [SetUp]
        public void SetUp()
        {
            tuning = ScriptableObject.CreateInstance<FlightTuning>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(tuning);
        }

        [Test]
        public void MovingVelocityAlwaysMatchesBodyForward()
        {
            FlightState state = FlightState.Create(Vector3.zero, Quaternion.identity, tuning.BoostCapacity);
            MovementIntent intent = new(new Vector2(0.7f, -0.4f), 0.3f, 1f, 0f, false);

            for (int tick = 0; tick < 120; tick++)
            {
                FlightSimulation.Step(ref state, intent, tuning, 1f / 60f);
            }

            Assert.That(state.Speed, Is.GreaterThan(0f));
            Assert.That(Vector3.Angle(state.Rotation * Vector3.forward, state.Velocity), Is.LessThan(0.001f));
        }

        [Test]
        public void ZeroSpeedCanRotateWithoutDrifting()
        {
            FlightState state = FlightState.Create(new Vector3(3f, 8f, -2f), Quaternion.identity, tuning.BoostCapacity);
            Vector3 startPosition = state.Position;
            MovementIntent intent = new(Vector2.right, 1f, 0f, 0f, false);

            FlightSimulation.Step(ref state, intent, tuning, 1f / 60f);

            Assert.That(state.Position, Is.EqualTo(startPosition));
            Assert.That(state.Velocity, Is.EqualTo(Vector3.zero));
            Assert.That(Quaternion.Angle(state.Rotation, Quaternion.identity), Is.GreaterThan(0f));
        }

        [Test]
        public void AirbrakeCanBringFlightToACompleteStop()
        {
            FlightState state = FlightState.Create(Vector3.zero, Quaternion.identity, tuning.BoostCapacity);
            state.Speed = 30f;
            MovementIntent intent = new(Vector2.zero, 0f, 0f, 1f, false);

            for (int tick = 0; tick < 120; tick++)
            {
                FlightSimulation.Step(ref state, intent, tuning, 1f / 60f);
            }

            Assert.That(state.Speed, Is.EqualTo(0f));
            Assert.That(state.Velocity, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void BoostNeverExceedsBoostSpeedCap()
        {
            FlightState state = FlightState.Create(Vector3.zero, Quaternion.identity, tuning.BoostCapacity);
            MovementIntent intent = new(Vector2.zero, 0f, 1f, 0f, true);

            for (int tick = 0; tick < 600; tick++)
            {
                FlightSimulation.Step(ref state, intent, tuning, 1f / 60f);
            }

            Assert.That(state.Speed, Is.LessThanOrEqualTo(tuning.BoostMaxSpeed));
            Assert.That(state.BoostEnergy, Is.InRange(0f, tuning.BoostCapacity));
        }

        [Test]
        public void OrdinaryAccelerationNeverExceedsNormalSpeedCap()
        {
            FlightState state = FlightState.Create(Vector3.zero, Quaternion.identity, tuning.BoostCapacity);
            MovementIntent intent = new(Vector2.zero, 0f, 1f, 0f, false);

            for (int tick = 0; tick < 600; tick++)
            {
                FlightSimulation.Step(ref state, intent, tuning, 1f / 60f);
            }

            Assert.That(state.Speed, Is.EqualTo(tuning.MaxSpeed).Within(0.001f));
        }

        [Test]
        public void AirbrakeSuppressesAccelerationAndBoost()
        {
            FlightState state = FlightState.Create(Vector3.zero, Quaternion.identity, tuning.BoostCapacity);
            state.Speed = 30f;
            float startingEnergy = state.BoostEnergy;
            MovementIntent intent = new(Vector2.zero, 0f, 1f, 1f, true);

            FlightSimulation.Step(ref state, intent, tuning, 1f / 60f);

            Assert.That(state.Speed, Is.LessThan(30f));
            Assert.That(state.IsAirbraking, Is.True);
            Assert.That(state.IsBoosting, Is.False);
            Assert.That(state.BoostEnergy, Is.EqualTo(startingEnergy));
        }

        [Test]
        public void FullSteeringBleedsMoreMomentumThanGentleSteering()
        {
            FlightState hardTurn = FlightState.Create(Vector3.zero, Quaternion.identity, tuning.BoostCapacity);
            FlightState gentleTurn = hardTurn;
            hardTurn.Speed = tuning.MaxSpeed;
            gentleTurn.Speed = tuning.MaxSpeed;
            MovementIntent hardIntent = new(Vector2.up, 0f, 0f, 0f, false);
            MovementIntent gentleIntent = new(Vector2.up * 0.35f, 0f, 0f, 0f, false);

            for (int tick = 0; tick < 60; tick++)
            {
                FlightSimulation.Step(ref hardTurn, hardIntent, tuning, 1f / 60f);
                FlightSimulation.Step(ref gentleTurn, gentleIntent, tuning, 1f / 60f);
            }

            Assert.That(hardTurn.Speed, Is.LessThan(gentleTurn.Speed));
            Assert.That(gentleTurn.Speed, Is.GreaterThan(tuning.MaxSpeed * 0.85f));
        }

        [Test]
        public void PitchIsTheStrongestTurningAxis()
        {
            FlightState pitchTurn = FlightState.Create(Vector3.zero, Quaternion.identity, tuning.BoostCapacity);
            FlightState yawTurn = pitchTurn;
            MovementIntent pitchIntent = new(Vector2.up, 0f, 0f, 0f, false);
            MovementIntent yawIntent = new(Vector2.right, 0f, 0f, 0f, false);

            for (int tick = 0; tick < 30; tick++)
            {
                FlightSimulation.Step(ref pitchTurn, pitchIntent, tuning, 1f / 60f);
                FlightSimulation.Step(ref yawTurn, yawIntent, tuning, 1f / 60f);
            }

            Assert.That(
                Quaternion.Angle(Quaternion.identity, pitchTurn.Rotation),
                Is.GreaterThan(Quaternion.Angle(Quaternion.identity, yawTurn.Rotation)));
        }

        [Test]
        public void EarnedAirbrakeExitCanTriggerPerfectBoost()
        {
            FlightState state = FlightState.Create(Vector3.zero, Quaternion.identity, tuning.BoostCapacity);
            state.Speed = tuning.MaxSpeed;
            MovementIntent airbrake = new(Vector2.up, 0f, 0f, 1f, false);

            int requiredTicks = Mathf.CeilToInt(
                tuning.PerfectBoostMinimumAirbrakeDuration * 60f) + 1;
            for (int tick = 0; tick < requiredTicks; tick++)
            {
                FlightSimulation.Step(ref state, airbrake, tuning, 1f / 60f);
            }

            MovementIntent release = new(Vector2.zero, 0f, 1f, 0f, false);
            FlightSimulation.Step(ref state, release, tuning, 1f / 60f);
            Assert.That(state.AirbrakeExitWindowRemaining, Is.GreaterThan(0f));

            MovementIntent boostPress = new(Vector2.zero, 0f, 1f, 0f, true);
            FlightSimulation.Step(ref state, boostPress, tuning, 1f / 60f);

            Assert.That(state.IsPerfectBoosting, Is.True);
            Assert.That(state.AirbrakeExitWindowRemaining, Is.EqualTo(0f));
        }

        [Test]
        public void OneTickAirbrakeCannotFarmPerfectBoost()
        {
            FlightState state = FlightState.Create(Vector3.zero, Quaternion.identity, tuning.BoostCapacity);
            state.Speed = tuning.MaxSpeed;

            FlightSimulation.Step(
                ref state,
                new MovementIntent(Vector2.zero, 0f, 0f, 1f, false),
                tuning,
                1f / 60f);
            FlightSimulation.Step(
                ref state,
                new MovementIntent(Vector2.zero, 0f, 1f, 0f, false),
                tuning,
                1f / 60f);
            FlightSimulation.Step(
                ref state,
                new MovementIntent(Vector2.zero, 0f, 1f, 0f, true),
                tuning,
                1f / 60f);

            Assert.That(state.IsPerfectBoosting, Is.False);
        }

        [Test]
        public void AutomaticRollCompletesOneFullRevolution()
        {
            FlightState state = FlightState.Create(Vector3.zero, Quaternion.identity, tuning.BoostCapacity);
            MovementIntent movement = MovementIntent.None;
            ManeuverIntent startRoll = new(-1);

            FlightSimulation.Step(ref state, movement, startRoll, tuning, 1f / 60f);
            for (int tick = 0; tick < 60; tick++)
            {
                ManeuverIntent noManeuver = ManeuverIntent.None;
                FlightSimulation.Step(ref state, movement, noManeuver, tuning, 1f / 60f);
            }

            Assert.That(state.AutomaticRollDegreesRemaining, Is.EqualTo(0f));
            Assert.That(Quaternion.Angle(Quaternion.identity, state.Rotation), Is.LessThan(0.01f));
        }

        [Test]
        public void AutomatedPilotCompletesRepresentativeSkillSequence()
        {
            const float tickDuration = 1f / 60f;

            FlightState accelerationRun = FlightState.Create(
                Vector3.zero,
                Quaternion.identity,
                tuning.BoostCapacity);
            MovementIntent accelerate = new(Vector2.zero, 0f, 1f, 0f, false);
            int accelerationTicks = StepUntil(
                ref accelerationRun,
                accelerate,
                tuning,
                tickDuration,
                300,
                state => state.Speed >= tuning.MaxSpeed - 0.001f);

            FlightState ordinaryTurn = FlightState.Create(
                Vector3.zero,
                Quaternion.identity,
                tuning.BoostCapacity);
            ordinaryTurn.Speed = tuning.MaxSpeed;
            MovementIntent ordinaryTurnIntent = new(Vector2.up, 0f, 1f, 0f, false);
            int ordinaryTurnTicks = StepUntil(
                ref ordinaryTurn,
                ordinaryTurnIntent,
                tuning,
                tickDuration,
                300,
                state => Quaternion.Angle(Quaternion.identity, state.Rotation) >= 179f);

            FlightState hookTurn = FlightState.Create(
                Vector3.zero,
                Quaternion.identity,
                tuning.BoostCapacity);
            hookTurn.Speed = tuning.MaxSpeed;
            MovementIntent hookTurnIntent = new(Vector2.up, 0f, 0f, 1f, false);
            int hookTurnTicks = StepUntil(
                ref hookTurn,
                hookTurnIntent,
                tuning,
                tickDuration,
                300,
                state => Quaternion.Angle(Quaternion.identity, state.Rotation) >= 179f);

            MovementIntent releaseAirbrake = new(Vector2.zero, 0f, 1f, 0f, false);
            FlightSimulation.Step(ref hookTurn, releaseAirbrake, tuning, tickDuration);
            FlightState perfectExit = hookTurn;
            FlightState ordinaryExit = hookTurn;
            ordinaryExit.AirbrakeExitWindowRemaining = 0f;
            MovementIntent boost = new(Vector2.zero, 0f, 1f, 0f, true);
            int burstTicks = Mathf.CeilToInt(tuning.PerfectBoostDuration / tickDuration);
            bool observedPerfectBoost = false;
            for (int tick = 0; tick < burstTicks; tick++)
            {
                FlightSimulation.Step(ref perfectExit, boost, tuning, tickDuration);
                FlightSimulation.Step(ref ordinaryExit, boost, tuning, tickDuration);
                observedPerfectBoost |= perfectExit.IsPerfectBoosting;
            }

            float alignmentError = perfectExit.Speed > FlightSimulation.SpeedEpsilon
                ? Vector3.Angle(perfectExit.Rotation * Vector3.forward, perfectExit.Velocity)
                : 0f;
            Debug.Log(
                "FLIGHT_FEEL "
                + $"acceleration_0_to_max={accelerationTicks * tickDuration:0.000}s "
                + $"ordinary_180={ordinaryTurnTicks * tickDuration:0.000}s "
                + $"ordinary_exit_speed={ordinaryTurn.Speed:0.00} "
                + $"airbrake_180={hookTurnTicks * tickDuration:0.000}s "
                + $"airbrake_exit_speed={hookTurn.Speed:0.00} "
                + $"perfect_burst_speed={perfectExit.Speed:0.00} "
                + $"ordinary_burst_speed={ordinaryExit.Speed:0.00} "
                + $"alignment_error={alignmentError:0.000000}deg");

            Assert.That(accelerationTicks, Is.LessThan(120));
            Assert.That(hookTurnTicks, Is.LessThan(ordinaryTurnTicks));
            Assert.That(hookTurn.Speed, Is.LessThan(tuning.MaxSpeed * 0.1f));
            Assert.That(observedPerfectBoost, Is.True);
            Assert.That(perfectExit.Speed, Is.GreaterThan(ordinaryExit.Speed + 5f));
            Assert.That(alignmentError, Is.LessThan(0.001f));
        }

        private static int StepUntil(
            ref FlightState state,
            in MovementIntent intent,
            FlightTuning flightTuning,
            float tickDuration,
            int maximumTicks,
            System.Func<FlightState, bool> predicate)
        {
            for (int tick = 1; tick <= maximumTicks; tick++)
            {
                FlightSimulation.Step(ref state, intent, flightTuning, tickDuration);
                if (predicate(state))
                {
                    return tick;
                }
            }

            Assert.Fail($"Automated pilot did not complete its maneuver within {maximumTicks} ticks.");
            return maximumTicks;
        }
    }
}
