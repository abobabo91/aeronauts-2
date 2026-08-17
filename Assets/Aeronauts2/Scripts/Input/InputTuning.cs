using UnityEngine;

namespace Aeronauts2.Input
{
    [CreateAssetMenu(fileName = "InputTuning", menuName = "Aeronauts 2/Input Tuning")]
    public sealed class InputTuning : ScriptableObject
    {
        [Header("Roll gestures")]
        [SerializeField, Min(0.05f)] private float automaticRollDoubleTapWindow = 0.25f;

        public float AutomaticRollDoubleTapWindow => automaticRollDoubleTapWindow;
    }
}
