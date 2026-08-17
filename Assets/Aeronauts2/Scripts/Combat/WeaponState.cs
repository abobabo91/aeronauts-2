namespace Aeronauts2.Combat
{
    public struct WeaponState
    {
        public float PrimaryCooldownRemaining;
        public float RailgunCooldownRemaining;
        public int RailgunAmmo;

        public static WeaponState Create(int railgunCapacity)
        {
            return new WeaponState
            {
                PrimaryCooldownRemaining = 0f,
                RailgunCooldownRemaining = 0f,
                RailgunAmmo = railgunCapacity,
            };
        }
    }
}
