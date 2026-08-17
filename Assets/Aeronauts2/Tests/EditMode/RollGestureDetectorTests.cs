using Aeronauts2.Input;
using NUnit.Framework;

namespace Aeronauts2.Tests
{
    public sealed class RollGestureDetectorTests
    {
        [Test]
        public void SameSideDoubleTapRequestsAutomaticRoll()
        {
            RollGestureDetector detector = new(0.25d);

            detector.RegisterPress(-1, 1d);
            detector.RegisterPress(-1, 1.2d);

            Assert.That(detector.ConsumeAutomaticRollDirection(), Is.EqualTo(-1));
            Assert.That(detector.ConsumeAutomaticRollDirection(), Is.EqualTo(0));
        }

        [Test]
        public void OppositeSidesDoNotFormDoubleTap()
        {
            RollGestureDetector detector = new(0.25d);

            detector.RegisterPress(-1, 1d);
            detector.RegisterPress(1, 1.1d);

            Assert.That(detector.ConsumeAutomaticRollDirection(), Is.EqualTo(0));
        }

        [Test]
        public void SlowSecondTapDoesNotRequestAutomaticRoll()
        {
            RollGestureDetector detector = new(0.25d);

            detector.RegisterPress(1, 1d);
            detector.RegisterPress(1, 1.3d);

            Assert.That(detector.ConsumeAutomaticRollDirection(), Is.EqualTo(0));
        }
    }
}
