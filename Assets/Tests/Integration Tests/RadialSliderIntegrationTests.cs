using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.Interactions;
using VARLab.Interfaces;
using VARLab.MPCircuits;

public class RadialSliderIntegrationTests : MPCIntegrationTestsSetUpHelper
{
    SettingsManager settingsManager;
    Interactable parent;

    RadialSlider radialSlider;
    RadialSliderKnob radialSliderKnob;

    CableControls cableControls;

    [SetUp]
    public void SetUp()
    {
        IntegrationTestHelper.ClearScene();

        cableControls = SetUpCableControl();
        parent = SetUpInteractable();
        radialSlider = SetUpRadialSlider(2, 14, 15, -300);
        SetUpUIStates();
    }

    [TearDown]
    public void TearDown()
    {
        IntegrationTestHelper.ClearScene();
    }

    [UnityTest]
    public IEnumerator OnDisableMouseExit_Verify_UIStateIsCorrect()
    {
        radialSlider.sliderUI.SetActive(true);  //sets active state to true before each case to confirm if function runs
        radialSlider.DisableOnMouseExit();

        yield return null;

        Assert.IsTrue(!radialSlider.sliderUI.activeInHierarchy);

        radialSlider.sliderUI.SetActive(true);

        radialSlider.IsSelected = true;
        radialSlider.DisableOnMouseExit();

        yield return null;

        //since isSelected is true, DisableOnMouseExit returns before anything happens, so the ui state does not change
        Assert.IsTrue(radialSlider.sliderUI.activeInHierarchy);

        radialSlider.sliderUI.SetActive(true);
        radialSlider.IsSelected = false;

        cableControls.currentlySelectedCable = SetUpCableConnector(SetUpCableLead(), SetUpCableLead(), new LineRenderer());
        radialSlider.DisableOnMouseExit();

        yield return null;

        //since currentlySelectedCable was initialized, cableControls.IsCableSelected will be true, and because of that,
        //SetUIStates (which is called from DisableOnMouseExit) will return so that UI can't be enabled if hovered while holding a lead
        Assert.IsTrue(radialSlider.sliderUI.activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator OnEnableUIByInputMethod_Assembles_CorrectList()
    {
        radialSlider.EnableUIByInputMethod(SettingsInputMethods.InputMethodTypes.Slider);

        yield return null;

        List<GameObject> expectedUiComponentsToEnable;

        expectedUiComponentsToEnable = new List<GameObject> { radialSlider.baseUI, radialSlider.sliderUI };

        yield return null;
        Assert.AreEqual(expectedUiComponentsToEnable, radialSlider.uiComponentsToEnable);

        radialSlider.EnableUIByInputMethod(SettingsInputMethods.InputMethodTypes.IncrementAndDecrement);
        expectedUiComponentsToEnable = new List<GameObject> { radialSlider.baseUI, radialSlider.buttonUI };

        yield return null;
        Assert.AreEqual(expectedUiComponentsToEnable, radialSlider.uiComponentsToEnable);
        radialSlider.backgroundHidden = true;

        yield return null;
        //checks that backgroundHidden is setting the background image alpha to 0
        Assert.IsTrue(radialSlider.background.GetComponent<CanvasRenderer>().GetAlpha() == 0);
    }

    public void SetUpUIStates()
    {
        radialSlider.background = SetUpImage();
        radialSlider.baseUI = SetUpGameObject();
        radialSlider.sliderUI = SetUpGameObject();
        radialSlider.buttonUI = SetUpGameObject();
    }


    public RadialSliderKnob SetUpRadialSliderKnob()
    {
        GameObject radialSliderKnobObj = new();
        radialSliderKnobObj.SetActive(false);

        RadialSliderKnob radialSliderKnob = radialSliderKnobObj.AddComponent<RadialSliderKnob>();
        radialSliderKnobObj.SetActive(true);

        return radialSliderKnob;
    }
}
