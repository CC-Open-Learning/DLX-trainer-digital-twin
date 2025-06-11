using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VARLab.MPCircuits
{
    public class MultimeterDial : MonoBehaviour, IMultimeterDial
    {
        [SerializeField] private float divisionCount = 7;
        [SerializeField] private float percentOfDial;
        [SerializeField] private GameObject voltmeterReadingText; // This should be the responsibility of multimeter.cs 

        public UnityEvent<MultimeterDial> OnDivisionChanged;

        private Vector3 initialRotation;

        public UnityEvent OnMouseHovered;

        private Division currentDivision;
        public Division CurrentDivision { get => currentDivision; set => currentDivision = value; }

        private readonly List<Division> divisions = new()
        {
            new(0, MultimeterDialUnit.Off),
            new(MultimeterDialSettings.ACVoltage, MultimeterDialUnit.Volts),
            new(MultimeterDialSettings.DCVoltage, MultimeterDialUnit.Volts),
            new(MultimeterDialSettings.ACVoltageMillivolts, MultimeterDialUnit.Volts),
            new(MultimeterDialSettings.Resistance, MultimeterDialUnit.Ohms),
            new(MultimeterDialSettings.Capacitance, MultimeterDialUnit.Ohms),
            new(MultimeterDialSettings.Current, MultimeterDialUnit.Amps),
        };

        private void OnEnable()
        {
            MultimeterSliderUI.OnSettingChanged += OnDialValueChanged;
        }
        private void OnDisable()
        {
            MultimeterSliderUI.OnSettingChanged -= OnDialValueChanged;
        }

        public void OnMouseHover()
        {
            if (FindObjectOfType<CableControls>().IsCableSelected || !Cursor.visible) return;

            OnMouseHovered?.Invoke();
        }

        public void OnDialValueChanged(int setting)
        {
            // We're going to use OFF as the default case 
            currentDivision = divisions[0];

            foreach (var division in divisions)
            {
                if (setting == (int)division.setting)
                {
                    currentDivision = division;
                }
            }

            // We're invoking the event to announce that the multimeter dial has been set to a new division setting
            OnDivisionChanged?.Invoke(this);

            RotateDial(setting);
        }

        /// <summary>
        ///     This method is in charge of moving the dial 
        /// </summary>
        /// <param name="setting"></param>
        private void RotateDial(int setting)
        {
            Vector3 newRotation = new Vector3(
                transform.localRotation.eulerAngles.x,
                initialRotation.y + setting * (360 * percentOfDial) / divisionCount,
                transform.localRotation.eulerAngles.z
            );

            transform.localRotation = Quaternion.Euler(newRotation);
        }

        // Start is called before the first frame update
        private void Start()
        {
            initialRotation = transform.localRotation.eulerAngles;
        }
    }

    [Serializable]
    public struct Division
    {
        public MultimeterDialSettings setting;
        public MultimeterDialUnit Unit;

        public Division(MultimeterDialSettings setting, MultimeterDialUnit unit)
        {
            this.setting = setting;     // Where we are on the dial
            Unit = unit;                // E.g. Volts
        }
    }

    // This enumerator could have been pulled from the data model, since there is a direct
    // mapping of dials to DMM internal configurations
    public enum MultimeterDialSettings
    {
        Off = 0,
        ACVoltage = 1,
        DCVoltage = 2,
        ACVoltageMillivolts = 3,
        Resistance = 4,
        Capacitance = 5,
        Current = 6,
    }

    // We're working with 3 units that we'll grab from this enum.
    // UPDATE I don't think the simulation actually ever cares what
    // unit we are in. It is certainly not using this enum for anything
    public enum MultimeterDialUnit
    {
        Off = 0,
        Volts = 1,
        Ohms = 2,
        Amps = 3,
        Other = 4
    }
}
