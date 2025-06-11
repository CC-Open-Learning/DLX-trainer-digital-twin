using System;
using System.Collections.Generic;

namespace VARLab.MPCircuits.Model
{


    /// <summary>
    ///     Each port is created in the Ports dictionary in <see cref="CircuitBoard"/>
    ///     and is connected a Unity gameObject. Each gameObject port must have 
    ///     PortName enum assigned to it for the voltages to update
    /// </summary>
    public class Port
    {
        public string Name;

        public double Voltage = double.NaN;

        public bool Visited = false;
        public bool Connected = false;

        public List<CircuitComponentModel> ConnectedComponents = new();

        /// <summary>
        ///     <see cref="PortBehaviour"/> listens to this event
        /// </summary>
        public Action<double> VoltageChanged;


        // ============================ CONSTRUCTOR ============================
        /// <summary>
        ///     Creates the port object and assigns the original port name/string PortName
        /// </summary>
        public Port(string portName)
        {
            Name = portName;
        }

        // ============================ METHODS ============================
        /// <summary>
        ///     Update voltage of port in circuit and the 
        ///     Unity gameObject port it's matched with
        /// </summary>
        public void SetVoltage(double voltage)
        {
            Voltage = voltage;
            VoltageChanged?.Invoke(voltage);
        }
    }
}