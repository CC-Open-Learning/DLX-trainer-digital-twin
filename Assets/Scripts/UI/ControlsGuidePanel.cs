using UnityEngine;

namespace VARLab.MPCircuits
{
    public class ControlsGuidePanel : MonoBehaviour
    {
        [SerializeField] private GameObject controlGuidePanel;
        [SerializeField] private GameObject helpButton;

        /// <summary>
        /// The control panel is visible/open at the start (unless the welcome panel is active)
        /// </summary>
        private void Start()
        {
            ShowControlGuidePanel(true);
        }

        /// <summary>
        /// When the help button is clicked, it is hidden and the control slide out panel is now visible/open.  When the close 
        /// button on the control panel is clicked, the panel becomes hidden and just the help icon button is visible.
        /// </summary>
        public void ShowControlGuidePanel(bool isVisible)
        {
            helpButton.SetActive(!isVisible);
            controlGuidePanel.SetActive(isVisible);
        }
    }
}
