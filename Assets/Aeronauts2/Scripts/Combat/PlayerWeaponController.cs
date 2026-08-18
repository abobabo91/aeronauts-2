using System;
using System.Collections.Generic;
using Aeronauts2.CameraSystem;
using Aeronauts2.Core;
using Aeronauts2.Input;
using UnityEngine;

namespace Aeronauts2.Combat
{
    public sealed class PlayerWeaponController : MonoBehaviour
    {
        private const int HitBufferSize = 32;
        private const float HitDisplayDuration = 0.14f;
        private const float ShieldBreakDisplayDuration = 0.5f;
        private const float EliminationDisplayDuration = 0.8f;
        private const float AimSurfaceTolerance = 0.1f;

        [SerializeField] private PlayerInputReader input;
        [SerializeField] private AimCameraController aimCamera;
        [SerializeField] private Transform combatOrigin;
        [SerializeField] private WeaponTuning tuning;
        [SerializeField] private Material primaryProjectileMaterial;
        [SerializeField] private Material railgunMaterial;

        private readonly List<ActiveProjectile> activeProjectiles = new();
        private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferSize];
        private WeaponState state;
        private CombatantHealth lastTarget;

        public WeaponState State => state;
        public WeaponTuning Tuning => tuning;
        public Transform CombatOrigin => combatOrigin;
        public int PrimaryShotsFired { get; private set; }
        public int RailgunShotsFired { get; private set; }
        public int Hits { get; private set; }
        public int CriticalHits { get; private set; }
        public int Eliminations { get; private set; }
        public int ActiveProjectileCount => activeProjectiles.Count;
        public int TotalShotsFired => PrimaryShotsFired + RailgunShotsFired;
        public float Accuracy => TotalShotsFired > 0 ? (float)Hits / TotalShotsFired : 0f;
        public int LastHitSequence { get; private set; }
        public bool LastHitWasCritical { get; private set; }
        public bool LastHitBrokeShield { get; private set; }
        public bool LastHitBypassedShield { get; private set; }
        public bool LastHitEliminated { get; private set; }
        public float LastHitShieldDamage { get; private set; }
        public float LastHitHullDamage { get; private set; }
        public float LastHitDisplayRemaining { get; private set; }
        public float LastTargetShield => lastTarget != null ? lastTarget.Shield : 0f;
        public float LastTargetHull => lastTarget != null ? lastTarget.Hull : 0f;
        public float LastTargetMaximumShield => lastTarget != null && lastTarget.Tuning != null
            ? lastTarget.Tuning.MaximumShield
            : 0f;
        public float LastTargetMaximumHull => lastTarget != null && lastTarget.Tuning != null
            ? lastTarget.Tuning.MaximumHull
            : 0f;
        public float LastTargetRespawnRemaining => lastTarget != null
            ? lastTarget.RespawnRemaining
            : 0f;
        public bool LastTargetIsEliminated => lastTarget != null && lastTarget.IsEliminated;
        public bool MuzzleBlocked { get; private set; }
        public bool LastShotWasBlocked { get; private set; }
        public Vector3 LastShotOrigin { get; private set; }
        public event Action<PlayerWeaponController, CombatantHealth> TargetEliminated;

        public void Configure(
            PlayerInputReader inputReader,
            AimCameraController cameraController,
            Transform headCombatOrigin,
            WeaponTuning weaponTuning,
            Material projectileMaterial,
            Material instantWeaponMaterial)
        {
            input = inputReader;
            aimCamera = cameraController;
            combatOrigin = headCombatOrigin;
            tuning = weaponTuning;
            primaryProjectileMaterial = projectileMaterial;
            railgunMaterial = instantWeaponMaterial;
        }

        private void Awake()
        {
            ResetCombat();
        }

        private void FixedUpdate()
        {
            if (input == null || aimCamera == null || tuning == null)
            {
                return;
            }

            UpdateMuzzleObstruction(aimCamera.AimRay);

            FireIntent fireIntent = input.ConsumeFireIntent();
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                fireIntent = FireIntent.None;
            }

            SimulateInputTick(fireIntent, aimCamera.AimRay, Time.fixedDeltaTime);
            SimulateProjectiles(Time.fixedDeltaTime);
        }

        private void OnDisable()
        {
            ClearProjectiles();
        }

        public WeaponShotType SimulateInputTick(
            in FireIntent fireIntent,
            Ray aimRay,
            float deltaTime)
        {
            WeaponShotType shot = WeaponSimulation.Step(
                ref state,
                fireIntent,
                tuning,
                deltaTime);
            switch (shot)
            {
                case WeaponShotType.PrimaryProjectile:
                    PrimaryShotsFired++;
                    FirePrimaryProjectile(aimRay);
                    break;
                case WeaponShotType.Railgun:
                    RailgunShotsFired++;
                    FireRailgun(aimRay);
                    break;
            }

            return shot;
        }

        public void SimulateProjectiles(float deltaTime)
        {
            if (tuning == null || deltaTime <= 0f)
            {
                return;
            }

            LastHitDisplayRemaining = Mathf.Max(0f, LastHitDisplayRemaining - deltaTime);
            for (int index = activeProjectiles.Count - 1; index >= 0; index--)
            {
                ActiveProjectile projectile = activeProjectiles[index];
                float travelDistance = tuning.PrimaryProjectileSpeed * deltaTime;
                int hitCount = Physics.SphereCastNonAlloc(
                    projectile.Position,
                    tuning.PrimaryProjectileRadius,
                    projectile.Direction,
                    hitBuffer,
                    travelDistance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);

                if (TryGetClosestHit(hitCount, out RaycastHit closestHit))
                {
                    projectile.Visual.transform.position = closestHit.point;
                    TargetHitRegion hitRegion = closestHit.collider.GetComponent<TargetHitRegion>();
                    if (hitRegion != null)
                    {
                        DamageReport report = hitRegion.ApplyDamage(
                            tuning.PrimaryDamageProfile);
                        RegisterHit(hitRegion.Target, report);
                    }

                    RemoveProjectile(index);
                    continue;
                }

                projectile.Position += projectile.Direction * travelDistance;
                projectile.LifetimeRemaining -= deltaTime;
                projectile.Visual.transform.position = projectile.Position;
                if (projectile.LifetimeRemaining <= 0f)
                {
                    RemoveProjectile(index);
                }
            }
        }

        public void ResetCombat()
        {
            int capacity = tuning != null ? tuning.RailgunCapacity : 0;
            state = WeaponState.Create(capacity);
            PrimaryShotsFired = 0;
            RailgunShotsFired = 0;
            Hits = 0;
            CriticalHits = 0;
            Eliminations = 0;
            LastHitSequence = 0;
            LastHitWasCritical = false;
            LastHitBrokeShield = false;
            LastHitBypassedShield = false;
            LastHitEliminated = false;
            LastHitShieldDamage = 0f;
            LastHitHullDamage = 0f;
            LastHitDisplayRemaining = 0f;
            lastTarget = null;
            MuzzleBlocked = false;
            LastShotWasBlocked = false;
            LastShotOrigin = GetCombatOriginPosition();
            ClearProjectiles();
        }

        public void UpdateMuzzleObstruction(Ray aimRay)
        {
            if (tuning == null)
            {
                MuzzleBlocked = false;
                return;
            }

            AimSolution solution = ResolveAimSolution(aimRay, tuning.RailgunRange);
            MuzzleBlocked = solution.IsBlocked;
        }

        private void FirePrimaryProjectile(Ray aimRay)
        {
            float range = tuning.PrimaryProjectileSpeed * tuning.PrimaryProjectileLifetime;
            AimSolution solution = ResolveAimSolution(aimRay, range);
            RecordShotSolution(solution);
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "PrimaryProjectile";
            visual.transform.position = solution.Origin;
            visual.transform.localScale = Vector3.one * tuning.PrimaryProjectileRadius * 2f;
            Collider projectileCollider = visual.GetComponent<Collider>();
            projectileCollider.enabled = false;
            Destroy(projectileCollider);
            visual.GetComponent<Renderer>().sharedMaterial = primaryProjectileMaterial;

            activeProjectiles.Add(new ActiveProjectile
            {
                Position = solution.Origin,
                Direction = solution.Direction,
                LifetimeRemaining = tuning.PrimaryProjectileLifetime,
                Visual = visual,
            });
        }

        private void FireRailgun(Ray aimRay)
        {
            AimSolution solution = ResolveAimSolution(aimRay, tuning.RailgunRange);
            RecordShotSolution(solution);
            int hitCount = Physics.RaycastNonAlloc(
                solution.Origin,
                solution.Direction,
                hitBuffer,
                solution.CastDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            Vector3 endPoint = solution.Origin
                + solution.Direction * solution.CastDistance;
            if (TryGetClosestHit(hitCount, out RaycastHit closestHit))
            {
                endPoint = closestHit.point;
                TargetHitRegion hitRegion = closestHit.collider.GetComponent<TargetHitRegion>();
                if (hitRegion != null)
                {
                    DamageReport report = hitRegion.ApplyDamage(
                        tuning.RailgunDamageProfile);
                    RegisterHit(hitRegion.Target, report);
                }
            }

            CreateRailgunTrace(solution.Origin, endPoint);
        }

        private AimSolution ResolveAimSolution(Ray aimRay, float maximumRange)
        {
            Vector3 cameraDirection = aimRay.direction.sqrMagnitude > 0f
                ? aimRay.direction.normalized
                : transform.forward;
            int cameraHitCount = Physics.RaycastNonAlloc(
                aimRay.origin,
                cameraDirection,
                hitBuffer,
                maximumRange,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            bool cameraHitFound = TryGetClosestHit(cameraHitCount, out RaycastHit cameraHit);
            Vector3 targetPoint = cameraHitFound
                ? cameraHit.point
                : aimRay.origin + cameraDirection * maximumRange;

            Vector3 origin = GetCombatOriginPosition();
            Vector3 targetOffset = targetPoint - origin;
            float targetDistance = targetOffset.magnitude;
            Vector3 shotDirection = targetDistance > Mathf.Epsilon
                ? targetOffset / targetDistance
                : cameraDirection;
            float castDistance = cameraHitFound
                ? Mathf.Min(maximumRange, targetDistance + AimSurfaceTolerance)
                : maximumRange;
            int muzzleHitCount = Physics.RaycastNonAlloc(
                origin,
                shotDirection,
                hitBuffer,
                castDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            bool muzzleHitFound = TryGetClosestHit(muzzleHitCount, out RaycastHit muzzleHit);
            bool blocked = muzzleHitFound
                && (!cameraHitFound || !AreEquivalentAimSurfaces(cameraHit.collider, muzzleHit.collider));

            return new AimSolution(
                origin,
                shotDirection,
                castDistance,
                blocked);
        }

        private Vector3 GetCombatOriginPosition()
        {
            return combatOrigin != null ? combatOrigin.position : transform.position;
        }

        private void RecordShotSolution(in AimSolution solution)
        {
            MuzzleBlocked = solution.IsBlocked;
            LastShotWasBlocked = solution.IsBlocked;
            LastShotOrigin = solution.Origin;
        }

        private bool TryGetClosestHit(int hitCount, out RaycastHit closestHit)
        {
            closestHit = default;
            float closestDistance = float.PositiveInfinity;
            bool found = false;
            for (int index = 0; index < hitCount; index++)
            {
                if (IsSelfCollider(hitBuffer[index].collider))
                {
                    continue;
                }

                if (hitBuffer[index].distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = hitBuffer[index].distance;
                closestHit = hitBuffer[index];
                found = true;
            }

            return found;
        }

        private bool IsSelfCollider(Collider candidate)
        {
            return candidate != null && candidate.transform.IsChildOf(transform);
        }

        private static bool AreEquivalentAimSurfaces(Collider cameraSurface, Collider muzzleSurface)
        {
            if (cameraSurface == muzzleSurface)
            {
                return true;
            }

            CombatantHealth cameraTarget = cameraSurface != null
                ? cameraSurface.GetComponentInParent<CombatantHealth>()
                : null;
            CombatantHealth muzzleTarget = muzzleSurface != null
                ? muzzleSurface.GetComponentInParent<CombatantHealth>()
                : null;
            return cameraTarget != null && cameraTarget == muzzleTarget;
        }

        private void RegisterHit(CombatantHealth target, DamageReport report)
        {
            if (report.DamageApplied <= 0f)
            {
                return;
            }

            Hits++;
            if (report.WasCritical)
            {
                CriticalHits++;
            }

            if (report.Eliminated)
            {
                Eliminations++;
                TargetEliminated?.Invoke(this, target);
            }

            LastHitSequence++;
            lastTarget = target;
            LastHitWasCritical = report.WasCritical;
            LastHitBrokeShield = report.ShieldBroken;
            LastHitBypassedShield = report.ShieldBypassed;
            LastHitEliminated = report.Eliminated;
            LastHitShieldDamage = report.ShieldDamage;
            LastHitHullDamage = report.HullDamage;
            LastHitDisplayRemaining = report.Eliminated
                ? EliminationDisplayDuration
                : report.ShieldBroken
                    ? ShieldBreakDisplayDuration
                    : HitDisplayDuration;
        }

        private void CreateRailgunTrace(Vector3 start, Vector3 end)
        {
            GameObject traceObject = new("RailgunTrace");
            LineRenderer trace = traceObject.AddComponent<LineRenderer>();
            trace.useWorldSpace = true;
            trace.positionCount = 2;
            trace.SetPosition(0, start);
            trace.SetPosition(1, end);
            trace.startWidth = 0.12f;
            trace.endWidth = 0.03f;
            trace.sharedMaterial = railgunMaterial;
            Destroy(traceObject, 0.08f);
        }

        private void RemoveProjectile(int index)
        {
            ActiveProjectile projectile = activeProjectiles[index];
            if (projectile.Visual != null)
            {
                Destroy(projectile.Visual);
            }

            activeProjectiles.RemoveAt(index);
        }

        private void ClearProjectiles()
        {
            foreach (ActiveProjectile projectile in activeProjectiles)
            {
                if (projectile.Visual != null)
                {
                    Destroy(projectile.Visual);
                }
            }

            activeProjectiles.Clear();
        }

        private sealed class ActiveProjectile
        {
            public Vector3 Position;
            public Vector3 Direction;
            public float LifetimeRemaining;
            public GameObject Visual;
        }

        private readonly struct AimSolution
        {
            public AimSolution(
                Vector3 origin,
                Vector3 direction,
                float castDistance,
                bool isBlocked)
            {
                Origin = origin;
                Direction = direction;
                CastDistance = castDistance;
                IsBlocked = isBlocked;
            }

            public Vector3 Origin { get; }
            public Vector3 Direction { get; }
            public float CastDistance { get; }
            public bool IsBlocked { get; }
        }
    }
}
