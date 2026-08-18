using System;
using Aeronauts2.Core;
using Aeronauts2.Input;
using UnityEngine;

namespace Aeronauts2.Flight
{
    public sealed class FlightMotor : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private FlightTuning tuning;

        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private FlightState state;

        public FlightState State => state;
        public FlightTuning Tuning => tuning;
        public event Action FlightReset;
        public event Action ManualResetRequested;

        public float VelocityAlignmentErrorDegrees
        {
            get
            {
                if (state.Speed <= FlightSimulation.SpeedEpsilon)
                {
                    return 0f;
                }

                return Vector3.Angle(transform.forward, state.Velocity);
            }
        }

        public void Configure(PlayerInputReader inputReader, FlightTuning flightTuning)
        {
            input = inputReader;
            tuning = flightTuning;
        }

        private void Awake()
        {
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            ResetFlight();
        }

        private void FixedUpdate()
        {
            if (input == null || tuning == null)
            {
                return;
            }

            if (input.ConsumeResetPressed())
            {
                RequestManualReset();
            }

            MovementIntent intent = input.ReadMovementIntent();
            ManeuverIntent maneuverIntent = input.ConsumeManeuverIntent();
            FlightSimulation.Step(ref state, intent, maneuverIntent, tuning, Time.fixedDeltaTime);
            transform.SetPositionAndRotation(state.Position, state.Rotation);
        }

        public void ResetFlight()
        {
            float capacity = tuning != null ? tuning.BoostCapacity : 1f;
            state = FlightState.Create(spawnPosition, spawnRotation, capacity);
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            FlightReset?.Invoke();
        }

        public void RequestManualReset()
        {
            ResetFlight();
            ManualResetRequested?.Invoke();
        }
    }
}
