using System;
using UnityEngine;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{
    /// <summary>
    /// This class is for the component B1, which is a buzzer that will make sound depending on the voltage that is passed through the component. This class will change the sound
    /// volume depending on the voltage that is passed through B1
    /// </summary>
    public class Buzzer : MonoBehaviour
    {
        public const int volumeFactor = 400;
        private const float BuzzerPitch = 1.14f;
        private const float ForwardVoltage = 1f;

        [Tooltip("The audio source for B1")]
        public AudioSource AudioSource;
        [Tooltip("The component name of B1")]
        public CircuitBoard.ComponentNames ComponentName;

        public DiodeModel BuzzerModel { get; private set; }

        public void Start()
        {
            BuzzerModel = FindFirstObjectByType<DigitalTwinManager>().CircuitBoard.GetComponent(ComponentName) as DiodeModel;
            BuzzerModel.OnValuesUpdated += ChangeBuzzerVolume;

            // Setting the buzzer pitch
            AudioSource.pitch = BuzzerPitch;
        }

        private void OnDisable()
        {
            BuzzerModel.OnValuesUpdated -= ChangeBuzzerVolume;
        }

        /// <summary>
        /// This method is used to change the volume of the buzzer sound depending on the voltage flowing through B1
        /// </summary>
        /// <param name="circuitComponentModel"></param>
        private void ChangeBuzzerVolume(CircuitComponentModel circuitComponentModel)
        {
            // Setting the buzzer volume
            double buzzerVolume = BuzzerModel.Voltage / volumeFactor;
            AudioSource.volume = BuzzerModel.Voltage >= ForwardVoltage ?
                (float)Math.Max(0.0, Math.Min(1.0, buzzerVolume * 2)) :
                0;

            BuzzerAudioState();
        }

        /// <summary>
        /// This method is to stop the buzzer when the audio is set to 0 and play the audio only when the audio source is not already playing
        /// </summary>
        private void BuzzerAudioState()
        {
            if (AudioSource.volume == 0)
            {
                AudioSource.Stop();
                return;
            }

            if (!AudioSource.isPlaying)
            {
                AudioSource.Play();
            }
        }
    }
}
