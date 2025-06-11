using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{
    /// <summary>
    /// This class is used for the functionality of the electronic flasher component, which will allow or disallow current
    /// to flow through to another component on a timer, this will make lightbulbs flash or the buzzer beep etc...
    /// </summary>
    public class ElectronicFlasherComponent : MonoBehaviour
    {
        [Tooltip("The audio source for EF")]
        public AudioSource AudioSource;
        [Tooltip("The component name of EF")]
        public CircuitBoard.ComponentNames ComponentName;

        public ElectronicFlasherModel ElectronicFlasherModel { get; private set; }
        public bool IsCoroutineRunning { get; private set; }

        [SerializeField] private float _signalVolume = 0.5f;

        public UnityEvent OnFlasherActuated;

        private void Start()
        {
            ElectronicFlasherModel = FindFirstObjectByType<DigitalTwinManager>().CircuitBoard.GetComponent(ComponentName) as ElectronicFlasherModel;
            ElectronicFlasherModel.OnValuesUpdated += UpdateFlasher;
        }

        private void OnDisable()
        {
            ElectronicFlasherModel.OnValuesUpdated -= UpdateFlasher;
        }

        /// <summary>
        /// Checks to make sure that the flasher is actuated before starting a coroutine to change the signal state
        /// </summary>
        /// <param name="circuitComponentModel"></param>
        private void UpdateFlasher(CircuitComponentModel circuitComponentModel)
        {
            // Boolean to stop multiple calls to the coroutine when updating values across the electronic flasher
            if (!IsCoroutineRunning)
            {
                StartCoroutine(UpdateFlasherTimer());
            }
        }

        /// <summary>
        /// Updates the signal speed and current if the conditions are met
        /// </summary>
        /// <returns>the seconds between each signal change or null</returns>
        private IEnumerator UpdateFlasherTimer()
        {
            IsCoroutineRunning = true;

            while (ElectronicFlasherModel.Voltage > ElectronicFlasherModel.VoltageThreshold)
            {
                // Signal on
                OnSignalOn();
                PlayFlasherSound(_signalVolume);
                yield return new WaitForSeconds(ElectronicFlasherModel.SignalTimer);

                // Signal off
                OnSignalOff();
                PlayFlasherSound(_signalVolume);
                yield return new WaitForSeconds(ElectronicFlasherModel.SignalTimer);
            }

            IsCoroutineRunning = false;
            yield return null;
        }

        /// <summary>
        // To set the electronic flasher on
        /// </summary>
        private void OnSignalOn()
        {
            ElectronicFlasherModel.IsSignalOn = true;

            OnFlasherActuated?.Invoke();
        }

        /// <summary>
        // To set the electronic flasher off
        /// </summary>
        private void OnSignalOff()
        {
            ElectronicFlasherModel.IsSignalOn = false;

            OnFlasherActuated?.Invoke();
        }

        /// <summary>
        /// Plays the "click" sound after checking that the flasher voltage is higher then the threshold
        /// </summary>
        private void PlayFlasherSound(float volume)
        {
            AudioSource.volume = volume;
            AudioSource.Play();
        }
    }
}
