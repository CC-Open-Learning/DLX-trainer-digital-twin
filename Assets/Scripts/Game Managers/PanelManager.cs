using UnityEngine;

namespace VARLab.MPCircuits
{
    public class PanelManager : MonoBehaviour
    {
        public int PanelsOpen = 0;

        public GameObject BlurPostProcessingVolume;

        public GameObject SettingsPanel;
        public GameObject WelcomePanel;
        public GameObject ControlsGuidePanel;


        public void OpenSettingsPanel(bool open)
        {
            int incrementAmount = open ? 1 : -1;
            IncrementOpenPanelsCount(incrementAmount);

            SettingsPanel.SetActive(open);
            DetermineBlurActiveState();
        }

        public void SetWelcomePanelActiveState(bool open)
        {
            int incrementAmount = open ? 1 : -1;
            IncrementOpenPanelsCount(incrementAmount);

            WelcomePanel.SetActive(open);
            DetermineBlurActiveState();
            SetControlsGuidePanelState();
        }

        /// <summary>
        /// Hide the control guide if the welcome panel is open & vice versa
        /// </summary>
        private void SetControlsGuidePanelState()
        {
            ControlsGuidePanel.GetComponent<ControlsGuidePanel>().ShowControlGuidePanel(!WelcomePanel.activeInHierarchy);
        }

        public void CloseAllPanels()
        {
            BlurPostProcessingVolume.SetActive(false);
            PanelsOpen = 0;
        }

        private void IncrementOpenPanelsCount(int incrementAmount = 1)
        {
            if (PanelsOpen < 0) return;

            PanelsOpen += incrementAmount;
            Debug.Log(PanelsOpen);
        }

        private void DetermineBlurActiveState()
        {
            if(PanelsOpen > 0)
            {
                BlurPostProcessingVolume.SetActive(true);
            }
            else
            {
                CloseAllPanels();
            }
        }
    }
}
