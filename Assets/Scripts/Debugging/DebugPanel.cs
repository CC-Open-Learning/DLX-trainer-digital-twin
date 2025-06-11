using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VARLab.MPCircuits.Model;

//********************************************************************************
//  DON'T USE CODE CLEANUP ON THIS SCRIPT  (TO KEEP PORT MAPPING SECTION TABBING)
//********************************************************************************

namespace VARLab.MPCircuits
{
    /// <summary>
    /// Displays a panel that shows the voltage, current, resistance, and state values of components/port objects
    /// across the circuit board (only in unity editor or development builds)
    /// </summary>
    public class DebugPanel : MonoBehaviour
    {
        [SerializeField] public GameObject DebugCanvasBackground;
        [SerializeField] public TextMeshProUGUI DebugText;

        [SerializeField] public Toggle OnOffToggle;
        [SerializeField] public Camera Camera;

        [SerializeField] public DigitalTwinManager DigitalTwinManager;
        [SerializeField] public PortBehaviour  L1_A, L1_B, L2_C, L2_D,
                                                POT_G, POT_H, POT_I,
                                                F1_Left, F1_Right,
                                                SW1_Left, SW1_Right,
                                                DMM_Voltage, DMM_Current, DMM_Ground,
                                                R1_J, R1_K, R2_L, R2_M, R3_N, R3_O, R4_P, R4_Q, R5_R, R5_S,
                                                L3_Lo, L3_Hi, L3_Com,
                                                D1_E, D1_F, D2_A, D2_K,
                                                PB1_Left, PB1_Right,
                                                B1_V, B1_Gnd,
                                                M1_Pos, M1_Neg,
                                                EF_Bat, EF_Gnd, EF_Sig,
                                                RL1_85, RL1_86, RL1_87, RL1_87A, RL1_30;

        private const string ReplaceVoltNaN = "<color=#ED2939>-</color>"; //red colour
        private const string Dash = "-"; //placeholder for ports not established yet

        private bool IsOpen;
        private LightbulbComponent lightBulb;


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Awake()
        {
            OnOffToggle.gameObject.SetActive(true);
            IsOpen = DebugCanvasBackground.activeInHierarchy;
            lightBulb = FindObjectOfType<LightbulbComponent>();
        }

        private void OnEnable()
        {
            if (DigitalTwinManager)
            {
                DigitalTwinManager.OnCircuitBoardSolved.AddListener(DisplayComponentValues);
            }
        }

        private void OnDisable()
        {
            if (DigitalTwinManager)
            {
                DigitalTwinManager.OnCircuitBoardSolved.RemoveListener(DisplayComponentValues);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.D)) //D for "debug"
            {
                IsOpen = !IsOpen;
                ShowDebugPanel(IsOpen);
            }
        }

        /// <summary>
        /// The Debug Panel Toggle in the Settings Panel has an event that will pass either
        /// true or false to determine if the debug panel should be visible or not
        /// </summary>
        /// <param name="isVisible"> true = panel visible, false = panel NOT visible </param>
        public void ShowDebugPanel(bool isVisible)
        {
            DebugCanvasBackground.SetActive(isVisible);
        }

        private void DisplayComponentValues()
        {
            //Debug.Log("--------------------- DisplayComponentValues() called");

            StringBuilder sb = new StringBuilder();

            string toggleSwitchState = DigitalTwinManager.ToggleSwitch.IsOn ? "ON" : "OFF";
            string pushButtonSwitchState = DigitalTwinManager.PushButtonSwitch.IsOn ? "ON" : "OFF";

            double l1Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(L1_A, L1_B);
            double l2Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(L2_C, L2_D);
            double f1Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(F1_Left, F1_Right);
            double r1Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(R1_J, R1_K);
            double r2Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(R2_L, R2_M);
            double r3Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(R3_N, R3_O);
            double r4Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(R4_P, R4_Q);
            double r5Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(R5_R, R5_S);
            double sw1Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(SW1_Left, SW1_Right);
            double pb1Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(PB1_Left, PB1_Right);
            double l3loVoltage = GetVoltageOfComponentBetweenTwoNeutralPorts(L3_Lo, L3_Com);
            double l3hiVoltage = GetVoltageOfComponentBetweenTwoNeutralPorts(L3_Hi, L3_Com);
            double potGHVoltage = GetVoltageOfComponentBetweenTwoNeutralPorts(POT_G, POT_H);
            double potHIVoltage = GetVoltageOfComponentBetweenTwoNeutralPorts(POT_H, POT_I);
            double potGIVoltage = GetVoltageOfComponentBetweenTwoNeutralPorts(POT_G, POT_I);
            double rl185_86Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(RL1_85, RL1_86);
            double rl187A_30Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(RL1_87A, RL1_30);
            double rl187_30Voltage = GetVoltageOfComponentBetweenTwoNeutralPorts(RL1_87, RL1_30);
            double dmmA_Voltage = GetVoltageOfComponentBetweenAnodeAndCathodePort(DMM_Current, DMM_Ground);
            double dmmV_Voltage = GetVoltageOfComponentBetweenAnodeAndCathodePort(DMM_Voltage, DMM_Ground);
            double d1Voltage = GetVoltageOfComponentBetweenAnodeAndCathodePort(D1_E, D1_F);
            double d2Voltage = GetVoltageOfComponentBetweenAnodeAndCathodePort(D2_A, D2_K);
            double m1Voltage = GetVoltageOfComponentBetweenAnodeAndCathodePort(M1_Pos, M1_Neg);
            double b1Voltage = GetVoltageOfComponentBetweenAnodeAndCathodePort(B1_V, B1_Gnd);
            double efVGndVoltage = GetVoltageOfComponentBetweenAnodeAndCathodePort(EF_Bat, EF_Gnd);

            float l1Resistance = (float)DigitalTwinManager.CircuitBoard.L1.Resistance;
            float l2Resistance = (float)DigitalTwinManager.CircuitBoard.L2.Resistance;
            float l3LoResistance = (float)DigitalTwinManager.CircuitBoard.L3Low.Resistance;
            float l3HiResistance = (float)DigitalTwinManager.CircuitBoard.L3High.Resistance;
            float f1Resistance = (float)DigitalTwinManager.CircuitBoard.F1.Resistance;
            float dmmAmpResistance = (float)DigitalTwinManager.CircuitBoard.DMM.Resistance;
            float diodeResistance = 0.5f;
            float LEDResistance = (float)DiodeModel.InternalLEDResistance;
            float B1Resistance = (float)DiodeModel.InternalB1Resistance;
            float motorResistance = (float)Resistance.MotorResistance;
            float efResistance = (float)ElectronicFlasherModel.InternalOpenResistance;

            sb.Append("<color=#FFFFCC>PORT VOLTAGES: ****************************************************</color>\n\n" + //beige yellowish colour

            GetPortVoltage(L1_A) + "\t" + GetPortVoltage(L2_C) + "\t" + GetPortVoltage(D1_E) + "\t" + GetPortVoltage(B1_V) + "\t" +                         "\t" + GetPortVoltage(Dash) + "\t" +                        "\t" + GetPortVoltage(Dash) +   "\t" + GetPortVoltage(D2_A) + "\n" +
            Label("L1") +          "\t" + Label("L2") +          "\t" + Label("D1") +          "\t" + Label("B1") +          "\t" + GetPortVoltage(Dash) +  "\t" + "T1" +                 "\t" + GetPortVoltage(Dash) + "\t" + "T2" +                   "\t" + Label("D2") +          "\n" +
            GetPortVoltage(L1_B) + "\t" + GetPortVoltage(L2_D) + "\t" + GetPortVoltage(D1_F) + "\t" + GetPortVoltage(B1_Gnd) +"\t" +                        "\t" + GetPortVoltage(Dash) + "\t" +                        "\t" + GetPortVoltage(Dash) +   "\t" + GetPortVoltage(D2_K) + "\n\n" +


            GetPortVoltage(L3_Lo) +  "\t" + GetPortVoltage(L3_Hi) + "\t" + "\t" +   GetPortVoltage(M1_Pos) + "\t" + GetPortVoltage(RL1_87) + "\t" + GetPortVoltage(RL1_30) +  "\t" + GetPortVoltage(RL1_87A) + "\t" + GetPortVoltage(EF_Bat) + "\t" + GetPortVoltage(EF_Sig) + "\n" +
            Label("L3") +            "\t" +                         "\t" + "\t" +   Label("M1")            + "\t" + GetPortVoltage(RL1_85) + "\t" + Label("RL1") +            "\t" + GetPortVoltage(RL1_86)  + "\t" + Label("EF") +                                         "\n" +
            GetPortVoltage(L3_Com) + "\t" + "\t" +                  "\t" +          GetPortVoltage(M1_Neg) + "\t" + "\t"+                    "\t" +                           "\t" +                                  GetPortVoltage(EF_Gnd) +                                 "\n\n" +


            GetPortVoltage(POT_G) + "\t" + "\t" + "\t" +            GetPortVoltage(R1_J) + "\t" + GetPortVoltage(R2_L) + "\t" + GetPortVoltage(R3_N) + "\t" + GetPortVoltage(R4_P) + "\t" + GetPortVoltage(R5_R) + "\n" +
            Label("POT") +  GetPortVoltage(POT_H) + "\t" + "\t" + "\t" + Label("R1") +          "\t" + Label("R2") +          "\t" + Label("R3") +          "\t" + Label("R4") +          "\t" + Label("R5") +          "\n" +
            GetPortVoltage(POT_I) + "\t" + "\t" + "\t" +            GetPortVoltage(R1_K) + "\t" + GetPortVoltage(R2_M) + "\t" + GetPortVoltage(R3_O) + "\t" + GetPortVoltage(R4_Q) + "\t" + GetPortVoltage(R5_S) + "\n\n" +


            GetPortVoltage(F1_Left) + Label("  F1  ") + GetPortVoltage(F1_Right) + "\t\t" + GetPortVoltage(SW1_Left) + Label("  SW1  ") + GetPortVoltage(SW1_Right) + "\t\t" + GetPortVoltage(PB1_Left) + Label("  PB1  ") + GetPortVoltage(PB1_Right) + "\n\n" +


            "\t\t\t\t\t\t\t\t" + Label("A") +                  "\t" + Label("COM") +               "\t" + Label("V") +                  "\n" +
            "\t\t\t\t\t\t\t\t" + GetPortVoltage(DMM_Current) + "\t" + GetPortVoltage(DMM_Ground) + "\t" + GetPortVoltage(DMM_Voltage) + "\n\n" +

            $"<color=#FFFFCC>***************************************************************************</color> \n\n" +

            $"<color=#FFFFCC>COMPONENT - VOLTAGE (V), RESISTANCE (R), CURRENT (I)</color>\n\n" +

            // \u2126 = omega/ohm symbol

            $"{Label("L1 -")} V: {FormatValue(l1Voltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {l1Resistance}\u2126,   " +
                            $"I: {CalculateCurrent(l1Voltage, l1Resistance)}A \n\n" +

            $"{Label("L2 -")} V: {FormatValue(l2Voltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {l2Resistance}\u2126,   " +
                            $"I: {CalculateCurrent(l2Voltage, l2Resistance)}A \n\n" +

            $"{Label("D1 -")} V: {FormatValue(d1Voltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {LEDResistance}\u2126,   " +
                            $"I: {CalculateCurrent(d1Voltage, LEDResistance)}A \n\n" +

            $"{Label("B1 -")} V: {FormatValue(b1Voltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {B1Resistance}\u2126,   " +
                            $"I: {CalculateCurrent(b1Voltage, B1Resistance)}A \n\n" +

            $"{Label("D2 -")} V: {FormatValue(d2Voltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {diodeResistance}\u2126,   " +
                            $"I: {CalculateCurrent(d2Voltage, diodeResistance)}A \n\n" +

            $"{Label("M1 -")} V: {FormatValue(m1Voltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {motorResistance}\u2126,   " +
                            $"I: {CalculateCurrent(m1Voltage, motorResistance)}A \n\n" +

            $"{Label("EF -")} V: {FormatValue(efVGndVoltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {efResistance}\u2126,   " +
                            $"I: {CalculateCurrent(efVGndVoltage, efResistance)}A \n\n" +

            $"{Label("F1 -")} V: {FormatValue(f1Voltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {f1Resistance}\u2126,   " +
                            $"I: {CalculateCurrent(f1Voltage, f1Resistance)}A \n\n" +

            $"{Label("R1 -")} V: {FormatValue(r1Voltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {DigitalTwinManager.CircuitBoard.R1.Resistance}\u2126,   " +
                            $"I: {CalculateCurrent(r1Voltage, DigitalTwinManager.CircuitBoard.R1.Resistance)}A \n\n" +

            $"{Label("R2 -")} V: {FormatValue(r2Voltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {DigitalTwinManager.CircuitBoard.R2.Resistance}\u2126,   " +
                            $"I: {CalculateCurrent(r2Voltage, DigitalTwinManager.CircuitBoard.R2.Resistance)}A \n\n" +

            $"{Label("R3 -")} V: {FormatValue(r3Voltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {DigitalTwinManager.CircuitBoard.R3.Resistance}\u2126,   " +
                            $"I: {CalculateCurrent(r3Voltage, DigitalTwinManager.CircuitBoard.R3.Resistance)}A \n\n" +

            $"{Label("R4 -")} V: {FormatValue(r4Voltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {DigitalTwinManager.CircuitBoard.R4.Resistance}\u2126,   " +
                            $"I: {CalculateCurrent(r4Voltage, DigitalTwinManager.CircuitBoard.R4.Resistance)}A \n\n" +

            $"{Label("R5 -")} V: {FormatValue(r5Voltage, 2, ReplaceVoltNaN)}V,   " +
                            $"R: {DigitalTwinManager.CircuitBoard.R5.Resistance}\u2126,   " +
                            $"I: {CalculateCurrent(r5Voltage, DigitalTwinManager.CircuitBoard.R5.Resistance)}A \n\n" +

            $"{Label("SW1 -")} ({toggleSwitchState}) - V: {FormatValue(sw1Voltage, 2, ReplaceVoltNaN)}V\n\n" +

            $"{Label("PB1 -")} ({pushButtonSwitchState}) - V: {FormatValue(pb1Voltage, 2, ReplaceVoltNaN)}V\n\n" +

            $"{Label("L3 (Dual Circuit) -")} \n" +
                $"\tLo - R: {l3LoResistance}\u2126,   " +
                       $"I: {CalculateCurrent(l3loVoltage, l3LoResistance)}A \n" +

                $"\tHi - R: {l3HiResistance}\u2126,   " +
                       $"I: {CalculateCurrent(l3hiVoltage, l3HiResistance)}A \n\n" +

            $"{Label("POT -")} \n" +
                $"\tGH - R: {Math.Round(DigitalTwinManager.CircuitBoard.POT.GH_Resistance, 2)}\u2126,   " +
                       $"I: {CalculateCurrent(potGHVoltage, DigitalTwinManager.CircuitBoard.POT.GH_Resistance)}A \n" +

                $"\tHI - R: {Math.Round(DigitalTwinManager.CircuitBoard.POT.HI_Resistance, 2)}\u2126,   " +
                       $"I: {CalculateCurrent(potHIVoltage, DigitalTwinManager.CircuitBoard.POT.HI_Resistance)}A \n" +

                $"\tGI - R: {DigitalTwinManager.CircuitBoard.POT.GI_Resistance}\u2126,   " +
                       $"I: {CalculateCurrent(potGIVoltage, DigitalTwinManager.CircuitBoard.POT.GI_Resistance)}A \n\n" +

            $"{Label("RL1 -")} \n" +
                $"\t85-86 - R: {RelayModel.CoilResistance}\u2126,   " +
                       $"I: {CalculateCurrent(rl185_86Voltage, RelayModel.CoilResistance)}A \n" +

                $"\t V: {FormatValue(DigitalTwinManager.CircuitBoard.RL1.Voltage, 2, ReplaceVoltNaN)}V \n" +

                $"\t Actuated?: {DigitalTwinManager.CircuitBoard.RL1.IsActuated} \n\n" +

            $"{Label("Multimeter -")} \n" +
                 $"\tA - R: {dmmAmpResistance}\u2126,   " +
                       $"I: {CalculateCurrent(dmmA_Voltage, dmmAmpResistance)}A \n" +

                 $"\tV - R: {CircuitComponentModel.DisconnectedResistance}\u2126,   " +
                       $"I: {CalculateCurrent(dmmV_Voltage, CircuitComponentModel.DisconnectedResistance)}A \n\n");

            DebugText.text = sb.ToString();
        }



        private double GetVoltageOfComponentBetweenTwoNeutralPorts(PortBehaviour port1, PortBehaviour port2)
        {
            // Highest voltage of 2 nodes   -   lowest voltage of 2 nodes 
            return Math.Abs(port1.Voltage - port2.Voltage); //get absolute value (neg. or pos. doesn't matter)
        }

        private double GetVoltageOfComponentBetweenAnodeAndCathodePort(PortBehaviour portAnode, PortBehaviour portCathode)
        {
            // The direction of the current matters, must go from Anode to Cathode.
            return portAnode.Voltage - portCathode.Voltage;
        }

        private string CalculateCurrent(double voltage, double resistance)
        {
            return CalculateCurrent(voltage, (float)resistance);
        }

        private string CalculateCurrent(double voltage, float resistance)
        {
            double current = voltage / (double)resistance;

            return FormatValue(current, 4, "0");
        }

        private string FormatValue(double value, int decimalPlaces, string stringToReplaceNaN)
        {
            if (Double.IsNaN(value))
            {
                return stringToReplaceNaN;
            }

            return Math.Round(value, decimalPlaces).ToString();
        }

        private string Label(string text)
        {
            return $"<color=#FBEC50>{text}</color>"; //corn yellow colour
        }

        public string GetPortVoltage(PortBehaviour port)
        {
            return FormatValue(port.Voltage, 1, ReplaceVoltNaN);
        }

        //this is a temporary method overload for ports that have not been established yet
        private string GetPortVoltage(string placeholder)
        {
            return placeholder;
        }

#endif
    }
}