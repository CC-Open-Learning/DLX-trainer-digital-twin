namespace VARLab.MPCircuits.Model
{
    /// <summary>
    ///     This class contains the structure for the model of a switch
    /// </summary>
    public class SwitchModel : ResistorModel
    {

        /// <summary>
        ///     Constructor for the SwitchModel.
        ///     The internal <see cref="CircuitComponentModel"/> is configured.
        /// </summary>
        public SwitchModel(string name, Port a, Port b, bool closed = false)
            : base(name, a, b, NoResistance)
        {
            IsConnected = closed;
        }
    }
}
