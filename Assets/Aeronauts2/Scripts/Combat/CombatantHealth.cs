using System;
using UnityEngine;

namespace Aeronauts2.Combat
{
    public class CombatantHealth : MonoBehaviour
    {
        private const float DamageFlashDuration = 0.12f;
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

        [SerializeField] private TargetTuning tuning;

        private Collider[] hitColliders;
        private Renderer[] renderers;
        private MaterialPropertyBlock presentationProperties;
        private TargetState state;
        private float damageFlashRemaining;

        public TargetState State => state;
        public TargetTuning Tuning => tuning;
        public float Shield => state.Shield;
        public float Hull => state.Hull;
        public float RespawnRemaining => state.RespawnRemaining;
        public bool IsEliminated => state.IsEliminated;
        public int DamageSequence { get; private set; }
        public DamageReport LastDamageReport { get; private set; }
        public event Action<CombatantHealth, DamageReport> Damaged;
        public event Action<CombatantHealth, DamageReport> Eliminated;
        public event Action<CombatantHealth> Respawned;

        public void Configure(TargetTuning combatantTuning)
        {
            tuning = combatantTuning;
            ResetCombatant();
        }

        protected virtual void Awake()
        {
            ResetCombatant();
        }

        private void FixedUpdate()
        {
            SimulateTick(Time.fixedDeltaTime);
        }

        public void SimulateTick(float deltaTime)
        {
            if (tuning == null || deltaTime <= 0f)
            {
                return;
            }

            if (damageFlashRemaining > 0f)
            {
                damageFlashRemaining = Mathf.Max(0f, damageFlashRemaining - deltaTime);
                if (damageFlashRemaining <= 0f)
                {
                    ClearDamageFlash();
                }
            }

            bool respawned = TargetSimulation.Step(ref state, tuning, deltaTime);
            if (!respawned)
            {
                return;
            }

            ClearDamageFlash();
            SetPresentationActive(true);
            Respawned?.Invoke(this);
        }

        public DamageReport ApplyDamage(
            in WeaponDamageProfile damageProfile,
            HitRegionType regionType)
        {
            DamageReport report = TargetSimulation.ApplyDamage(
                ref state,
                damageProfile,
                regionType,
                tuning);
            if (report.DamageApplied <= 0f)
            {
                return report;
            }

            DamageSequence++;
            LastDamageReport = report;
            ApplyDamageFlash(report);
            Damaged?.Invoke(this, report);
            if (!report.Eliminated)
            {
                return report;
            }

            SetPresentationActive(false);
            Eliminated?.Invoke(this, report);
            return report;
        }

        public void ResetCombatant()
        {
            if (tuning == null)
            {
                return;
            }

            CachePresentation();
            state = TargetState.Create(tuning);
            DamageSequence = 0;
            LastDamageReport = default;
            damageFlashRemaining = 0f;
            ClearDamageFlash();
            SetPresentationActive(true);
            Respawned?.Invoke(this);
        }

        private void CachePresentation()
        {
            hitColliders = GetComponentsInChildren<Collider>(true);
            renderers = GetComponentsInChildren<Renderer>(true);
            presentationProperties ??= new MaterialPropertyBlock();
        }

        private void SetPresentationActive(bool active)
        {
            CachePresentation();
            foreach (Collider hitCollider in hitColliders)
            {
                hitCollider.enabled = active;
            }

            foreach (Renderer combatantRenderer in renderers)
            {
                combatantRenderer.enabled = active;
            }
        }

        private void ApplyDamageFlash(in DamageReport report)
        {
            CachePresentation();
            Color flashColor;
            if (report.Eliminated)
            {
                flashColor = new Color(1f, 0.12f, 0.04f);
            }
            else if (report.ShieldBroken)
            {
                flashColor = Color.white;
            }
            else if (report.WasCritical)
            {
                flashColor = new Color(1f, 0.3f, 0.05f);
            }
            else if (report.ShieldDamage > 0f)
            {
                flashColor = new Color(0.1f, 0.9f, 1f);
            }
            else
            {
                flashColor = new Color(1f, 0.22f, 0.12f);
            }

            presentationProperties.Clear();
            presentationProperties.SetColor(BaseColorProperty, flashColor);
            foreach (Renderer combatantRenderer in renderers)
            {
                combatantRenderer.SetPropertyBlock(presentationProperties);
            }

            damageFlashRemaining = DamageFlashDuration;
        }

        private void ClearDamageFlash()
        {
            CachePresentation();
            presentationProperties.Clear();
            foreach (Renderer combatantRenderer in renderers)
            {
                combatantRenderer.SetPropertyBlock(presentationProperties);
            }
        }
    }
}
