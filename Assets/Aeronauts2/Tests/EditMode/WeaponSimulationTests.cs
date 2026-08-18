using Aeronauts2.Combat;
using Aeronauts2.Core;
using NUnit.Framework;
using UnityEngine;

namespace Aeronauts2.Tests
{
    public sealed class WeaponSimulationTests
    {
        private WeaponTuning weaponTuning;
        private TargetTuning targetTuning;

        [SetUp]
        public void SetUp()
        {
            weaponTuning = ScriptableObject.CreateInstance<WeaponTuning>();
            targetTuning = ScriptableObject.CreateInstance<TargetTuning>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(weaponTuning);
            Object.DestroyImmediate(targetTuning);
        }

        [Test]
        public void RailgunHasPriorityAndConsumesLimitedAmmo()
        {
            WeaponState state = WeaponState.Create(weaponTuning.RailgunCapacity);
            FireIntent bothWeapons = new(true, true);

            WeaponShotType firstShot = WeaponSimulation.Step(
                ref state,
                bothWeapons,
                weaponTuning,
                1f / 60f);

            Assert.That(firstShot, Is.EqualTo(WeaponShotType.Railgun));
            Assert.That(state.RailgunAmmo, Is.EqualTo(weaponTuning.RailgunCapacity - 1));
        }

        [Test]
        public void RailgunCannotFireAfterReserveIsEmpty()
        {
            WeaponState state = WeaponState.Create(weaponTuning.RailgunCapacity);
            FireIntent railgunPress = new(false, true);

            for (int shot = 0; shot < weaponTuning.RailgunCapacity; shot++)
            {
                WeaponShotType decision = WeaponSimulation.Step(
                    ref state,
                    railgunPress,
                    weaponTuning,
                    weaponTuning.RailgunCooldown + 0.01f);
                Assert.That(decision, Is.EqualTo(WeaponShotType.Railgun));
            }

            WeaponShotType emptyDecision = WeaponSimulation.Step(
                ref state,
                railgunPress,
                weaponTuning,
                weaponTuning.RailgunCooldown + 0.01f);

            Assert.That(emptyDecision, Is.EqualTo(WeaponShotType.None));
            Assert.That(state.RailgunAmmo, Is.EqualTo(0));
        }

        [Test]
        public void HeldPrimaryRespectsConfiguredFireRate()
        {
            WeaponState state = WeaponState.Create(weaponTuning.RailgunCapacity);
            FireIntent primaryHeld = new(true, false);
            int shots = 0;

            for (int tick = 0; tick < 60; tick++)
            {
                if (WeaponSimulation.Step(
                        ref state,
                        primaryHeld,
                        weaponTuning,
                        1f / 60f)
                    == WeaponShotType.PrimaryProjectile)
                {
                    shots++;
                }
            }

            Assert.That(shots, Is.InRange(8, 9));
        }

        [Test]
        public void ShieldedBodyAndCockpitHitsTakeEqualDamage()
        {
            TargetState bodyState = TargetState.Create(targetTuning);
            TargetState criticalState = TargetState.Create(targetTuning);
            DamageReport bodyHit = TargetSimulation.ApplyDamage(
                ref bodyState,
                weaponTuning.PrimaryDamageProfile,
                HitRegionType.Body,
                targetTuning);
            DamageReport criticalHit = TargetSimulation.ApplyDamage(
                ref criticalState,
                weaponTuning.PrimaryDamageProfile,
                HitRegionType.Critical,
                targetTuning);

            Assert.That(bodyHit.WasCritical, Is.False);
            Assert.That(criticalHit.WasCritical, Is.True);
            Assert.That(criticalHit.ShieldDamage, Is.EqualTo(bodyHit.ShieldDamage));
            Assert.That(criticalHit.HullDamage, Is.EqualTo(0f));
            Assert.That(criticalState.Hull, Is.EqualTo(bodyState.Hull));
        }

        [Test]
        public void ShieldDoesNotRegenerateOverTime()
        {
            TargetState state = TargetState.Create(targetTuning);
            DamageReport hit = TargetSimulation.ApplyDamage(
                ref state,
                weaponTuning.PrimaryDamageProfile,
                HitRegionType.Body,
                targetTuning);
            float shieldAfterHit = state.Shield;

            TargetSimulation.Step(ref state, targetTuning, 60f);

            Assert.That(hit.ShieldDamage, Is.GreaterThan(0f));
            Assert.That(state.Shield, Is.EqualTo(shieldAfterHit));
        }

        [Test]
        public void ShieldBreakingShotDoesNotSpillDamageIntoHull()
        {
            TargetState state = TargetState.Create(targetTuning);
            state.Shield = 1f;
            DamageReport report = TargetSimulation.ApplyDamage(
                ref state,
                weaponTuning.PrimaryDamageProfile,
                HitRegionType.Critical,
                targetTuning);

            Assert.That(report.ShieldBroken, Is.True);
            Assert.That(report.ShieldDamage, Is.EqualTo(1f));
            Assert.That(report.HullDamage, Is.EqualTo(0f));
            Assert.That(state.Hull, Is.EqualTo(targetTuning.MaximumHull));
            Assert.That(report.Eliminated, Is.False);
        }

        [Test]
        public void CriticalRailgunEliminatesFullShieldAndHullTarget()
        {
            TargetState state = TargetState.Create(targetTuning);
            DamageReport report = TargetSimulation.ApplyDamage(
                ref state,
                weaponTuning.RailgunDamageProfile,
                HitRegionType.Critical,
                targetTuning);

            Assert.That(report.WasCritical, Is.True);
            Assert.That(report.ShieldBypassed, Is.True);
            Assert.That(report.ShieldDamage, Is.EqualTo(0f));
            Assert.That(report.HullDamage, Is.EqualTo(targetTuning.MaximumHull));
            Assert.That(state.Shield, Is.EqualTo(targetTuning.MaximumShield));
            Assert.That(report.Eliminated, Is.True);
            Assert.That(state.IsEliminated, Is.True);
        }

        [Test]
        public void UnshieldedPrimaryCockpitHitEliminatesFullHullTarget()
        {
            TargetState state = TargetState.Create(targetTuning);
            state.Shield = 0f;
            DamageReport report = TargetSimulation.ApplyDamage(
                ref state,
                weaponTuning.PrimaryDamageProfile,
                HitRegionType.Critical,
                targetTuning);

            Assert.That(report.ShieldBypassed, Is.False);
            Assert.That(report.HullDamage, Is.EqualTo(targetTuning.MaximumHull));
            Assert.That(report.Eliminated, Is.True);
        }

        [Test]
        public void EliminatedTargetRespawnsAtFullStrengthAfterDelay()
        {
            TargetState state = TargetState.Create(targetTuning);
            TargetSimulation.ApplyDamage(
                ref state,
                weaponTuning.RailgunDamageProfile,
                HitRegionType.Critical,
                targetTuning);

            bool earlyRespawn = TargetSimulation.Step(
                ref state,
                targetTuning,
                targetTuning.RespawnDelay - 0.1f);
            bool completedRespawn = TargetSimulation.Step(
                ref state,
                targetTuning,
                0.1f);

            Assert.That(earlyRespawn, Is.False);
            Assert.That(completedRespawn, Is.True);
            Assert.That(state.IsEliminated, Is.False);
            Assert.That(state.Shield, Is.EqualTo(targetTuning.MaximumShield));
            Assert.That(state.Hull, Is.EqualTo(targetTuning.MaximumHull));
        }
    }
}
