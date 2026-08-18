using UnityEngine;

namespace Aeronauts2.Combat
{
    public static class TargetSimulation
    {
        private const float StateEpsilon = 0.0001f;

        public static bool Step(
            ref TargetState state,
            TargetTuning tuning,
            float deltaTime)
        {
            if (tuning == null || deltaTime <= 0f)
            {
                return false;
            }

            if (state.IsEliminated)
            {
                state.RespawnRemaining = Mathf.Max(
                    0f,
                    state.RespawnRemaining - deltaTime);
                if (state.RespawnRemaining > StateEpsilon)
                {
                    return false;
                }

                state = TargetState.Create(tuning);
                return true;
            }

            return false;
        }

        public static DamageReport ApplyDamage(
            ref TargetState state,
            in WeaponDamageProfile damageProfile,
            HitRegionType regionType,
            TargetTuning tuning)
        {
            bool critical = regionType == HitRegionType.Critical;
            bool shieldBypassed = critical
                && damageProfile.CriticalBypassesShield
                && state.Shield > StateEpsilon;
            float requestedDamage = state.Shield > StateEpsilon && !shieldBypassed
                ? damageProfile.ShieldDamage
                : critical
                    ? damageProfile.CriticalHullDamage
                    : damageProfile.BodyHullDamage;
            if (tuning == null || state.IsEliminated || requestedDamage <= 0f)
            {
                return new DamageReport(
                    critical,
                    0f,
                    0f,
                    0f,
                    state.Shield,
                    state.Hull,
                    tuning != null ? tuning.MaximumShield : 0f,
                    tuning != null ? tuning.MaximumHull : 0f,
                    false,
                    false,
                    false);
            }

            float shieldBeforeDamage = state.Shield;
            float shieldDamage = 0f;
            float hullDamage = 0f;
            if (state.Shield > StateEpsilon && !shieldBypassed)
            {
                shieldDamage = Mathf.Min(state.Shield, requestedDamage);
                state.Shield -= shieldDamage;
            }
            else
            {
                hullDamage = Mathf.Min(state.Hull, requestedDamage);
                state.Hull -= hullDamage;
            }

            float damageApplied = shieldDamage + hullDamage;
            bool shieldBroken = shieldBeforeDamage > StateEpsilon
                && state.Shield <= StateEpsilon;
            bool eliminated = state.Hull <= StateEpsilon;
            if (eliminated)
            {
                state.Hull = 0f;
                state.IsEliminated = true;
                state.RespawnRemaining = tuning.RespawnDelay;
            }

            return new DamageReport(
                critical,
                damageApplied,
                shieldDamage,
                hullDamage,
                state.Shield,
                state.Hull,
                tuning.MaximumShield,
                tuning.MaximumHull,
                shieldBroken,
                shieldBypassed,
                eliminated);
        }
    }
}
