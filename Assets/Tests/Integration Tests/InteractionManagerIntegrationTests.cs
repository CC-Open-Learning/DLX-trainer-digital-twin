using EPOOutline;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VARLab.Interactions;
using VARLab.Interfaces;
using VARLab.MPCircuits;

public class InteractionManagerIntegrationTests : MPCIntegrationTestsSetUpHelper
{
    InteractionManager interactionManager;
    InteractionManager interactionMgr; //non-singleton
    CableControls cableControls;
    GameObject light1;

    string mouseEnter = "MouseEnter";
    string mouseExit = "MouseExit";

    // Setup / Teardown --------------------------------------------------------------
    [UnitySetUp]
    public IEnumerator Setup()
    {
        IntegrationTestHelper.ClearScene();

        interactionManager = SetUpInteractionManagerSingleton();
        SetUpSliderComponents();

        yield return null;

        
        interactionMgr = SetUpInteractionManager();
        light1 = SetUpInteractableComponentGameObject();
        interactionMgr.OutlinableComponents.Add(light1.GetComponent<Interactable>());

        AddMouseEnterAndExitListeners(light1);

        cableControls = SetUpCableControl();
        cableControls.interactionManager = interactionMgr;
        cableControls.CanCreateCables(int.MaxValue);

        Assert.AreEqual(1, interactionMgr.OutlinableComponents.Count);
    }

    [UnityTearDown]
    public IEnumerator Teardown()
    {
        GameObject.Destroy(GameObject.FindObjectOfType<Singleton>());
        GameObject.Destroy(GameObject.FindObjectOfType<InteractionManager>());

        IntegrationTestHelper.ClearScene();

        yield return null;
    }

    // Tests -------------------------------------------------------------------------
    [UnityTest]
    public IEnumerator Cable_Bunch_Hover_And_Selected_Outlines_Work_Properly()
    {
        VerifyCableBunchOutlineIsHidden(cableControls.redCableBundle);
        VerifyCableBunchOutlineIsHidden(cableControls.blackCableBundle);
        yield return null;

        //hover black cable bunch
        cableControls.OnCableBunchHovered(true); //true = black cable   (listener method)
        yield return null;

        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.blackCableBundle, interactionMgr.InteractableHoverColor);
        VerifyCableBunchOutlineIsHidden(cableControls.redCableBundle);
        yield return null;

        //click black cable bunch
        cableControls.OnCableBundleClicked(true); //true = black cable   (listener method)
        yield return null;

        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.blackCableBundle, interactionMgr.SelectedObjectOutlineColor);
        VerifyCableBunchOutlineIsHidden(cableControls.redCableBundle);
        yield return null;

        cableControls.OnCableBunchHoveredOff();
        yield return null;

        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.blackCableBundle, interactionMgr.SelectedObjectOutlineColor);
        VerifyCableBunchOutlineIsHidden(cableControls.redCableBundle);

        //hover red cable bunch
        cableControls.OnCableBunchHovered(false); //false = red cable   (listener method)
        yield return null;

        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.redCableBundle, interactionMgr.InteractableHoverColor);
        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.blackCableBundle, interactionMgr.SelectedObjectOutlineColor);
        yield return null;

        //hover off red
        cableControls.OnCableBunchHoveredOff();
        yield return null;

        VerifyCableBunchOutlineIsHidden(cableControls.redCableBundle);
        yield return null;

        //hover red cable bunch again
        cableControls.OnCableBunchHovered(false); //false = red cable   (listener method)
        yield return null;

        //click red cable bunch
        cableControls.OnCableBundleClicked(false); //false = red cable   (listener method)
        yield return null;

        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.redCableBundle, interactionMgr.SelectedObjectOutlineColor);
        VerifyCableBunchOutlineIsHidden(cableControls.blackCableBundle);
        yield return null;

        //hover off red
        cableControls.OnCableBunchHoveredOff();
        yield return null;

        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.redCableBundle, interactionMgr.SelectedObjectOutlineColor);
        VerifyCableBunchOutlineIsHidden(cableControls.blackCableBundle);
        yield return null;

        //hover black cable bunch
        cableControls.OnCableBunchHovered(true); //true = black cable   (listener method)
        yield return null;

        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.blackCableBundle, interactionMgr.InteractableHoverColor);
        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.redCableBundle, interactionMgr.SelectedObjectOutlineColor);
        yield return null;

        //hover off black
        cableControls.OnCableBunchHoveredOff();
        yield return null;

        VerifyCableBunchOutlineIsHidden(cableControls.blackCableBundle);
        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.redCableBundle, interactionMgr.SelectedObjectOutlineColor);
    }

    [UnityTest]
    public IEnumerator Components_Interactable_Only_When_Cable_Bunch_Not_Selected()
    {
        //since there are 2 interaction managers in setup, use the non-singleton one
        light1.GetComponent<OutlineComponentOnHover>().InteractionManager = interactionMgr;

        //light and cable bunch have no outline at start
        VerifyInteractableComponentOutlineIsHidden(light1);
        VerifyCableBunchOutlineIsHidden(cableControls.redCableBundle);
        Assert.IsNull(cableControls.CurrentlySelectedTransparentCableLead);
        yield return null;

        //hover light
        SimulateMouseEvent(mouseEnter, light1);
        yield return null;

        //light outline visible since no cable bunch selected (& cable outline hidden)
        Assert.IsTrue(interactionMgr.OutlinableComponents[0].enabled);
        VerifyInteractableComponentOutlineIsVisibleAndProperColor(light1, interactionMgr.InteractableHoverColor);
        VerifyCableBunchOutlineIsHidden(cableControls.redCableBundle);
        yield return null;

        //hover off light
        SimulateMouseEvent(mouseExit, light1);
        yield return null;

        VerifyInteractableComponentOutlineIsHidden(light1);

        //hover red cable bunch
        cableControls.OnCableBunchHovered(false); //false = red cable   (listener method)
        yield return null;

        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.redCableBundle, interactionMgr.InteractableHoverColor);
        VerifyInteractableComponentOutlineIsHidden(light1);
        yield return null;

        //click red cable bunch
        cableControls.OnCableBundleClicked(false); //false = red cable   (listener method)
        yield return null;

        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.redCableBundle, interactionMgr.SelectedObjectOutlineColor);
        VerifyInteractableComponentOutlineIsHidden(light1);
        yield return null;

        //hover off red cable bunch
        cableControls.OnCableBunchHoveredOff();
        yield return null;

        VerifyCableBunchOutlineIsVisibleAndProperColor(cableControls.redCableBundle, interactionMgr.SelectedObjectOutlineColor);
        Assert.IsNotNull(cableControls.CurrentlySelectedTransparentCableLead);

        //hover light
        SimulateMouseEvent(mouseEnter, light1);
        yield return null;

        //confirm no outline since it can't be interacted with when cable bunch is selected
        Assert.IsFalse(interactionMgr.OutlinableComponents[0].enabled);
        VerifyInteractableComponentOutlineIsHidden(light1);
    }

    [UnityTest]
    public IEnumerator Verify_ComponentInteractions_Get_Disabled_And_Enabled_Correctly()
    {
        Assert.IsNull(cableControls.currentlySelectedLead); //at start of sim
        cableControls.UpdateSelectedLeadPosition();
        yield return null;

        // Interactions enabled since no lead currently selected
        Assert.IsTrue(interactionMgr.OutlinableComponents[0].enabled);
        yield return null;

        // Lead selected
        cableControls.currentlySelectedLead = SetUpCableLead();
        yield return null;
        cableControls.UpdateSelectedLeadPosition();
        yield return null;

        // Interactions are disabled now that a lead is selected
        Assert.IsFalse(interactionMgr.OutlinableComponents[0].enabled);
    }

    [UnityTest]
    public IEnumerator OnEnable_And_OnDisable_All_Interactions_Verify_Correct_Values()
    {
        // Disabling all interactions
        interactionManager.EnableAllInteractions(false);

        // Assert the method disables all interactions
        Assert.AreEqual(false, interactionManager.ports[0].gameObject.GetComponent<PortBehaviour>().InteractionEnabled);
        Assert.AreEqual(false, interactionManager.cableBundles[0].enabled);
        Assert.AreEqual(false, interactionManager.boardVoltage[0].enabled);
        Assert.AreEqual(false, interactionManager.boardVoltageSlider.enabled);
        Assert.AreEqual(false, interactionManager.multimeter[0].enabled);
        Assert.AreEqual(false, interactionManager.multimeterDial.enabled);
        Assert.AreEqual(false, interactionManager.switchToggle[0].enabled);
        Assert.AreEqual(false, interactionManager.fuse[0].enabled);
        Assert.AreEqual(false, interactionManager.potentiometers[0].enabled);
        Assert.AreEqual(false, interactionManager.potSlider.enabled);

        yield return null;

        // Enabling all interactions
        interactionManager.EnableAllInteractions(true);

        // Assert the method enables all interactions
        Assert.AreEqual(true, interactionManager.ports[0].gameObject.GetComponent<PortBehaviour>().InteractionEnabled);
        Assert.AreEqual(true, interactionManager.cableBundles[0].enabled);
        Assert.AreEqual(true, interactionManager.boardVoltage[0].enabled);
        Assert.AreEqual(true, interactionManager.boardVoltageSlider.gameObject.activeSelf);
        Assert.AreEqual(true, interactionManager.multimeter[0].enabled);
        Assert.AreEqual(true, interactionManager.switchToggle[0].enabled);
        Assert.AreEqual(true, interactionManager.fuse[0].enabled);
        Assert.AreEqual(true, interactionManager.potentiometers[0].enabled);
        Assert.AreEqual(true, interactionManager.potSlider.gameObject.activeSelf);

        yield return null;
    }

    [UnityTest]
    public IEnumerator OnMultiple_Interactions_Enabled_And_Disabled_Verify_Correct_Values()
    {
        // Enabling some using some methods from interaction manager
        interactionManager.EnableSwitchInteractions(true);
        interactionManager.EnableFuseInteractions(true);
        interactionManager.EnablePOTInteractions(true);

        // Disabling some interactions from interaction manager
        interactionManager.EnablePortInteractions(false);
        interactionManager.EnableBoardVoltageInteractions(false);
        interactionManager.EnableMultimeterInteractions(false);

        // Verify that the selected interactions to enable are working
        Assert.AreEqual(true, interactionManager.switchToggle[0].enabled);
        Assert.AreEqual(true, interactionManager.fuse[0].enabled);
        Assert.AreEqual(true, interactionManager.potentiometers[0].enabled);
        Assert.AreEqual(true, interactionManager.potSlider.gameObject.activeSelf);

        yield return null;

        // Verify that the selected interactions to disable are working
        Assert.AreEqual(false, interactionManager.ports[0].gameObject.GetComponent<PortBehaviour>().InteractionEnabled);
        Assert.AreEqual(false, interactionManager.boardVoltage[0].enabled);
        Assert.AreEqual(false, interactionManager.boardVoltageSlider.enabled);
        Assert.AreEqual(false, interactionManager.multimeter[0].enabled);
        Assert.AreEqual(false, interactionManager.multimeterDial.enabled);

        yield return null;

        // Then verify that the interactions for a component only includes interactions from the name that was setup
        Assert.AreEqual("ports", interactionManager.ports[0].gameObject.name);
        Assert.AreEqual("cableBundles", interactionManager.cableBundles[0].gameObject.name);
        Assert.AreEqual("boardVoltage", interactionManager.boardVoltage[0].gameObject.name);
        Assert.AreEqual("boardVoltageSlider", interactionManager.boardVoltageSlider.gameObject.name);
        Assert.AreEqual("multimeter", interactionManager.multimeter[0].gameObject.name);
        Assert.AreEqual("multimeterDial", interactionManager.multimeterDial.gameObject.name);
        Assert.AreEqual("switchToggle", interactionManager.switchToggle[0].gameObject.name);
        Assert.AreEqual("fuse", interactionManager.fuse[0].gameObject.name);
        Assert.AreEqual("potentiometers", interactionManager.potentiometers[0].gameObject.name);
        Assert.AreEqual("potSlider", interactionManager.potSlider.gameObject.name);

        yield return null;
    }


    // Smaller set ups ------------------------------------------------------------------------
    public InteractionManager SetUpInteractionManagerSingleton()
    {
        GameObject interactionManagerSingletonObj = new GameObject();
        interactionManagerSingletonObj.SetActive(false);

        interactionManagerSingletonObj.AddComponent<Singleton>();
        InteractionManager interactionManager = interactionManagerSingletonObj.AddComponent<InteractionManager>();

        interactionManager.ports = new() { SetUpInteractable("ports") };
        interactionManager.cableBundles = new() { SetUpInteractable("cableBundles") };
        interactionManager.boardVoltage = new() { SetUpInteractable("boardVoltage") };
        interactionManager.multimeter = new() { SetUpInteractable("multimeter") };
        interactionManager.switchToggle = new() { SetUpInteractable("switchToggle") };
        interactionManager.fuse = new() { SetUpInteractable("fuse") };
        interactionManager.potentiometers = new() { SetUpInteractable("potentiometers") };

        //interactionManager.multimeterSliderUI = new();

        interactionManager.boardVoltageSlider = SetUpInteractable("boardVoltageSlider");
        interactionManager.potSlider = SetUpInteractable("potSlider");
        interactionManager.multimeterDial = SetUpInteractable("multimeterDial");

        interactionManagerSingletonObj.SetActive(true);

        return interactionManager;
    }

    void SetUpSliderComponents()
    {
        interactionManager.boardVoltageSlider.transform.parent = interactionManager.gameObject.transform;
        interactionManager.potSlider.transform.parent = interactionManager.gameObject.transform;
        interactionManager.multimeterDial.transform.parent = interactionManager.gameObject.transform;

        interactionManager.ports[0].gameObject.AddComponent<PortBehaviour>();

        //interactionManager.boardVoltageSlider.gameObject.AddComponent<CanvasRenderer>();
        //interactionManager.boardVoltageSlider.gameObject.AddComponent<CableControls>();
        //interactionManager.boardVoltageSlider.gameObject.GetComponent<CableControls>().currentlySelectedCable = new();
        //interactionManager.boardVoltageSlider.gameObject.GetComponent<CableControls>().enabled = false;

        //interactionManager.multimeterDial.gameObject.AddComponent<CanvasRenderer>();
        //interactionManager.multimeterDial.gameObject.AddComponent<CableControls>();
        //interactionManager.multimeterDial.gameObject.GetComponent<CableControls>().currentlySelectedCable = new();
        //interactionManager.multimeterDial.gameObject.GetComponent<CableControls>().enabled = false;

        //interactionManager.potSlider.gameObject.AddComponent<CanvasRenderer>();
        //interactionManager.potSlider.gameObject.AddComponent<CableControls>();
        //interactionManager.potSlider.gameObject.GetComponent<CableControls>().currentlySelectedCable = new();
        //interactionManager.potSlider.gameObject.GetComponent<CableControls>().enabled = false;

        RadialSlider slider;

        slider = interactionManager.boardVoltageSlider.gameObject.AddComponent<RadialSlider>();
        slider.Background = interactionManager.boardVoltageSlider.gameObject.AddComponent<Image>();
        slider.Slider = interactionManager.boardVoltageSlider.gameObject.AddComponent<Slider>();
        slider.baseUI = interactionManager.boardVoltageSlider.gameObject;
        slider.sliderUI = interactionManager.boardVoltageSlider.gameObject;
        slider.KnobRotationRoot = interactionManager.boardVoltageSlider.transform;

        slider = interactionManager.multimeterDial.gameObject.AddComponent<RadialSlider>();
        slider.Background = interactionManager.multimeterDial.gameObject.AddComponent<Image>();
        slider.Slider = interactionManager.multimeterDial.gameObject.AddComponent<Slider>();
        slider.baseUI = interactionManager.multimeterDial.gameObject;
        slider.sliderUI = interactionManager.multimeterDial.gameObject;
        slider.KnobRotationRoot = interactionManager.multimeterDial.transform;

        slider = interactionManager.potSlider.gameObject.AddComponent<RadialSlider>();
        slider.Background = interactionManager.potSlider.gameObject.AddComponent<Image>();
        slider.Slider = interactionManager.potSlider.gameObject.AddComponent<Slider>();
        slider.baseUI = interactionManager.potSlider.gameObject;
        slider.sliderUI = interactionManager.potSlider.gameObject;
        slider.KnobRotationRoot = interactionManager.potSlider.transform;
    }

    public Interactable SetUpInteractable(string name)
    {
        GameObject interactableObj = new GameObject();
        interactableObj.name = name;
        interactableObj.SetActive(false);

        Interactable interactable = interactableObj.AddComponent<Interactable>();
        interactable.MouseClick = new();
        interactable.MouseDown = new();
        interactable.MouseEnter = new();
        interactable.MouseUp = new();
        interactable.MouseExit = new();

        interactableObj.SetActive(true);

        return interactable;
    }

    





    // *********************** Helper methods for easier readability ***********************
    private void AddMouseEnterAndExitListeners(GameObject _object)
    {
        _object.GetComponent<Interactable>().MouseEnter?.AddListener((GameObject go) => _object.GetComponent<OutlineComponentOnHover>().OnMouseEnter());
        _object.GetComponent<Interactable>().MouseExit?.AddListener((GameObject go) => _object.GetComponent<OutlineComponentOnHover>().OnMouseExit());
    }

    private void SimulateMouseEvent(string mouseEvent, GameObject gameObject)
    {
        if (mouseEvent == mouseEnter)
        {
            gameObject.GetComponent<Interactable>().MouseEnter?.Invoke(gameObject);
        }
        else if (mouseEvent == mouseExit)
        {
            gameObject.GetComponent<Interactable>().MouseExit?.Invoke(gameObject);
        }
        else
        {
            Debug.Log("Mouse event parameter not initialized yet in SimulateMouseEvent() in InteractionManagerIntegrationTests");
        }
    }

    private void VerifyInteractableComponentOutlineIsVisibleAndProperColor(GameObject boardComponent, Color outlineColor)
    {
        Assert.AreEqual(HighlightInteractions.VisibleLayer, boardComponent.GetComponent<OutlineComponentOnHover>().ObjectToHighlight.GetComponent<Outlinable>().OutlineLayer);
        Assert.AreEqual(outlineColor, boardComponent.GetComponent<OutlineComponentOnHover>().ObjectToHighlight.GetComponent<Outlinable>().OutlineParameters.Color);
    }

    private void VerifyInteractableComponentOutlineIsHidden(GameObject boardComponent)
    {
        Assert.AreEqual(HighlightInteractions.HiddenLayer, boardComponent.GetComponent<OutlineComponentOnHover>().ObjectToHighlight.GetComponent<Outlinable>().OutlineLayer);
    }

    private void VerifyCableBunchOutlineIsVisibleAndProperColor(GameObject cableBunch, Color outlineColor)
    {
        Assert.AreEqual(HighlightInteractions.VisibleLayer, cableBunch.GetComponent<Outlinable>().OutlineLayer);
        Assert.AreEqual(outlineColor, cableBunch.GetComponent<Outlinable>().OutlineParameters.Color);
    }

    private void VerifyCableBunchOutlineIsHidden(GameObject cableBunch)
    {
        Assert.AreEqual(HighlightInteractions.HiddenLayer, cableBunch.GetComponent<Outlinable>().OutlineLayer);
    }
}
