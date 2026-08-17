using UnityEngine;

namespace Aeronauts2.Core
{
    public readonly struct AimIntent
    {
        public AimIntent(Vector2 angularDeltaDegrees)
        {
            AngularDeltaDegrees = angularDeltaDegrees;
        }

        public Vector2 AngularDeltaDegrees { get; }

        public static AimIntent None => new(Vector2.zero);
    }
}
