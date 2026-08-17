namespace Aeronauts2.Input
{
    public sealed class RollGestureDetector
    {
        private readonly double doubleTapWindow;
        private double lastLeftPressTime = double.NegativeInfinity;
        private double lastRightPressTime = double.NegativeInfinity;
        private int pendingAutomaticRollDirection;

        public RollGestureDetector(double doubleTapWindowSeconds)
        {
            doubleTapWindow = doubleTapWindowSeconds;
        }

        public void RegisterPress(int direction, double time)
        {
            if (direction == 0)
            {
                return;
            }

            direction = direction < 0 ? -1 : 1;
            double lastPressTime = direction < 0 ? lastLeftPressTime : lastRightPressTime;
            double elapsed = time - lastPressTime;
            if (elapsed >= 0d && elapsed <= doubleTapWindow)
            {
                pendingAutomaticRollDirection = direction;
                if (direction < 0)
                {
                    lastLeftPressTime = double.NegativeInfinity;
                }
                else
                {
                    lastRightPressTime = double.NegativeInfinity;
                }

                return;
            }

            if (direction < 0)
            {
                lastLeftPressTime = time;
            }
            else
            {
                lastRightPressTime = time;
            }
        }

        public int ConsumeAutomaticRollDirection()
        {
            int direction = pendingAutomaticRollDirection;
            pendingAutomaticRollDirection = 0;
            return direction;
        }
    }
}
