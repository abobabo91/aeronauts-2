namespace Aeronauts2.Combat
{
    public readonly struct DamageReport
    {
        public DamageReport(
            bool wasCritical,
            float damageApplied,
            float shieldRemaining,
            float hullRemaining,
            bool eliminated)
        {
            WasCritical = wasCritical;
            DamageApplied = damageApplied;
            ShieldRemaining = shieldRemaining;
            HullRemaining = hullRemaining;
            Eliminated = eliminated;
        }

        public bool WasCritical { get; }
        public float DamageApplied { get; }
        public float ShieldRemaining { get; }
        public float HullRemaining { get; }
        public bool Eliminated { get; }
    }
}
