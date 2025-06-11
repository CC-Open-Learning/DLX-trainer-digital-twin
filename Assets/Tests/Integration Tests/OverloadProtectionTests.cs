using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VARLab.Interfaces;
using VARLab.MPCircuits;
using VARLab.MPCircuits.Model;

public class OverloadProtectionTests : MPCIntegrationTestsSetUpHelper
{
    OverloadProtection overloadProtection;
    private DigitalTwinManager digitalTwinManager;



    [UnitySetUp]
    public IEnumerator SetUp()
    {
        IntegrationTestHelper.ClearScene();
        yield return null; // Wait for one frame to allow initialization

        digitalTwinManager = SetUpDigitalTwinManager();
        overloadProtection = SetUpOverloadProtection();
        Debug.Log("Test Setup Complete");

        yield return null; // Wait for one frame to allow initialization
    }



    [UnityTearDown]
    public IEnumerator TearDown()
    {
        IntegrationTestHelper.ClearScene(); // Clear the scene
        Debug.Log("Scene cleared");
        yield return null;
    }



    [UnityTest]
    public IEnumerator Battery_Reporting_Overload_On_Short()
    {

        Debug.Log("Entering Battery_Reporting_Overload_On_Short Test");

        Assert.IsNotNull(digitalTwinManager.CircuitBoard.Battery.IsOverloaded, 
            "IsOverloaded is null");

        // Connect the circuit (shorted)
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, 
            CircuitBoard.PortNames.Battery_Gnd);
        digitalTwinManager.CircuitBoard.SolveCircuit();

        // Check for overload 
        Assert.IsTrue(digitalTwinManager.CircuitBoard.Battery.IsOverloaded);

        yield return null;
        
    }



    [UnityTest]
    public IEnumerator Overload_Reset_After_Reset_Triggered()
    {
        Debug.Log("Entering Overload_Reset_After_Reset_Triggered Test");

        Assert.IsNotNull(digitalTwinManager.CircuitBoard.Battery.IsOverloaded, 
            "IsOverloaded is null");

        // set up initial voltages
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;
        overloadProtection.VoltageKnob.value = 10f;

        // connect the circuit (shorted)
        digitalTwinManager.CircuitBoard.PlaceCable("short", CircuitBoard.PortNames.Battery_V, 
            CircuitBoard.PortNames.Battery_Gnd);
        digitalTwinManager.CircuitBoard.SolveCircuit();

        // validate the circuit is overloaded
        Assert.IsTrue(digitalTwinManager.CircuitBoard.Battery.IsOverloaded);
        
        // disconnect the short and reset the circuit
        digitalTwinManager.CircuitBoard.RemoveCable("short");
        overloadProtection.CanReset = true;  // allow a reset
        overloadProtection.StartCoroutine(overloadProtection.ResetOverloadCondition());
        yield return new WaitForSeconds(3);  // allow time for coroutine to complete

        // connect a circuit with a good configuration (L1)
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, 
            CircuitBoard.PortNames.L1_A);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_Gnd, 
            CircuitBoard.PortNames.L1_B);
        yield return null;
        digitalTwinManager.CircuitBoard.SolveCircuit();

        // validate the circuit is no longer overloaded
        Assert.IsFalse(digitalTwinManager.CircuitBoard.Battery.IsOverloaded);

        yield return null;
    }



    [UnityTest]
    public IEnumerator Validate_Button_Not_Pressed_At_Start()
    {
        Assert.IsFalse(overloadProtection.IsPressed);
        yield return null;
    }



    [UnityTest]
    public IEnumerator Validate_Holding_Button_Sets_Up_Reset()
    {

        // Connect the circuit (shorted)
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;

        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V,
            CircuitBoard.PortNames.Battery_Gnd);
        digitalTwinManager.CircuitBoard.SolveCircuit();

        overloadProtection.ButtonHeld = true;
        yield return new WaitForSeconds(1);

        // validate holding the button increases timer
        Assert.Greater(overloadProtection.PressTimer, 0);
        yield return new WaitForSeconds(1);

        // validate the circuit can be reset
        Assert.IsTrue(overloadProtection.CanReset);

        yield return null;
    }



    [UnityTest]
    public IEnumerator ResetButtonState_Resets_Buttons()
    {
        Debug.Log("Entering ResetButtonState_Resets_Buttons Test");

        // set up variables with active conditions
        overloadProtection.ClosedSprite.SetActive(false);
        overloadProtection.OpenSprite.SetActive(true);
        overloadProtection.PressTimer = 1f;
        overloadProtection.CanReset = true;
        overloadProtection.ButtonHeld = true;

        // trigger the reset
        overloadProtection.ResetButtonState();

        // validate the variables have been reset to initial states
        Assert.AreEqual(false, overloadProtection.CanReset);
        Assert.AreEqual(false, overloadProtection.ButtonHeld);
        Assert.AreEqual(0, overloadProtection.PressTimer);
        Assert.AreEqual(false, overloadProtection.ClosedSprite.activeSelf);
        Assert.AreEqual(true, overloadProtection.OpenSprite.activeSelf);
        yield return null;
    }



    public OverloadProtection SetUpOverloadProtection()
    {
        GameObject gameObject = new();

        OverloadProtection overloadProtection = 
            gameObject.AddComponent<OverloadProtection>();

        GameObject closedSprite = new();
        GameObject openSprite = new();
        Slider voltageKnob = gameObject.AddComponent<Slider>();
        overloadProtection.VoltageKnob = voltageKnob;
        TextMeshPro boardDisplay = new();
        overloadProtection.BoardDisplay = boardDisplay;

        overloadProtection.OverloadLightAnimator = gameObject.AddComponent<Animator>();
        overloadProtection.OverloadLightAnimator.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>
            ("Assets/Animations/Overload Protection/Light Overload Light.controller");

        overloadProtection.ClosedSprite = closedSprite;
        overloadProtection.OpenSprite = openSprite;
        overloadProtection.ClosedSprite.SetActive(false);
        overloadProtection.OpenSprite.SetActive(true);

        gameObject.SetActive(true);

        return overloadProtection;
    }
}
