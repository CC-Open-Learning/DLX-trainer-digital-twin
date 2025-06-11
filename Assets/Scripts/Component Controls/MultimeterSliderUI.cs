using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VARLab.MPCircuits
{
    public class MultimeterSliderUI : MonoBehaviour
    {
        [SerializeField] private float dialRotationVelocity = 1f;
        [SerializeField] public RectTransform dial;
        [SerializeField] public AudioClip settingSwitchSound;
        [SerializeField] public AudioSource settingSwitchSource;
        [SerializeField] private float settingSwitchSoundVolume = 0.5f;
        [SerializeField] public MultimeterSliderOptionButton[] dialSettings;
        [SerializeField] public Slider multimeterSlider;
        public RadialSlider radialSlider;

        [HideInInspector] public MultimeterSliderOptionButton PreviousOption;

        [HideInInspector] public MultimeterDialSettings currentSetting;
        [HideInInspector] public int currentZRotation;

        // Event to notify other scripts when the setting is changed
        public static event System.Action<int> OnSettingChanged;

        // Store the previous setting and rotation for smooth transitions
        private MultimeterDialSettings previousSetting = MultimeterDialSettings.Off;

        private float previousZrotation = 0f;

        public Coroutine currentAnimation = null;

        public bool IsDragging;

        public bool CurrentlyHovered { get; private set; } = false;


        private void Start()
        {
            radialSlider = GetComponent<RadialSlider>();

            PreviousOption = dialSettings[0];
            previousZrotation = dial.localRotation.eulerAngles.z;
        }

        private void OnEnable()
        {
            // Subscribe to the event when this script is enabled
            MultimeterSliderOptionButton.OnOptionButtonSelected += OnOptionButtonSelected;
        }

        private void OnDisable()
        {
            // Unsubscribe from the event when this script is disabled
            MultimeterSliderOptionButton.OnOptionButtonSelected -= OnOptionButtonSelected;
        }

        //called from Unity from increment/decrement button events (1 = increment, -1 = decrement)
        public void SelectNextOption(int offset)
        {
            int nextSettingPosition = Mathf.Clamp((int)currentSetting + offset, 0, dialSettings.Length - 1);

            MultimeterSliderOptionButton nextButton = dialSettings[nextSettingPosition];
            OnOptionButtonSelected(nextButton.Setting, nextButton.TargetDialZRotation);
            OnKnobRelease();
            multimeterSlider.value = (int)currentSetting + 1;
            radialSlider.UpdateKnobWithSliderValue();
        }

        //TODO: review if this can be removed as not being used?
        public void SetToSettingAndZRotation(MultimeterDialSettings setting, int previousZRotation, int currentZRotation)
        {
            previousZrotation = previousZRotation;
            OnOptionButtonSelected(setting, currentZRotation);
        }

        // This method is called when a slider option button is selected
        private void OnOptionButtonSelected(MultimeterDialSettings setting, int targetDialZRotation)
        {
            currentSetting = setting;
            currentZRotation = targetDialZRotation;
        }

        //called from Unity "multimeter slider" game object RADIAL SLIDER event
        public void OnKnobRelease()
        {
            if (currentAnimation != null) return;

            // Start the coroutine to smoothly rotate the dial
            currentAnimation = StartCoroutine(RotateDialCoroutine(currentSetting, currentZRotation));
        }

        //called from Unity multimeter "slider" game object SLIDER event
        public void OnValueChange()
        {
            int multimeterValue = ((int)multimeterSlider.value - 1) % dialSettings.Length;

            PreviousOption?.TurnHighlight(false);
            dialSettings[multimeterValue].OnButtonClick();
            dialSettings[multimeterValue].TurnHighlight(true);
            PreviousOption = dialSettings[multimeterValue];
        }

        // Coroutine to smoothly rotate the dial to the target setting
        private IEnumerator RotateDialCoroutine(MultimeterDialSettings setting, int targetDialZRotation)
        {
            //Stop the slider from being dragged when the animation is being played
            radialSlider.CanMove = false;

            float currentRotationTime = 0f;
            float startingRotation = previousZrotation;
            float endingRotation = targetDialZRotation;

            // Calculate the number of clicks needed to reach the target setting
            int numberOfClicks = Mathf.Abs((int)setting - (int)previousSetting);


            // Calculate the total time needed for the rotation based on velocity
            float totalTimeNeeded = Mathf.Abs(endingRotation - startingRotation) / dialRotationVelocity;
            // Calculate the time between each click to play the sound
            float timeBetweenClicks = totalTimeNeeded / numberOfClicks;
            int numberOfClicksDone = 0;

            while (currentRotationTime < totalTimeNeeded)
            {
                currentRotationTime += Time.deltaTime;

                // Play sound when reaching the next click position
                if (currentRotationTime > (numberOfClicksDone + 1) * timeBetweenClicks)
                {
                    numberOfClicksDone++;
                    settingSwitchSource.PlayOneShot(settingSwitchSound, settingSwitchSoundVolume);
                }

                // Interpolate the dial rotation smoothly
                dial.localRotation = Quaternion.Euler(new Vector3(
                    dial.localRotation.x,
                    dial.localRotation.y,
                    Mathf.Lerp(startingRotation, endingRotation, currentRotationTime / totalTimeNeeded)
                ));

                yield return null;
            }

            if (previousSetting != currentSetting)
                OnSettingChanged?.Invoke((int)setting);

            // Update previous setting and rotation for the next iteration
            previousSetting = setting;
            previousZrotation = targetDialZRotation;

            // Notify other scripts about the new setting value

            currentAnimation = null;

            radialSlider.CanMove = true;
        }

        //called from Unity multimeter "slider" game object slider event
        public void OnPointerExit()
        {
            CurrentlyHovered = false;
            //Update the DMM to the latest value
            if (!radialSlider.IsSelected && currentAnimation == null)
                radialSlider.DisableOnMouseExit();
        }

        public void OnPointerEnter()
        {
            CurrentlyHovered = true;
        }
    }
}
