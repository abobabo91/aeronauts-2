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
            TargetDummy target = GameObject.Find("StationaryTarget").GetComponent<TargetDummy>();
            TargetHitRegion criticalRegion = target
                .GetComponentsInChildren<TargetHitRegion>()
                .Single(region => region.RegionType == HitRegionType.Critical);
            TargetHitRegion bodyRegion = target
                .GetComponentsInChildren<TargetHitRegion>()
                .First(region => region.RegionType == HitRegionType.Body && region.name == "BodyRegion");
            Assert.That(weapon, Is.Not.Null);
            Assert.That(aimCamera, Is.Not.Null);

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
            Assert.That(weapon.State.RailgunAmmo, Is.EqualTo(weapon.Tuning.RailgunCapacity - 1));
            Assert.That(weapon.LastShotOrigin, Is.EqualTo(weapon.CombatOrigin.position));
            Assert.That(weapon.LastShotWasBlocked, Is.False);

            target.ResetTarget();
            weapon.ResetCombat();
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
