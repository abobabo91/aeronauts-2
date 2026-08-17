using Aeronauts2.Core;
using UnityEngine;

namespace Aeronauts2.Combat
{
    public static class WeaponSimulation
    {
        private const float CooldownEpsilon = 0.0001f;

        public static WeaponShotType Step(
            ref WeaponState state,
            in FireIntent intent,
            WeaponTuning tuning,
            float deltaTime)
        {
            if (tuning == null || deltaTime <= 0f)
            {
                return WeaponShotType.None;
            }

            state.PrimaryCooldownRemaining = Mathf.Max(
                -deltaTime,
                state.PrimaryCooldownRemaining - deltaTime);
            state.RailgunCooldownRemaining = Mathf.Max(
                -deltaTime,
                state.RailgunCooldownRemaining - deltaTime);

            if (intent.SecondaryPressed
                && state.RailgunAmmo > 0
                && state.RailgunCooldownRemaining <= CooldownEpsilon)
            {
                state.RailgunAmmo--;
                state.RailgunCooldownRemaining = Mathf.Max(
                    0f,
                    state.RailgunCooldownRemaining + tuning.RailgunCooldown);
                return WeaponShotType.Railgun;
            }

            if (intent.PrimaryHeld && state.PrimaryCooldownRemaining <= CooldownEpsilon)
            {
                state.PrimaryCooldownRemaining = Mathf.Max(
                    0f,
                    state.PrimaryCooldownRemaining + tuning.PrimaryFireInterval);
                return WeaponShotType.PrimaryProjectile;
            }

            return WeaponShotType.None;
        }
    }
}
