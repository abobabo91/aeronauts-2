namespace Aeronauts2.Combat
{
    public readonly struct WeaponDamageProfile
    {
        public WeaponDamageProfile(
            float shieldDamage,
            float bodyHullDamage,
            float criticalHullDamage,
            bool criticalBypassesShield)
        {
            ShieldDamage = shieldDamage;
            BodyHullDamage = bodyHullDamage;
            CriticalHullDamage = criticalHullDamage;
            CriticalBypassesShield = criticalBypassesShield;
        }

        public float ShieldDamage { get; }
        public float BodyHullDamage { get; }
        public float CriticalHullDamage { get; }
        public bool CriticalBypassesShield { get; }
    }
}
