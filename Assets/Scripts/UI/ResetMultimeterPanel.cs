using UnityEngine;
using UnityEngine.UI;

namespace VARLab.MPCircuits
{
    public class ResetMultimeterPanel : MonoBehaviour
    {
        [Tooltip("Take the button under Reset DMM Button Border")]
        [SerializeField] private Button resetButton;
        [Tooltip("Take the button under DMM Reset OK Button")]
        [SerializeField] private Button panelOKButton;
        [Tooltip("Take the Reset DMM Message Content panel")]
        [SerializeField] private GameObject resetDMMPanel;

        private MultimeterComponent multimeterComponent;
        private SettingsManager settingsManager;
        private StartPanelButton startPanelButton;
        private void Start()
        {
            resetDMMPanel.SetActive(false);
            multimeterComponent = FindFirstObjectByType<MultimeterComponent>();
            settingsManager = FindFirstObjectByType<SettingsManager>();
            startPanelButton = resetButton.GetComponentInParent<StartPanelButton>();
        }

        public void ResetMultimeter()
        {
            if (multimeterComponent.MeasuringCurrent)
            {
                resetDMMPanel.SetActive(true);
            }
            else 
            {
                multimeterComponent.SetFuseOverloaded(false);
                settingsManager.OnCloseButtonClicked();
                startPanelButton.OnPointerExit(null);
            }           
        }

        public void ChangeResetDMMPanelVisibility(bool status)
        {
            resetDMMPanel.SetActive(status);
        }
    }
}
