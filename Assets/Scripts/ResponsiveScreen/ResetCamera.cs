using UnityEngine;

namespace VARLab.MPCircuits
{
    public class ResetCamera : MonoBehaviour
    {
        [Header("Controls")]
        public KeyCode ResetKey = KeyCode.Space;

        [Header("Connected Scripts")]
        public RotateAround RotateAround;
        public ZoomInAndOut ZoomInAndOut;

        /// <summary>
        /// Resets the rotation, zoom and sliders back to their default positions
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(ResetKey))
            {
                RotateAround.ResetRotation();
                ZoomInAndOut.ResetZoom();
            }
        }
    }
}
