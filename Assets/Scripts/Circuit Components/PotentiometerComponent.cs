using UnityEngine;
using UnityEngine.Events;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{
    public class PotentiometerComponent : MonoBehaviour
    {
        public float Resistance = 30f;      // Arbitrary starting value

        public RadialSlider PotentiometerSlider;

        public UnityEvent OnPOTResistanceChanged = new();

        private PotentiometerModel model;

        private void Start()
        {
            // Only one Potentiometer in the data model so we can reference it directly
            // In theory this could be injected from the DigitalTwinManager
            model ??= FindFirstObjectByType<DigitalTwinManager>().CircuitBoard.POT;

            if (model != null)
            {
                model.HI_Resistance = Resistance;
            }

            if (PotentiometerSlider)
            {
                PotentiometerSlider.SetSliderValue(Resistance);
            }
        }


        /// <summary> Change resistance through potentiometer. </summary>
        /// <param name="resistance"> Input resistance set through ports H and I on POT. </param>
        public void ChangePOTResistance(float resistance)
        {
            if (model == null) { return; }

            model.HI_Resistance = resistance;
            Resistance = resistance;
            OnPOTResistanceChanged?.Invoke();
        }
    }
}
