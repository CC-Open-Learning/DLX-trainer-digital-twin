namespace VARLab.MPCircuits.Model
{

    /// <summary>
    ///     The motor is a resistor but we want to get voltage as an integer,
    ///     which can be positive (clockwise rotation) or negative (counter-clockwise)
    /// </summary>
    public class MotorModel : ResistorModel
    {
        public MotorModel(string name, Port a, Port b, double resistance)
            : base(name, a, b, resistance) { }

        protected override void OnAllPortsUpdated()
        {
            if (IsConnected && !double.IsNaN(a.Voltage) && !double.IsNaN(b.Voltage))
            {
                Voltage = a.Voltage - b.Voltage;
                Current = Voltage / Resistance;
            }
            else
            {
                Voltage = double.NaN;
                Current = double.NaN;
            }

            OnValuesUpdated?.Invoke(this);
        }
    }
}
