using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{

    /// <summary>
    ///     Reads from two <see cref="LightbulbModel"/> component models in the circuit 
    ///     simulator in order to control the brightness of a single light in the scene, 
    ///     to represent the behaviour of a dual-circuit LED.
    /// </summary>
    /// <remarks>
    ///     It is likely that the <see cref="CircuitComponent"/> base class is now redundant
    ///     since we are not reading from the ports directly, and are instead reading 
    ///     from the component model. 
    ///     
    ///     On another note, the behaviour to remove this component is also identical in
    ///     the <see cref="LightbulbComponent"/> component, which controls the incandescent lights 
    ///     L1 and L2. This behaviour could be consolidated into a specialized component 
    ///     such as <see cref="Removable"/> (which currently exists as a stub)
    /// </remarks>
    public class DualCircuitLEDComponent : MonoBehaviour
    {
        public static readonly int AnimationRemoveKey = Animator.StringToHash("Remove");
        public static readonly int AnimationPlaceKey = Animator.StringToHash("Place");


        [Tooltip("The Animator which controls the bulb remove & replace animation")]
        public Animator Animator;

        [Tooltip("Collider which handles click actions on the component")]
        public Collider InteractionCollider;

        [Header("Lighting Settings")]

        [Tooltip("Minimum voltage in order to start illuminating the LED (defaults to 4.8)")]
        public float VoltageThreshold = 4.8f;
        [Tooltip("")]
        public float LightingCoefficient = 1f;
        [Tooltip("Light in the scene which will be illuminated")]
        public Light PointLight;


        [Header("Events")]
        public UnityEvent OnLightbulbPlaced;
        public UnityEvent OnLightbulbRemoved;


        // Flags to indicate when the values have been updated
        // across each of the LO, HI ports
        private bool updatedLow = false;
        private bool updatedHigh = false;


        /// <summary>
        ///     Indicates whether the LED is placed in the socket on the board.
        /// </summary>
        /// <remarks>
        ///     Could this be directly derived from the model? Absolutely. 
        ///     Could that also remove the responsibility of <see cref="DigitalTwinManager"/>
        ///     to set the 'IsConnected' fields of both of the models through a layer of events, 
        ///     and instead have this class directly manipulate the model? Absolutely.
        /// </remarks>
        public bool IsConnected { get; protected set; } = true;

        public ResistorModel ModelLow { get; protected set; }
        public ResistorModel ModelHigh { get; protected set; }


        /// <summary>
        ///     Determines lighting modifier for the given model, based on the current
        ///     in the model. A voltage threshold is provided by the caller, which
        ///     is then converted into a current threshold based on the resistance
        ///     of the model.
        /// </summary>
        /// <param name="model">The Lightbulb data model which </param>
        /// <param name="voltageThreshold"></param>
        /// <returns></returns>
        public static float CalculateLightingModifier(ResistorModel model, float voltageThreshold)
        {
            if (model == null) { return 0f; }
            float currentThreshold = voltageThreshold / (float)model.Resistance;
            return Mathf.Max((float)model.Current - currentThreshold, 0f);
        }


        /// <summary>
        ///     Static coroutine which disables a provided collider
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static IEnumerator DelayInteraction(Collider collider, float seconds)
        {
            if (collider) { collider.enabled = false; }
            yield return new WaitForSeconds(seconds);
            if (collider) { collider.enabled = true; }
        }


        /// <summary>
        ///     Hooks up UpdateActiveDualCircuit to the corresponding events
        /// </summary>
        public void OnEnable()
        {
            ModelLow = FindFirstObjectByType<DigitalTwinManager>().CircuitBoard.GetComponent(CircuitBoard.ComponentNames.L3Lo) as ResistorModel;
            ModelHigh = FindFirstObjectByType<DigitalTwinManager>().CircuitBoard.GetComponent(CircuitBoard.ComponentNames.L3Hi) as ResistorModel;

            if (ModelLow != null)
            {
                ModelLow.OnValuesUpdated += MarkLowUpdated;
            }

            if (ModelHigh != null)
            {
                ModelHigh.OnValuesUpdated += MarkHighUpdated;
            }
        }

        /// <summary>
        ///     Unhooks UpdateActiveDualCircuit from the corresponding events
        /// </summary>
        public void OnDisable()
        {
            if (ModelLow != null)
            {
                ModelLow.OnValuesUpdated -= MarkLowUpdated;
            }

            if (ModelHigh != null)
            {
                ModelHigh.OnValuesUpdated -= MarkHighUpdated;
            }
        }

        /// <summary>
        ///     Marks the <see cref="updatedLow"/> boolean true, indicating
        ///     that the <see cref="ModelLow"/> has new values
        /// </summary>
        /// <param name="model">
        ///     Model provided from the circuit simulator. Though we do not read
        ///     from the model directly, it is assumed to be reference-equal to ModelLow.
        /// </param>
        public void MarkLowUpdated(CircuitComponentModel _)
        {
            updatedLow = true;

            TryUpdateLighting();
        }

        /// <summary>
        ///     Marks the <see cref="updatedHigh"/> boolean true, indicating
        ///     that the <see cref="ModelHigh"/> has new values
        /// </summary>
        /// <param name="model">
        ///     Model provided from the circuit simulator. Though we do not read
        ///     from the model directly, it is assumed to be reference-equal to ModelHigh
        /// </param>
        public void MarkHighUpdated(CircuitComponentModel _)
        {
            updatedHigh = true;

            TryUpdateLighting();
        }


        /// <summary>
        ///     Checks both 'updated' flags and updates the lighting settings only if 
        ///     both have been set. The flags are then reset after updating the lighting.
        /// </summary>
        public void TryUpdateLighting()
        {
            if (!updatedLow || !updatedHigh) { return; }

            UpdateActiveDualCircuit();
            updatedLow = false;
            updatedHigh = false;
        }

        /// <summary>
        ///     Sets the intensity of the <see cref="PointLight"/> based on the
        ///     values specified in both <see cref="ModelLow"/> and <see cref="ModelHigh"/>
        /// </summary>
        /// <remarks>
        ///     Using the <see cref="CalculateLightingModifier(LightbulbModel, float)"/> method
        ///     ensures that when one model is below its voltage threshold, it does not subtract
        ///     from the intensity of the lighting.
        /// </remarks>
        public void UpdateActiveDualCircuit()
        {
            if (!PointLight) { return; }

            PointLight.intensity = LightingCoefficient *
                (CalculateLightingModifier(ModelLow, VoltageThreshold)
                    + CalculateLightingModifier(ModelHigh, VoltageThreshold));
        }

        /// <summary>
        ///     Swaps the current <see cref="IsConnected"/> state of the component and
        ///     calls the the associated event to broadcast and animation to play
        /// </summary>
        public void ToggleBulbPlaced()
        {
            IsConnected = !IsConnected;

            AnimateBulb();
            NotifyBulbChanged();
        }

        /// <summary>
        ///     Sets the appropriate animation trigger based on the current "removed"
        ///     state of the bulb, causing the animation for placing or removing the
        ///     bulb to be played.
        /// </summary>
        public void AnimateBulb()
        {
            Animator.SetTrigger(IsConnected ? AnimationPlaceKey : AnimationRemoveKey);
            float delay = Animator.GetCurrentAnimatorStateInfo(0).length;
            StartCoroutine(DelayInteraction(InteractionCollider, delay));
        }

        /// <summary>
        ///     Invoke the appropriate event based on the current "removed" state 
        ///     of the bulb.
        /// </summary>
        public void NotifyBulbChanged()
        {
            (IsConnected ? OnLightbulbPlaced : OnLightbulbRemoved)?.Invoke();
        }
    }
}
