using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;

public class SettingsIntegrationTests : MPCIntegrationTestsSetUpHelper
{
    SettingsManager settingsManager;
    MixerGroupManager voiceoverManager;
    MixerGroupManager sfxManager;

    [SetUp]
    public void SetUp()
    {
        settingsManager = SetUpSettingsManager();
        sfxManager = SetUpMixerGroupManager(MixerGroupManager.AudioManagerType.SFX, "SFXVolume", MixerGroupManager.AudioManagerType.SFX.ToString(), SetUpAudioSource());
        voiceoverManager = SetUpMixerGroupManager(MixerGroupManager.AudioManagerType.Voiceover, "VoiceOverVolume", MixerGroupManager.AudioManagerType.Voiceover.ToString(), SetUpAudioSource());
    }

    [TearDown]
    public void TearDown()
    {
        IntegrationTestHelper.ClearScene();

        settingsManager = null;
    }

    [UnityTest]
    public IEnumerator SettingsManager_OnInputMethodChanged_ReturnsCorrectInputMethod()
    {
        SettingsInputMethods.InputMethodTypes expectedInputMethod = SettingsInputMethods.InputMethodTypes.Slider;
        Assert.AreEqual(expectedInputMethod, settingsManager.CurrentInputMethod.InputMethodType);

        yield return null;

        //changes input method to increment and decrement
        SettingsInputMethods.InputMethodTypes expectedInputMethodAfterChange = SettingsInputMethods.InputMethodTypes.IncrementAndDecrement;
        SettingsManager.OnInputMethodChanged?.Invoke(expectedInputMethodAfterChange);

        yield return null;

        Assert.AreEqual(expectedInputMethod, settingsManager.CurrentInputMethod.InputMethodType);
    }

    [UnityTest]
    public IEnumerator SettingsManager_InitializeDefaultValues_ReturnsCorrectValues()
    {
        var inputMethodList = SetUpInputMethodList();

        settingsManager.InitializeDefaultValues(inputMethodList[0]);

        SettingsInputMethods.InputMethodTypes expectedInputMethod = SettingsInputMethods.InputMethodTypes.Slider;

        yield return null;

        Assert.AreEqual(expectedInputMethod, settingsManager.CurrentInputMethod.InputMethodType);

        Assert.AreEqual(settingsManager.VoiceOverSlider.value, Settings.VoiceOverVolume);
        Assert.AreEqual(settingsManager.SoundEffectsSlider.value, Settings.SoundEffectVolume);
    }

    [UnityTest]
    public IEnumerator SettingsManager_SelectNextInputMethod_ReturnsCorrectInputMethod()
    {
        string expectedInputMethodText = "Slider";
        yield return null;

        Assert.AreEqual(expectedInputMethodText, settingsManager.InputMethodText.text);
        string nextExpectedInputMethodText = "Increment & Decrement";
        settingsManager.SelectNextInputMethod(1);
        yield return null;

        Assert.AreEqual(nextExpectedInputMethodText, settingsManager.InputMethodText.text);
        settingsManager.SelectNextInputMethod(1);
        yield return null;

        //should return to beginning of list at this point
        Assert.AreEqual(expectedInputMethodText, settingsManager.InputMethodText.text);
        settingsManager.SelectNextInputMethod(-1);
        yield return null;

        //should return to end of list at this point
        Assert.AreEqual(nextExpectedInputMethodText, settingsManager.InputMethodText.text);
    }

    [UnityTest]
    public IEnumerator SettingsManager_SelectNextInputMethod_SavedOnceRestart()
    {
        string expectedInputMethodText = "Slider";
        string nextExpectedInputMethodText = "Increment & Decrement";

        Assert.AreEqual(expectedInputMethodText, settingsManager.InputMethodText.text);

        settingsManager.SelectNextInputMethod(1);
        yield return null;

        // Load scene (restart) without adding the scene to BuildSetting
        var path = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("TestScene", new string[] { "Assets" })[0]);
        EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
        yield return null;

        Assert.AreEqual(nextExpectedInputMethodText, settingsManager.InputMethodText.text);
        Assert.AreEqual(nextExpectedInputMethodText, Settings.SelectedInputMethod.InputMethodName);
    }
}
