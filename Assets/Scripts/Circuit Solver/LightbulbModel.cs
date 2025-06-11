using SpiceSharp;
using SpiceSharp.Components;
using System;

namespace VARLab.MPCircuits.Model
{

    /// <summary>
    ///     Data model responsible for representing a lightbulb in the circuit
    /// </summary>
    /// <remarks>
    ///     Extends the <see cref="ResistorModel"/> to add the consideration of the
    ///     bulb being either connected or disconnected
    /// </remarks>
    public class LightbulbModel : ResistorModel
    {

        // Reference values are based on a 12V, 3.78W incandescent bulb
        public const float ReferenceResistance = 38.1f;
        public const float ReferenceVoltage = 12f;
        public const float ReferenceCoefficient = 0.72f;


        protected float _baseResistance;
        protected float _referenceVoltage;
        protected float _coefficient;

        public LightbulbModel(string name, Port a, Port b, 
            float resistance = ReferenceResistance, 
            float referenceVoltage = ReferenceVoltage, 
            float coefficient = ReferenceCoefficient)
            : base(name, a, b, resistance) 
        {
            _baseResistance = resistance;
            _referenceVoltage = referenceVoltage;
            _coefficient = coefficient;
        }

        /// <summary>
        /// If both of our ports are connected, then add the appropriate spicesharp model to the circuit
        /// </summary>
        /// <param name="c">Circuit object to add the component to</param>
        public override void AddComponentsToCircuit(Circuit c)
        {
            if (a.Connected && b.Connected)
            {
                c.Add(new Resistor(Name, a.Name, b.Name, Resistance));
                UnityEngine.Debug.Log($"{Name}");
            }
        }

        protected override void OnAllPortsUpdated()
        {
            if (!double.IsNaN(a.Voltage) && !double.IsNaN(b.Voltage))
            {
                Voltage = Math.Abs(a.Voltage - b.Voltage);
                Current = Voltage / Resistance;

                Resistance = CalculateResistance((float)Voltage);
            }
            else
            {
                Voltage = double.NaN;
                Current = double.NaN;
                Resistance = _baseResistance;
            }

            OnValuesUpdated?.Invoke(this);
        }

        /// <summary>
        ///     Linear variable resistance calculation
        /// </summary>
        /// <param name="voltage">Input voltage</param>
        /// <returns>
        ///     Resistance of the component based on voltage and the base resistance value provided to the model
        /// </returns>
        public float CalculateResistance(float voltage)
        {
            return _baseResistance + _coefficient * (voltage - _referenceVoltage);
        }
    }
}
