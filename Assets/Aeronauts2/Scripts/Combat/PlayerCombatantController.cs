using Aeronauts2.Flight;
using UnityEngine;

namespace Aeronauts2.Combat
{
    public sealed class PlayerCombatantController : MonoBehaviour
    {
        [SerializeField] private CombatantHealth health;
        [SerializeField] private FlightMotor flightMotor;
        [SerializeField] private PlayerWeaponController weapon;

        private CombatantHealth subscribedHealth;

        public void Configure(
            CombatantHealth playerHealth,
            FlightMotor motor,
            PlayerWeaponController weaponController)
        {
            Unsubscribe();
            health = playerHealth;
            flightMotor = motor;
            weapon = weaponController;
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

        private void Subscribe()
        {
            if (health == null || subscribedHealth == health)
            {
                return;
            }

            Unsubscribe();
            subscribedHealth = health;
            subscribedHealth.Eliminated += OnEliminated;
            subscribedHealth.Respawned += OnRespawned;
        }

        private void Unsubscribe()
        {
            if (subscribedHealth == null)
            {
                return;
            }

            subscribedHealth.Eliminated -= OnEliminated;
            subscribedHealth.Respawned -= OnRespawned;
            subscribedHealth = null;
        }

        private void OnEliminated(CombatantHealth _, DamageReport __)
        {
            if (flightMotor != null)
            {
                flightMotor.enabled = false;
            }

            if (weapon != null)
            {
                weapon.enabled = false;
            }
        }

        private void OnRespawned(CombatantHealth _)
        {
            if (flightMotor != null)
            {
                flightMotor.enabled = true;
                flightMotor.ResetFlight();
            }

            if (weapon != null)
            {
                weapon.enabled = true;
                weapon.ResetCombat();
            }
        }
    }
}
