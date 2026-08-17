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
        [SerializeField, Min(0f)] private float primaryDamage = 16f;

        [Header("Railgun")]
        [SerializeField, Min(1f)] private float railgunRange = 500f;
        [SerializeField, Min(0.01f)] private float railgunCooldown = 0.85f;
        [SerializeField, Min(0)] private int railgunCapacity = 3;
        [SerializeField, Min(0f)] private float railgunDamage = 80f;

        [Header("Hit regions")]
        [SerializeField, Min(1f)] private float criticalDamageMultiplier = 2.5f;

        public float PrimaryProjectileSpeed => primaryProjectileSpeed;
        public float PrimaryProjectileRadius => primaryProjectileRadius;
        public float PrimaryProjectileLifetime => primaryProjectileLifetime;
        public float PrimaryShotsPerSecond => primaryShotsPerSecond;
        public float PrimaryFireInterval => 1f / primaryShotsPerSecond;
        public float PrimaryDamage => primaryDamage;
        public float RailgunRange => railgunRange;
        public float RailgunCooldown => railgunCooldown;
        public int RailgunCapacity => railgunCapacity;
        public float RailgunDamage => railgunDamage;
        public float CriticalDamageMultiplier => criticalDamageMultiplier;
    }
}
