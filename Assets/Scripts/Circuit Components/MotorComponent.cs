using System;
using UnityEngine;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{
    public class MotorComponent : MonoBehaviour
    {
        // Consts
        private const int SpeedMultiplier = 60;
        private const double SpinningStartVoltage = 2.3;
        private const float SoundPitchAdjustment = 0.30f;

        // Input fields in editor
        public Transform MotorTransform;

        /// <summary>
        ///     Sound effect sourced from
        ///     https://pixabay.com/sound-effects/slide-projector-with-ventilation-field-recording-23099/ 
        /// </summary>
        public AudioSource SoundEffect;

        // Private fields
        private MotorModel model;
        private Vector3 rotationAxisY = Vector3.up; //rotateLeftOrRightParallelToBoard
        private float currentRotatingSpeed;

        // Retrieve private fields for testing
        public float CurrentRotatingSpeed { get => currentRotatingSpeed; }

        /// <summary>
        /// Find the battery and motor diode models. Add a listener for the motor ports being updated.
        /// </summary>
        private void Start()
        {
            model ??= FindFirstObjectByType<DigitalTwinManager>().CircuitBoard.M1;
            model.OnValuesUpdated += UpdateMotorSpeedAndSound;
        }

        private void OnDisable()
        {
            model.OnValuesUpdated -= UpdateMotorSpeedAndSound;
        }

        /// <summary>
        /// Rotates the motor object to the right in unity via the Y axis if there is a rotating speed above 0.  If the pos/neg 
        /// ports are reversed, it spins the opposite way (since currentRotatingSpeed will be negative)
        /// </summary>
        private void Update()
        {
            if (currentRotatingSpeed != 0)
            {
                MotorTransform.Rotate(currentRotatingSpeed * Time.deltaTime * rotationAxisY);
            }
        }

        /// <summary>
        /// Sets the current rotating speed & pitch depending on the board voltage.  The motor doesn't spin until about 2.3V.
        /// </summary>
        private void UpdateMotorSpeedAndSound(CircuitComponentModel c)
        {
            double voltage = model.Voltage;

            if (double.IsNaN(voltage)) { voltage = 0; }

            if (Math.Abs(voltage) < SpinningStartVoltage)
            {
                currentRotatingSpeed = 0;
                SoundEffect.Stop();
                return;
            }

            currentRotatingSpeed = (float)voltage * SpeedMultiplier;

            // speed up the motor rotating sound and ensure it is playing
            SoundEffect.pitch = (float)Math.Abs(voltage) / 100 + SoundPitchAdjustment;

            // prevents frequency issues when holding & moving voltage slider
            if (SoundEffect.isPlaying == false)
            {
                SoundEffect.Play();
            }
        }
    }
}