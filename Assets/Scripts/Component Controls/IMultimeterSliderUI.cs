using System;
using UnityEngine;

namespace VARLab.MPCircuits
{
    public class IMultimeterSliderUI : MonoBehaviour
    {
        private static Action<int> OnSettingChanged;

        private MultimeterDialSettings CurrentSetting { get; set; }
    }
}
