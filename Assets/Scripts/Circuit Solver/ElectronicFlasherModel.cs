using SpiceSharp;
using SpiceSharp.Components;
using System.Collections.Generic;

namespace VARLab.MPCircuits.Model
{
    public class ElectronicFlasherModel : CircuitComponentModel
    {
        public const double InternalBaseCapacitance = 1e-6; // A base capacitance value
        public const float InternalClosedResistance = 0.1f; // A base closed resistance value
        public const float InternalOpenResistance = 7800000; // A base open resistance value
        public const float SignalTimer = 0.50f; // A base signal timer
        public const float VoltageThreshold = 8.80f; // A base voltage threshold

        protected CapacitorModel capacitorModel;
        protected Port anode;
        protected Port cathode;
        protected Port signal;
        protected double capacitance;
        protected bool anodeUpdated;
        protected bool cathodeUpdated;
        protected bool signalUpdated;

        public bool IsSignalOn;

        /// <summary>
        ///     Constructor for the CapacitorModel which allows capacitance to be specified.
        ///     The internal <see cref="CapacitorModel"/> is configured
        ///     based on these settings
        /// </summary>
        public ElectronicFlasherModel(string name, Port anode, Port cathode, Port signal, double capacitance, double resistance)
            : base(name, anode, cathode, signal)
        {
            this.signal = signal;
            this.anode = anode;
            this.cathode = cathode;
            this.capacitance = capacitance;

            Resistance = resistance;

            SetUpCapacitorModel(capacitance);

            anode.VoltageChanged += SetAnodeUpdated;
            cathode.VoltageChanged += SetCathodeUpdated;
            signal.VoltageChanged += SetSignalUpdated;
        }

        /// <summary>
        ///     Configures the <see cref="CapacitorModel"/> with the given values.
        /// </summary>
        /// <remarks>
        ///     Additional parameters are used from an online source that should be cited here
        /// </remarks>
        /// <param name="capacitance">the maximum charge value of the capacitor</param>
        protected virtual void SetUpCapacitorModel(double capacitance)
        {
            capacitorModel = new CapacitorModel("CapacitorModel_" + Name);
            capacitorModel.Parameters.JunctionCap = capacitance;
            capacitorModel.Parameters.JunctionCapSidewall = capacitance;
            capacitorModel.Parameters.TemperatureCoefficient1 = 1.752;
            capacitorModel.Parameters.Narrow = 1;
            capacitorModel.Parameters.DefaultWidth = 1;
        }

        public override void AddComponentsToCircuit(Circuit c)
        {
            // the connection between battery positive and battery negative
            //if (anode.SharedPorts.Count > 0 && cathode.SharedPorts.Count > 0 && Port.IsPartOfCircuit(anode, cathode))
            if (anode.Connected && cathode.Connected)
            {
                c.Add(new Capacitor(Name, anode.Name, cathode.Name, capacitorModel.Name));
                c.Add(capacitorModel);
            }

            // the connection between battery positive and signal
            //if (anode.SharedPorts.Count > 0 && signal.SharedPorts.Count > 0 && Port.IsPartOfCircuit(anode, signal))
            if (anode.Connected && signal.Connected)
            {
                c.Add(new Resistor(Name + "_VSig", anode.Name, signal.Name, IsSignalOn ? InternalClosedResistance : InternalOpenResistance));
            }
        }

        /// <summary>
        ///     Marks the anode as "updated"
        /// </summary>
        /// <remarks>
        ///     When anode, cathode and signal are updated, the current should be recalculated    
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
        ///     When anode, cathode and signal are updated, the current should be recalculated    
        /// </remarks>
        /// <param name="voltage"></param>
        protected virtual void SetCathodeUpdated(double voltage)
        {
            cathodeUpdated = true;

            TryUpdateCurrent();
        }

        /// <summary>
        ///     Marks signal as "updated"
        /// </summary>
        /// <remarks>
        ///     When anode, cathode and signal are updated, the current should be recalculated    
        /// </remarks>
        /// <param name="voltage"></param>
        protected virtual void SetSignalUpdated(double voltage)
        {
            signalUpdated = true;

            TryUpdateCurrent();
        }

        /// <summary>
        ///     Checks to see if current should be updated, only if
        ///     both anode and cathode voltages have been updated.
        /// </summary>
        protected void TryUpdateCurrent()
        {
            if (!anodeUpdated || !cathodeUpdated || !signalUpdated) { return; }

            CalculateCurrent();
            anodeUpdated = false;
            cathodeUpdated = false;
            signalUpdated = false;
        }

        protected virtual void CalculateCurrent()
        {
            if (double.IsNaN(anode.Voltage) || double.IsNaN(cathode.Voltage) || double.IsNaN(signal.Voltage))
            {
                Voltage = 0;
                Current = 0;
            }
            else
            {
                Voltage = (anode.Voltage - cathode.Voltage);
                Current = Voltage / Resistance;

                UpdateFlasher();
            }
        }

        /// <summary>
        /// Updates the flasher model logic that will be used for the functionality of the flasher script
        /// </summary>
        protected void UpdateFlasher()
        {
            OnValuesUpdated?.Invoke(this);
        }

        /// <summary>
        ///     Defines the internal port mapping for this component
        /// </summary>
        /// <remarks>
        ///     The ANODE port is connected to both CATHODE and SIGNAL
        ///     
        ///     CATHODE and SIGNAL are not directly connected
        /// </remarks>
        /// <param name="port"></param>
        /// <returns></returns>
        public override List<Port> GetOppositePorts(Port port)
        {
            List<Port> ports = new();

            if (port == anode)
            {
                ports.Add(cathode);
                ports.Add(signal);
            }
            else if (port == cathode)
            {
                ports.Add(anode);
            }
            else if (port == signal)
            {
                ports.Add(anode);
            }

            return ports;
        }
    }
}
