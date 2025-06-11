using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{
    public class LightbulbComponent : MonoBehaviour
    {
        // Marked as static readonly since they are calculated values but need only be set once for all bulbs
        public static readonly int AnimationRemoveKey = Animator.StringToHash("Remove");
        public static readonly int AnimationPlaceKey = Animator.StringToHash("Place");

        private const float EmissionIntensityMax = 7;
        private const float MaxBoardVoltage = 14f;
        /// <summary> Minimum voltage required to see emission from filament </summary>
        private const float EmissionVoltageThreshold = 1f;
        private const string MaterialEmissionKey = "_EmissionColor";


        [Header("Component Settings")]
        [FormerlySerializedAs("_componentName")]
        public CircuitBoard.ComponentNames ComponentName;

        [Tooltip("Is this component defective at the start of the simulation")]
        [FormerlySerializedAs("_isDefective")]
        public bool IsDefective = false;  // is the component good or defective

        /// <summary> Data model representing the lightbulb as a resistor </summary>
        public LightbulbModel Model;

        public Light PointLight;
        public GameObject LightBulbGO;  // TODO this could be replaced with the Renderer explicitly
        public Animator Animator;
        [SerializeField] private Collider interactionCollider;

        [Header("Illumination Settings")]
        [FormerlySerializedAs("_lightIntensityMultiplier")]
        public float LightIntensityMultiplier = 0.3f;
        public Color EmissionColor;
        public Color EmissionColorFilament;
        public AnimationCurve EmissionCurve;
        public AnimationCurve EmissionCurveFilament;
        public Material[] Materials;


        [Header("Events")]
        public UnityEvent OnLightbulbPlaced;
        public UnityEvent OnLightbulbRemoved;


        private bool _isLightbulbRemoved;

        public void Start()
        {
            Model ??= FindFirstObjectByType<DigitalTwinManager>().CircuitBoard.GetComponent(ComponentName) as LightbulbModel;

            Model.OnValuesUpdated += UpdateBrightnessOnVoltage;

            Materials = LightBulbGO.GetComponent<Renderer>().materials;

            _ = UpdateLightBrightness(0);

            // trigger initial Invoke to ensure the proper resistance is present for the bulb
            NotifyBulbChanged();
        }

        private void OnDisable()
        {
            Model.OnValuesUpdated -= UpdateBrightnessOnVoltage;
        }


        /// <summary>
        /// Checks if the bulb is removed, circuit is incomplete, bulb is defective or is working
        /// and calls the appropriate method to update the brightness
        /// </summary>
        /// <param name="c"></param>
        private void UpdateBrightnessOnVoltage(CircuitComponentModel c)
        {
            if (double.IsNaN(Model.Voltage) || _isLightbulbRemoved || IsDefective) //when circuit is not complete, port voltage is not a number (NaN) -> set brightness to none (0%)
            {
                _ = UpdateLightBrightness(0);
                if (Materials.Length > 1)
                {
                    UpdateFilamentBrightness(0);
                }
            }
            else
            {
                _ = UpdateLightBrightness((float)Model.Voltage / MaxBoardVoltage);
                if (Materials.Length > 1)
                {
                    UpdateFilamentBrightness((float)(Model.Voltage - EmissionVoltageThreshold) / MaxBoardVoltage);
                }
            }
        }

        /// <summary>
        /// Updates the brightness of the Light based on a given percentage
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns>The intensity of the Light after being set</returns>
        public float UpdateLightBrightness(float percentage)
        {
            float emissionValue = EmissionCurve.Evaluate(percentage);

            PointLight.intensity = LightIntensityMultiplier * emissionValue;

            float desiredIntensity = EmissionIntensityMax * emissionValue;
            Materials[0].SetColor(MaterialEmissionKey, EmissionColor * Mathf.Pow(2, desiredIntensity));

            return PointLight.intensity;
        }

        /// <summary>
        /// Updates the brightness of the Lightfilaments by a given percentage
        /// </summary>
        /// <param name="percentage"></param>
        public void UpdateFilamentBrightness(float percentage)
        {
            float emissionValue = EmissionCurveFilament.Evaluate(percentage);

            float desiredIntensity = EmissionIntensityMax * emissionValue;
            Materials[1].SetColor(MaterialEmissionKey, EmissionColorFilament * Mathf.Pow(2, desiredIntensity));
        }


        /// <summary>
        ///     Handles removing or replacing the bulb on the board.
        /// </summary>
        public void ToggleBulbPlaced()
        {
            // Invert current removed state
            _isLightbulbRemoved = !_isLightbulbRemoved;

            AnimateBulb();
            NotifyBulbChanged();

            // sets bulb to not defective on replacing
            IsDefective = false;

            StartCoroutine(DisableUntilAnimationIsComplete(interactionCollider, Animator.GetCurrentAnimatorStateInfo(0).length));
        }

        /// <summary>
        ///     Sets the appropriate animation trigger based on the current "removed"
        ///     state of the bulb, causing the animation for placing or removing the
        ///     bulb to be played.
        /// </summary>
        public void AnimateBulb()
        {
            Animator.SetTrigger(_isLightbulbRemoved ? AnimationRemoveKey : AnimationPlaceKey);
        }

        /// <summary>
        ///     Invoke the appropriate event based on the current "removed" state 
        ///     of the bulb.
        /// </summary>
        public void NotifyBulbChanged()
        {
            (_isLightbulbRemoved ? OnLightbulbRemoved : OnLightbulbPlaced)?.Invoke();
        }

        private IEnumerator DisableUntilAnimationIsComplete(Collider collider, float duration)
        {
            if (collider) { collider.enabled = false; }
            yield return new WaitForSeconds(duration);
            if (collider) { collider.enabled = true; }
        }
    }
}
