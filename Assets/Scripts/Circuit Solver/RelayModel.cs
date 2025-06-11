using SpiceSharp;
using SpiceSharp.Components;
using System;
using System.Collections.Generic;

namespace VARLab.MPCircuits.Model
{
    public class RelayModel : CircuitComponentModel
    {
        //Standard naming + numbering convention for ports in a relay

        private protected Port RL_85;   // (relay coil negative) RL_85 and RL_86 are linked by an electromagnet and a resistor
        private protected Port RL_86;   // (relay coil positive) When connections are present at both of these ports (i.e Battery+ to RL_85, RL_86 to Ground), with required current flowing through, the relay will be actuated (if this makes 0 sense, read on) 

        private protected Port RL_87;   //(to load (normally open)) While actuated, current will flow through RL_87
        private protected Port RL_87A;  //(to load (normally closed)) Otherwise, current flows through RL_87A. This will also be the default route if there are not connections between RL_85 and RL_86
        private protected Port RL_30;   //Connected to the battery(+) typically

        protected bool RL_85Updated;
        protected bool RL_86Updated;

        public bool IsActuated { get; protected set; }
        public bool HasStateChanged { get; set; }   //true if relay has actuated or deactuated this update

        public const float CoilResistance = 85f;    //85 ohms

        public float CurrentThresholdAmps = 0.08f;  //will only actuate if the current flowing through the relay surpasses this value
        public float CurrentThresholdAmpsLow = 0.02f;   //will only deactuate if the current flowing through the relay is less than this value


        /// <summary>
        /// An instance of this model is created in CircuitBoard.cs
        /// </summary>
        /// <param name="name">Component name</param>
        /// <param name="RL_85"></param>
        /// <param name="RL_86"></param>
        /// <param name="RL_87"></param>
        /// <param name="RL_87A"></param>
        /// <param name="RL_30"></param>
        public RelayModel(string name, Port RL_85, Port RL_86, Port RL_87, Port RL_87A, Port RL_30, float RL85_86Resistance)
            : base(name, RL_85, RL_86, RL_87, RL_87A, RL_30)
        {
            this.RL_85 = RL_85;
            this.RL_86 = RL_86;
            this.RL_87 = RL_87;
            this.RL_87A = RL_87A;
            this.RL_30 = RL_30;

            // adds the corresponding port function as a listener to each ports VoltageChanged event
            RL_85.VoltageChanged += UpdateCoilNegative;
            RL_86.VoltageChanged += UpdateCoilPositive;
        }

        public override void AddComponentsToCircuit(Circuit c)
        {
            //for the electromagnet and resistor
            //if (RL_85.SharedPorts.Count > 0 && RL_86.SharedPorts.Count > 0 && Port.IsPartOfCircuit(RL_85, RL_86))
            if (RL_85.Connected && RL_86.Connected)
            {
                c.Add(new Resistor(Name + "Coil", RL_85.Name, RL_86.Name, CoilResistance));
            }

            //If RL_30 shares a connection with RL_87A, sets the resistance to either disconnected or none based on the actuation state of the relay
            //(i.e if IsActuated, voltage flows through RL_87 so RL_87A resistance needs to become DisconnectedResistance)
            //if (RL_30.SharedPorts.Count > 0 && RL_87A.SharedPorts.Count > 0 && Port.IsPartOfCircuit(RL_30, RL_87A))
            if (RL_30.Connected && RL_87A.Connected)
            {
                c.Add(new Resistor(Name + "A", RL_30.Name, RL_87A.Name, IsActuated ? DisconnectedResistance : NoResistance));
            }

            //If RL_30 shares a connection with RL_87, sets the resistance to either no resistance or disconnected resistance based on the actuation state of the relay
            //(i.e if not IsActuated, voltage flows through RL_87A, so RL_87 resistance needs to become DisconnectedResistance
            //if (RL_30.SharedPorts.Count > 0 && RL_87.SharedPorts.Count > 0 && Port.IsPartOfCircuit(RL_30, RL_87))
            if (RL_30.Connected && RL_87.Connected)
            {
                c.Add(new Resistor(Name + "B", RL_30.Name, RL_87.Name, IsActuated ? NoResistance : DisconnectedResistance));
            }
        }

        /// <summary>
        /// Updates RL_85Updated status and then tries to update the voltage
        /// </summary>
        /// <param name="voltage"></param>
        private void UpdateCoilNegative(double voltage)
        {
            RL_85Updated = true;

            TryUpdateVoltage();
        }

        /// <summary>
        /// Updates RL_86Updated status and then tries to update the voltage
        /// </summary>
        /// <param name="voltage"></param>
        private void UpdateCoilPositive(double voltage)
        {
            RL_86Updated = true;

            TryUpdateVoltage();
        }

        /// <summary>
        /// Calculates voltage if both RL_85 and 86 have been connected to
        /// </summary>
        protected void TryUpdateVoltage()
        {
            if (!RL_85Updated || !RL_86Updated) { return; }

            CalculateVoltage();
            RL_85Updated = false;
            RL_86Updated = false;
        }

        private void CalculateVoltage()
        {
            //if either port is disconnected, overrides current and voltage to 0
            if (double.IsNaN(RL_85.Voltage) || double.IsNaN(RL_86.Voltage))
            {
                Current = 0;
                Voltage = 0;
            }
            else
            {
                Voltage = Math.Abs(RL_85.Voltage - RL_86.Voltage);
                Current = Voltage / CoilResistance;     //I = V / R
            }

            //Relay is not actuated and current through RL1 is greater than CurrentThresholdAmps will cause relay to actuate
            if (!IsActuated && Current >= CurrentThresholdAmps)
            {
                IsActuated = true;
                HasStateChanged = true;
            }

            //Relay is actuated and current going through RL1 is less than CurrentThresholdAmpsLow will cause relay to deactuate (still don't care that this isn't a word)
            if (IsActuated && Current <= CurrentThresholdAmpsLow)
            {
                IsActuated = false;
                HasStateChanged = true;
            }

            //Updates port values if the state changed (i.e relay was either actuated or deactuated) and resets HasStateChanged to false
            if (HasStateChanged)
            {
                OnValuesUpdated?.Invoke(this);
                HasStateChanged = false;
            }
        }

        /// <summary>
        ///     Defines the internal port map for this component
        /// </summary>
        /// <remarks>
        ///     Ports 85 and 86 are connected across the electromagnet (and resistor)
        ///     
        ///     Port 30 is connected to:
        ///         * 87A when the relay is not actuated
        ///         * 87 when the relay is actuated
        /// </remarks>
        /// <param name="port"></param>
        /// <returns></returns>
        public override List<Port> GetOppositePorts(Port port)
        {
            List<Port> ports = new();

            if (port == RL_85)
            {
                ports.Add(RL_86);
            }
            else if (port == RL_86)
            {
                ports.Add(RL_85);
            }
            else if (port == RL_30)
            {
                ports.Add(IsActuated ? RL_87 : RL_87A);
            }
            else if (port == RL_87 && IsActuated)
            {
                ports.Add(RL_30);
            }
            else if (port == RL_87A && !IsActuated)
            {
                ports.Add(RL_30);
            }

            return ports;
        }
    }
}
