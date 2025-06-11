using UnityEngine;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{
    public class LEDComponent : MonoBehaviour
    {
        public const float MinimumActuationCurrent = 0.0033f;   // Current (amps) minimum to see change in LED (confirm this on trainer board?)

        private DiodeModel model;

        [SerializeField] private CircuitBoard.ComponentNames componentName;

        [SerializeField] private Light pointLight;

        [Tooltip("Multiplied by current to determine light intensity. Used to adjust the lighting output on a linear scale")]
        public float LightingCoefficient = 24f;

        public float TargetIntensity { get; protected set; }

        private void Start()
        {
            model = FindFirstObjectByType<DigitalTwinManager>().CircuitBoard.GetComponent(componentName) as DiodeModel;
            model.OnValuesUpdated += CalculateLightBrightness;
            
        }

        private void OnDisable()
        {
            model.OnValuesUpdated -= CalculateLightBrightness;
        }


        /// <summary>
        ///     Update the brightness of the <see cref="pointLight"/> based on
        ///     the current provided by the <see cref="model"/>, as LED brightness is 
        ///     proportional to current.
        /// </summary>
        /// <param name="c">Parameter is not used, but is expected to be reference-equal to <see cref="model"/></param>
        private void CalculateLightBrightness(CircuitComponentModel component)
        {
            TargetIntensity = (float)(model.Current - MinimumActuationCurrent) * LightingCoefficient;
            pointLight.intensity = TargetIntensity;
        }
    }
}
