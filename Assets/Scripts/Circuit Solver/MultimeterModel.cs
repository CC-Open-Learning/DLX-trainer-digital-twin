using SpiceSharp;
using SpiceSharp.Components;
using System.Collections.Generic;

namespace VARLab.MPCircuits.Model
{
    public class MultimeterModel : CircuitComponentModel
    {
        /// <summary>
        ///     Enumerator which determines the 
        /// </summary>
        public enum MultimeterState 
        {
            Undefined = -1,
            Off = 0,
            DCVoltage, 
            DCCurrent, 
            Resistance
        }

        /// <summary>
        ///     Ammeter constant resistance 1.8 Ohms based on 60mA to 400 mA range.
        ///     <br />
        ///     Reference:
        ///     <see href="https://www.grainger.com/ec/pdf/Fluke-Digital-Multimeters-Detailed-Specifications-Sheet.pdf">
        ///         Fluke Multimeter Specification
        ///     </see>
        /// </summary>
        /// <remarks>
        ///     Through experimentation with the physical trainer board, 
        ///     it appears that the resistance of the Ammeter should be about that of
        ///     the fuse F1, at 0.1 Ohms. This then provides a "split" amperage reading
        ///     when connecting the fuse and the ammeter in parallel, 
        ///     as observed values on the real board.
        /// </remarks>
        public const double AmmeterMinimumResistance = 0.1;

        /// <summary>
        ///     Input resistance of 10M Ohms when measuring DC Voltage, 
        ///     as defined in the 
        ///     <see href="https://www.fluke.com/en-ca/product/electrical-testing/digital-multimeters/fluke-107">
        ///         FLUKE 107 Multimeter specification
        ///     </see>
        /// </summary>
        public const double InputImpedance = 1e7; //34000;
        
        /// <summary>
        ///     Threshold for the fuse inside the Ammeter. 
        ///     If current across Ammeter exceeds this value, the fuse will blow.
        /// </summary>
        public const double AmmeterFuseThresholdAmps = 10;


        // These may be constants, also may change to 1 and 1? curious
        public readonly double OhmmeterSourceVoltage = 0.01;
        public readonly double OhmmeterSeriesResistance = 0.01;

        public MultimeterState CurrentState;

        public bool IsAmmeterFuseBlown;

        protected Port v;
        protected Port g;
        protected Port a;
        protected Port resistorPort;
        protected Port groundReference;

        protected bool vUpdated;
        protected bool gUpdated;
        protected bool aUpdated;

        public double VoltmeterVoltage { get; private set; }
        public double AmmeterCurrent { get; private set; }
        public double OhmmeterResistance { get; set; } // public set is for testing purpose

        public bool IsOhmmeterConnected => CurrentState == MultimeterState.Resistance;

        public double AmmeterResistance => IsAmmeterFuseBlown ? DisconnectedResistance : AmmeterMinimumResistance;


        public override double Resistance => CurrentState switch
        {
            MultimeterState.DCVoltage => InputImpedance,
            MultimeterState.DCCurrent => AmmeterResistance,
            MultimeterState.Resistance => OhmmeterResistance,
            _ => DisconnectedResistance,
        };


        public MultimeterModel(Port v, Port g, Port a, Port resistorPort, Port groundReference)
            : base("DMM", v, g, a)
        {
            this.v = v;
            this.g = g;
            this.a = a;
            this.resistorPort = resistorPort;
            this.groundReference = groundReference;

            v.VoltageChanged += OnVoltageChanged_V;
            g.VoltageChanged += OnVoltageChanged_G;
            a.VoltageChanged += OnVoltageChanged_A;
        }


        public override List<Port> GetOppositePorts(Port p)
        {
            List<Port> ports = new() { v, g, a };

            switch (CurrentState)
            {
                case MultimeterState.DCVoltage:
                    if (p == v || p == g)
                    {
                        ports.Remove(a);
                        ports.Remove(p); //self
                    }
                    return ports;
                case MultimeterState.DCCurrent:
                    // Could replace 'Disconnected Resistance' with simply
                    // returning an empty list if the ammeter fuse is blown
                    if (p == a || p == g)
                    {
                        ports.Remove(v);
                        ports.Remove(p); //self
                    }
                    return ports;
            }

            return new();
        }

        public override void AddComponentsToCircuit(Circuit c)
        {

            switch (CurrentState)
            {
                case MultimeterState.Off:
                    break;
                case MultimeterState.DCVoltage:
                    if (v.Connected && g.Connected)
                    {
                        c.Add(new Resistor("Voltmeter", v.Name, g.Name, InputImpedance));
                    }
                    break;
                case MultimeterState.DCCurrent:
                    if (a.Connected && g.Connected)
                    {
                        c.Add(new Resistor("Ammeter", a.Name, g.Name, AmmeterResistance));
                    }
                    break;
                case MultimeterState.Resistance:
                    c.Add(new Resistor("Rohmmeter", v.Name, resistorPort.Name, OhmmeterSeriesResistance));
                    c.Add(new VoltageSource("Vohmmeter", resistorPort.Name, g.Name, OhmmeterSourceVoltage));
                    break;
                default:
                    break;
            }
        }

        private void OnAllPortsUpdated()
        {
            switch (CurrentState)
            {
                case MultimeterState.Off:
                    break;
                case MultimeterState.DCVoltage:
                    if (!double.IsNaN(v.Voltage) && !double.IsNaN(g.Voltage))
                    {
                        VoltmeterVoltage = v.Voltage - g.Voltage;
                    }
                    else
                    {
                        VoltmeterVoltage = double.NaN;
                    }
                    break;
                case MultimeterState.DCCurrent:
                    if (!double.IsNaN(a.Voltage) && !double.IsNaN(g.Voltage))
                    {
                        AmmeterCurrent = (a.Voltage - g.Voltage) / AmmeterResistance;
                    }
                    else
                    {
                        AmmeterCurrent = 0;
                    }
                    break;
                case MultimeterState.Resistance:
                    if (!double.IsNaN(v.Voltage) && !double.IsNaN(g.Voltage))
                    {
                        OhmmeterResistance = CalculateExternalResistance(v.Voltage, g.Voltage, OhmmeterSourceVoltage, OhmmeterSeriesResistance);

                        //since we're not using recursion to find which components are attached, when connecting both v and g to ports that don't connect to anything in turn
                        //it results in a resistance of -9999
                        if (OhmmeterResistance < 0)
                        {
                            OhmmeterResistance = DisconnectedResistance;
                        }
                    }
                    else
                    {
                        OhmmeterResistance = DisconnectedResistance;
                    }
                    break;
                default:
                    break;
            }

            OnValuesUpdated?.Invoke(this);
        }

        /// <summary>
        /// Calculate the external resistance attached to the "V" and "G" ports of the ohmmeter
        /// </summary>
        /// <param name="v">The voltage at the "V" port of the multimeter</param>
        /// <param name="g">The voltage at the "G" port of the multimeter</param>
        /// <param name="Vs">The source voltage used by the ohmmeter to find the external resistance</param>
        /// <param name="Ro">The series resistance used by the ohmmeter to find the external resistance</param>
        /// <returns>The external resistance connected to the "V" and "G" ports of the multimeter</returns>
        private double CalculateExternalResistance(double v, double g, double Vs, double Ro)
        {
            double VRo = Vs - (v - g);  //VRo is the voltage drop across the internal resistor; since g is not guaranteed to be 0, the voltage at other end of the resistor is v - g
            double I = VRo / Ro;        //I is the current in the loop created when you attach an external resistance to the ohmmeter

            double VR1 = v - g;         //VR1 is the voltage drop across the external resistance
            double R1 = VR1 / I;        //R1 is the external resistance, we find it using ohm's law

            return R1;
        }

        private void OnVoltageChanged_V(double voltage)
        {
            vUpdated = true;

            if (vUpdated && gUpdated && aUpdated)
            {
                OnAllPortsUpdated();
                vUpdated = false;
                gUpdated = false;
                aUpdated = false;
            }
        }
        private void OnVoltageChanged_G(double voltage)
        {
            gUpdated = true;

            if (vUpdated && gUpdated && aUpdated)
            {
                OnAllPortsUpdated();
                vUpdated = false;
                gUpdated = false;
                aUpdated = false;
            }
        }
        private void OnVoltageChanged_A(double voltage)
        {
            aUpdated = true;

            if (vUpdated && gUpdated && aUpdated)
            {
                OnAllPortsUpdated();
                vUpdated = false;
                gUpdated = false;
                aUpdated = false;
            }
        }
    }
}
