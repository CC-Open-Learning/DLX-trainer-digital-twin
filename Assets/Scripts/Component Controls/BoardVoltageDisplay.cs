using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace VARLab.MPCircuits
{
    public class BoardVoltageDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshPro boardVoltageValueText;

        public TextMeshPro BoardVoltageValueText { get => boardVoltageValueText; set => boardVoltageValueText = value; }

        public void OnVoltageKnobValueChanged(float value)
        {
            boardVoltageValueText.text = FormatVoltageValue(value);
        }

        string FormatVoltageValue(float boardVoltageValue)
        {
            string voltageValueString = String.Format(CultureInfo.CreateSpecificCulture("en-CA"), "{0:00.00}", boardVoltageValue).ToString();
            return voltageValueString;
        }
    }
}
