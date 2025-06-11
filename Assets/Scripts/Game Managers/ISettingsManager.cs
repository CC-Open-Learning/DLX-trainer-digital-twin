using System;
using TMPro;

namespace VARLab.MPCircuits
{
    public interface ISettingsManager
    {
        TextMeshProUGUI VoiceOverText {  get; }
        TextMeshProUGUI SoundEffectsText { get; }
        TextMeshProUGUI InputMethodText { get; }

        static Action<SettingsInputMethods.InputMethodTypes> OnInputMethodChanged;

        void SelectNextInputMethod(int offset);
    }
}
