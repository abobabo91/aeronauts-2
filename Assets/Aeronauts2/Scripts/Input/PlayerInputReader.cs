using Aeronauts2.CameraSystem;
using Aeronauts2.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Aeronauts2.Input
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private const string FlightMapName = "Flight";

        [SerializeField] private InputActionAsset actions;
        [SerializeField] private InputTuning tuning;

        private InputActionMap flightMap;
        private InputAction bodySteerAction;
        private InputAction rollLeftAction;
        private InputAction rollRightAction;
        private InputAction accelerateAction;
        private InputAction airbrakeAction;
        private InputAction boostAction;
        private InputAction aimAction;
        private InputAction primaryFireAction;
        private InputAction secondaryFireAction;
        private InputAction resetAction;
        private InputAction toggleAimLimitsAction;
        private InputAction toggleViewAction;
        private RollGestureDetector rollGestureDetector;
        private bool rollCallbacksSubscribed;
        private bool fireCallbacksSubscribed;
        private int pendingSecondaryPresses;

        public void Configure(InputActionAsset actionAsset, InputTuning inputTuning)
        {
            UnsubscribeRollCallbacks();
            UnsubscribeFireCallbacks();
            actions = actionAsset;
            tuning = inputTuning;
            CacheActions();
            CreateRollGestureDetector();
            if (isActiveAndEnabled)
            {
                SubscribeRollCallbacks();
                SubscribeFireCallbacks();
            }
        }

        private void OnEnable()
        {
            CacheActions();
            CreateRollGestureDetector();
            SubscribeRollCallbacks();
            SubscribeFireCallbacks();
            flightMap?.Enable();
        }

        private void OnDisable()
        {
            UnsubscribeRollCallbacks();
            UnsubscribeFireCallbacks();
            pendingSecondaryPresses = 0;
            flightMap?.Disable();
        }

        public MovementIntent ReadMovementIntent()
        {
            if (flightMap == null)
            {
                return MovementIntent.None;
            }

            return new MovementIntent(
                bodySteerAction.ReadValue<Vector2>(),
                rollRightAction.ReadValue<float>() - rollLeftAction.ReadValue<float>(),
                accelerateAction.ReadValue<float>(),
                airbrakeAction.ReadValue<float>(),
                boostAction.IsPressed());
        }

        public ManeuverIntent ConsumeManeuverIntent()
        {
            return rollGestureDetector == null
                ? ManeuverIntent.None
                : new ManeuverIntent(rollGestureDetector.ConsumeAutomaticRollDirection());
        }

        public FireIntent ConsumeFireIntent()
        {
            if (flightMap == null)
            {
                return FireIntent.None;
            }

            bool secondaryPressed = pendingSecondaryPresses > 0;
            if (secondaryPressed)
            {
                pendingSecondaryPresses--;
            }

            return new FireIntent(primaryFireAction.IsPressed(), secondaryPressed);
        }

        public AimIntent ReadAimIntent(float deltaTime, AimTuning tuning)
        {
            if (aimAction == null || tuning == null)
            {
                return AimIntent.None;
            }

            Vector2 rawLook = aimAction.ReadValue<Vector2>();
            if (aimAction.activeControl?.device is Pointer)
            {
                return new AimIntent(new Vector2(
                    rawLook.x * tuning.MouseDegreesPerPixel,
                    -rawLook.y * tuning.MouseDegreesPerPixel));
            }

            float magnitude = rawLook.magnitude;
            if (magnitude <= tuning.ControllerDeadZone)
            {
                return AimIntent.None;
            }

            float normalizedMagnitude = Mathf.InverseLerp(tuning.ControllerDeadZone, 1f, magnitude);
            float curvedMagnitude = Mathf.Pow(normalizedMagnitude, tuning.ControllerResponseExponent);
            Vector2 angularDelta = rawLook.normalized
                * curvedMagnitude
                * tuning.ControllerDegreesPerSecond
                * deltaTime;
            angularDelta.y = -angularDelta.y;
            return new AimIntent(angularDelta);
        }

        public bool ConsumeResetPressed()
        {
            return resetAction != null && resetAction.WasPressedThisFrame();
        }

        public bool ConsumeToggleAimLimitsPressed()
        {
            return toggleAimLimitsAction != null
                && toggleAimLimitsAction.WasPressedThisFrame();
        }

        public bool ConsumeToggleViewPressed()
        {
            return toggleViewAction != null && toggleViewAction.WasPressedThisFrame();
        }

        private void CacheActions()
        {
            if (actions == null)
            {
                return;
            }

            flightMap = actions.FindActionMap(FlightMapName, true);
            bodySteerAction = flightMap.FindAction("BodySteer", true);
            rollLeftAction = flightMap.FindAction("RollLeft", true);
            rollRightAction = flightMap.FindAction("RollRight", true);
            accelerateAction = flightMap.FindAction("Accelerate", true);
            airbrakeAction = flightMap.FindAction("Airbrake", true);
            boostAction = flightMap.FindAction("Boost", true);
            aimAction = flightMap.FindAction("Aim", true);
            primaryFireAction = flightMap.FindAction("PrimaryFire", true);
            secondaryFireAction = flightMap.FindAction("SecondaryFire", true);
            resetAction = flightMap.FindAction("Reset", true);
            toggleAimLimitsAction = flightMap.FindAction("ToggleAimLimits", true);
            toggleViewAction = flightMap.FindAction("ToggleView", true);
        }

        private void CreateRollGestureDetector()
        {
            rollGestureDetector = tuning == null
                ? null
                : new RollGestureDetector(tuning.AutomaticRollDoubleTapWindow);
        }

        private void SubscribeRollCallbacks()
        {
            if (rollCallbacksSubscribed || rollLeftAction == null || rollRightAction == null)
            {
                return;
            }

            rollLeftAction.performed += OnRollLeftPerformed;
            rollRightAction.performed += OnRollRightPerformed;
            rollCallbacksSubscribed = true;
        }

        private void UnsubscribeRollCallbacks()
        {
            if (!rollCallbacksSubscribed)
            {
                return;
            }

            rollLeftAction.performed -= OnRollLeftPerformed;
            rollRightAction.performed -= OnRollRightPerformed;
            rollCallbacksSubscribed = false;
        }

        private void OnRollLeftPerformed(InputAction.CallbackContext context)
        {
            rollGestureDetector?.RegisterPress(-1, context.time);
        }

        private void OnRollRightPerformed(InputAction.CallbackContext context)
        {
            rollGestureDetector?.RegisterPress(1, context.time);
        }

        private void SubscribeFireCallbacks()
        {
            if (fireCallbacksSubscribed || secondaryFireAction == null)
            {
                return;
            }

            secondaryFireAction.performed += OnSecondaryFirePerformed;
            fireCallbacksSubscribed = true;
        }

        private void UnsubscribeFireCallbacks()
        {
            if (!fireCallbacksSubscribed)
            {
                return;
            }

            secondaryFireAction.performed -= OnSecondaryFirePerformed;
            fireCallbacksSubscribed = false;
        }

        private void OnSecondaryFirePerformed(InputAction.CallbackContext context)
        {
            pendingSecondaryPresses++;
        }
    }
}
