using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VARLab.Interactions;
using static VARLab.MPCircuits.SettingsInputMethods;

namespace VARLab.MPCircuits
{
    public class RadialSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] public Interactable parent;
        [SerializeField] public Transform knobRotationRoot; // Root object of the knob's rotation.
        [SerializeField] public float minKnobRotationValue, maxKnobRotationValue; // Minimum and maximum rotation angles for the knob.
        [SerializeField] public Slider slider; // Unity UI slider component to which the knob is attached.
        [SerializeField] public List<GameObject> uiComponentsToEnable; // pin to be enabled/disabled based on input method
        public GameObject sliderUI, buttonUI;
        public GameObject baseUI;
        [SerializeField] public Image background;
        [SerializeField] private float mouseRotationAngleDegrees; // Starting mouse position angle in degrees.
        [SerializeField] private float percentOfCircle = 0.85f; // The portion of the circle that the knob can travel along.
        [SerializeField] private bool isConstrained = true; // The portion of the circle that the knob can travel along.
        [SerializeField] public bool backgroundHidden = true; // hides the baseUI if true
        [SerializeField] private float delay; //Float used for selecting delay length
        private float timer = 0f;// Float Used for time delay
        private bool isSelected; // Indicates whether the knob is currently being dragged or not.
        private bool isParentHovered;

        public Image Background { get => background; set => background = value; }

        public Slider Slider { get => slider; set => slider = value; }

        public Transform KnobRotationRoot { get => knobRotationRoot; set => knobRotationRoot = value; }

        public bool IsParentHovered { get { return isParentHovered; } set { isParentHovered = value; } }

        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

        public UnityEvent<float> OnValueSelected;


        private float lastAngle;
        private float maxKnobDistanceAllowedPerFrame = 0.25f;

        public bool CanMove = true;

        private void OnEnable()
        {
            isSelected = false;
            SettingsManager.OnInputMethodChanged += EnableUIByInputMethod;
        }

        private void OnDisable()
        {
            SettingsManager.OnInputMethodChanged -= EnableUIByInputMethod;
        }

        private void Start()
        {
            EnableUIByInputMethod(Settings.SelectedInputMethod.InputMethodType);

            UpdateKnobWithSliderValue();
            // slider = max -> 1
            // slider = min -> 0
        }

        private void Update()
        {
            if (!isSelected || !CanMove) { return; } // If the knob is not selected (dragged), exit the method.
            UpdateKnobAndSliderWithMousePosition();
        }

        public void SetSliderValue(float value)
        {
            slider.value = value;
            UpdateKnobWithSliderValue();
            slider.onValueChanged?.Invoke(value);
            this.OnValueSelected?.Invoke(value);
        }

        private void UpdateKnobAndSliderWithMousePosition()
        {
            // Convert the mouse position from screen coordinates to the local coordinates of the slider's RectTransform.
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(slider.transform as RectTransform, Input.mousePosition, null, out localPos);

            // Rotate the local position to adjust for the starting mouse position angle in radians.
            localPos = rotate(localPos, Mathf.Deg2Rad * mouseRotationAngleDegrees);

            // Calculate the angle (in normalized 0-1 range) between the mouse position and the center of the slider.
            float newAngle = (Mathf.Atan2(-localPos.y, localPos.x) * 180f / Mathf.PI + 180f) / 360f;

            // Adjust the angle based on the defined percentage of the circle, limiting the knob's travel range.
            newAngle = newAngle / percentOfCircle;

            float constrainedAngle = GetConstrainedAngle(lastAngle, newAngle);

            // Update the slider's value based on the calculated angle.
            slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, constrainedAngle);

            // Update the knob's rotation based on the calculated angle.
            UpdateKnobPosition(constrainedAngle);

            lastAngle = constrainedAngle;
        }

        private float GetConstrainedAngle(float oldAngle, float newAngle)
        {
            if (!isConstrained)
            {
                return newAngle;
            }

            bool snappingFromOtherSide = Mathf.Abs(oldAngle - newAngle) > maxKnobDistanceAllowedPerFrame;
            bool hitLeftSideAndMovingLeft = oldAngle > newAngle && slider.value == slider.minValue;
            bool hitRightSideAndMovingRight = oldAngle < newAngle && slider.value == slider.maxValue;

            bool canUpdateSliderKnob = !(snappingFromOtherSide || hitLeftSideAndMovingLeft || hitRightSideAndMovingRight);

            return canUpdateSliderKnob ? newAngle : oldAngle;
        }

        // Updates the knob's rotation based on the normalized angle.
        public void UpdateKnobPosition(float angle)
        {
            knobRotationRoot.rotation = Quaternion.Euler(new Vector3(
                knobRotationRoot.rotation.eulerAngles.x,
                knobRotationRoot.rotation.eulerAngles.y,
                Mathf.Lerp(minKnobRotationValue, maxKnobRotationValue, angle)
            ));
        }

        // Rotates a 2D vector by the given delta angle in radians.
        private Vector2 rotate(Vector2 v, float delta)
        {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isParentHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isParentHovered = false;
            DisableOnMouseExit();
        }

        // Called when the knob is being dragged.
        public void OnDrag(PointerEventData eventData)
        {
            isSelected = true;
        }

        // Called when the knob drag ends.
        public void OnEndDrag(PointerEventData eventData)
        {
            isSelected = false;
            OnValueSelected?.Invoke(slider.value);
        }

        // Disables the slider only if the user isn't currently adjusting the dial value
        public void DisableOnMouseExit()
        {
            if (isSelected || isParentHovered) { return; }

            SetUIActiveState(false);
        }

        public void UpdateKnobWithSliderValue()
        {
            lastAngle = (slider.value - slider.minValue) / (slider.maxValue - slider.minValue);
            UpdateKnobPosition(lastAngle);
        }

        public void OnIncrementButtonClicked(int value)
        {
            float fillValue = value > 0 ? value - slider.value % value : -(slider.value % value);

            if (slider.value % value != 0)
            {
                slider.value += fillValue;
            }
            else
            {
                slider.value += value;
            }

            UpdateKnobWithSliderValue();

            OnValueSelected?.Invoke(slider.value);
        }

        /// <summary>
        /// composes the uiComponentsToEnable list based on the input method passed in
        /// </summary>
        /// <param name="inputMethodType"></param>
        public void EnableUIByInputMethod(SettingsInputMethods.InputMethodTypes inputMethodType)
        {

            uiComponentsToEnable = new List<GameObject> { baseUI };
            background.GetComponent<CanvasRenderer>().SetAlpha(1f);

            switch (inputMethodType)
            {
                case InputMethodTypes.Slider:
                    uiComponentsToEnable.Add(sliderUI);
                    break;

                case InputMethodTypes.IncrementAndDecrement:
                    uiComponentsToEnable.Add(buttonUI);

                    if (!backgroundHidden) { return; }
                    background.GetComponent<CanvasRenderer>().SetAlpha(0f);
                    break;
            }
        }

        /// <summary>
        /// Sets the active state equal to the bool passed in
        /// State does not change if a lead is held or a cable color is selected
        /// </summary>
        /// <param name="active">The target state of the gameobjects held in the uiComponentsToEnable list</param>
        public void SetUIActiveState(bool active)
        {
            CableControls cableControls = FindObjectOfType<CableControls>();

            if (cableControls.IsCableColorSelected || cableControls.IsCableSelected) { return; }

            if (GetComponent<MultimeterSliderUI>() != null)
            {
                if (FindObjectOfType<MultimeterComponent>().PortsConnected) { return; }
                SetToolTipState(active);
            }

            foreach (GameObject uiGameobjects in uiComponentsToEnable)
            {
                uiGameobjects.SetActive(active);
            }
        }

        private void SetToolTipState(bool enabled)
        {
            ToolTip[] toolTips = FindObjectsOfType<ToolTip>();

            foreach (ToolTip toolTip in toolTips)
            {
                toolTip.SetToolTipState(enabled);
            }
        }
    }
}