using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VARLab.MPCircuits
{
    public class WelcomePanel : MonoBehaviour
    {
        [SerializeField] private string labTitle;
        [SerializeField] private string labDescription;
        [SerializeField] private TextMeshProUGUI labText;
        [SerializeField] private TextMeshProUGUI labDescriptionText;

        [SerializeField] private bool activeInScene = true;

        [Tooltip ("Buttons")]
        [SerializeField] private Button StartLabButton;
        [SerializeField] private Button SoundReminderButton;

        [SerializeField] private AudioSource welcomeAudioSource;
        [SerializeField] private AudioClip welcomeAudio;

        private void OnEnable()
        {
            StartLabButton.onClick.AddListener(CloseWelcomePanel);
            SoundReminderButton.onClick.AddListener(PlayWelcomeAudio);
        }

        private void Awake()
        {
            gameObject.SetActive(activeInScene);
            this.enabled = activeInScene;
        }

        IEnumerator Start()
        {
            yield return null; //wait for all game objects to load first to prevent race conditions in panel manager
            Singleton.Instance.PanelManager.SetWelcomePanelActiveState(activeInScene);
            if (!activeInScene) yield break;

            PlayWelcomeAudio();
            PopulateValues();
        }

        void PopulateValues()
        {
            labText.text = labTitle;
            labDescriptionText.text = labDescription;
        }

        void CloseWelcomePanel()
        {
            Singleton.Instance.PanelManager.SetWelcomePanelActiveState(false);
        }

        void PlayWelcomeAudio()
        {
            if (welcomeAudio == null) return;

            if (welcomeAudioSource.isPlaying)
            {
                welcomeAudioSource.Stop();
                welcomeAudioSource.PlayOneShot(welcomeAudio, Settings.VoiceOverVolume);
                return;
            }

            welcomeAudioSource.PlayOneShot(welcomeAudio, Settings.VoiceOverVolume);
        }
    }
}
