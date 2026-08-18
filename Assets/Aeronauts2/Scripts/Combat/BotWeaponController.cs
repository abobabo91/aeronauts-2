using System;
using System.Collections.Generic;
using Aeronauts2.Core;
using Aeronauts2.Flight;
using UnityEngine;

namespace Aeronauts2.Combat
{
    public sealed class BotWeaponController : MonoBehaviour
    {
        private const int HitBufferSize = 32;

        [SerializeField] private CombatantHealth health;
        [SerializeField] private CombatantHealth targetHealth;
        [SerializeField] private Transform targetAimPoint;
        [SerializeField] private FlightMotor targetFlightMotor;
        [SerializeField] private Transform combatOrigin;
        [SerializeField] private WeaponTuning weaponTuning;
        [SerializeField] private BotTuning botTuning;
        [SerializeField] private Material projectileMaterial;

        private readonly List<ActiveProjectile> activeProjectiles = new();
        private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferSize];
        private WeaponState state;
        private CombatantHealth subscribedHealth;
        private float reactionRemaining;
        private float combatTime;

        public WeaponState State => state;
        public int PrimaryShotsFired { get; private set; }
        public int Hits { get; private set; }
        public int Eliminations { get; private set; }
        public int ActiveProjectileCount => activeProjectiles.Count;
        public Vector3 AimDirection { get; private set; }
        public Transform CombatOrigin => combatOrigin;
        public WeaponTuning Tuning => weaponTuning;
        public event Action<BotWeaponController, CombatantHealth> TargetEliminated;

        public void Configure(
            CombatantHealth botHealth,
            CombatantHealth targetCombatant,
            Transform targetCriticalPoint,
            FlightMotor targetMotor,
            Transform headCombatOrigin,
            WeaponTuning sharedWeaponTuning,
            BotTuning behaviorTuning,
            Material primaryMaterial)
        {
            Unsubscribe();
            health = botHealth;
            targetHealth = targetCombatant;
            targetAimPoint = targetCriticalPoint;
            targetFlightMotor = targetMotor;
            combatOrigin = headCombatOrigin;
            weaponTuning = sharedWeaponTuning;
            botTuning = behaviorTuning;
            projectileMaterial = primaryMaterial;
            ResetWeapon();
            Subscribe();
        }

        private void Awake()
        {
            ResetWeapon();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            ClearProjectiles();
        }

        private void FixedUpdate()
        {
            if (weaponTuning == null || botTuning == null)
            {
                return;
            }

            float deltaTime = Time.fixedDeltaTime;
            combatTime += deltaTime;
            reactionRemaining = Mathf.Max(0f, reactionRemaining - deltaTime);
            bool canEngage = health != null
                && !health.IsEliminated
                && targetHealth != null
                && !targetHealth.IsEliminated
                && targetAimPoint != null
                && combatOrigin != null;
            bool firePrimary = canEngage && ShouldFirePrimary();
            WeaponShotType shot = WeaponSimulation.Step(
                ref state,
                new FireIntent(firePrimary, false),
                weaponTuning,
                deltaTime);
            if (shot == WeaponShotType.PrimaryProjectile)
            {
                PrimaryShotsFired++;
                SpawnProjectile(AimDirection);
            }

            SimulateProjectiles(deltaTime);
        }

        public bool ShouldFirePrimary()
        {
            Vector3 origin = combatOrigin.position;
            Vector3 targetPosition = targetAimPoint.position;
            Vector3 targetVelocity = targetFlightMotor != null
                ? targetFlightMotor.State.Velocity
                : Vector3.zero;
            float directDistance = Vector3.Distance(origin, targetPosition);
            float travelTime = weaponTuning.PrimaryProjectileSpeed > 0f
                ? directDistance / weaponTuning.PrimaryProjectileSpeed
                : 0f;
            Vector3 predictedPosition = targetPosition
                + targetVelocity * travelTime * botTuning.LeadStrength;
            Vector3 directAim = (predictedPosition - origin).normalized;
            float yawError = Mathf.Sin(combatTime * 1.73f) * botTuning.AimErrorDegrees;
            float pitchError = Mathf.Cos(combatTime * 1.21f) * botTuning.AimErrorDegrees * 0.65f;
            AimDirection = Quaternion.AngleAxis(yawError, Vector3.up)
                * Quaternion.AngleAxis(pitchError, transform.right)
                * directAim;
            AimDirection.Normalize();

            float aimAngle = Vector3.Angle(transform.forward, AimDirection);
            float dutyCycle = Mathf.Lerp(0.3f, 0.9f, botTuning.Aggression);
            float burstPhase = Mathf.Repeat(combatTime, botTuning.BurstCycleDuration)
                / botTuning.BurstCycleDuration;
            return reactionRemaining <= 0f
                && directDistance <= botTuning.FireRange
                && aimAngle <= botTuning.MaximumAimAngle
                && burstPhase <= dutyCycle;
        }

        public void SimulateProjectiles(float deltaTime)
        {
            if (weaponTuning == null || deltaTime <= 0f)
            {
                return;
            }

            for (int index = activeProjectiles.Count - 1; index >= 0; index--)
            {
                ActiveProjectile projectile = activeProjectiles[index];
                float distance = weaponTuning.PrimaryProjectileSpeed * deltaTime;
                int hitCount = Physics.SphereCastNonAlloc(
                    projectile.Position,
                    weaponTuning.PrimaryProjectileRadius,
                    projectile.Direction,
                    hitBuffer,
                    distance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);
                if (TryGetClosestHit(hitCount, out RaycastHit closestHit))
                {
                    projectile.Visual.transform.position = closestHit.point;
                    TargetHitRegion hitRegion = closestHit.collider.GetComponent<TargetHitRegion>();
                    if (hitRegion != null)
                    {
                        RegisterHit(
                            hitRegion.Target,
                            hitRegion.ApplyDamage(weaponTuning.PrimaryDamageProfile));
                    }

                    RemoveProjectile(index);
                    continue;
                }

                projectile.Position += projectile.Direction * distance;
                projectile.LifetimeRemaining -= deltaTime;
                projectile.Visual.transform.position = projectile.Position;
                if (projectile.LifetimeRemaining <= 0f)
                {
                    RemoveProjectile(index);
                }
            }
        }

        public void ResetWeapon()
        {
            state = WeaponState.Create(weaponTuning != null ? weaponTuning.RailgunCapacity : 0);
            PrimaryShotsFired = 0;
            Hits = 0;
            Eliminations = 0;
            reactionRemaining = botTuning != null ? botTuning.ReactionTime : 0f;
            combatTime = 0f;
            AimDirection = transform.forward;
            ClearProjectiles();
        }

        private void SpawnProjectile(Vector3 direction)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "BotPrimaryProjectile";
            visual.transform.position = combatOrigin.position;
            visual.transform.localScale = Vector3.one * weaponTuning.PrimaryProjectileRadius * 2f;
            Collider projectileCollider = visual.GetComponent<Collider>();
            projectileCollider.enabled = false;
            Destroy(projectileCollider);
            visual.GetComponent<Renderer>().sharedMaterial = projectileMaterial;
            activeProjectiles.Add(new ActiveProjectile
            {
                Position = combatOrigin.position,
                Direction = direction,
                LifetimeRemaining = weaponTuning.PrimaryProjectileLifetime,
                Visual = visual,
            });
        }

        private bool TryGetClosestHit(int hitCount, out RaycastHit closestHit)
        {
            closestHit = default;
            float closestDistance = float.PositiveInfinity;
            bool found = false;
            for (int index = 0; index < hitCount; index++)
            {
                Collider candidate = hitBuffer[index].collider;
                if (candidate == null
                    || (health != null && candidate.transform.IsChildOf(health.transform))
                    || hitBuffer[index].distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = hitBuffer[index].distance;
                closestHit = hitBuffer[index];
                found = true;
            }

            return found;
        }

        private void RegisterHit(CombatantHealth target, in DamageReport report)
        {
            if (report.DamageApplied <= 0f)
            {
                return;
            }

            Hits++;
            if (!report.Eliminated)
            {
                return;
            }

            Eliminations++;
            TargetEliminated?.Invoke(this, target);
        }

        private void Subscribe()
        {
            if (health == null || subscribedHealth == health)
            {
                return;
            }

            Unsubscribe();
            subscribedHealth = health;
            subscribedHealth.Respawned += OnRespawned;
        }

        private void Unsubscribe()
        {
            if (subscribedHealth == null)
            {
                return;
            }

            subscribedHealth.Respawned -= OnRespawned;
            subscribedHealth = null;
        }

        private void OnRespawned(CombatantHealth _)
        {
            ResetWeapon();
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
    }
}
