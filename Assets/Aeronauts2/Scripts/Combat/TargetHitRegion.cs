using UnityEngine;

namespace Aeronauts2.Combat
{
    public sealed class TargetHitRegion : MonoBehaviour
    {
        [SerializeField] private CombatantHealth target;
        [SerializeField] private HitRegionType regionType;

        public CombatantHealth Target => target;
        public HitRegionType RegionType => regionType;

        public void Configure(CombatantHealth combatant, HitRegionType type)
        {
            target = combatant;
            regionType = type;
        }

        public DamageReport ApplyDamage(in WeaponDamageProfile damageProfile)
        {
            return target == null
                ? default
                : target.ApplyDamage(damageProfile, regionType);
        }
    }
}
