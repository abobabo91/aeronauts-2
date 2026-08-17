using Aeronauts2.Core;
using Aeronauts2.Flight;
using Aeronauts2.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Aeronauts2.CameraSystem
{
    public enum CameraViewMode
    {
        Aim = 0,
        Forward = 1,
    }

    public sealed class AimCameraController : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera controlledCamera;
        [SerializeField] private FlightMotor target;
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private AimTuning tuning;

        private float yaw;
        private float pitch;
        private bool initialized;
        private AimLimitMode activeLimitMode;
        private bool targetResetSubscribed;
        private bool recenterRequested;
        private CameraViewMode activeViewMode;

        public Vector3 AimForward => controlledCamera != null ? controlledCamera.transform.forward : transform.forward;

        public Ray AimRay => controlledCamera != null
            ? new Ray(controlledCamera.transform.position, controlledCamera.transform.forward)
            : new Ray(transform.position, transform.forward);

        public float BodyAimAngleDegrees => target != null
            ? Vector3.Angle(target.transform.forward, AimForward)
            : 0f;

        public AimLimitMode ActiveLimitMode => activeLimitMode;
        public CameraViewMode ActiveViewMode => activeViewMode;

        public float ActiveHorizontalLimitDegrees => tuning != null
            ? tuning.GetHorizontalLimit(activeLimitMode)
            : 0f;

        public float ActiveUpLimitDegrees => tuning != null
            ? tuning.GetUpLimit(activeLimitMode)
            : 0f;

        public float ActiveDownLimitDegrees => tuning != null
            ? tuning.GetDownLimit(activeLimitMode)
            : 0f;

        public void Configure(
            UnityEngine.Camera cameraToControl,
            FlightMotor flightTarget,
            PlayerInputReader inputReader,
            AimTuning aimTuning)
        {
            UnsubscribeFromTargetReset();
            controlledCamera = cameraToControl;
            target = flightTarget;
            input = inputReader;
            tuning = aimTuning;
            activeLimitMode = tuning != null
                ? tuning.DefaultLimitMode
                : AimLimitMode.Constrained;
            activeViewMode = CameraViewMode.Aim;
            initialized = false;
            SubscribeToTargetReset();
        }

        public void SetLimitMode(AimLimitMode mode)
        {
            activeLimitMode = mode;
        }

        public void SetViewMode(CameraViewMode mode)
        {
            if (activeViewMode == mode)
            {
                return;
            }

            activeViewMode = mode;
            recenterRequested = true;
        }

        private void Awake()
        {
            activeLimitMode = tuning != null
                ? tuning.DefaultLimitMode
                : AimLimitMode.Constrained;
            activeViewMode = CameraViewMode.Aim;
        }

        private void OnEnable()
        {
            SubscribeToTargetReset();
            LockCursor();
        }

        private void OnDisable()
        {
            UnsubscribeFromTargetReset();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Mouse.current != null
                && Mouse.current.leftButton.wasPressedThisFrame
                && Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }

            if (input == null || tuning == null)
            {
                return;
            }

            if (input.ConsumeToggleAimLimitsPressed())
            {
                activeLimitMode = activeLimitMode == AimLimitMode.Constrained
                    ? AimLimitMode.Full360
                    : AimLimitMode.Constrained;
            }

            if (input.ConsumeToggleViewPressed())
            {
                SetViewMode(activeViewMode == CameraViewMode.Aim
                    ? CameraViewMode.Forward
                    : CameraViewMode.Aim);
            }

            if (Cursor.lockState != CursorLockMode.Locked
                || activeViewMode == CameraViewMode.Forward)
            {
                return;
            }

            AimIntent intent = input.ReadAimIntent(Time.unscaledDeltaTime, tuning);
            yaw += intent.AngularDeltaDegrees.x;
            pitch = Mathf.Clamp(pitch + intent.AngularDeltaDegrees.y, -89f, 89f);
        }

        private void LateUpdate()
        {
            if (controlledCamera == null || target == null || tuning == null)
            {
                return;
            }

            if (!initialized
                || recenterRequested
                || activeViewMode == CameraViewMode.Forward)
            {
                SetAnglesFromDirection(target.transform.forward);
                initialized = true;
                recenterRequested = false;
            }

            Vector3 viewDirection;
            if (activeViewMode == CameraViewMode.Forward)
            {
                viewDirection = target.transform.forward;
            }
            else
            {
                Vector3 desiredDirection = Quaternion.Euler(pitch, yaw, 0f)
                    * Vector3.forward;
                viewDirection = AimConstraint.ClampDirection(
                    desiredDirection,
                    target.transform.rotation,
                    tuning.GetHorizontalLimit(activeLimitMode),
                    tuning.GetUpLimit(activeLimitMode),
                    tuning.GetDownLimit(activeLimitMode));
            }

            SetAnglesFromDirection(viewDirection);
            Quaternion aimRotation = Quaternion.LookRotation(viewDirection, Vector3.up);
            Vector3 pivot = target.transform.position;
            Vector3 desiredPosition = pivot
                - aimRotation * Vector3.forward * tuning.Distance
                + aimRotation * Vector3.up * tuning.Height
                + aimRotation * Vector3.right * tuning.ShoulderOffset;

            float blend = 1f - Mathf.Exp(-tuning.FollowSharpness * Time.unscaledDeltaTime);
            controlledCamera.transform.position = Vector3.Lerp(
                controlledCamera.transform.position,
                desiredPosition,
                blend);
            controlledCamera.transform.rotation = aimRotation;
        }

        private void SetAnglesFromDirection(Vector3 direction)
        {
            Vector3 normalizedDirection = direction.normalized;
            yaw = Mathf.Atan2(normalizedDirection.x, normalizedDirection.z) * Mathf.Rad2Deg;
            pitch = -Mathf.Atan2(
                normalizedDirection.y,
                Mathf.Sqrt(
                    normalizedDirection.x * normalizedDirection.x
                    + normalizedDirection.z * normalizedDirection.z)) * Mathf.Rad2Deg;
        }

        private void SubscribeToTargetReset()
        {
            if (targetResetSubscribed || target == null || !isActiveAndEnabled)
            {
                return;
            }

            target.FlightReset += OnTargetFlightReset;
            targetResetSubscribed = true;
        }

        private void UnsubscribeFromTargetReset()
        {
            if (!targetResetSubscribed || target == null)
            {
                targetResetSubscribed = false;
                return;
            }

            target.FlightReset -= OnTargetFlightReset;
            targetResetSubscribed = false;
        }

        private void OnTargetFlightReset()
        {
            recenterRequested = true;
        }

        private static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
