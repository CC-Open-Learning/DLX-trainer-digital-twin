using SpiceSharp;
using SpiceSharp.Components;
using System;
using System.Collections.Generic;

namespace VARLab.MPCircuits.Model
{
    public class ResistorModel : CircuitComponentModel
    {
        /// <summary>
        ///     Indicates whether the component is connected to the circuit board.
        /// </summary>
        public bool IsConnected = true;

        private protected Port a;
        private protected Port b;

        private protected bool aUpdated;
        private protected bool bUpdated;


        public ResistorModel(string name, Port a, Port b, double resistance)
            : base(name, a, b)
        {
            this.a = a;
            this.b = b;
            base.Resistance = resistance;

            a.VoltageChanged += OnVoltageChanged_A;
            b.VoltageChanged += OnVoltageChanged_B;
        }


        /// <summary>
        /// If both of our ports are connected, then add the appropriate spicesharp model to the circuit
        /// </summary>
        /// <param name="c">Circuit object to add the component to</param>
        public override void AddComponentsToCircuit(Circuit c)
        {
            if (IsConnected && a.Connected && b.Connected)
            {
                c.Add(new Resistor(Name, a.Name, b.Name, Resistance));
                UnityEngine.Debug.Log($"Creating {Name}");
            }
        }

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
        public override List<Port> GetOppositePorts(Port port)
        {
            return IsConnected ? base.GetOppositePorts(port) : new();
        }

        protected virtual void OnAllPortsUpdated()
        {
            if (IsConnected && !double.IsNaN(a.Voltage) && !double.IsNaN(b.Voltage))
            {
                Voltage = Math.Abs(a.Voltage - b.Voltage);
                Current = Voltage / Resistance;
            }
            else
            {
                Voltage = double.NaN;
                Current = double.NaN;
            }

            OnValuesUpdated?.Invoke(this);
        }

        /// <summary>
        ///     If both ports have a change in voltage, this method will
        ///     reset the port voltage update flags to false and call the
        ///     CalculateCurrent method.
        /// </summary>
        protected void TryUpdateCurrent()
        {
            if (!aUpdated || !bUpdated) { return; }

            OnAllPortsUpdated();
            aUpdated = false;
            bUpdated = false;
        }

        /// <summary>
        ///     Marks port "a" as "updated"
        /// </summary>
        /// <remarks>
        ///     When both port "a" and port "b" are updated, the current should 
        ///     be recalculated    
        /// </remarks>
        /// <param name="voltage"></param>
        private void OnVoltageChanged_A(double voltage)
        {
            aUpdated = true;

            TryUpdateCurrent();
        }


        /// <summary>
        ///     Marks port "b" as "updated"
        /// </summary>
        /// <remarks>
        ///     When both port "a" and port "b" are updated, the current should 
        ///     be recalculated    
        /// </remarks>
        /// <param name="voltage"></param>
        private void OnVoltageChanged_B(double voltage)
        {
            bUpdated = true;

            TryUpdateCurrent();
        }

    }
}
