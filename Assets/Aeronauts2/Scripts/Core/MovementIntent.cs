using UnityEngine;

namespace Aeronauts2.Core
{
    public readonly struct MovementIntent
    {
        public MovementIntent(Vector2 steer, float roll, float accelerate, float airbrake, bool boost)
        {
            Steer = Vector2.ClampMagnitude(steer, 1f);
            Roll = Mathf.Clamp(roll, -1f, 1f);
            Accelerate = Mathf.Clamp01(accelerate);
            Airbrake = Mathf.Clamp01(airbrake);
            Boost = boost;
        }

        public Vector2 Steer { get; }

        public float Roll { get; }

        public float Accelerate { get; }

        public float Airbrake { get; }

        public bool Boost { get; }

        public static MovementIntent None => new(Vector2.zero, 0f, 0f, 0f, false);
    }
}
