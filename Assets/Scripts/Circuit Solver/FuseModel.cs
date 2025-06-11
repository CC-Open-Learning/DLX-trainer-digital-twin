namespace VARLab.MPCircuits.Model
{
    /// <summary>
    ///     This class contains the structure for the model of a fuse. 
    ///     It is effectively a resistor, with the added property that 
    ///     it can be "defective", and thus have a higher resistance.
    /// </summary>
    public class FuseModel : ResistorModel
    {

        /// <summary>
        ///     Resistance of a 3A 32V fuse
        /// </summary>
        /// <remarks>
        ///     Calculates resistance as 3/32
        /// </remarks>
        public const double DefaultResistance = 0.09375f;

        /// <summary>
        ///     Resistance of a defective fuse
        /// </summary>
        public const double DefectiveResistance = 93.0;


        public bool IsDefective = false;  // is the fuse defective

        /// <summary> Resistance defined for the fuse </summary>
        public override double Resistance
        {
            get => IsDefective ? DefectiveResistance : base.Resistance;
        }


        /// <summary>
        ///     Constructor for the FuseModel.
        ///     The internal <see cref="CircuitComponentModel"/> 
        ///     is configured.
        /// </summary>
        public FuseModel(string name, Port a, Port b, bool connected = true) 
            : base(name, a, b, DefaultResistance)
        {
            IsConnected = connected;
        }
    }
}
