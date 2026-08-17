namespace Aeronauts2.Core
{
    public readonly struct ManeuverIntent
    {
        public ManeuverIntent(int automaticRollDirection)
        {
            AutomaticRollDirection = automaticRollDirection < 0
                ? -1
                : automaticRollDirection > 0
                    ? 1
                    : 0;
        }

        public int AutomaticRollDirection { get; }

        public static ManeuverIntent None => new(0);
    }
}
