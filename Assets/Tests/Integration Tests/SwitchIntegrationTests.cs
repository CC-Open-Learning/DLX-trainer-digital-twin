using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;
using VARLab.MPCircuits.Model;

public class SwitchIntegrationTests : MPCIntegrationTestsSetUpHelper
{
    // variables ---------------------------------------------------------------
    DigitalTwinManager digitalTwinManager;
    SwitchComponent toggleSwitch;
    
    

    // set up/ tear down --------------------------------------------------------
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        IntegrationTestHelper.ClearScene();
        yield return null;

        digitalTwinManager = SetUpDigitalTwinManager();

        toggleSwitch = SetUpSwitchComponent(); //for old tests
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        yield return null;
        IntegrationTestHelper.ClearScene();
    }



    // tests --------------------------------------------------------------------
    [UnityTest]
    public IEnumerator ToggleSwitch_Turns_light_OffOrOn()
    {
        //at start of simulation
        Assert.IsFalse(digitalTwinManager.ToggleSwitch.IsOn);
        Assert.IsFalse(digitalTwinManager.PushButtonSwitch.IsOn);
        VerifyLightIsOff();
        yield return null;

        //build simple circuit complete with SW1
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;
        digitalTwinManager.CircuitBoard.PlaceCable("cable1", CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.SW1_Left, false);
        digitalTwinManager.CircuitBoard.PlaceCable("cable2", CircuitBoard.PortNames.SW1_Right, CircuitBoard.PortNames.L1_A, false);
        digitalTwinManager.CircuitBoard.PlaceCable("cable3", CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.L1_B, false);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;
        VerifyLightIsOff(); //no switch is closed/on yet

        PushButtonSwitch();
        yield return null;
        Assert.IsTrue(digitalTwinManager.PushButtonSwitch.IsOn);
        Assert.IsFalse(digitalTwinManager.ToggleSwitch.IsOn);
        VerifyLightIsOff(); //push button not connected in this circuit

        FlipToggleSwitch();
        yield return null;
        Assert.IsTrue(digitalTwinManager.ToggleSwitch.IsOn);
        VerifyLightIsOn();

        FlipToggleSwitch();
        yield return null;
        Assert.IsFalse(digitalTwinManager.ToggleSwitch.IsOn);
        VerifyLightIsOff();

        FlipToggleSwitch();
        yield return null;
        Assert.IsTrue(digitalTwinManager.ToggleSwitch.IsOn);
        VerifyLightIsOn();
    }

    [UnityTest]
    public IEnumerator PushButton_Switch_Turns_light_OffOrOn()
    {
        //at start of simulation
        Assert.IsFalse(digitalTwinManager.ToggleSwitch.IsOn);
        Assert.IsFalse(digitalTwinManager.PushButtonSwitch.IsOn);
        VerifyLightIsOff();
        yield return null;

        //build simple circuit complete with PB1
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;
        digitalTwinManager.CircuitBoard.PlaceCable("cable1", CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.PB1_Left, false);
        digitalTwinManager.CircuitBoard.PlaceCable("cable2", CircuitBoard.PortNames.PB1_Right, CircuitBoard.PortNames.L1_A, false);
        digitalTwinManager.CircuitBoard.PlaceCable("cable3", CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.L1_B, false);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;
        VerifyLightIsOff(); //no switch is closed/on yet

        FlipToggleSwitch();
        yield return null;
        Assert.IsTrue(digitalTwinManager.ToggleSwitch.IsOn);
        Assert.IsFalse(digitalTwinManager.PushButtonSwitch.IsOn);
        VerifyLightIsOff(); //ToggleSwitch not connected in this circuit

        PushButtonSwitch();
        yield return null;
        Assert.IsTrue(digitalTwinManager.PushButtonSwitch.IsOn);
        VerifyLightIsOn();

        PushButtonSwitch();
        yield return null;
        Assert.IsFalse(digitalTwinManager.PushButtonSwitch.IsOn);
        VerifyLightIsOff();

        PushButtonSwitch();
        yield return null;
        Assert.IsTrue(digitalTwinManager.PushButtonSwitch.IsOn);
        VerifyLightIsOn();
    }

    [UnityTest]
    public IEnumerator Toggle_And_PushButton_Switch_Turn_light_OffOrOn_Appropriately()
    {
        //at start of simulation
        Assert.IsFalse(digitalTwinManager.ToggleSwitch.IsOn);
        Assert.IsFalse(digitalTwinManager.PushButtonSwitch.IsOn);
        VerifyLightIsOff();
        yield return null;

        //simple circuit complete with SW1 *AND* PB1 (both off)
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;
        digitalTwinManager.CircuitBoard.PlaceCable("cable1", CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.SW1_Left, false);
        digitalTwinManager.CircuitBoard.PlaceCable("cable2", CircuitBoard.PortNames.SW1_Right, CircuitBoard.PortNames.PB1_Left, false);
        digitalTwinManager.CircuitBoard.PlaceCable("cable3", CircuitBoard.PortNames.PB1_Right, CircuitBoard.PortNames.L1_A, false);
        digitalTwinManager.CircuitBoard.PlaceCable("cable4", CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.L1_B, false);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;
        VerifyLightIsOff(); //no switch is closed/on yet

        FlipToggleSwitch();
        yield return null;
        Assert.IsTrue(digitalTwinManager.ToggleSwitch.IsOn);
        Assert.IsFalse(digitalTwinManager.PushButtonSwitch.IsOn);
        VerifyLightIsOff(); //toggle closed but push btn still open

        FlipToggleSwitch();
        PushButtonSwitch();
        yield return null;
        Assert.IsFalse(digitalTwinManager.ToggleSwitch.IsOn);
        Assert.IsTrue(digitalTwinManager.PushButtonSwitch.IsOn);
        VerifyLightIsOff(); //push btn closed but toggle open

        FlipToggleSwitch();
        yield return null;
        Assert.IsTrue(digitalTwinManager.ToggleSwitch.IsOn);
        Assert.IsTrue(digitalTwinManager.PushButtonSwitch.IsOn);
        VerifyLightIsOn(); //both PB1 and SW1 closed in circuit
    }

    [UnityTest]
    public IEnumerator ToggleLightBulbs_Settings_Correctly_Set()
    {
        // Make sure the mouse click listener is only being added once
        Assert.AreEqual(1, toggleSwitch.switchModel.MouseClick.GetPersistentEventCount());

        // Check to see if calling the invoked class turns the light bulbs values on
        toggleSwitch.Toggle();

        Assert.AreEqual(true, toggleSwitch.IsOn);
        Assert.AreEqual("ToggleOn", toggleSwitch.switchModelAnimator.runtimeAnimatorController.animationClips[1].name);
        Assert.AreEqual(true, toggleSwitch.onSprite.activeSelf);
        Assert.AreEqual(false, toggleSwitch.offSprite.activeSelf);

        yield return null;

        // Check to see if calling the invoked class again turns the light bulbs values off
        toggleSwitch.Toggle();

        Assert.AreEqual(false, toggleSwitch.IsOn);
        Assert.AreEqual("ToggleOff", toggleSwitch.switchModelAnimator.runtimeAnimatorController.animationClips[0].name);
        Assert.AreEqual(false, toggleSwitch.onSprite.activeSelf);
        Assert.AreEqual(true, toggleSwitch.offSprite.activeSelf);

        yield return null;
    }

    // helper methods for easier readability ----------------------------------------------------------------
    private void FlipToggleSwitch()
    {
        digitalTwinManager.ToggleSwitch.Toggle(); //simulate mouse click
    }

    private void PushButtonSwitch()
    {
        digitalTwinManager.PushButtonSwitch.Toggle(); //simulate mouse click
    }

    private void VerifyLightIsOn()
    {
        Assert.AreNotEqual(0, digitalTwinManager.L1.PointLight.intensity);
    }

    private void VerifyLightIsOff()
    {
        Assert.AreEqual(0, Mathf.Round(digitalTwinManager.L1.PointLight.intensity));
    }
}
