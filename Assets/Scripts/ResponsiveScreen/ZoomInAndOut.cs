using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VARLab.MPCircuits
{
    public class ZoomInAndOut : MonoBehaviour
    {
        private const int DefaultFOV = 60;
        private const int SmallerAspectRatioFOV = 90;
        private const float ScreenZoomOutSwitchingPoint = 1.4f;
        private const float MouseScrollMinValue = 0f;

        [SerializeField] private float zoomSpeed = 1f;
        [SerializeField] private float mouseScrollSpeed = 1f;
        [SerializeField] private float zoomDepth = 10f;

        [Header("Controls")]
        public KeyCode ZoomInKey = KeyCode.UpArrow;
        public KeyCode ZoomOutKey = KeyCode.DownArrow;
        public KeyCode ResetKey = KeyCode.Space;

        [Header("UI")]
        public Slider ZoomSlider;

        private Camera _mainCamera;
        private float _currentDisplaySizeBaseFOV;
        private float _currentAspectRatioPercentage;
        private float _defaultZoomSpeed;

        //public GET properties for testing
        public int SmallerScreenFOV { get => SmallerAspectRatioFOV; }
        public int FullScreenFOV { get => DefaultFOV; }

        private void Start()
        {
            _mainCamera = Camera.main;
            _defaultZoomSpeed = zoomSpeed;

            AdjustZoomLevelBasedOnAspectRatio();

            ZoomSlider.minValue = DefaultFOV - zoomDepth;
            ZoomSlider.maxValue = DefaultFOV + zoomDepth;
        }

        /// <summary>
        ///     Each frame checks for keyboard input
        ///     * ZoomIn or ZoomOut held
        ///     & the current aspect ratio of the display screen.
        ///     The camera is zoomed in or out accordingly
        /// </summary>
        private void Update()
        {
            AdjustZoomLevelBasedOnAspectRatio();
            ZoomSlider.SetValueWithoutNotify(_mainCamera.fieldOfView);

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            SetMouseScrollSpeed();

            if (Input.GetKey(ZoomInKey) || Input.GetAxis("Mouse ScrollWheel") > MouseScrollMinValue)
            {
                ZoomIn();
            }
            if (Input.GetKey(ZoomOutKey) || Input.GetAxis("Mouse ScrollWheel") < MouseScrollMinValue)
            {
                ZoomOut();
            }

            ZoomSlider.SetValueWithoutNotify(_mainCamera.fieldOfView);
        }

        /// <summary>
        ///     Display will zoom in more if they are not already fully zoomed in
        /// </summary>
        private void ZoomIn()
        {
            if (_mainCamera.fieldOfView > ZoomSlider.minValue)
            {
                _mainCamera.fieldOfView -= zoomSpeed * Time.deltaTime;
            }
        }

        /// <summary>
        ///     Display will zoom out further if they are not already fully zoomed out
        /// </summary>
        private void ZoomOut()
        {
            if (_mainCamera.fieldOfView < ZoomSlider.maxValue)
            {
                _mainCamera.fieldOfView += zoomSpeed * Time.deltaTime;
            }
        }

        /// <summary>
        ///     Zoom out fully if the display screen width starts to cut into cable bundles,
        ///     otherwise keep it at the zoom half-point default field of view.
        /// </summary>
        private void AdjustZoomLevelBasedOnAspectRatio()
        {
            float aspectRatioPercentage = (float)Math.Round((double)Camera.main.aspect, 2);

            if (aspectRatioPercentage != _currentAspectRatioPercentage)
            {
                _currentAspectRatioPercentage = aspectRatioPercentage;

                // smaller aspect ratio (width/height) --> zoom out fully
                if (_currentAspectRatioPercentage <= ScreenZoomOutSwitchingPoint)
                {
                    _mainCamera.fieldOfView = _currentDisplaySizeBaseFOV = SmallerAspectRatioFOV;
                    return;
                }

                // wider/normal aspect ratio --> set to default/middle zoom level
                _mainCamera.fieldOfView = _currentDisplaySizeBaseFOV = DefaultFOV;
            }
        }

        /// <summary>
        ///     Resets the zoom of the camera and the position of the slider back to their default position
        /// </summary>
        public void ResetZoom()
        {
            _mainCamera.fieldOfView = _currentDisplaySizeBaseFOV;
            ZoomSlider.SetValueWithoutNotify(_mainCamera.fieldOfView);
        }

        public void HandleCameraZoomInAndOut(float value)
        {
            _mainCamera.fieldOfView = value;
        }

        private void SetMouseScrollSpeed()
        {
            zoomSpeed = Input.mouseScrollDelta.y != MouseScrollMinValue ? mouseScrollSpeed : _defaultZoomSpeed;
        }
    }
}
