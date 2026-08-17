namespace Aeronauts2.Core
{
    public readonly struct FireIntent
    {
        public FireIntent(bool primaryHeld, bool secondaryPressed)
        {
            PrimaryHeld = primaryHeld;
            SecondaryPressed = secondaryPressed;
        }

        public bool PrimaryHeld { get; }

        public bool SecondaryPressed { get; }

        public static FireIntent None => new(false, false);
    }
}
