using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VARLab.Interfaces;

namespace VARLab.MPCircuits
{
    public class SettingsManager : MonoBehaviour, ISettingsManager
    {
        SettingsManagerImpl settingsManagerImpl;

        [SerializeField] private TextMeshProUGUI voiceOverText;
        [SerializeField] private TextMeshProUGUI soundEffectsText;
        [SerializeField] private TextMeshProUGUI inputMethodText;

        [SerializeField] private Button voiceOverIncrementButton;
        [SerializeField] private Button voiceOverDecrementButton;
        [SerializeField] private Button soundEffectIncrementButton;
        [SerializeField] private Button soundEffectDecrementButton;
        [SerializeField] private Button inputMethodIncrementButton;
        [SerializeField] private Button inputMethodDecrementButton;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private Slider voiceOverSlider;
        [SerializeField] private Slider soundEffectsSlider;

        [SerializeField] private GameObject closeButtonBorder;
        [SerializeField] private StartPanelButton startPanelButton;
        [SerializeField] private SettingsInputMethods inputMethods;
        [SerializeField] static Action<SettingsInputMethods.InputMethodTypes> onInputMethodChanged;
        private SettingsInputMethods.InputMethod currentInputMethod;
        private int inputMethodsCount;

        //text
        public TextMeshProUGUI VoiceOverText { get => voiceOverText; set => voiceOverText = value; }
        public TextMeshProUGUI SoundEffectsText { get => soundEffectsText; set => soundEffectsText = value; }
        public TextMeshProUGUI InputMethodText { get => inputMethodText; set => inputMethodText = value; }

        //buttons
        public Button VoiceOverIncrementButton { get => voiceOverIncrementButton; set => voiceOverIncrementButton = value; }
        public Button VoiceOverDecrementButton { get => voiceOverDecrementButton; set => voiceOverDecrementButton = value; }
        public Button SoundEffectsIncrementButton { get => soundEffectIncrementButton; set => soundEffectIncrementButton = value; }
        public Button SoundEffectsDecrementButton { get => soundEffectDecrementButton; set => soundEffectDecrementButton = value; }
        public Button InputMethodIncrementButton { get => inputMethodIncrementButton; set => soundEffectIncrementButton = value; }
        public Button InputMethodDecrementButton { get => inputMethodDecrementButton; set => soundEffectDecrementButton = value; }
        public Button OpenButton { get => openButton; set => openButton = value; }
        public Button CloseButton { get => closeButton; set => closeButton = value; }

        //sliders
        public Slider VoiceOverSlider { get => voiceOverSlider; set => voiceOverSlider = value; }
        public Slider SoundEffectsSlider { get => soundEffectsSlider; set => soundEffectsSlider = value; }


        //input methods
        public SettingsInputMethods InputMethods { get => inputMethods; set => inputMethods = value; }
        public SettingsInputMethods.InputMethod CurrentInputMethod { get => currentInputMethod; set => currentInputMethod = value; }
        public static Action<SettingsInputMethods.InputMethodTypes> OnInputMethodChanged { get => onInputMethodChanged; set => onInputMethodChanged = value; }
        public int InputMethodsCount { get => inputMethodsCount; set => inputMethodsCount = value; }

        //other
        public GameObject CloseButtonBorder { get => closeButtonBorder; set => closeButtonBorder = value; }

        public StartPanelButton StartPanelButton { get => startPanelButton; set => startPanelButton = value; }

        //Creates an instance of SettingsManagerImpl that this class will call functions of (SettingsManager is the Unity layer)
        SettingsManagerImpl SettingsManagerImpl
        {
            get
            {
                if (settingsManagerImpl == null)
                {
                    settingsManagerImpl = new(
                        new TextMeshProUGUIWrapper(voiceOverText),
                        new TextMeshProUGUIWrapper(soundEffectsText),
                        new TextMeshProUGUIWrapper(inputMethodText),
                        new ButtonWrapper(voiceOverIncrementButton),
                        new ButtonWrapper(soundEffectDecrementButton),
                        new ButtonWrapper(soundEffectIncrementButton),
                        new ButtonWrapper(soundEffectDecrementButton),
                        new ButtonWrapper(inputMethodIncrementButton),
                        new ButtonWrapper(inputMethodDecrementButton),
                        new ButtonWrapper(openButton),
                        new ButtonWrapper(closeButton),
                        new GameObjectWrapper(closeButtonBorder),
                        closeButtonBorder.GetComponent<StartPanelButton>(),
                        FindObjectOfType<SettingsInputMethods>(),
                        FindObjectOfType<SettingsInputMethods>().InputMethods[0],
                        FindObjectOfType<SettingsInputMethods>().InputMethods.Count
                        );
                }

                return settingsManagerImpl;
            }
        }

        private void Start()
        {
            if (MixerGroupManager.FindObjectByManagerType(MixerGroupManager.AudioManagerType.SFX) == null || MixerGroupManager.FindObjectByManagerType(MixerGroupManager.AudioManagerType.Voiceover) == null) return;

            //Initializes Correct volumes for audio Mixer groups, settings panel, and settings panel sliders
            SettingsManagerImpl.InitializeDefaultValues(Settings.SelectedInputMethod);

            SettingsManagerImpl.SetVolume(Settings.SoundEffectVolume, MixerGroupManager.FindObjectByManagerType(MixerGroupManager.AudioManagerType.SFX));
            SettingsManagerImpl.SetVolume(Settings.VoiceOverVolume, MixerGroupManager.FindObjectByManagerType(MixerGroupManager.AudioManagerType.Voiceover));

            VoiceOverSlider.SetValueWithoutNotify(Settings.VoiceOverVolume);
            SoundEffectsSlider.SetValueWithoutNotify(Settings.SoundEffectVolume);
        }
        private void OnEnable()
        {
            CurrentInputMethod = SettingsManagerImpl.CurrentInputMethod;
            InputMethodsCount = SettingsManagerImpl.InputMethodCount;
            InputMethodText.text = CurrentInputMethod.InputMethodName;

            VoiceOverSlider.onValueChanged.AddListener(HandleVoiceOverSliderVolumeChanged);
            SoundEffectsSlider.onValueChanged.AddListener(HandleSoundEffectsSliderVolumeChanged);
        }

        public void OnDisable()
        {
            VoiceOverSlider.onValueChanged.RemoveListener(HandleVoiceOverSliderVolumeChanged);
            SoundEffectsSlider.onValueChanged.RemoveListener(HandleSoundEffectsSliderVolumeChanged);
        }

        /// <summary>
        /// Initializes Default Values for the sliders, volume levels, and input method
        /// </summary>
        /// <param name="selectedInputMethod"></param>
        public void InitializeDefaultValues(SettingsInputMethods.InputMethod selectedInputMethod)
        {
            SettingsManagerImpl.InitializeDefaultValues(selectedInputMethod);

            VoiceOverSlider.value = Settings.VoiceOverVolume;
            SoundEffectsSlider.value = Settings.SoundEffectVolume;

            ShowCurrentInputMethod(Settings.SelectedInputMethod);
        }

        public void ShowCurrentInputMethod(SettingsInputMethods.InputMethod inputMethod)
        {
            SettingsManagerImpl.ShowCurrentInputMethod(inputMethod);
        }

        public void SelectNextInputMethod(int offset)
        {
            SettingsManagerImpl.SelectNextInputMethod(offset);
        }

        public void OnOpenButtonClicked()
        {
            SettingsManagerImpl.OnOpenButtonClicked();
        }

        public void OnCloseButtonClicked()
        {
            SettingsManagerImpl.OnCloseButtonClicked();

            CurrentInputMethod = SettingsManagerImpl.CurrentInputMethod;
            OnInputMethodChanged.Invoke(CurrentInputMethod.InputMethodType);
        }

        /// <summary>
        /// Sets the volume of the Voiceover Mixer group.
        /// Called onValueChanged of the associated slider and removes the listener for this function while it is being adjusted, and readds the listener after adjustment is done
        /// </summary>
        /// <param name="volume"></param>
        public void HandleVoiceOverSliderVolumeChanged(float volume)
        {
            VoiceOverSlider.onValueChanged.RemoveListener(HandleVoiceOverSliderVolumeChanged);

            VoiceOverSlider.value = volume;
            SettingsManagerImpl.SetVolume(volume, MixerGroupManager.FindObjectByManagerType(MixerGroupManager.AudioManagerType.Voiceover));

            VoiceOverSlider.onValueChanged.AddListener(HandleVoiceOverSliderVolumeChanged);
        }

        /// <summary>
        /// Sets the volume of the SFX  Mixer group. 
        /// Called onValueChanged of the associated slider and removes the listener for this function while it is being adjusted, and readds the listener after adjustment is done
        /// </summary>
        /// <param name="volume"></param>
        public void HandleSoundEffectsSliderVolumeChanged(float volume)
        {
            SoundEffectsSlider.onValueChanged.RemoveListener(HandleSoundEffectsSliderVolumeChanged);

            SoundEffectsSlider.value = volume;
            SettingsManagerImpl.SetVolume(volume, MixerGroupManager.FindObjectByManagerType(MixerGroupManager.AudioManagerType.SFX));

            SoundEffectsSlider.onValueChanged.AddListener(HandleSoundEffectsSliderVolumeChanged);
        }

        //calculates the final voice over volume after clicking a button by using clamp to set a range
        public void OnVoiceOverVolumeButtonClick(float increment)
        {
            VoiceOverSlider.value = Settings.VoiceOverVolume + increment;
            SettingsManagerImpl.OnVoiceOverVolumeButtonClick(increment);
        }

        //calculates the final sound effect volume after clicking a button by using clamp to set a range
        public void OnSFXVolumeButtonClick(float increment)
        {
            SoundEffectsSlider.value = Settings.SoundEffectVolume + increment;
            SettingsManagerImpl.OnSFXVolumeButtonClick(increment);
        }
    }
}
