using Aeronauts2.Flight;
using UnityEngine;

namespace Aeronauts2.Combat
{
    public sealed class CombatDrillController : MonoBehaviour
    {
        [SerializeField] private FlightMotor flightMotor;
        [SerializeField] private PlayerWeaponController weapon;
        [SerializeField] private CombatantHealth[] combatants;

        private FlightMotor subscribedMotor;

        public void Configure(
            FlightMotor motor,
            PlayerWeaponController weaponController,
            CombatantHealth[] drillCombatants)
        {
            UnsubscribeFromReset();
            flightMotor = motor;
            weapon = weaponController;
            combatants = drillCombatants;
            SubscribeToReset();
        }

        private void OnEnable()
        {
            SubscribeToReset();
        }

        private void OnDisable()
        {
            UnsubscribeFromReset();
        }

        public void ResetDrill()
        {
            weapon?.ResetCombat();
            if (combatants == null)
            {
                return;
            }

            foreach (CombatantHealth combatant in combatants)
            {
                combatant?.ResetCombatant();
            }
        }

        private void SubscribeToReset()
        {
            if (flightMotor == null || subscribedMotor == flightMotor)
            {
                return;
            }

            UnsubscribeFromReset();
            subscribedMotor = flightMotor;
            subscribedMotor.ManualResetRequested += ResetDrill;
        }

        private void UnsubscribeFromReset()
        {
            if (subscribedMotor == null)
            {
                return;
            }

            subscribedMotor.ManualResetRequested -= ResetDrill;
            subscribedMotor = null;
        }
    }
}
