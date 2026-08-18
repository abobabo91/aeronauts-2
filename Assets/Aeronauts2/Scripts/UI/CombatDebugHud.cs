using Aeronauts2.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace Aeronauts2.UI
{
    public sealed class CombatDebugHud : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponController weapon;
        [SerializeField] private CombatantHealth playerHealth;
        [SerializeField] private CombatantHealth botHealth;
        [SerializeField] private BotFlightController botFlight;
        [SerializeField] private BotWeaponController botWeapon;
        [SerializeField] private DuelMatchController match;
        [SerializeField] private Text combatStatusText;
        [SerializeField] private Text hitMarkerText;
        [SerializeField] private Text blockedMarkerText;

        public void Configure(
            PlayerWeaponController weaponController,
            CombatantHealth playerCombatant,
            CombatantHealth botCombatant,
            BotFlightController botFlightController,
            BotWeaponController botWeaponController,
            DuelMatchController duelMatch,
            Text statusText,
            Text hitMarker,
            Text blockedMarker)
        {
            weapon = weaponController;
            playerHealth = playerCombatant;
            botHealth = botCombatant;
            botFlight = botFlightController;
            botWeapon = botWeaponController;
            match = duelMatch;
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
                string targetLifecycle;
                if (weapon.LastTargetIsEliminated)
                {
                    targetLifecycle = $"RESPAWN {weapon.LastTargetRespawnRemaining:0.0}s";
                }
                else if (weapon.LastTargetMaximumShield <= 0f)
                {
                    targetLifecycle = "NO TARGET";
                }
                else if (weapon.LastTargetShield > 0f)
                {
                    targetLifecycle = "SHIELDED";
                }
                else
                {
                    targetLifecycle = "SHIELD DOWN";
                }

                string matchState = match == null
                    ? "DUEL UNAVAILABLE"
                    : match.MatchComplete
                        ? $"MATCH OVER  {match.Winner} WINS"
                        : $"FIRST TO {match.ScoreToWin}  PLAYER {match.PlayerScore} - {match.BotScore} BOT";
                string playerState = FormatCombatantState("PLAYER", playerHealth);
                string botState = FormatCombatantState("BOT", botHealth);
                float botSpeed = botFlight != null ? botFlight.State.Speed : 0f;
                int botShots = botWeapon != null ? botWeapon.PrimaryShotsFired : 0;
                int botHits = botWeapon != null ? botWeapon.Hits : 0;
                combatStatusText.text =
                    $"{matchState}\n"
                    + $"{playerState}\n"
                    + $"{botState}\n"
                    + $"BOT FLIGHT    {botSpeed:0} u/s  FIRE {botHits}/{botShots}\n"
                    + $"PRIMARY BOLTS  {weapon.PrimaryShotsFired}\n"
                    + $"RAILGUN       {state.RailgunAmmo}/{railgunCapacity}\n"
                    + $"HITS          {weapon.Hits}/{weapon.TotalShotsFired}  ({weapon.Accuracy * 100f:0}%)\n"
                    + $"CRITICAL      {weapon.CriticalHits}\n"
                    + $"ELIMINATIONS  {weapon.Eliminations}\n"
                    + $"HEAD ORIGIN   {(weapon.MuzzleBlocked ? "BLOCKED" : "CLEAR")}\n"
                    + $"LAST TARGET   S {weapon.LastTargetShield:0}/{weapon.LastTargetMaximumShield:0}  "
                    + $"H {weapon.LastTargetHull:0}/{weapon.LastTargetMaximumHull:0}\n"
                    + $"TARGET STATE  {targetLifecycle}";
            }

            if (hitMarkerText != null)
            {
                bool showHit = weapon.LastHitDisplayRemaining > 0f;
                hitMarkerText.enabled = showHit;
                if (showHit)
                {
                    if (weapon.LastHitEliminated)
                    {
                        hitMarkerText.text = weapon.LastHitBypassedShield
                            ? "RAIL CRITICAL"
                            : "ELIMINATION";
                        hitMarkerText.color = new Color(1f, 0.15f, 0.05f);
                    }
                    else if (weapon.LastHitBrokeShield)
                    {
                        hitMarkerText.text = weapon.LastHitWasCritical
                            ? "CRITICAL BREAK"
                            : "SHIELD BREAK";
                        hitMarkerText.color = Color.white;
                    }
                    else
                    {
                        hitMarkerText.text = weapon.LastHitWasCritical ? "X!" : "X";
                        hitMarkerText.color = weapon.LastHitWasCritical
                            ? new Color(1f, 0.28f, 0.08f)
                            : weapon.LastHitShieldDamage > 0f
                                ? new Color(0.55f, 0.95f, 1f)
                                : new Color(1f, 0.25f, 0.12f);
                    }
                }
            }

            if (blockedMarkerText != null)
            {
                blockedMarkerText.enabled = weapon.MuzzleBlocked;
            }
        }

        private static string FormatCombatantState(string label, CombatantHealth combatant)
        {
            if (combatant == null || combatant.Tuning == null)
            {
                return $"{label,-6}        UNAVAILABLE";
            }

            if (combatant.IsEliminated)
            {
                return $"{label,-6}        RESPAWN {combatant.RespawnRemaining:0.0}s";
            }

            return $"{label,-6}        S {combatant.Shield:0}/{combatant.Tuning.MaximumShield:0}  "
                + $"H {combatant.Hull:0}/{combatant.Tuning.MaximumHull:0}";
        }
    }
}
