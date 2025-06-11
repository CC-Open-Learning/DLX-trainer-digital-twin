using SpiceSharp;
using System;
using System.Collections.Generic;

namespace VARLab.MPCircuits.Model
{
    public class CircuitComponentModel
    {
        public static readonly float DisconnectedResistance = 1e8f;
        public static readonly float NoResistance = 0f;


        public readonly string Name;

        public List<Port> Ports = new();

        /// <summary> Voltage across the component </summary>
        public virtual double Voltage { get; set; }

        /// <summary> Current calculated based on voltage and resistance </summary>
        public virtual double Current { get; set; }

        public virtual double Resistance { get; set; }


        public Action<CircuitComponentModel> OnValuesUpdated;

        public CircuitComponentModel(string name, params Port[] port)
        {
            Name = name;
            Ports = new(port);

            ConnectPorts();
        }

        /// <summary>
        /// Add the component resistor(s) (and possibly voltage source) to the circuit if all component ports
        /// are connected to other ports
        /// </summary>
        public virtual void AddComponentsToCircuit(Circuit circuit) { }

        /// <summary>
        ///     Returns the port on the other side of the component.
        ///     Needed for traversal alghorithm that uses this function
        /// </summary>
        /// <param name="port">
        ///     The port whose neighbor we want to find
        /// </param>
        /// <returns>
        ///     In the default implementation, returns an a copy of the list 
        ///     with all ports, with the paramter <paramref name="port"/> removed.
        /// </returns>
        public virtual List<Port> GetOppositePorts(Port port)
        {
            List<Port> ports = new(Ports);

            ports.Remove(port);

            return ports;
        }

        /// <summary>
        ///     Adds the component to the ConnectedComponents list
        ///     for all of its ports
        /// </summary>
        public virtual void ConnectPorts()
        {
            foreach (Port port in Ports)
            {
                port.ConnectedComponents.Add(this);
            }
        }

        /// <summary>
        ///     Removes the component to the ConnectedComponents list
        ///     from all of its ports
        /// </summary>
        public virtual void DisconnectPorts()
        {
            foreach (Port port in Ports)
            {
                port.ConnectedComponents.Remove(this);
            }
        }
    }
}
