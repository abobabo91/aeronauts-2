using UnityEngine;

namespace Aeronauts2.CameraSystem
{
    public enum AimLimitMode
    {
        Constrained = 0,
        Full360 = 1,
    }

    [CreateAssetMenu(fileName = "AimTuning", menuName = "Aeronauts 2/Aim Tuning")]
    public sealed class AimTuning : ScriptableObject
    {
        [Header("Mouse")]
        [SerializeField, Min(0.001f)] private float mouseDegreesPerPixel = 0.08f;

        [Header("Controller")]
        [SerializeField, Range(0f, 0.95f)] private float controllerDeadZone = 0.15f;
        [SerializeField, Min(1f)] private float controllerResponseExponent = 1.6f;
        [SerializeField, Min(1f)] private float controllerDegreesPerSecond = 190f;

        [Header("View limits")]
        [SerializeField] private AimLimitMode defaultLimitMode = AimLimitMode.Constrained;
        [SerializeField, Range(0f, 180f)] private float constrainedHorizontalDegrees = 110f;
        [SerializeField, Range(0f, 89f)] private float constrainedUpDegrees = 78f;
        [SerializeField, Range(0f, 89f)] private float constrainedDownDegrees = 78f;
        [SerializeField, Range(0f, 180f)] private float full360HorizontalDegrees = 180f;
        [SerializeField, Range(0f, 89f)] private float full360UpDegrees = 89f;
        [SerializeField, Range(0f, 89f)] private float full360DownDegrees = 89f;

        [Header("Third-person camera")]
        [SerializeField, Min(0.1f)] private float distance = 10f;
        [SerializeField] private float height = 2.2f;
        [SerializeField] private float shoulderOffset = 1.4f;
        [SerializeField, Min(0f)] private float followSharpness = 18f;

        public float MouseDegreesPerPixel => mouseDegreesPerPixel;
        public float ControllerDeadZone => controllerDeadZone;
        public float ControllerResponseExponent => controllerResponseExponent;
        public float ControllerDegreesPerSecond => controllerDegreesPerSecond;
        public AimLimitMode DefaultLimitMode => defaultLimitMode;
        public float Distance => distance;
        public float Height => height;
        public float ShoulderOffset => shoulderOffset;
        public float FollowSharpness => followSharpness;

        public float GetHorizontalLimit(AimLimitMode mode)
        {
            return mode == AimLimitMode.Full360
                ? full360HorizontalDegrees
                : constrainedHorizontalDegrees;
        }

        public float GetUpLimit(AimLimitMode mode)
        {
            return mode == AimLimitMode.Full360
                ? full360UpDegrees
                : constrainedUpDegrees;
        }

        public float GetDownLimit(AimLimitMode mode)
        {
            return mode == AimLimitMode.Full360
                ? full360DownDegrees
                : constrainedDownDegrees;
        }
    }
}
