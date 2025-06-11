using UnityEngine;
using UnityEngine.UI;

namespace VARLab.MPCircuits
{
    public class RotateAround : MonoBehaviour
    {
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private float offsetCache;

        //Assign a GameObject in the Inspector to rotate around
        [SerializeField] private Transform target;
        public float RotationSpeed = 10f;
        public float IntervalAngle = 10f;

        [Header("Controls")]
        public KeyCode RotateLeftKey = KeyCode.LeftArrow;
        public KeyCode RotateRightKey = KeyCode.RightArrow;

        [Header("UI")]
        [Tooltip("The slider used to modify camera rotation. " +
            "This is assumed to be a right-to-left slider for the angles to work intuitively")]
        public Slider RotationSlider;


        private void Awake()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        public void HandleRotateCamera(float offset)
        {
            transform.SetPositionAndRotation(initialPosition, initialRotation);
            transform.RotateAround(target.position, target.up, IntervalAngle * offset);

            offsetCache = offset;
        }

        /// <summary>
        /// Resets the rotation of the camera and the position of the slider back to their default position
        /// </summary>
        public void ResetRotation()
        {
            HandleRotateCamera(0f);
            RotationSlider.SetValueWithoutNotify(offsetCache);
        }

        /// <summary>
        ///     Each frame checks for keyboard input
        ///     * RotateLeft or RotateRight held
        ///     When an input is received, the camera is rotated accordingly 
        ///     and the linked <see cref="RotationSlider"/> is updated so that
        ///     it stays in sync with the current camera offset value.
        /// </summary>
        private void Update()
        {
            if (Input.GetKey(RotateLeftKey) && offsetCache < RotationSlider.maxValue)
            {
                HandleRotateCamera(offsetCache + (RotationSpeed * Time.deltaTime)); // offsetCache is now updated
            }
            else if (Input.GetKey(RotateRightKey) && offsetCache > RotationSlider.minValue)
            {
                HandleRotateCamera(offsetCache - (RotationSpeed * Time.deltaTime)); // offsetCache is now updated
            }
            else
            {
                return;
            }

            RotationSlider.SetValueWithoutNotify(offsetCache);
        }
    }
}
