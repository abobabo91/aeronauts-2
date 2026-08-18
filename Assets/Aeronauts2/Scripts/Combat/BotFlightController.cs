using Aeronauts2.Core;
using Aeronauts2.Flight;
using UnityEngine;

namespace Aeronauts2.Combat
{
    public sealed class BotFlightController : MonoBehaviour
    {
        [SerializeField] private FlightTuning flightTuning;
        [SerializeField] private BotTuning botTuning;
        [SerializeField] private CombatantHealth health;
        [SerializeField] private Transform target;
        [SerializeField] private CombatantHealth targetHealth;
        [SerializeField] private Vector3 arenaCenter = new(0f, 28f, 55f);

        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private FlightState state;
        private CombatantHealth subscribedHealth;

        public FlightState State => state;
        public float VelocityAlignmentErrorDegrees => state.Speed <= FlightSimulation.SpeedEpsilon
            ? 0f
            : Vector3.Angle(transform.forward, state.Velocity);

        public void Configure(
            FlightTuning sharedFlightTuning,
            BotTuning behaviorTuning,
            CombatantHealth botHealth,
            Transform targetTransform,
            CombatantHealth targetCombatant,
            Vector3 combatArenaCenter)
        {
            Unsubscribe();
            flightTuning = sharedFlightTuning;
            botTuning = behaviorTuning;
            health = botHealth;
            target = targetTransform;
            targetHealth = targetCombatant;
            arenaCenter = combatArenaCenter;
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            ResetBot();
            Subscribe();
        }

        private void Awake()
        {
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            ResetBot();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void FixedUpdate()
        {
            if (flightTuning == null
                || botTuning == null
                || health == null
                || health.IsEliminated)
            {
                return;
            }

            MovementIntent intent = BuildIntent();
            FlightSimulation.Step(ref state, intent, flightTuning, Time.fixedDeltaTime);
            transform.SetPositionAndRotation(state.Position, state.Rotation);
        }

        public MovementIntent BuildIntent()
        {
            if (target == null || (targetHealth != null && targetHealth.IsEliminated))
            {
                return new MovementIntent(Vector2.zero, 0f, 1f, 0f, false);
            }

            Vector3 toTarget = target.position - state.Position;
            float distance = toTarget.magnitude;
            Vector3 radial = distance > 0.001f ? toTarget / distance : state.Rotation * Vector3.forward;
            Vector3 orbit = Vector3.Cross(Vector3.up, radial);
            if (orbit.sqrMagnitude <= 0.001f)
            {
                orbit = state.Rotation * Vector3.right;
            }
            else
            {
                orbit.Normalize();
            }

            float rangeError = Mathf.Clamp(
                (distance - botTuning.PreferredRange) / botTuning.PreferredRange,
                -1f,
                1f);
            Vector3 desiredDirection = radial * rangeError
                + orbit * botTuning.OrbitWeight * Mathf.Lerp(0.35f, 1f, botTuning.Aggression);

            Vector3 fromArenaCenter = state.Position - arenaCenter;
            if (fromArenaCenter.magnitude > botTuning.ArenaRadius)
            {
                desiredDirection = (arenaCenter - state.Position).normalized;
            }

            if (desiredDirection.sqrMagnitude <= 0.001f)
            {
                desiredDirection = radial;
            }

            desiredDirection.Normalize();
            Vector3 localDirection = Quaternion.Inverse(state.Rotation) * desiredDirection;
            Vector2 steer = Vector2.ClampMagnitude(
                new Vector2(localDirection.x, localDirection.y) * botTuning.SteeringGain,
                1f);
            float roll = Mathf.Clamp(localDirection.x * botTuning.BankGain, -1f, 1f);
            float turnAngle = Vector3.Angle(state.Rotation * Vector3.forward, desiredDirection);
            float airbrake = turnAngle >= botTuning.AirbrakeAngle
                && state.Speed >= botTuning.AirbrakeMinimumSpeed
                ? 1f
                : 0f;
            bool boost = distance >= botTuning.BoostDistance
                && turnAngle <= botTuning.BoostAngle;
            return new MovementIntent(steer, roll, 1f, airbrake, boost);
        }

        public void ResetBot()
        {
            float boost = flightTuning != null ? flightTuning.BoostCapacity : 1f;
            state = FlightState.Create(spawnPosition, spawnRotation, boost);
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        }

        private void Subscribe()
        {
            if (health == null || subscribedHealth == health)
            {
                return;
            }

            Unsubscribe();
            subscribedHealth = health;
            subscribedHealth.Respawned += OnRespawned;
        }

        private void Unsubscribe()
        {
            if (subscribedHealth == null)
            {
                return;
            }

            subscribedHealth.Respawned -= OnRespawned;
            subscribedHealth = null;
        }

        private void OnRespawned(CombatantHealth _)
        {
            ResetBot();
        }
    }
}
