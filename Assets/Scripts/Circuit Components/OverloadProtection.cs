using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VARLab.Interactions;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{
    /// <summary>
    /// This class contains all the functionality for the Overload Protection feature
    /// of the Circuit Trainer Board
    /// </summary>
    public class OverloadProtection : MonoBehaviour
    {
        /// <summary>
        /// Duration and formatting constants
        /// </summary>
        // the time required to hold the reset button before the overload can be reset
        private const float PressDuration = 1.5f;
        // the time required for the reset function to take affect
        private const float ResetDuration = 1.12f;
        // format of the board voltage display
        private const string DisplayFormat = "00.00";

        /// <summary>
        /// animation name constants
        /// </summary>
        // overload can be reset light flashing
        private const string CanResetFlash = "CanResetFlash";
        private const string PushDown = "PushDown";  // button is pushed
        private const string Release = "Release";  // button is released
        // circuit is resetting light flashing
        private const string ResettingFlash = "ResettingFlash";
        private const string Off = "Off";  // light is off
        private const string OnSolid = "OnSolid"; // light is on with flashing

        [Tooltip("The game object for the button to be interacted with by the user")]
        public Interactable ButtonModel;
        [Tooltip("The animation controller for the button")]
        public Animator ButtonModelAnimator;
        [Tooltip("The animation controller for the overload light")]
        public Animator OverloadLightAnimator;
        [Tooltip("The sprite icon for the closed position")]
        public GameObject ClosedSprite;
        [Tooltip("The sprite icon for the open position")]
        public GameObject OpenSprite;
        [Tooltip("The knob that adjusts the board voltage")]
        public Slider VoltageKnob;
        [Tooltip("The object that controls the board digital display")]
        public TextMeshPro BoardDisplay;

        public UnityEvent OnButtonPressed;

        [HideInInspector]
        public bool ButtonHeld = false;   // indicates the button hasn't been released 
        [HideInInspector]
        public float PressTimer;  // how long the reset button has been held 
        [HideInInspector]
        public bool CanReset;  // flag to determine if the overload can be reset

        private CircuitBoard circuitBoard;

        private bool isPressed = false;

        public bool IsPressed => isPressed;



        /// <summary>
        /// Resets button to initial released state, ensures a circuitBoard object
        /// exists and assigns the UpdateOverloadCondition method to the
        /// OnValuesUpdated event.
        /// </summary>
        void Start()
        {
            ResetButtonState();  // reset button state to ensure clean start
            circuitBoard = FindFirstObjectByType<DigitalTwinManager>().CircuitBoard;
            if (circuitBoard == null)
            {
                Debug.LogWarning("Unable to get component: Circuitboard");
                return;
            }

            // assign the UpdateOverloadCondition method to run whenever the circuitboard
            // battery voltages change
            circuitBoard.Battery.OnValuesUpdated += UpdateOverloadCondition;
        }



        /// <summary>
        /// Increments the timer is the button is held down and sets the circuit to be
        /// resettable if held long enough and a short has occured
        /// </summary>
        void Update()
        {
            if (ButtonHeld)
            {
                PressTimer += Time.deltaTime;
            }
            if (PressTimer > PressDuration && !CanReset && circuitBoard.Battery.IsShorted)
            {
                CanReset = true;
                OverloadLightAnimator.Play(CanResetFlash);
            }
        }



        /// <summary>
        /// Unassigns the UpdateOverloadCondition method from the OnValuesUpdated event
        /// </summary>
        private void OnDisable()
        {
            // unassign the UpdateOverloadCondition method to run whenever the circuitboard
            // battery voltages change
            circuitBoard.Battery.OnValuesUpdated -= UpdateOverloadCondition;
        }



        /// <summary>
        /// Resets the button to original released state and resets
        /// the held down timer and CanReset flag
        /// </summary>
        public void ResetButtonState()
        {
            ButtonHeld = false;
            PressTimer = 0;
            CanReset = false;
            ClosedSprite.SetActive(false);
            OpenSprite.SetActive(true);
        }



        /// <summary>
        /// Toggles button between pressed and released states and trigger animations
        /// for each state.  If released after pressed, also triggers the
        /// ReserOverloadCondition method
        /// </summary>
        public void ButtonToggle()
        {
            isPressed = !isPressed;

            if (isPressed)
            {
                ButtonHeld = true;
                ButtonModelAnimator.Play(PushDown);
                OpenSprite.SetActive(false);
                ClosedSprite.SetActive(true);
            }
            if (!isPressed)
            {
                ButtonModelAnimator.StopPlayback(); // Stop all playing animations
                ButtonModelAnimator.Play(Release);  // play button release animation
                StartCoroutine(ResetOverloadCondition());
            }
            OnButtonPressed?.Invoke();
        }



        /// <summary>
        /// Checks if the overload can be reset (timer has elapsed) and
        /// Resets the ButtonState to "released" position values, 
        /// triggers resetting flash animation and restores the held
        /// voltage to the battery, voltage adjustment knob and the board 
        /// voltage display, then reruns the circuit to allow the simulation
        /// to clear the short.
        /// </summary>
        /// <returns></returns>
        public IEnumerator ResetOverloadCondition()
        {
            if (CanReset)
            {
                ResetButtonState();
                OverloadLightAnimator.StopPlayback();
                OverloadLightAnimator.Play(ResettingFlash);
                yield return new WaitForSeconds(ResetDuration);
                OverloadLightAnimator.Play(Off);
                BoardDisplay.text = VoltageKnob.value.ToString(DisplayFormat);
                circuitBoard.Battery.ResetBatteryOverload(VoltageKnob.value);
                circuitBoard.SolveCircuit();
                yield return null;
            }
            ResetButtonState();
            yield return null;
        }



        /// <summary>
        /// Checks if the board is overloaded (short circuit) and turns on the overload 
        /// light if so.  If the board is overloaded, the board voltage is also set to 0 
        /// to simulate the board locking to prevent damage to the components.
        /// If the board is not overloaded, the overload light will be turned off.
        /// </summary>
        /// <param name="c"></param>
        public void UpdateOverloadCondition(CircuitComponentModel c)
        {
            if (circuitBoard.Battery.IsOverloaded)
            {            
                circuitBoard.Battery.BoardVoltage = 0;
                BoardDisplay.text = 
                    circuitBoard.Battery.BoardVoltage.ToString(DisplayFormat);
                OverloadLightAnimator.Play(OnSolid);
            }
            else
            {
                OverloadLightAnimator.Play(Off);
            }
        }
    }
}
