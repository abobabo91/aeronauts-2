using System.Collections;
using System.Linq;
using Aeronauts2.CameraSystem;
using Aeronauts2.Combat;
using Aeronauts2.Core;
using Aeronauts2.Flight;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Aeronauts2.Tests
{
    public sealed class FlightLabSmokeTests
    {
        [UnityTest]
        public IEnumerator FlightLabLoadsAndAdvancesWithoutRuntimeErrors()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("FlightLab", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            yield return new WaitForFixedUpdate();
            yield return null;

            FlightMotor motor = Object.FindFirstObjectByType<FlightMotor>();
            AimCameraController aimCamera = Object.FindFirstObjectByType<AimCameraController>();
            Assert.That(motor, Is.Not.Null);
            Assert.That(aimCamera, Is.Not.Null);
            Assert.That(motor.State.Speed, Is.EqualTo(0f));
            Assert.That(motor.VelocityAlignmentErrorDegrees, Is.EqualTo(0f));
        }

        [UnityTest]
        public IEnumerator RailgunIsInstantWhilePrimaryProjectileHasTravelTime()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("FlightLab", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            yield return new WaitForFixedUpdate();

            PlayerWeaponController weapon = Object.FindFirstObjectByType<PlayerWeaponController>();
            AimCameraController aimCamera = Object.FindFirstObjectByType<AimCameraController>();
            FlightMotor motor = Object.FindFirstObjectByType<FlightMotor>();
            TargetDummy target = GameObject.Find("StationaryTarget").GetComponent<TargetDummy>();
            TargetHitRegion criticalRegion = target
                .GetComponentsInChildren<TargetHitRegion>()
                .Single(region => region.RegionType == HitRegionType.Critical);
            TargetHitRegion bodyRegion = target
                .GetComponentsInChildren<TargetHitRegion>()
                .First(region => region.RegionType == HitRegionType.Body && region.name == "BodyRegion");
            Assert.That(weapon, Is.Not.Null);
            Assert.That(aimCamera, Is.Not.Null);
            Assert.That(motor, Is.Not.Null);

            target.ResetTarget();
            weapon.ResetCombat();
            Physics.SyncTransforms();
            Vector3 railgunTarget = criticalRegion.GetComponent<Collider>().bounds.center;
            Ray railgunRay = new(
                aimCamera.AimRay.origin,
                (railgunTarget - aimCamera.AimRay.origin).normalized);
            WeaponShotType railgunShot = weapon.SimulateInputTick(
                new FireIntent(false, true),
                railgunRay,
                1f / 60f);

            Assert.That(railgunShot, Is.EqualTo(WeaponShotType.Railgun));
            Assert.That(weapon.ActiveProjectileCount, Is.EqualTo(0));
            Assert.That(weapon.Hits, Is.EqualTo(1));
            Assert.That(weapon.CriticalHits, Is.EqualTo(1));
            Assert.That(weapon.Eliminations, Is.EqualTo(1));
            Assert.That(weapon.LastHitEliminated, Is.True);
            Assert.That(weapon.LastHitBrokeShield, Is.False);
            Assert.That(weapon.LastHitBypassedShield, Is.True);
            Assert.That(target.IsEliminated, Is.True);
            Assert.That(target.Shield, Is.EqualTo(target.Tuning.MaximumShield));
            Assert.That(weapon.State.RailgunAmmo, Is.EqualTo(weapon.Tuning.RailgunCapacity - 1));
            Assert.That(weapon.LastShotOrigin, Is.EqualTo(weapon.CombatOrigin.position));
            Assert.That(weapon.LastShotWasBlocked, Is.False);

            motor.RequestManualReset();
            Assert.That(target.IsEliminated, Is.False);
            Assert.That(target.Shield, Is.EqualTo(target.Tuning.MaximumShield));
            Assert.That(target.Hull, Is.EqualTo(target.Tuning.MaximumHull));
            Assert.That(weapon.Eliminations, Is.EqualTo(0));
            Assert.That(weapon.State.RailgunAmmo, Is.EqualTo(weapon.Tuning.RailgunCapacity));
            Physics.SyncTransforms();
            Bounds bodyBounds = bodyRegion.GetComponent<Collider>().bounds;
            Vector3 primaryTarget = bodyBounds.center + Vector3.down * bodyBounds.extents.y * 0.65f;
            Ray primaryRay = new(
                aimCamera.AimRay.origin,
                (primaryTarget - aimCamera.AimRay.origin).normalized);
            float shieldBeforeProjectileTravel = target.Shield;
            WeaponShotType primaryShot = weapon.SimulateInputTick(
                new FireIntent(true, false),
                primaryRay,
                1f / 60f);

            Assert.That(primaryShot, Is.EqualTo(WeaponShotType.PrimaryProjectile));
            Assert.That(weapon.ActiveProjectileCount, Is.EqualTo(1));
            Assert.That(target.Shield, Is.EqualTo(shieldBeforeProjectileTravel));

            int travelTicks = 0;
            while (weapon.ActiveProjectileCount > 0 && travelTicks < 180)
            {
                weapon.SimulateProjectiles(1f / 60f);
                travelTicks++;
            }

            Assert.That(travelTicks, Is.GreaterThan(1));
            Assert.That(weapon.ActiveProjectileCount, Is.EqualTo(0));
            Assert.That(weapon.Hits, Is.EqualTo(1));
            Assert.That(target.Shield, Is.LessThan(shieldBeforeProjectileTravel));
            Debug.Log(
                "COMBAT_FEEL "
                + "railgun_resolution=instant "
                + $"primary_travel={travelTicks / 60f:0.000}s "
                + $"primary_speed={weapon.Tuning.PrimaryProjectileSpeed:0.0} "
                + $"railgun_reserve={weapon.Tuning.RailgunCapacity}");
        }

        [UnityTest]
        public IEnumerator DuelBotUsesSharedFlightAndFiresRealPrimaryProjectiles()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("FlightLab", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            BotFlightController botFlight = Object.FindFirstObjectByType<BotFlightController>();
            BotWeaponController botWeapon = Object.FindFirstObjectByType<BotWeaponController>();
            Assert.That(botFlight, Is.Not.Null);
            Assert.That(botWeapon, Is.Not.Null);

            Vector3 startPosition = botFlight.transform.position;
            for (int tick = 0; tick < 120; tick++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(Vector3.Distance(botFlight.transform.position, startPosition), Is.GreaterThan(1f));
            Assert.That(botFlight.State.Speed, Is.GreaterThan(1f));
            Assert.That(botFlight.VelocityAlignmentErrorDegrees, Is.LessThan(0.1f));
            Assert.That(botWeapon.PrimaryShotsFired, Is.GreaterThan(0));
            Assert.That(botWeapon.Hits, Is.GreaterThan(0));
            Assert.That(botWeapon.Tuning.PrimaryProjectileSpeed, Is.EqualTo(120f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator PlayerWinsFirstToFiveByRailgunCockpitEliminations()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("FlightLab", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            yield return new WaitForFixedUpdate();
            PlayerWeaponController playerWeapon = Object.FindFirstObjectByType<PlayerWeaponController>();
            AimCameraController aimCamera = Object.FindFirstObjectByType<AimCameraController>();
            BotFlightController botFlight = Object.FindFirstObjectByType<BotFlightController>();
            BotWeaponController botWeapon = Object.FindFirstObjectByType<BotWeaponController>();
            DuelMatchController match = Object.FindFirstObjectByType<DuelMatchController>();
            CombatantHealth botHealth = botWeapon.GetComponent<CombatantHealth>();
            TargetHitRegion botCockpit = botHealth
                .GetComponentsInChildren<TargetHitRegion>()
                .Single(region => region.RegionType == HitRegionType.Critical);
            Assert.That(playerWeapon, Is.Not.Null);
            Assert.That(aimCamera, Is.Not.Null);
            Assert.That(match, Is.Not.Null);

            botFlight.enabled = false;
            botWeapon.enabled = false;
            for (int elimination = 1; elimination <= match.ScoreToWin; elimination++)
            {
                botHealth.ResetCombatant();
                playerWeapon.ResetCombat();
                Physics.SyncTransforms();
                Vector3 targetPoint = botCockpit.GetComponent<Collider>().bounds.center;
                Ray railgunRay = new(
                    aimCamera.AimRay.origin,
                    (targetPoint - aimCamera.AimRay.origin).normalized);
                WeaponShotType shot = playerWeapon.SimulateInputTick(
                    new FireIntent(false, true),
                    railgunRay,
                    1f / 60f);

                Assert.That(shot, Is.EqualTo(WeaponShotType.Railgun));
                Assert.That(botHealth.IsEliminated, Is.True);
                Assert.That(match.PlayerScore, Is.EqualTo(elimination));
                Assert.That(match.BotScore, Is.EqualTo(0));
            }

            Assert.That(match.MatchComplete, Is.True);
            Assert.That(match.Winner, Is.EqualTo("PLAYER"));
        }

        [UnityTest]
        public IEnumerator UnshieldedPrimaryCockpitProjectileEliminatesTarget()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("FlightLab", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            yield return new WaitForFixedUpdate();

            PlayerWeaponController weapon = Object.FindFirstObjectByType<PlayerWeaponController>();
            AimCameraController aimCamera = Object.FindFirstObjectByType<AimCameraController>();
            TargetDummy target = GameObject.Find("StationaryTarget").GetComponent<TargetDummy>();
            TargetHitRegion criticalRegion = target
                .GetComponentsInChildren<TargetHitRegion>()
                .Single(region => region.RegionType == HitRegionType.Critical);

            target.ResetTarget();
            weapon.ResetCombat();
            while (target.Shield > 0f)
            {
                DamageReport shieldHit = target.ApplyDamage(
                    weapon.Tuning.RailgunDamageProfile,
                    HitRegionType.Body);
                Assert.That(shieldHit.HullDamage, Is.EqualTo(0f));
            }

            Assert.That(target.Hull, Is.EqualTo(target.Tuning.MaximumHull));
            Physics.SyncTransforms();
            Vector3 targetPoint = criticalRegion.GetComponent<Collider>().bounds.center;
            Ray criticalRay = new(
                aimCamera.AimRay.origin,
                (targetPoint - aimCamera.AimRay.origin).normalized);
            WeaponShotType shot = weapon.SimulateInputTick(
                new FireIntent(true, false),
                criticalRay,
                1f / 60f);

            int travelTicks = 0;
            while (weapon.ActiveProjectileCount > 0 && travelTicks < 180)
            {
                weapon.SimulateProjectiles(1f / 60f);
                travelTicks++;
            }

            Assert.That(shot, Is.EqualTo(WeaponShotType.PrimaryProjectile));
            Assert.That(weapon.Hits, Is.EqualTo(1));
            Assert.That(weapon.CriticalHits, Is.EqualTo(1));
            Assert.That(weapon.Eliminations, Is.EqualTo(1));
            Assert.That(weapon.LastHitBypassedShield, Is.False);
            Assert.That(target.IsEliminated, Is.True);
        }

        [UnityTest]
        public IEnumerator ForwardViewTracksBodyAndAimViewRemainsIndependent()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("FlightLab", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            FlightMotor motor = Object.FindFirstObjectByType<FlightMotor>();
            AimCameraController aimCamera = Object.FindFirstObjectByType<AimCameraController>();
            Assert.That(motor, Is.Not.Null);
            Assert.That(aimCamera, Is.Not.Null);
            Assert.That(aimCamera.ActiveViewMode, Is.EqualTo(CameraViewMode.Aim));

            motor.enabled = false;
            motor.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            aimCamera.SetViewMode(CameraViewMode.Forward);
            yield return null;

            Assert.That(aimCamera.ActiveViewMode, Is.EqualTo(CameraViewMode.Forward));
            Assert.That(
                Vector3.Angle(aimCamera.AimForward, motor.transform.forward),
                Is.LessThan(0.1f));

            aimCamera.SetViewMode(CameraViewMode.Aim);
            yield return null;
            Vector3 independentAim = aimCamera.AimForward;
            motor.transform.rotation = Quaternion.Euler(0f, 120f, 0f);
            yield return null;

            Assert.That(aimCamera.ActiveViewMode, Is.EqualTo(CameraViewMode.Aim));
            Assert.That(Vector3.Angle(aimCamera.AimForward, independentAim), Is.LessThan(0.1f));
            Assert.That(aimCamera.BodyAimAngleDegrees, Is.EqualTo(30f).Within(0.1f));
        }

        [UnityTest]
        public IEnumerator HeadOriginCoverBlocksCameraVisibleRailgunShot()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("FlightLab", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            yield return new WaitForFixedUpdate();
            PlayerWeaponController weapon = Object.FindFirstObjectByType<PlayerWeaponController>();
            AimCameraController aimCamera = Object.FindFirstObjectByType<AimCameraController>();
            TargetDummy target = GameObject.Find("StationaryTarget").GetComponent<TargetDummy>();
            TargetHitRegion criticalRegion = target
                .GetComponentsInChildren<TargetHitRegion>()
                .Single(region => region.RegionType == HitRegionType.Critical);
            Assert.That(weapon.CombatOrigin, Is.Not.Null);

            Vector3 targetPoint = criticalRegion.GetComponent<Collider>().bounds.center;
            Ray cameraRay = new(
                aimCamera.AimRay.origin,
                (targetPoint - aimCamera.AimRay.origin).normalized);
            GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cover.name = "HeadOriginTestCover";
            cover.transform.position = Vector3.Lerp(
                weapon.CombatOrigin.position,
                targetPoint,
                0.18f);
            cover.transform.localScale = Vector3.one * 1.5f;
            Physics.SyncTransforms();

            target.ResetTarget();
            weapon.ResetCombat();
            WeaponShotType shot = weapon.SimulateInputTick(
                new FireIntent(false, true),
                cameraRay,
                1f / 60f);

            Assert.That(shot, Is.EqualTo(WeaponShotType.Railgun));
            Assert.That(weapon.LastShotOrigin, Is.EqualTo(weapon.CombatOrigin.position));
            Assert.That(weapon.LastShotWasBlocked, Is.True);
            Assert.That(weapon.MuzzleBlocked, Is.True);
            Assert.That(weapon.Hits, Is.EqualTo(0));
            Assert.That(weapon.State.RailgunAmmo, Is.EqualTo(weapon.Tuning.RailgunCapacity - 1));

            Object.Destroy(cover);
        }
    }
}
