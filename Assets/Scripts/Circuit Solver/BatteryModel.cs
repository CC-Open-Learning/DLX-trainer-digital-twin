using SpiceSharp;
using SpiceSharp.Components;
using System.Collections.Generic;

namespace VARLab.MPCircuits.Model
{
    public class BatteryModel : CircuitComponentModel
    {
        // minimum resistance for the small resister in series with the battery
        // used to detect a short circuit
        private const float BatteryMinResistance = 0.00001f;
        // minimum amount of voltage drop across the small resister of the resister
        // in series with the battery that will cause a short circuit
        private const float ShortCircuitTolerance = 0.0001f;

        private protected Port v, g, betweenBatteryAndResistance;

        private protected bool vUpdated, gUpdated;

        public bool IsShorted { get; private set; } = false;

        public bool IsOverloaded { get; private set; } = false;

        public float BoardVoltage;

        public BatteryModel(Port v, Port g, Port betweenBatteryAndResistance, float boardVoltage)
            : base("Battery", v, g, betweenBatteryAndResistance)
        {
            this.v = v;
            this.g = g;
            this.betweenBatteryAndResistance = betweenBatteryAndResistance;

            v.VoltageChanged += OnVoltageChanged_V;
            g.VoltageChanged += OnVoltageChanged_G;
            BoardVoltage = boardVoltage;
        }

        public override void AddComponentsToCircuit(Circuit c)
        {
            c.Add(new Resistor($"{Name}_Res", v.Name, betweenBatteryAndResistance.Name, BatteryMinResistance));
            c.Add(new VoltageSource(Name, betweenBatteryAndResistance.Name, g.Name, BoardVoltage));
        }

        public override List<Port> GetOppositePorts(Port port) => new();

        public void OnAllPortsUpdated()
        {
            if ((v.Voltage - g.Voltage) < ShortCircuitTolerance)
            {
                IsShorted = true;
            }
            if (IsShorted)
            {
                IsOverloaded = true;  // clamps the overload condition to on
                BoardVoltage = 0f;
            }
            OnValuesUpdated?.Invoke(this);
        }

        private void OnVoltageChanged_V(double voltage)
        {
            vUpdated = true;
            if (vUpdated && gUpdated)
            {
                OnAllPortsUpdated();
                vUpdated = false;
                gUpdated = false;
            }
        }

        private void OnVoltageChanged_G(double voltage)
        {
            gUpdated = true;
            if (vUpdated && gUpdated)
            {
                OnAllPortsUpdated();
                vUpdated = false;
                gUpdated = false;
            }
        }

        public void ResetBatteryOverload(float newVoltage)
        {
            BoardVoltage = newVoltage;
            v.Voltage = BoardVoltage;
            g.Voltage = 0;
            IsShorted = false;
            IsOverloaded = false;
            OnAllPortsUpdated();
        }
    }
}
