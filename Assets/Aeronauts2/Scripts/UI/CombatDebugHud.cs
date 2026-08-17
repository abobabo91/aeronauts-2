using Aeronauts2.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace Aeronauts2.UI
{
    public sealed class CombatDebugHud : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponController weapon;
        [SerializeField] private Text combatStatusText;
        [SerializeField] private Text hitMarkerText;
        [SerializeField] private Text blockedMarkerText;

        public void Configure(
            PlayerWeaponController weaponController,
            Text statusText,
            Text hitMarker,
            Text blockedMarker)
        {
            weapon = weaponController;
            combatStatusText = statusText;
            hitMarkerText = hitMarker;
            blockedMarkerText = blockedMarker;
        }

        private void Update()
        {
            if (weapon == null)
            {
                return;
            }

            if (combatStatusText != null)
            {
                WeaponState state = weapon.State;
                int railgunCapacity = weapon.Tuning != null ? weapon.Tuning.RailgunCapacity : 0;
                combatStatusText.text =
                    $"PRIMARY BOLTS  {weapon.PrimaryShotsFired}\n"
                    + $"RAILGUN       {state.RailgunAmmo}/{railgunCapacity}\n"
                    + $"HITS          {weapon.Hits}/{weapon.TotalShotsFired}  ({weapon.Accuracy * 100f:0}%)\n"
                    + $"CRITICAL      {weapon.CriticalHits}\n"
                    + $"HEAD ORIGIN   {(weapon.MuzzleBlocked ? "BLOCKED" : "CLEAR")}\n"
                    + $"LAST TARGET   S {weapon.LastTargetShield:0}  H {weapon.LastTargetHull:0}";
            }

            if (hitMarkerText != null)
            {
                bool showHit = weapon.LastHitDisplayRemaining > 0f;
                hitMarkerText.enabled = showHit;
                if (showHit)
                {
                    hitMarkerText.text = weapon.LastHitWasCritical ? "X!" : "X";
                    hitMarkerText.color = weapon.LastHitWasCritical
                        ? new Color(1f, 0.28f, 0.08f)
                        : new Color(0.55f, 0.95f, 1f);
                }
            }

            if (blockedMarkerText != null)
            {
                blockedMarkerText.enabled = weapon.MuzzleBlocked;
            }
        }
    }
}
