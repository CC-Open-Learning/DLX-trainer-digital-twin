using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{
    public class MultimeterComponent : MonoBehaviour
    {
        // variables to configure the resistance ohmmeter display
        public const double MaxDisplayedResistance = 9999;
        public const double ThousandsScale = 1000;
        public const double HundredsScale = 100;
        public const double TensScale = 10;

        public const int RoundingPrecision = 3;

        public const string TextFormatOverload = "O.L";
        public const string TextFormatInteger = "0000";
        public const string TextFormatTenths = "000.0";
        public const string TextFormatHundredths = "00.00";
        public const string TextFormatThousandths = "0.000";

        public readonly int AnimationSmokeKey = Animator.StringToHash("SmokeOn");

        /// <summary> Data model used to calculate multimeter values </summary>
        public MultimeterModel Model;

        [Tooltip("The animation controller for the multimeter")]
        public Animator Animator;

        [Tooltip("The GameObject to use for the multimeter dial")]
        public MultimeterDial multimeterDial;


        [Header("Display")]
        [Tooltip("The multimeter display text object")]
        public TextMeshPro multimeterText;


        [Header("Ports")]
        [Tooltip("The port in the simulation to use for the multimeter V (Voltage)")]
        public PortBehaviour Volts;

        [Tooltip("The port in the simulation to use for the multimeter A (Amperage) ")]
        public PortBehaviour Amps;

        [Tooltip("The port in the simulation to use for the multimeter Ground")]
        public PortBehaviour Ground;


        [Header("Events")]
        [Tooltip("Notifies when the Multimeter changes its measurement setting")]
        public UnityEvent OnSettingChanged;

        [Tooltip("Notifies when the fuse for the Ammeter is overloaded or replaced")]
        public UnityEvent OnFuseStateChanged;


        /// <summary>
        ///     Returns false only if there are no leads connected to any of the three multimeter ports
        /// </summary>
        public bool PortsConnected =>
            Volts.NumberLeadsConnected != 0 || Amps.NumberLeadsConnected != 0 || Ground.NumberLeadsConnected != 0;

        /// <summary>
        ///     Returns true when the multimeter is set to Current and leads are connected to Ground and Amp ports
        /// </summary>
        public bool MeasuringCurrent =>
            Ground.NumberLeadsConnected != 0 && Amps.NumberLeadsConnected != 0 && multimeterDial.CurrentDivision.setting == MultimeterDialSettings.Current;


        private void Start()
        {
            // Only one multimeter so we can reference the component directly.
            // In theory the model could be injected by DT Manager
            Model ??= FindFirstObjectByType<DigitalTwinManager>().CircuitBoard.DMM;

            if (Model != null)
            {
                Model.OnValuesUpdated += UpdateDisplay;
            }

            if (multimeterDial)
            {
                multimeterDial.OnDivisionChanged.AddListener(UpdateModelSetting);
            }

            // Screen should be blank when DMM is off
            ClearScreen();
        }

        private void OnDisable()
        {
            if (Model != null)
            {
                Model.OnValuesUpdated -= UpdateDisplay;
            }
        }

        /// <summary>
        ///     Updates the internal data model, plays or stops the smoke animation,
        ///     and announces that the state of the fuse has changed.
        /// </summary>
        /// <param name="overloaded">Indicates whether or not the fuse is "blown"</param>
        public void SetFuseOverloaded(bool overloaded)
        {
            // Update internal data model
            Model.IsAmmeterFuseBlown = overloaded;

            // Play smoke animation
            Animator.SetBool(AnimationSmokeKey, overloaded);

            // Invoke event notifying fuse state changed
            OnFuseStateChanged?.Invoke();
        }


        /// <summary>
        ///     Changes the enumerator in the data model based on the setting in the dial
        /// </summary>
        /// <param name="dial">Same as the already serialized field <see cref="multimeterDial"/></param>
        public void UpdateModelSetting(MultimeterDial dial)
        {
            switch (dial.CurrentDivision.setting)
            {
                case MultimeterDialSettings.Off:
                    Model.CurrentState = MultimeterModel.MultimeterState.Off;
                    break;
                case MultimeterDialSettings.DCVoltage:
                    Model.CurrentState = MultimeterModel.MultimeterState.DCVoltage;
                    break;
                case MultimeterDialSettings.Current:
                    Model.CurrentState = MultimeterModel.MultimeterState.DCCurrent;
                    break;
                case MultimeterDialSettings.Resistance:
                    Model.CurrentState = MultimeterModel.MultimeterState.Resistance;
                    break;
                default:
                    Model.CurrentState = MultimeterModel.MultimeterState.Undefined;
                    break;
            }

            OnSettingChanged?.Invoke();
        }

        /// <summary>
        ///     Updates the DMM display screen based on the <see cref="multimeterDial"/> setting
        /// </summary>
        /// <remarks>
        ///     Should be updating based on the data model setting instead, 
        ///     now that we have the "Undefined" enum state
        /// </remarks>
        /// <param name="model">
        ///     Unused parameter, but expected to be equal to <see cref="Model"/>
        /// </param>
        private void UpdateDisplay(CircuitComponentModel model)
        {
            multimeterText.text = TextFormatThousandths;

            switch (multimeterDial.CurrentDivision.setting)
            {
                case MultimeterDialSettings.Current:
                    DisplayCurrent();
                    break;

                case MultimeterDialSettings.DCVoltage:
                    DisplayVoltage();
                    break;

                case MultimeterDialSettings.Resistance:
                    DisplayResistance();
                    break;

                case MultimeterDialSettings.Off:
                    ClearScreen();
                    break;


                // Any other dial setting will read "0.000" but will
                // not respond to changes in the circuit
                default:
                    break;
            }
        }

        public void ClearScreen()
        {
            multimeterText.text = string.Empty;
        }

        /// <summary> Display resistance on the DMM screen. </summary>
        /// <remarks> pulbic method is for testing purpose. </remarks>
        public void DisplayResistance()
        {
            if (double.IsNaN(Volts.Voltage))
            {
                multimeterText.text = TextFormatOverload;
                return;
            }

            // The circuit in the ohmmeter is a 1V battery in series with a 1 ohm resistor
            // If we know the voltage at the end of the resistor that isn't connected to the battery,
            // we can calculate the external resistance using the following formula.

            // ^ the above comment may be referring to the calculation formula in MultimeterModel


            // We will use the absolute value to determine the scale, 
            // since the negative sign does not affect how many digits 
            // of precision should be displayed.
            // Also resistance will likely not be negative in our use cases
            double resAmplitude = Math.Abs(Model.OhmmeterResistance);

            if (resAmplitude > MaxDisplayedResistance)
            {
                multimeterText.text = TextFormatOverload;
            }
            else if (resAmplitude >= ThousandsScale)   // 1000 or greater
            {
                multimeterText.text = Model.OhmmeterResistance.ToString(TextFormatInteger);
            }
            else if (resAmplitude >= HundredsScale)   // 100 or greater
            {
                multimeterText.text = Model.OhmmeterResistance.ToString(TextFormatTenths);
            }
            else if (resAmplitude >= TensScale)  // 10 or greater
            {
                multimeterText.text = Model.OhmmeterResistance.ToString(TextFormatHundredths);
            }
            else // any single digit value or value between 0 and 1
            {
                multimeterText.text = Model.OhmmeterResistance.ToString(TextFormatThousandths);
            }
        }

        public void DisplayVoltage()
        {
            double voltage = Math.Round(Model.VoltmeterVoltage, RoundingPrecision);

            if (double.IsNaN(voltage)) { voltage = 0; }

            string format = Math.Abs(voltage) >= 10.0
                ? TextFormatHundredths
                : TextFormatThousandths;

            multimeterText.text = voltage.ToString(format);
        }

        public void DisplayCurrent()
        {
            double current = Model.AmmeterCurrent;

            if (double.IsNaN(current)) { current = 0; }

            // It is still bad practice that we are checking for "is the fuse overloaded"
            // and invoking the events and animations in a method for updating the display.
            // However, we need to ensure that the circuit solve algorithm runs a second time
            // after the fuse is overloaded, and invoking the OnFuseStateChanged event
            // in SetFuseOverloaded will cause the circuit to be recalculated by the DTManager
            if (Math.Abs(current) > MultimeterModel.AmmeterFuseThresholdAmps)
            {
                Debug.Log($"Multimeter fuse overloaded with {current} amps");
                SetFuseOverloaded(true);
                return;
            }

            multimeterText.text = current.ToString(TextFormatThousandths);
        }
    }
}

