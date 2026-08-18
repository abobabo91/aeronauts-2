namespace Aeronauts2.Combat
{
    public struct TargetState
    {
        public float Shield;
        public float Hull;
        public float RespawnRemaining;
        public bool IsEliminated;

        public static TargetState Create(TargetTuning tuning)
        {
            if (tuning == null)
            {
                return default;
            }

            return new TargetState
            {
                Shield = tuning.MaximumShield,
                Hull = tuning.MaximumHull,
                RespawnRemaining = 0f,
                IsEliminated = false,
            };
        }
    }
}
