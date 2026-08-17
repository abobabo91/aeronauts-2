using UnityEngine;

namespace Aeronauts2.Combat
{
    [CreateAssetMenu(fileName = "TargetTuning", menuName = "Aeronauts 2/Target Tuning")]
    public sealed class TargetTuning : ScriptableObject
    {
        [SerializeField, Min(0f)] private float maximumShield = 60f;
        [SerializeField, Min(1f)] private float maximumHull = 100f;
        [SerializeField, Min(0f)] private float respawnDelay = 2f;

        public float MaximumShield => maximumShield;
        public float MaximumHull => maximumHull;
        public float RespawnDelay => respawnDelay;
    }
}
