using UnityEngine;

namespace Aeronauts2.Combat
{
    [CreateAssetMenu(fileName = "WeaponTuning", menuName = "Aeronauts 2/Weapon Tuning")]
    public sealed class WeaponTuning : ScriptableObject
    {
        [Header("Primary projectile")]
        [SerializeField, Min(0.1f)] private float primaryProjectileSpeed = 120f;
        [SerializeField, Min(0.001f)] private float primaryProjectileRadius = 0.16f;
        [SerializeField, Min(0.1f)] private float primaryProjectileLifetime = 3f;
        [SerializeField, Min(0.1f)] private float primaryShotsPerSecond = 8f;
        [SerializeField, Min(0f)] private float primaryShieldDamage = 16f;
        [SerializeField, Min(0f)] private float primaryBodyHullDamage = 16f;
        [SerializeField, Min(0f)] private float primaryCriticalHullDamage = 100f;

        [Header("Railgun")]
        [SerializeField, Min(1f)] private float railgunRange = 500f;
        [SerializeField, Min(0.01f)] private float railgunCooldown = 0.85f;
        [SerializeField, Min(0)] private int railgunCapacity = 3;
        [SerializeField, Min(0f)] private float railgunShieldDamage = 80f;
        [SerializeField, Min(0f)] private float railgunBodyHullDamage = 80f;
        [SerializeField, Min(0f)] private float railgunCriticalHullDamage = 100f;
        [SerializeField] private bool railgunCriticalBypassesShield = true;

        public float PrimaryProjectileSpeed => primaryProjectileSpeed;
        public float PrimaryProjectileRadius => primaryProjectileRadius;
        public float PrimaryProjectileLifetime => primaryProjectileLifetime;
        public float PrimaryShotsPerSecond => primaryShotsPerSecond;
        public float PrimaryFireInterval => 1f / primaryShotsPerSecond;
        public WeaponDamageProfile PrimaryDamageProfile => new(
            primaryShieldDamage,
            primaryBodyHullDamage,
            primaryCriticalHullDamage,
            false);
        public float RailgunRange => railgunRange;
        public float RailgunCooldown => railgunCooldown;
        public int RailgunCapacity => railgunCapacity;
        public WeaponDamageProfile RailgunDamageProfile => new(
            railgunShieldDamage,
            railgunBodyHullDamage,
            railgunCriticalHullDamage,
            railgunCriticalBypassesShield);
    }
}
