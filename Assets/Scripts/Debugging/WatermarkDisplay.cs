using TMPro;
using UnityEngine;

namespace VARLab.MPCircuits
{
    /// <summary>
    /// This class displays the build version watermark on Development Builds and
    /// when using the Unity Editor.
    /// </summary>
    public class WatermarkDisplay : MonoBehaviour
    {
        [Tooltip("Indicates whether the build label watermark should be shown in the Unity Editor")]
        public bool ShowInEditor = true;
        [Tooltip("The game object parent that will hold the watermark text")]
        [SerializeField] private GameObject _watermarkPanel;
        [Tooltip("The TextMeshProUI object that will display the watermark text")]
        [SerializeField] private TextMeshProUGUI _watermarkContent;
        [Tooltip("The scriptable object to store the build version information in")]
        [SerializeField] private BuildInfoSO _buildInfoSO;

        /// <summary>
        /// Turns off the watermark display panel unless using the Unity Editor or 
        /// Development Build.  Loads the version text into the display text if 
        /// applicable.
        /// </summary>
        private void Awake()
        {
            _watermarkPanel.SetActive(false);
            // Unity Editor || Development Build

#if UNITY_EDITOR
            _watermarkPanel.SetActive(ShowInEditor);
            // Load the build version from the scriptable object into the 
            // display text
            _watermarkContent.text = PlayerPrefs.GetString("Manifest_Identifier");
#elif DEVELOPMENT_BUILD
            _watermarkPanel.SetActive(true);
            // Load the build version from the scriptable object into the
            // display text
            _watermarkContent.text = _buildInfoSO.buildVersion;
#endif
        }
    }
}
