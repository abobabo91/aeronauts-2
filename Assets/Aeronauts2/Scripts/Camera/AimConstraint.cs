using UnityEngine;

namespace Aeronauts2.CameraSystem
{
    public static class AimConstraint
    {
        private const float FullHorizontalRotationDegrees = 179.999f;

        public static Vector3 ClampDirection(
            Vector3 worldDirection,
            Quaternion bodyRotation,
            float horizontalLimitDegrees,
            float upLimitDegrees,
            float downLimitDegrees)
        {
            if (worldDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return bodyRotation * Vector3.forward;
            }

            Vector3 localDirection = Quaternion.Inverse(bodyRotation)
                * worldDirection.normalized;
            float horizontalMagnitude = Mathf.Sqrt(
                localDirection.x * localDirection.x
                + localDirection.z * localDirection.z);
            float localYaw = Mathf.Atan2(localDirection.x, localDirection.z)
                * Mathf.Rad2Deg;
            float localPitch = -Mathf.Atan2(localDirection.y, horizontalMagnitude)
                * Mathf.Rad2Deg;

            float horizontalLimit = Mathf.Clamp(horizontalLimitDegrees, 0f, 180f);
            if (horizontalLimit < FullHorizontalRotationDegrees)
            {
                localYaw = Mathf.Clamp(localYaw, -horizontalLimit, horizontalLimit);
            }

            localPitch = Mathf.Clamp(
                localPitch,
                -Mathf.Clamp(upLimitDegrees, 0f, 89f),
                Mathf.Clamp(downLimitDegrees, 0f, 89f));
            Vector3 clampedLocalDirection = Quaternion.Euler(localPitch, localYaw, 0f)
                * Vector3.forward;
            return (bodyRotation * clampedLocalDirection).normalized;
        }
    }
}
