using Aeronauts2.CameraSystem;
using Aeronauts2.Flight;
using UnityEngine;
using UnityEngine.UI;

namespace Aeronauts2.UI
{
    public sealed class FlightDebugHud : MonoBehaviour
    {
        [SerializeField] private FlightMotor motor;
        [SerializeField] private AimCameraController aimCamera;
        [SerializeField] private Text statusText;

        public void Configure(FlightMotor flightMotor, AimCameraController cameraController, Text text)
        {
            motor = flightMotor;
            aimCamera = cameraController;
            statusText = text;
        }

        private void Update()
        {
            if (motor == null || statusText == null)
            {
                return;
            }

            FlightState state = motor.State;
            float boostPercent = motor.Tuning != null && motor.Tuning.BoostCapacity > 0f
                ? state.BoostEnergy / motor.Tuning.BoostCapacity * 100f
                : 0f;
            string flightMode = state.IsAirbraking
                ? "AIRBRAKE"
                : state.IsPerfectBoosting
                    ? "SLINGSHOT"
                    : state.IsBoosting
                        ? "BOOST"
                        : state.AutomaticRollDegreesRemaining > 0f
                            ? "AUTO ROLL"
                            : "CRUISE";
            string aimLimitMode = aimCamera == null
                ? "N/A"
                : aimCamera.ActiveLimitMode == AimLimitMode.Full360
                    ? "FULL 360"
                    : "CONSTRAINED";
            string viewMode = aimCamera != null
                && aimCamera.ActiveViewMode == CameraViewMode.Forward
                    ? "FORWARD"
                    : "AIM";
            statusText.text =
                $"SPEED  {state.Speed,6:0.0}\n"
                + $"BOOST {boostPercent,6:0}%\n"
                + $"MODE   {flightMode}\n"
                + $"VIEW   {viewMode}\n"
                + $"AIM LIMIT {aimLimitMode}  H{aimCamera?.ActiveHorizontalLimitDegrees ?? 0f:0} U{aimCamera?.ActiveUpLimitDegrees ?? 0f:0} D{aimCamera?.ActiveDownLimitDegrees ?? 0f:0}\n"
                + $"AIM/BODY {aimCamera?.BodyAimAngleDegrees ?? 0f,5:0.0} deg\n"
                + $"ALIGNMENT ERROR {motor.VelocityAlignmentErrorDegrees,6:0.000} deg";
        }
    }
}
