namespace Aeronauts2.Combat
{
    public readonly struct DamageReport
    {
        public DamageReport(
            bool wasCritical,
            float damageApplied,
            float shieldDamage,
            float hullDamage,
            float shieldRemaining,
            float hullRemaining,
            float maximumShield,
            float maximumHull,
            bool shieldBroken,
            bool shieldBypassed,
            bool eliminated)
        {
            WasCritical = wasCritical;
            DamageApplied = damageApplied;
            ShieldDamage = shieldDamage;
            HullDamage = hullDamage;
            ShieldRemaining = shieldRemaining;
            HullRemaining = hullRemaining;
            MaximumShield = maximumShield;
            MaximumHull = maximumHull;
            ShieldBroken = shieldBroken;
            ShieldBypassed = shieldBypassed;
            Eliminated = eliminated;
        }

        public bool WasCritical { get; }
        public float DamageApplied { get; }
        public float ShieldDamage { get; }
        public float HullDamage { get; }
        public float ShieldRemaining { get; }
        public float HullRemaining { get; }
        public float MaximumShield { get; }
        public float MaximumHull { get; }
        public bool ShieldBroken { get; }
        public bool ShieldBypassed { get; }
        public bool Eliminated { get; }
    }
}
