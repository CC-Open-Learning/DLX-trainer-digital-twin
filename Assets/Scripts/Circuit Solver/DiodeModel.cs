using SpiceSharp;
using SpiceSharp.Components;
using System;

namespace VARLab.MPCircuits.Model
{
    public class DiodeModel : CircuitComponentModel
    {
        public const double InternalBaseResistance = 0.568f;
        public const double InternalLEDResistance = 750.0f;   // Calculated resistance in Ohms of the resistor inside the LED
        public const double InternalB1Resistance = 3000.0f;

        protected SpiceSharp.Components.DiodeModel diodeModel;

        protected Port anode;
        protected Port cathode;

        protected bool anodeUpdated;
        protected bool cathodeUpdated;


        /// <summary> Resistance defined for the diode </summary>
        public override double Resistance
        {
            get => diodeModel == null ? double.NaN : diodeModel.Parameters.Resistance;
            set
            {
                if (diodeModel != null) { diodeModel.Parameters.Resistance = value; }
            }
        }


        /// <summary>
        ///     Constructor with no resistance value specified uses 
        ///     the <see cref="InternalBaseResistance"/> constant
        /// </summary>
        public DiodeModel(string name, Port anode, Port cathode)
            : this(name, anode, cathode, InternalBaseResistance) { }


        /// <summary>
        ///     Constructor for the DiodeModel which allows resistance to be specified.
        ///     The internal <see cref="SpiceSharp.Components.DiodeModel"/> is configured
        ///     based on these settings
        /// </summary>
        public DiodeModel(string name, Port anode, Port cathode, double resistance)
            : base(name, anode, cathode)
        {
            this.anode = anode;
            this.cathode = cathode;

            SetupDiodeModel(resistance);

            anode.VoltageChanged += SetAnodeUpdated;
            cathode.VoltageChanged += SetCathodeUpdated;
        }

        public override void AddComponentsToCircuit(Circuit c)
        {
            //if (anode.SharedPorts.Count > 0 && cathode.SharedPorts.Count > 0 && Port.IsPartOfCircuit(anode, cathode))
            if (anode.Connected && cathode.Connected)
            {
                c.Add(new Diode(Name, anode.Name, cathode.Name, diodeModel.Name));
                c.Add(diodeModel);

                UnityEngine.Debug.Log($"Creating {Name}");
            }
        }

        /// <summary>
        ///     Configures the <see cref="SpiceSharp.Components.DiodeModel"/> with the given resistance.
        /// </summary>
        /// <remarks>
        ///     Additional parameters are used from an online source that should be cited here
        /// </remarks>
        /// <param name="resistance"></param>
        protected virtual void SetupDiodeModel(double resistance)
        {
            diodeModel = new SpiceSharp.Components.DiodeModel("DiodeModel_" + Name);
            diodeModel.Parameters.Resistance = resistance;
            diodeModel.Parameters.SaturationCurrent = 2.52e-9;
            diodeModel.Parameters.EmissionCoefficient = 1.752;
            diodeModel.Parameters.JunctionCap = 4e-12;
            diodeModel.Parameters.GradingCoefficient = 0.4f;
            diodeModel.Parameters.TransitTime = 20e-9;
        }

        /// <summary>
        ///     Marks the anode as "updated"
        /// </summary>
        /// <remarks>
        ///     When both anode and cathode are updated, the current should be recalculated    
        /// </remarks>
        /// <param name="voltage"></param>
        protected virtual void SetAnodeUpdated(double voltage)
        {
            anodeUpdated = true;

            TryUpdateCurrent();

        }

        /// <summary>
        ///     Marks the cathode as "updated"
        /// </summary>
        /// <remarks>
        ///     When both anode and cathode are updated, the current should be recalculated    
        /// </remarks>
        /// <param name="voltage"></param>
        protected virtual void SetCathodeUpdated(double voltage)
        {
            cathodeUpdated = true;

            TryUpdateCurrent();
        }

        /// <summary>
        ///     Checks to see if current should be updated, only if
        ///     both anode and cathode voltages have been updated.
        /// </summary>
        protected void TryUpdateCurrent()
        {
            if (!anodeUpdated || !cathodeUpdated) { return; }

            CalculateCurrent();
            anodeUpdated = false;
            cathodeUpdated = false;
        }

        protected virtual void CalculateCurrent()
        {
            if (double.IsNaN(anode.Voltage) || double.IsNaN(cathode.Voltage))
            {
                Voltage = 0;
                Current = 0;
            }
            else
            {
                Voltage = anode.Voltage - cathode.Voltage;
                Current = Math.Max(Voltage / diodeModel.Parameters.Resistance, 0);
            }

            OnValuesUpdated?.Invoke(this);
        }
    }
}
