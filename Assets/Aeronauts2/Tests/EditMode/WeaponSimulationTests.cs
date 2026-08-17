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
        public void CriticalRegionMultipliesDamageBeforeShieldAndHullResolution()
        {
            GameObject targetObject = new("DamageTestTarget");
            TargetDummy target = targetObject.AddComponent<TargetDummy>();
            target.Configure(targetTuning);
            target.ResetTarget();

            DamageReport bodyHit = target.ApplyDamage(
                weaponTuning.PrimaryDamage,
                HitRegionType.Body,
                weaponTuning.CriticalDamageMultiplier);
            target.ResetTarget();
            DamageReport criticalHit = target.ApplyDamage(
                weaponTuning.PrimaryDamage,
                HitRegionType.Critical,
                weaponTuning.CriticalDamageMultiplier);

            Assert.That(bodyHit.WasCritical, Is.False);
            Assert.That(criticalHit.WasCritical, Is.True);
            Assert.That(criticalHit.DamageApplied, Is.EqualTo(
                bodyHit.DamageApplied * weaponTuning.CriticalDamageMultiplier).Within(0.001f));

            Object.DestroyImmediate(targetObject);
        }
    }
}
