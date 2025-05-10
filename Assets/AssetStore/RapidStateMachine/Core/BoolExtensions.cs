namespace RSM
{
    public static class BoolExtensions
    {
        public static void Trigger(this ref bool value) => value = true;

        public static void Cancel(this ref bool value) => value = false;
    }
}