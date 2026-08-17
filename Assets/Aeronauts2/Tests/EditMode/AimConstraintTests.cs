using Aeronauts2.CameraSystem;
using NUnit.Framework;
using UnityEngine;

namespace Aeronauts2.Tests
{
    public sealed class AimConstraintTests
    {
        [Test]
        public void DefaultProfilesUseRecommendedAndFull360Limits()
        {
            AimTuning tuning = ScriptableObject.CreateInstance<AimTuning>();

            Assert.That(tuning.DefaultLimitMode, Is.EqualTo(AimLimitMode.Constrained));
            Assert.That(tuning.GetHorizontalLimit(AimLimitMode.Constrained), Is.EqualTo(110f));
            Assert.That(tuning.GetUpLimit(AimLimitMode.Constrained), Is.EqualTo(78f));
            Assert.That(tuning.GetDownLimit(AimLimitMode.Constrained), Is.EqualTo(78f));
            Assert.That(tuning.GetHorizontalLimit(AimLimitMode.Full360), Is.EqualTo(180f));
            Assert.That(tuning.GetUpLimit(AimLimitMode.Full360), Is.EqualTo(89f));
            Assert.That(tuning.GetDownLimit(AimLimitMode.Full360), Is.EqualTo(89f));

            Object.DestroyImmediate(tuning);
        }

        [Test]
        public void ConstrainedProfileClampsHorizontalAimRelativeToBody()
        {
            Vector3 desiredDirection = Quaternion.Euler(0f, 150f, 0f) * Vector3.forward;

            Vector3 constrainedDirection = AimConstraint.ClampDirection(
                desiredDirection,
                Quaternion.identity,
                110f,
                78f,
                78f);

            float constrainedYaw = Mathf.Atan2(
                constrainedDirection.x,
                constrainedDirection.z) * Mathf.Rad2Deg;
            Assert.That(constrainedYaw, Is.EqualTo(110f).Within(0.01f));
        }

        [Test]
        public void Full360ProfilePreservesRearAim()
        {
            Vector3 desiredDirection = Quaternion.Euler(0f, 170f, 0f) * Vector3.forward;

            Vector3 constrainedDirection = AimConstraint.ClampDirection(
                desiredDirection,
                Quaternion.identity,
                180f,
                89f,
                89f);

            Assert.That(Vector3.Angle(desiredDirection, constrainedDirection), Is.LessThan(0.01f));
        }

        [Test]
        public void VerticalLimitsRemainBodyRelativeWhileBodyIsRolled()
        {
            Quaternion bodyRotation = Quaternion.Euler(20f, 45f, 60f);
            Vector3 desiredLocalDirection = Quaternion.Euler(-86f, 35f, 0f) * Vector3.forward;
            Vector3 desiredWorldDirection = bodyRotation * desiredLocalDirection;

            Vector3 constrainedWorldDirection = AimConstraint.ClampDirection(
                desiredWorldDirection,
                bodyRotation,
                110f,
                78f,
                78f);
            Vector3 constrainedLocalDirection = Quaternion.Inverse(bodyRotation)
                * constrainedWorldDirection;
            float constrainedPitch = -Mathf.Atan2(
                constrainedLocalDirection.y,
                Mathf.Sqrt(
                    constrainedLocalDirection.x * constrainedLocalDirection.x
                    + constrainedLocalDirection.z * constrainedLocalDirection.z)) * Mathf.Rad2Deg;

            Assert.That(constrainedPitch, Is.EqualTo(-78f).Within(0.01f));
        }
    }
}
