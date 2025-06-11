using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;

public class MultimeterUIIntegrationTests : MPCIntegrationTestsSetUpHelper
{
    private MultimeterSliderUI multimeterSliderUI;

    [SetUp]
    public void SetUp()
    {
        IntegrationTestHelper.ClearScene();

        multimeterSliderUI = SetUpMultimeterSliderUI();
    }

    [TearDown]
    public void TearDown()
    {
        IntegrationTestHelper.ClearScene();
    }

    [UnityTest]
    public IEnumerator ChangeMultimeterDialToNewSettings_ByIncrementAndDecrement()
    {
        // int # is sent through unity event to signify increment or decrement 
        int incrementButtonClicked = 1;
        int decrementButtonClicked = -1;

        multimeterSliderUI.radialSlider = SetUpRadialSlider(1f, 7.5f, 0f, -180f);
        multimeterSliderUI.multimeterSlider = multimeterSliderUI.radialSlider.slider;
        yield return null;
        Assert.IsNotNull(multimeterSliderUI.radialSlider);

        //sim starts DMM at 'off'
        Assert.AreEqual(MultimeterDialSettings.Off, multimeterSliderUI.currentSetting);

        //test clamp so dial cannot go past minimum setting
        multimeterSliderUI.SelectNextOption(decrementButtonClicked); //method called when increment/decrement buttons clicked
        yield return null;
        VerifyCurrentDialSettingAndRotationIsCorrect(MultimeterDialSettings.Off, 110);

        //check each setting as you increment by 1
        multimeterSliderUI.SelectNextOption(incrementButtonClicked);
        yield return null;
        VerifyCurrentDialSettingAndRotationIsCorrect(MultimeterDialSettings.ACVoltage, 80);

        multimeterSliderUI.SelectNextOption(incrementButtonClicked);
        yield return null;
        VerifyCurrentDialSettingAndRotationIsCorrect(MultimeterDialSettings.DCVoltage, 45);

        multimeterSliderUI.SelectNextOption(incrementButtonClicked);
        yield return null;
        VerifyCurrentDialSettingAndRotationIsCorrect(MultimeterDialSettings.ACVoltageMillivolts, 15);

        multimeterSliderUI.SelectNextOption(incrementButtonClicked);
        yield return null;
        VerifyCurrentDialSettingAndRotationIsCorrect(MultimeterDialSettings.Resistance, -15);

        multimeterSliderUI.SelectNextOption(incrementButtonClicked);
        yield return null;
        VerifyCurrentDialSettingAndRotationIsCorrect(MultimeterDialSettings.Capacitance, -45);

        multimeterSliderUI.SelectNextOption(incrementButtonClicked);
        yield return null;
        VerifyCurrentDialSettingAndRotationIsCorrect(MultimeterDialSettings.Current, -75);

        //trying to move past the last DMM option (test clamp)
        multimeterSliderUI.SelectNextOption(incrementButtonClicked);
        yield return null;
        VerifyCurrentDialSettingAndRotationIsCorrect(MultimeterDialSettings.Current, -75);

        //move back 4 clicks/options with decrement
        for (int i = 0; i < 4; i++)
        {
            multimeterSliderUI.SelectNextOption(decrementButtonClicked);
        }
        yield return null;
        VerifyCurrentDialSettingAndRotationIsCorrect(MultimeterDialSettings.DCVoltage, 45);
    }

    // Helper methods for easier readability
    private void VerifyCurrentDialSettingAndRotationIsCorrect(MultimeterDialSettings expectedDialSetting, int expectedDialRotation)
    {
        Assert.AreEqual(expectedDialSetting, multimeterSliderUI.currentSetting);
        Assert.AreEqual(expectedDialRotation, multimeterSliderUI.currentZRotation);
    }
}
