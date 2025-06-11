using System;

namespace VARLab.MPCircuits
{
    public interface IMultimeterDial
    {
        static Action<Division> OnDivisionChanged;
        Division CurrentDivision { get; set; }
    }
}
