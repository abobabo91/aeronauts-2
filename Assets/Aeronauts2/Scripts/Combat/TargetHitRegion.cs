using UnityEngine;

namespace Aeronauts2.Combat
{
    public sealed class TargetHitRegion : MonoBehaviour
    {
        [SerializeField] private TargetDummy target;
        [SerializeField] private HitRegionType regionType;

        public TargetDummy Target => target;
        public HitRegionType RegionType => regionType;

        public void Configure(TargetDummy targetDummy, HitRegionType type)
        {
            target = targetDummy;
            regionType = type;
        }

        public DamageReport ApplyDamage(float damage, float criticalMultiplier)
        {
            return target == null
                ? default
                : target.ApplyDamage(damage, regionType, criticalMultiplier);
        }
    }
}
