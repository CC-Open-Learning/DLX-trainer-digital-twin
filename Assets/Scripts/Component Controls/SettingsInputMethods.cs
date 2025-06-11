using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VARLab.MPCircuits
{
    public class SettingsInputMethods : MonoBehaviour
    {
        public List<InputMethod> InputMethods;

        [Serializable]
        public struct InputMethod
        {
            public string InputMethodName; //just to make visibility better in the editor
            public InputMethodTypes InputMethodType;

            public InputMethod(string name, InputMethodTypes type)
            {
                InputMethodName = name;
                InputMethodType = type;
            }
        }

        public enum InputMethodTypes
        {
            Slider,
            IncrementAndDecrement,
        }
    }
}
