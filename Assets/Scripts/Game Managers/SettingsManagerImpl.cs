using System;
using UnityEngine;
using UnityEngine.EventSystems;
using VARLab.Interfaces;

namespace VARLab.MPCircuits
{
    public class SettingsManagerImpl
    {
        public ITextMeshProUGUI VoiceOverText { get; set; }
        public ITextMeshProUGUI SoundEffectsText { get; set; }
        public ITextMeshProUGUI InputMethodText { get; set; }
        public IButton VoiceOverIncrementButton { get; set; }
        public IButton VoiceOverDecrementButton { get; set; }
        public IButton SoundEffectsIncrementButton {  get; set; }
        public IButton SoundEffectsDecrementButton { get; set; }
        public IButton InputMethodIncrementButton { get; set; }
        public IButton InputMethodDecrementButton { get; set; }
        public IButton OpenButton { get; set; }
        public IButton CloseButton { get; set; }

        public IGameObject CloseButtonBorder { get; set; }

        public StartPanelButton StartPanelButton { get; set; }
        public SettingsInputMethods InputMethods { get; set; }
        public SettingsInputMethods.InputMethod CurrentInputMethod { get; set; }
        public int InputMethodCount { get; set; }

        public SettingsManagerImpl(
            ITextMeshProUGUI _voiceOverText,
            ITextMeshProUGUI _soundEffectsText,
            ITextMeshProUGUI _inputMethodText,
            IButton _voiceOverIncrementButton,
            IButton _voiceOverDecrementButton,
            IButton _soundEffectsIncrementButton,
            IButton _soundEffectsDecrementButton,
            IButton _inputMethodIncrementButton,
            IButton _inputMethodDecrementButton,
            IButton _openButton,
            IButton _closeButton,
            IGameObject _closeButtonBorder,
            StartPanelButton _startPanelButton,
            SettingsInputMethods _inputMethods,
            SettingsInputMethods.InputMethod _currentInputMethod,
            int _inputMethodCount
            )
        {
            VoiceOverText = _voiceOverText;
            SoundEffectsText = _soundEffectsText;
            InputMethodText = _inputMethodText;
            VoiceOverIncrementButton = _voiceOverIncrementButton;
            VoiceOverDecrementButton = _voiceOverDecrementButton;
            SoundEffectsIncrementButton = _soundEffectsIncrementButton;
            SoundEffectsDecrementButton = _soundEffectsDecrementButton;
            InputMethodIncrementButton = _inputMethodIncrementButton;
            InputMethodDecrementButton = _inputMethodDecrementButton;
            OpenButton = _openButton;
            CloseButton = _closeButton;
            CloseButtonBorder = _closeButtonBorder;
            StartPanelButton = _startPanelButton;
            InputMethods = _inputMethods;
            CurrentInputMethod = _currentInputMethod;
            InputMethodCount = _inputMethodCount;
        }

        //sets the volume, slider and text to be the correct volume value
        public void SetVolume(float volume, MixerGroupManager manager)
        {
            if(manager.ManagerType == MixerGroupManager.AudioManagerType.SFX)
            {
                Settings.SoundEffectVolume = volume;
                SoundEffectsText.text = String.Format("{0:P0}", volume);
            }
            else if(manager.ManagerType == MixerGroupManager.AudioManagerType.Voiceover)
            {
                Settings.VoiceOverVolume = volume;
                VoiceOverText.text = String.Format("{0:P0}", volume);
            }

            manager.SetMixerGroupVolume(manager.VolumeParameter, volume);
        }

        public void SaveInputMethod(SettingsInputMethods.InputMethod selectedInputMethod)
        {
            Settings.SelectedInputMethod = selectedInputMethod;
        }

        public void InitializeDefaultValues(SettingsInputMethods.InputMethod selectedInputMethod)
        {
            SetVolume(Settings.VoiceOverVolume, MixerGroupManager.FindObjectByManagerType(MixerGroupManager.AudioManagerType.Voiceover));
            SetVolume(Settings.SoundEffectVolume, MixerGroupManager.FindObjectByManagerType(MixerGroupManager.AudioManagerType.SFX));
            SaveInputMethod(selectedInputMethod);
        }

        public void OnOpenButtonClicked()
        {
            Singleton.Instance.PanelManager.OpenSettingsPanel(true);
        }

        public void OnCloseButtonClicked()
        {
            Singleton.Instance.PanelManager.OpenSettingsPanel(false);
            // un-highlights close button using the OnPointerExit method
            StartPanelButton.OnPointerExit(new PointerEventData(EventSystem.current));
        }

        //set up in editor
        public void SelectNextInputMethod(int offset)
        {
            if ((int)CurrentInputMethod.InputMethodType + offset < 0)
            {
                CurrentInputMethod = SetInputMethod(InputMethods, InputMethods.InputMethods[InputMethodCount - 1], InputMethodText);
            }
            else if ((int)CurrentInputMethod.InputMethodType + offset > InputMethodCount - 1)
            {
                CurrentInputMethod = SetInputMethod(InputMethods, InputMethods.InputMethods[0], InputMethodText);
            }
            else
            {
                CurrentInputMethod = SetInputMethod(InputMethods, InputMethods.InputMethods[(int)CurrentInputMethod.InputMethodType + offset], InputMethodText);
            }

            SaveInputMethod(CurrentInputMethod);
        }

        //same function as SelectNextInputMethod, but set up for testing (since the standard is attached to buttons as an onclick event, it can't have multiple arguments
        public SettingsInputMethods.InputMethod SelectNextInputMethodTest(SettingsInputMethods inputMethods, SettingsInputMethods.InputMethod currentInputMethod, int inputMethodsCount, int offset)
        {
            if ((int)currentInputMethod.InputMethodType + offset < 0)
            {
                currentInputMethod = SetInputMethod(inputMethods, inputMethods.InputMethods[inputMethodsCount - 1], InputMethodText);
            }
            else if ((int)currentInputMethod.InputMethodType + offset > inputMethodsCount - 1)
            {
                currentInputMethod = SetInputMethod(inputMethods, inputMethods.InputMethods[0], InputMethodText);
            }
            else
            {
                currentInputMethod = SetInputMethod(inputMethods, inputMethods.InputMethods[(int)currentInputMethod.InputMethodType + offset], InputMethodText);
            }

            return currentInputMethod;
        }

        public SettingsInputMethods.InputMethod SetInputMethod(SettingsInputMethods inputMethods, SettingsInputMethods.InputMethod inputMethod, ITextMeshProUGUI inputMethodText)
        {
            foreach (SettingsInputMethods.InputMethod i in inputMethods.InputMethods)
            {
                if (inputMethod.InputMethodName != i.InputMethodName)
                    continue;

                SettingsInputMethods.InputMethod selectedInputMethod = i;
                inputMethodText.text = i.InputMethodName;

                return selectedInputMethod;
            }

            throw new NullReferenceException("Selected Input method does not exist");
        }

        public void ShowCurrentInputMethod(SettingsInputMethods.InputMethod inputMethod)
        {
            CurrentInputMethod = SetInputMethod(InputMethods, InputMethods.InputMethods[(int)inputMethod.InputMethodType], InputMethodText);
        }

        //calculates the final voiceover volume after clicking a button by using clamp to set a range
        public void OnVoiceOverVolumeButtonClick(float increment)
        {
            float finalValue = Mathf.Clamp(Settings.VoiceOverVolume + increment, 0f, 1f);
            SetVolume(finalValue, MixerGroupManager.FindObjectByManagerType(MixerGroupManager.AudioManagerType.Voiceover));
        }

        //calculates the final sound effect volume after clicking a button by using clamp to set a range
        public void OnSFXVolumeButtonClick(float increment)
        {
            float finalValue = Mathf.Clamp(Settings.SoundEffectVolume + increment, 0f, 1f);
            SetVolume(finalValue, MixerGroupManager.FindObjectByManagerType(MixerGroupManager.AudioManagerType.SFX));
        }
    }
}
