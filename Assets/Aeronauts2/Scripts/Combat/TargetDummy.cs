using UnityEngine;

namespace Aeronauts2.Combat
{
    public sealed class TargetDummy : MonoBehaviour
    {
        [SerializeField] private TargetTuning tuning;

        private Collider[] hitColliders;
        private Renderer[] renderers;
        private float respawnRemaining;

        public float Shield { get; private set; }
        public float Hull { get; private set; }
        public bool IsEliminated { get; private set; }

        public void Configure(TargetTuning targetTuning)
        {
            tuning = targetTuning;
        }

        private void Awake()
        {
            CachePresentation();
            ResetTarget();
        }

        private void FixedUpdate()
        {
            if (!IsEliminated || tuning == null)
            {
                return;
            }

            respawnRemaining -= Time.fixedDeltaTime;
            if (respawnRemaining <= 0f)
            {
                ResetTarget();
            }
        }

        public DamageReport ApplyDamage(
            float baseDamage,
            HitRegionType regionType,
            float criticalMultiplier)
        {
            if (tuning == null || IsEliminated || baseDamage <= 0f)
            {
                return new DamageReport(
                    regionType == HitRegionType.Critical,
                    0f,
                    Shield,
                    Hull,
                    IsEliminated);
            }

            bool critical = regionType == HitRegionType.Critical;
            float requestedDamage = baseDamage * (critical ? criticalMultiplier : 1f);
            float damageRemaining = requestedDamage;
            float shieldDamage = Mathf.Min(Shield, damageRemaining);
            Shield -= shieldDamage;
            damageRemaining -= shieldDamage;
            float hullDamage = Mathf.Min(Hull, damageRemaining);
            Hull -= hullDamage;

            IsEliminated = Hull <= 0f;
            if (IsEliminated)
            {
                Hull = 0f;
                respawnRemaining = tuning.RespawnDelay;
                SetPresentationActive(false);
            }

            return new DamageReport(
                critical,
                shieldDamage + hullDamage,
                Shield,
                Hull,
                IsEliminated);
        }

        public void ResetTarget()
        {
            if (tuning == null)
            {
                return;
            }

            CachePresentation();
            Shield = tuning.MaximumShield;
            Hull = tuning.MaximumHull;
            IsEliminated = false;
            respawnRemaining = 0f;
            SetPresentationActive(true);
        }

        private void CachePresentation()
        {
            hitColliders ??= GetComponentsInChildren<Collider>(true);
            renderers ??= GetComponentsInChildren<Renderer>(true);
        }

        private void SetPresentationActive(bool active)
        {
            foreach (Collider hitCollider in hitColliders)
            {
                hitCollider.enabled = active;
            }

            foreach (Renderer targetRenderer in renderers)
            {
                targetRenderer.enabled = active;
            }
        }
    }
}
