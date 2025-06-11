namespace VARLab.MPCircuits
{
    public static class Settings
    {
        public static float VoiceOverVolume { get; set; } = 1f;
        public static float SoundEffectVolume { get; set; } = 0.18f;
        public static SettingsInputMethods.InputMethod SelectedInputMethod { get; set; } = 
            new SettingsInputMethods.InputMethod("Slider", SettingsInputMethods.InputMethodTypes.Slider);
    }
}
