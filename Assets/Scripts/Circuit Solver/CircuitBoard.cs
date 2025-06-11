using SpiceSharp;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace VARLab.MPCircuits.Model
{
    public class CircuitBoard
    {
        private readonly CircuitSolver solver;

        public Action OnCircuitSolveEventHandler;   // Proposing changing this to OnVoltageUpdated

        // ============================ COMPONENTS ============================
        public MultimeterModel DMM;
        public BatteryModel Battery;

        public ResistorModel R1, R2, R3, R4, R5;
        public ResistorModel L3Low;
        public ResistorModel L3High;

        public LightbulbModel L1;
        public LightbulbModel L2;

        public SwitchModel SW1;
        public SwitchModel PB1;

        public FuseModel F1;

        public DiodeModel D1;
        public DiodeModel D2;
        public DiodeModel B1;

        public MotorModel M1;

        public PotentiometerModel POT;

        public ElectronicFlasherModel EF;

        public RelayModel RL1;


        // ============================ DICTIONARIES ============================
        public Dictionary<string, Port> Ports = new();

        public Dictionary<string, ResistorModel> CableDictionary = new();

        public Dictionary<ComponentNames, CircuitComponentModel> ComponentDictionary;

        public static Dictionary<PortNames, string> PortStringDictionary = new()
        {
            {PortNames.Battery_Gnd, "0"},
            {PortNames.Battery_V, "1"},
            {PortNames.L1_A, "A"},
            {PortNames.L1_B, "B"},
            {PortNames.L2_C, "C"},
            {PortNames.L2_D, "D"},
            {PortNames.L3_Lo, "Lo" },
            {PortNames.L3_Hi, "Hi" },
            {PortNames.L3_Com, "Com" },
            {PortNames.POT_G, "G"},
            {PortNames.POT_H, "H"},
            {PortNames.POT_I, "I"},
            {PortNames.F1_Left, "F1_0"},
            {PortNames.F1_Right, "F1_1"},
            {PortNames.SW1_Left, "SW1_0"},
            {PortNames.SW1_Right, "SW1_1"},
            {PortNames.DMM_Voltage, "Dmm_V"},
            {PortNames.DMM_Current, "Dmm_A"},
            {PortNames.DMM_Ground, "Dmm_Gnd"},
            {PortNames.RBatteryNegative, "Rbat-"},
            {PortNames.RBatteryPositive, "Rbat+"},
            {PortNames.R1_J, "J"},
            {PortNames.R1_K, "K"},
            {PortNames.R2_L, "L"},
            {PortNames.R2_M, "M"},
            {PortNames.R3_N, "N"},
            {PortNames.R3_O, "O"},
            {PortNames.R4_P, "P"},
            {PortNames.R4_Q, "Q"},
            {PortNames.R5_R, "R"},
            {PortNames.R5_S, "S"},
            {PortNames.D1_E, "D1_E"},
            {PortNames.D1_F, "D1_F"},
            {PortNames.D2_A, "D2_A"},
            {PortNames.D2_K, "D2_K"},
            {PortNames.B1_V, "B1_V"},
            {PortNames.B1_Gnd, "B1_Gnd"},
            {PortNames.PB1_Left, "PB1_0"},
            {PortNames.PB1_Right, "PB1_1"},
            {PortNames.DMM_OhmmeterVoltageSourcePositive, "DMM_O+"},
            {PortNames.DMM_OhmmeterResistorNegative, "Rdmm-"},
            {PortNames.M1_Pos, "M1_Pos"},
            {PortNames.M1_Neg, "M1_Neg"},
            {PortNames.RL1_85, "RL_85"},
            {PortNames.RL1_86, "RL_86"},
            {PortNames.RL1_87, "RL_87"},
            {PortNames.RL1_87A, "RL_87A"},
            {PortNames.RL1_30, "RL_30"},
            {PortNames.EF_Bat, "EF_Bat"},
            {PortNames.EF_Gnd, "EF_Gnd"},
            {PortNames.EF_Sig, "EF_Sig"}
        };

        // ============================ ENUMS ============================
        public enum PortNames
        {
            None,
            Battery_V,
            Battery_Gnd,
            L1_A,
            L1_B,
            L2_C,
            L2_D,
            POT_G,
            POT_H,
            POT_I,
            F1_Left,
            F1_Right,
            SW1_Left,
            SW1_Right,
            DMM_Voltage,
            DMM_Current,
            DMM_Ground,
            RBatteryNegative, //small resistor in series with the battery
            RBatteryPositive, //small resistor in series with the battery
            R1_J,
            R1_K,
            R2_L,
            R2_M,
            R3_N,
            R3_O,
            R4_P,
            R4_Q,
            R5_R,
            R5_S,
            L3_Lo,
            L3_Hi,
            L3_Com,
            D1_E,
            D1_F,
            D2_A,
            D2_K,
            B1_V,
            B1_Gnd,
            PB1_Left,
            PB1_Right,
            DMM_OhmmeterVoltageSourcePositive,
            DMM_OhmmeterResistorNegative,
            M1_Pos,
            M1_Neg,
            RL1_85,
            RL1_86,
            RL1_87,
            RL1_87A,
            RL1_30,
            EF_Bat,
            EF_Gnd,
            EF_Sig
        }

        public enum ComponentNames
        {
            None,
            Battery,
            R1,
            R2,
            R3,
            R4,
            R5,
            L1,
            L2,
            L3Lo,
            L3Hi,
            F1,
            D1,
            D2,
            B1,
            SW1,
            PB1,
            POT,
            DMM,
            M1,
            RL1,
            EF
        }

        // ============================ CONSTRUCTOR / SET UP METHODS ============================
        public CircuitBoard(float BoardVoltageDC = 10f)
        {
            solver = new CircuitSolver();
            solver.ExportDataEvent += StoreVoltages;

            SetupPortDict();
            SetupComponents(BoardVoltageDC);
        }

        /// <summary>
        ///     Uses <see cref="CircuitSolver"/> to attempt to solve the circuit
        /// </summary>
        /// <remarks>
        /// TODO
        ///     Catching an exception here and then re-throwing that same exception does not 
        ///     provide any benefit and is computationally wasteful.
        ///     We should instead change this method to return a boolean (or more complex object if necessary)
        ///     and then handle all exceptions within this method.
        ///     
        ///     The <see cref="DigitalTwinManager"/> then does not need to also wrap its calls in a try/catch block,
        ///     nor do our tests of the CircuitBoard class.
        ///     Tests of CircuitSolver.cs will still need to handle exceptions.
        /// </remarks>
        public void SolveCircuit()
        {

            solver.SolveCircuit(GetCircuit(), Battery.BoardVoltage, DMM.IsOhmmeterConnected, DMM.OhmmeterSourceVoltage);

        }

        private void SetupPortDict()
        {
            Ports = new Dictionary<string, Port>();

            foreach (string s in PortStringDictionary.Values)
            {
                Ports.Add(s, new Port(s));
            }
        }

        private void SetupComponents(float boardVoltageDC)
        {

            // L1 and L2 use default values for resistance and other properties as defined in the constructor
            L1 = new LightbulbModel("L1", GetPort(PortNames.L1_A), GetPort(PortNames.L1_B));
            L2 = new LightbulbModel("L2", GetPort(PortNames.L2_C), GetPort(PortNames.L2_D));

            // L3 uses 'ResistorModel' since the component on the board is not polarized, so a diode is not needed.
            // The resistance values here are arbitrary are useful for having L3 interact sanely with other components,
            // may not produce real-world values when measured across the ammeter or Ohmmeter.
            L3High = new ResistorModel("L3High", GetPort(PortNames.L3_Hi), GetPort(PortNames.L3_Com), (float)DiodeModel.InternalLEDResistance);
            L3Low = new ResistorModel("L3Low", GetPort(PortNames.L3_Lo), GetPort(PortNames.L3_Com), (float)DiodeModel.InternalLEDResistance * 2);

            R1 = new ResistorModel("R1", GetPort(PortNames.R1_J), GetPort(PortNames.R1_K), 100);
            R2 = new ResistorModel("R2", GetPort(PortNames.R2_L), GetPort(PortNames.R2_M), 40);
            R3 = new ResistorModel("R3", GetPort(PortNames.R3_N), GetPort(PortNames.R3_O), 20);
            R4 = new ResistorModel("R4", GetPort(PortNames.R4_P), GetPort(PortNames.R4_Q), 20);
            R5 = new ResistorModel("R5", GetPort(PortNames.R5_R), GetPort(PortNames.R5_S), 2);

            SW1 = new SwitchModel("SW1", GetPort(PortNames.SW1_Left), GetPort(PortNames.SW1_Right));
            PB1 = new SwitchModel("PB1", GetPort(PortNames.PB1_Left), GetPort(PortNames.PB1_Right));

            F1 = new FuseModel("F1", GetPort(PortNames.F1_Left), GetPort(PortNames.F1_Right), true);

            POT = new PotentiometerModel(GetPort(PortNames.POT_G), GetPort(PortNames.POT_H), GetPort(PortNames.POT_I), 75f, 30f);

            // D1 - Light-emitting diode
            D1 = new DiodeModel("D1", GetPort(PortNames.D1_E), GetPort(PortNames.D1_F), DiodeModel.InternalLEDResistance);

            D2 = new DiodeModel("D2", GetPort(PortNames.D2_A), GetPort(PortNames.D2_K));

            B1 = new DiodeModel("B1", GetPort(PortNames.B1_V), GetPort(PortNames.B1_Gnd), DiodeModel.InternalB1Resistance);

            M1 = new MotorModel("M1", GetPort(PortNames.M1_Pos), GetPort(PortNames.M1_Neg), Resistance.MotorResistance);

            EF = new ElectronicFlasherModel("EF", GetPort(PortNames.EF_Bat), GetPort(PortNames.EF_Gnd), GetPort(PortNames.EF_Sig), ElectronicFlasherModel.InternalBaseCapacitance, ElectronicFlasherModel.InternalOpenResistance);

            DMM = new MultimeterModel(GetPort(PortNames.DMM_Voltage), GetPort(PortNames.DMM_Ground), GetPort(PortNames.DMM_Current), GetPort(PortNames.RBatteryNegative), GetPort(PortNames.Battery_Gnd));
            Battery = new BatteryModel(GetPort(PortNames.Battery_V), GetPort(PortNames.Battery_Gnd), GetPort(PortNames.RBatteryNegative), boardVoltageDC);

            RL1 = new RelayModel("RL1", GetPort(PortNames.RL1_85), GetPort(PortNames.RL1_86), GetPort(PortNames.RL1_87), GetPort(PortNames.RL1_87A), GetPort(PortNames.RL1_30), RelayModel.CoilResistance);

            ComponentDictionary = new Dictionary<ComponentNames, CircuitComponentModel>
            {
                {ComponentNames.L1, L1},
                {ComponentNames.L2, L2},
                {ComponentNames.L3Hi, L3High},
                {ComponentNames.L3Lo, L3Low},
                {ComponentNames.R1, R1},
                {ComponentNames.R2, R2},
                {ComponentNames.R3, R3},
                {ComponentNames.R4, R4},
                {ComponentNames.R5, R5},
                {ComponentNames.POT, POT},
                {ComponentNames.F1, F1},
                {ComponentNames.PB1, PB1},
                {ComponentNames.SW1, SW1},
                {ComponentNames.Battery, Battery},
                {ComponentNames.DMM, DMM},
                {ComponentNames.D1, D1},
                {ComponentNames.D2, D2},
                {ComponentNames.B1, B1},
                {ComponentNames.M1, M1},
                {ComponentNames.EF, EF},
                {ComponentNames.RL1, RL1},
            };
        }

        /// <summary>
        /// For each port, set visited to false & active key to the original port name/key again
        /// </summary>
        public void Reset()
        {
            foreach (var p in Ports)
            {
                p.Value.Visited = false;
            }
        }

        public Port GetPort(PortNames name)
        {
            if (name == PortNames.None) { return null; }

            return Ports[PortStringDictionary[name]];
        }

        public CircuitComponentModel GetComponent(ComponentNames name)
        {
            if (name == ComponentNames.None) { return null; }

            return ComponentDictionary[name];
        }

        // ===================================== ADD / REMOVE CABLE ======================================
        public void PlaceCable(string cableID, PortNames p1, PortNames p2, bool isFaulty = false)
        {
            double resistance = isFaulty ? Resistance.FaultyCableResistance : Resistance.NegligibleResistance;

            CableDictionary[cableID] = new ResistorModel($"cable_{cableID}", GetPort(p1), GetPort(p2), resistance);
        }

        public void PlaceCable(PortNames p1, PortNames p2, bool isFaulty = false)
        {
            PlaceCable(Guid.NewGuid().ToString(), p1, p2, isFaulty);
        }

        public void RemoveCable(string cableID)
        {
            CableDictionary[cableID].DisconnectPorts();
            CableDictionary.Remove(cableID);
        }


        /// <summary>
        /// Reset the port info, squash the port names between components, then add the resistors/voltage source
        /// to the circuit if all ports of the component are connected to other ports
        /// </summary>
        /// <returns></returns>
        public Circuit GetCircuit()
        {
            ResetPorts();

            SetConnectedPortsToBattery();
            SetConnectedPortsToOhmmeter();

            Circuit circuit = new();

            foreach (CircuitComponentModel component in ComponentDictionary.Values)
            {
                component.AddComponentsToCircuit(circuit);
            }

            foreach (CircuitComponentModel cable in CableDictionary.Values)
            {
                cable.AddComponentsToCircuit(circuit);
            }

            return circuit;
        }

        /// <summary>
        ///     Clears voltage values in all ports and marks them as not 
        ///     connected to the circuit.
        /// </summary>
        public void ResetPorts()
        {
            foreach (Port port in Ports.Values)
            {
                port.Connected = false;
                port.Voltage = double.NaN;
            }
        }

        /// <summary>
        /// Traverse the circuit connecting Battery+ to Battery-
        /// to mark ports as "connected" so we add all the connected components to the circuit
        /// </summary>
        private void SetConnectedPortsToBattery()
        {
            foreach (var p in Ports)
            {
                p.Value.Visited = false;
            }

            _ = IsPortConnected(GetPort(PortNames.Battery_V), GetPort(PortNames.Battery_Gnd));
        }

        /// <summary>
        /// If the ohmmeter is active in the circuit, traverse the circuit connecting DMM-V to DMM-G
        /// to mark ports as "connected" so we add all the connected components to the circuit
        /// </summary>
        private void SetConnectedPortsToOhmmeter()
        {
            if (!DMM.IsOhmmeterConnected) { return; }

            foreach (var p in Ports)
            {
                p.Value.Visited = false;
            }

            _ = IsPortConnected(GetPort(PortNames.DMM_Voltage), GetPort(PortNames.DMM_Ground));
        }

        /// <summary>
        /// Recursive backtracking function whose job it is to mark ports as "connected" if they're part of a 
        /// path that connects the start port to the end port
        /// </summary>
        /// <param name="currentPort">The current port being checked in the function</param>
        /// <param name="endGroundPort">The port marked as the end of the circuit</param>
        /// <returns></returns>
        private bool IsPortConnected(Port currentPort, Port endGroundPort)
        {
            List<Port> uncheckedOpposedPorts = GetUnvisitedOppositePorts(currentPort);

            currentPort.Visited = true; //Avoid infinite recursion

            //--------------------------------------------- Exit Cases ---------------------------------------

            //If we're at the end port, we know the circuit is complete
            if (currentPort.Name == endGroundPort.Name)
            {
                currentPort.Visited = false;
                currentPort.Connected = true;
                return true;
            }

            //If there are no more unchecked ports connected to this port being checked and we haven't reached the end port as
            // determined above, then this port is floating and not part of the complete circuit (an unconnected branch)
            if (uncheckedOpposedPorts.Count == 0)
            {
                return false;
            }

            //If this port is already marked as connected, that means the port checking us must also be connected
            // (reduces number of function calls if already checked)
            if (currentPort.Connected)
            {
                return true;
            }

            //--------------------------------------------- Recursive Part ---------------------------------------
            //Loop through all neighbours. If any one of them is flagged as "connected", this port is also connected
            foreach (Port port in uncheckedOpposedPorts)
            {
                if (IsPortConnected(port, endGroundPort) == true)
                {
                    currentPort.Connected = true;
                }
            }

            //---------------------------------------------------------------------------------------------

            currentPort.Visited = false; //Set back to false so other paths can check this port

            return currentPort.Connected;
        }

        /// <summary>
        /// For each component/cable connected to the current port being checked, add the other/opposite port(s) of the component/cable 
        /// to a list if those ports haven't been checked yet so they can be checked
        /// </summary>
        private List<Port> GetUnvisitedOppositePorts(Port currentPort)
        {
            List<Port> unvisitedPorts = new();

            foreach (CircuitComponentModel component in currentPort.ConnectedComponents)
            {
                foreach (Port port in component.GetOppositePorts(currentPort))
                {
                    if (port.Visited == false)
                    {
                        unvisitedPorts.Add(port);
                    }
                }
            }

            return unvisitedPorts;
        }


        // ============================ GET & STORE VOLTAGE ============================
        public double GetVoltage(string port)
        {
            return Ports[port].Voltage;
        }


        /// <summary>
        /// Save voltage data to each port object from the last simulation (default is NaN)
        /// </summary>
        private void StoreVoltages(object sender, ExportDataEventArgs e)
        {
            foreach (string port in Ports.Keys)
            {
                try
                {
                    double readVoltage = e.GetVoltage(Ports[port].Name);
                    Ports[port].SetVoltage(readVoltage);
                }
                catch
                {
                    Ports[port].SetVoltage(double.NaN);
                }
            }

            OnCircuitSolveEventHandler?.Invoke();
        }
    }
}
