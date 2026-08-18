using Aeronauts2.Flight;
using UnityEngine;

namespace Aeronauts2.Combat
{
    public sealed class DuelMatchController : MonoBehaviour
    {
        [SerializeField] private FlightMotor playerFlightMotor;
        [SerializeField] private PlayerWeaponController playerWeapon;
        [SerializeField] private BotWeaponController botWeapon;
        [SerializeField] private CombatantHealth playerHealth;
        [SerializeField] private CombatantHealth botHealth;
        [SerializeField, Min(1)] private int scoreToWin = 5;

        private PlayerWeaponController subscribedPlayerWeapon;
        private BotWeaponController subscribedBotWeapon;
        private FlightMotor subscribedFlightMotor;

        public int PlayerScore { get; private set; }
        public int BotScore { get; private set; }
        public int ScoreToWin => scoreToWin;
        public bool MatchComplete { get; private set; }
        public string Winner => !MatchComplete
            ? string.Empty
            : PlayerScore >= scoreToWin ? "PLAYER" : "BOT";

        public void Configure(
            FlightMotor motor,
            PlayerWeaponController playerWeaponController,
            BotWeaponController botWeaponController,
            CombatantHealth playerCombatant,
            CombatantHealth botCombatant,
            int winningScore)
        {
            Unsubscribe();
            playerFlightMotor = motor;
            playerWeapon = playerWeaponController;
            botWeapon = botWeaponController;
            playerHealth = playerCombatant;
            botHealth = botCombatant;
            scoreToWin = Mathf.Max(1, winningScore);
            ResetMatch();
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void ResetMatch()
        {
            PlayerScore = 0;
            BotScore = 0;
            MatchComplete = false;
        }

        private void OnPlayerEliminatedTarget(
            PlayerWeaponController _,
            CombatantHealth eliminatedTarget)
        {
            if (MatchComplete || eliminatedTarget != botHealth)
            {
                return;
            }

            PlayerScore++;
            MatchComplete = PlayerScore >= scoreToWin;
        }

        private void OnBotEliminatedTarget(
            BotWeaponController _,
            CombatantHealth eliminatedTarget)
        {
            if (MatchComplete || eliminatedTarget != playerHealth)
            {
                return;
            }

            BotScore++;
            MatchComplete = BotScore >= scoreToWin;
        }

        private void Subscribe()
        {
            if (playerWeapon != null && subscribedPlayerWeapon != playerWeapon)
            {
                subscribedPlayerWeapon = playerWeapon;
                subscribedPlayerWeapon.TargetEliminated += OnPlayerEliminatedTarget;
            }

            if (botWeapon != null && subscribedBotWeapon != botWeapon)
            {
                subscribedBotWeapon = botWeapon;
                subscribedBotWeapon.TargetEliminated += OnBotEliminatedTarget;
            }

            if (playerFlightMotor != null && subscribedFlightMotor != playerFlightMotor)
            {
                subscribedFlightMotor = playerFlightMotor;
                subscribedFlightMotor.ManualResetRequested += ResetMatch;
            }
        }

        private void Unsubscribe()
        {
            if (subscribedPlayerWeapon != null)
            {
                subscribedPlayerWeapon.TargetEliminated -= OnPlayerEliminatedTarget;
                subscribedPlayerWeapon = null;
            }

            if (subscribedBotWeapon != null)
            {
                subscribedBotWeapon.TargetEliminated -= OnBotEliminatedTarget;
                subscribedBotWeapon = null;
            }

            if (subscribedFlightMotor != null)
            {
                subscribedFlightMotor.ManualResetRequested -= ResetMatch;
                subscribedFlightMotor = null;
            }
        }
    }
}
