namespace VARLab.MPCircuits.Model
{
    public static class Resistance
    {
        // Creating user story to address this later on.
        // Non-zero values cause malfunction in board overload (it never shorts)
        public const double NegligibleResistance = 0f;//1e-6f;
        public const double FaultyCableResistance = 20.0;
        public const double MotorResistance = 1000.0f;
    }
}
