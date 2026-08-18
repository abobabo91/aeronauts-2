using UnityEngine;

namespace Aeronauts2.Combat
{
    [CreateAssetMenu(fileName = "BotTuning", menuName = "Aeronauts 2/Bot Tuning")]
    public sealed class BotTuning : ScriptableObject
    {
        [Header("Flight intent")]
        [SerializeField, Min(5f)] private float preferredRange = 60f;
        [SerializeField, Range(0f, 1f)] private float aggression = 0.65f;
        [SerializeField, Min(0f)] private float orbitWeight = 0.6f;
        [SerializeField, Min(0.1f)] private float steeringGain = 2.2f;
        [SerializeField, Range(0f, 1f)] private float bankGain = 0.8f;
        [SerializeField, Range(1f, 179f)] private float airbrakeAngle = 95f;
        [SerializeField, Min(0f)] private float airbrakeMinimumSpeed = 22f;
        [SerializeField, Min(0f)] private float boostDistance = 90f;
        [SerializeField, Range(0f, 90f)] private float boostAngle = 20f;
        [SerializeField, Min(10f)] private float arenaRadius = 175f;

        [Header("Aim and fire")]
        [SerializeField, Min(0f)] private float reactionTime = 0.75f;
        [SerializeField, Range(0f, 30f)] private float aimErrorDegrees = 1.25f;
        [SerializeField, Min(1f)] private float fireRange = 150f;
        [SerializeField, Range(1f, 180f)] private float maximumAimAngle = 80f;
        [SerializeField, Range(0f, 1.5f)] private float leadStrength = 0.9f;
        [SerializeField, Min(0.1f)] private float burstCycleDuration = 1.5f;

        public float PreferredRange => preferredRange;
        public float Aggression => aggression;
        public float OrbitWeight => orbitWeight;
        public float SteeringGain => steeringGain;
        public float BankGain => bankGain;
        public float AirbrakeAngle => airbrakeAngle;
        public float AirbrakeMinimumSpeed => airbrakeMinimumSpeed;
        public float BoostDistance => boostDistance;
        public float BoostAngle => boostAngle;
        public float ArenaRadius => arenaRadius;
        public float ReactionTime => reactionTime;
        public float AimErrorDegrees => aimErrorDegrees;
        public float FireRange => fireRange;
        public float MaximumAimAngle => maximumAimAngle;
        public float LeadStrength => leadStrength;
        public float BurstCycleDuration => burstCycleDuration;
    }
}
