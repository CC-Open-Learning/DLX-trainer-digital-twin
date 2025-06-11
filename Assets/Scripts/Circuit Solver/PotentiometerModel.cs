using SpiceSharp;
using SpiceSharp.Components;
using System.Collections.Generic;

namespace VARLab.MPCircuits.Model
{
    public class PotentiometerModel : CircuitComponentModel
    {
        private Port g, h, i;
        private float gi_resistance, hi_resistance;

        public float GH_Resistance { get { return gi_resistance - hi_resistance; } }
        public float GI_Resistance { get { return gi_resistance; } }
        public float HI_Resistance { get { return hi_resistance; } set { hi_resistance = value; } }

        public PotentiometerModel(Port g, Port h, Port i, float gi_resistance, float hi_resistance)
            : base("POT", g, h, i)
        {
            this.g = g;
            this.h = h;
            this.i = i;

            this.gi_resistance = gi_resistance;
            this.hi_resistance = hi_resistance;
        }


        public override void AddComponentsToCircuit(Circuit c)
        {
            if (g.Connected && (h.Connected || i.Connected))
            {
                c.Add(new Resistor("POT_GH", g.Name, h.Name, GH_Resistance));
            }

            if (i.Connected && (h.Connected || g.Connected))
            {
                c.Add(new Resistor("POT_HI", h.Name, i.Name, HI_Resistance));
            }
        }

        public override List<Port> GetOppositePorts(Port port)
        {
            if (port == h)
            {
                return new() { g, i };
            }
            else
            {
                return new() { h };
            }
        }
    }
}
