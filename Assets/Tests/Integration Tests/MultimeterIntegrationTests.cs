using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;
using VARLab.MPCircuits.Model;

public class MultimeterIntegrationTests : MPCIntegrationTestsSetUpHelper
{
    private const string OverloadText = MultimeterComponent.TextFormatOverload;

    private DigitalTwinManager digitalTwinManager;
    private MultimeterComponent multimeterComponent;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        IntegrationTestHelper.ClearScene();

        digitalTwinManager = SetUpDigitalTwinManager();
        multimeterComponent = digitalTwinManager.Multimeter;

        yield return null;
        //multimeterComponent.multimeterDial = digitalTwinManager.MultimeterDial;
    }

    [TearDown]
    public void TearDown()
    {
        IntegrationTestHelper.ClearScene();
    }

    [UnityTest]
    public IEnumerator Measuring_Voltage_In_Series_Circuit_With_Two_Bulbs()
    {

        //Series circuit with 2 bulbs, switch open, 10V on battery
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;
        SetMultimeterDialToVoltsDC();
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.F1_Left);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.F1_Right, CircuitBoard.PortNames.SW1_Left);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.SW1_Right, CircuitBoard.PortNames.L1_A);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.L1_B, CircuitBoard.PortNames.L2_C);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.L2_D, CircuitBoard.PortNames.Battery_Gnd);
        digitalTwinManager.CircuitBoard.SW1.IsConnected = false;
        
        //connect dmm to ground
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.DMM_Ground, CircuitBoard.PortNames.Battery_Gnd);

        //check SW_Left port voltage
        digitalTwinManager.CircuitBoard.PlaceCable("DMM_cable", CircuitBoard.PortNames.DMM_Voltage, CircuitBoard.PortNames.SW1_Left);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.DCVoltage, "10.00");

        //check SW_Right port voltage
        MoveDMMVoltageLeadFromOldPortToNewPort("DMM_cable", CircuitBoard.PortNames.SW1_Left, CircuitBoard.PortNames.SW1_Right);
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.DCVoltage, "0.000"); //switch open so no voltage

        //close switch and re-check SW_Right port voltage
        digitalTwinManager.CircuitBoard.SW1.IsConnected = true;
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.DCVoltage, "9.988");

        //check L1-A port voltage
        MoveDMMVoltageLeadFromOldPortToNewPort("DMM_cable", CircuitBoard.PortNames.SW1_Right, CircuitBoard.PortNames.L1_A);
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.DCVoltage, "9.986");

        //check L1-B port voltage
        MoveDMMVoltageLeadFromOldPortToNewPort("DMM_cable", CircuitBoard.PortNames.L1_A, CircuitBoard.PortNames.L1_B);
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.DCVoltage, "4.989");

        //check L2_C port voltage
        MoveDMMVoltageLeadFromOldPortToNewPort("DMM_cable", CircuitBoard.PortNames.L1_B, CircuitBoard.PortNames.L2_C);
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.DCVoltage, "4.989");

        //check L2_D port voltage
        MoveDMMVoltageLeadFromOldPortToNewPort("DMM_cable", CircuitBoard.PortNames.L2_C, CircuitBoard.PortNames.L2_D);
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.DCVoltage, "0.000");
    }

    [UnityTest]
    public IEnumerator Measuring_Current_In_Simple_Circuit()
    {
        //simple circuit with current being measured between L2-D and battery ground
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;
        SetMultimeterDialToAmps();
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.L2_C);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.L2_D, CircuitBoard.PortNames.DMM_Current);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.DMM_Ground, CircuitBoard.PortNames.Battery_Gnd);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.Current, "0.262");
        VerifySmokeAnimation_IsOff();
    }

    [UnityTest]
    public IEnumerator Measuring_Current_In_Parallel_Circuit()
    {
        //parallel circuit with switch closed
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;
        SetMultimeterDialToAmps();
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.F1_Left);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.F1_Right, CircuitBoard.PortNames.SW1_Left);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.SW1_Right, CircuitBoard.PortNames.L1_A);
        digitalTwinManager.CircuitBoard.PlaceCable("SW1_L2", CircuitBoard.PortNames.SW1_Right, CircuitBoard.PortNames.L2_C);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.L1_B, CircuitBoard.PortNames.Battery_Gnd);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.L2_D, CircuitBoard.PortNames.Battery_Gnd);
        digitalTwinManager.CircuitBoard.SW1.IsConnected = true;

        //removed fuse and connect dmm leads to each fuse port
        digitalTwinManager.CircuitBoard.F1.IsConnected = false;
        digitalTwinManager.CircuitBoard.PlaceCable("DMM_F1_in", CircuitBoard.PortNames.DMM_Current, CircuitBoard.PortNames.F1_Left);
        digitalTwinManager.CircuitBoard.PlaceCable("DMM_F1_out", CircuitBoard.PortNames.DMM_Ground, CircuitBoard.PortNames.F1_Right);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.Current, "0.525"); //current before split
        VerifySmokeAnimation_IsOff();

        //remove dmm leads and replace fuse
        digitalTwinManager.CircuitBoard.RemoveCable("DMM_F1_in");
        digitalTwinManager.CircuitBoard.RemoveCable("DMM_F1_out");
        digitalTwinManager.CircuitBoard.F1.IsConnected = true;

        //remove a cable/lead where current splits at parallel section and attach dmm in that path instead
        digitalTwinManager.CircuitBoard.RemoveCable("SW1_L2");
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.DMM_Current, CircuitBoard.PortNames.L2_C);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.DMM_Ground, CircuitBoard.PortNames.SW1_Right);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        //approx half current in split path
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.Current, "-0.271"); 
        VerifySmokeAnimation_IsOff();

        //open switch
        digitalTwinManager.CircuitBoard.SW1.IsConnected = false;
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.Current, "0.000"); //there should be no current with switch open
        VerifySmokeAnimation_IsOff();
    }

    [UnityTest]
    public IEnumerator Measuring_Current_With_Fuse_And_Switch_Disconnected()
    {
        //series circuit with switch open and fuse removed
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;
        SetMultimeterDialToAmps();
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.F1_Left);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.F1_Right, CircuitBoard.PortNames.SW1_Left);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.SW1_Right, CircuitBoard.PortNames.L1_A);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.L1_B, CircuitBoard.PortNames.Battery_Gnd);
        digitalTwinManager.CircuitBoard.SW1.IsConnected = false;
        digitalTwinManager.CircuitBoard.F1.IsConnected = false;

        //connect dmm leads to each fuse port
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.DMM_Current, CircuitBoard.PortNames.F1_Left);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.DMM_Ground, CircuitBoard.PortNames.F1_Right);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.Current, "0.000"); //no current as switch open
        VerifySmokeAnimation_IsOff();
    }

    [UnityTest]
    public IEnumerator Multimeter_Fuse_Blows_When_Appropriate_And_Resets()
    {
        //confirm animations
        Assert.AreEqual("Smoke_On", multimeterComponent.Animator.runtimeAnimatorController.animationClips[0].name);
        Assert.AreEqual("Idle", multimeterComponent.Animator.runtimeAnimatorController.animationClips[1].name);

        //no smoke present on sim start
        VerifySmokeAnimation_IsOff();

        //create simple circuit
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.L2_C);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.L2_D, CircuitBoard.PortNames.Battery_Gnd);

        //connect DMM in parallel with circuit with enough current to blow it
        SetMultimeterDialToAmps();
        digitalTwinManager.CircuitBoard.PlaceCable("DMM_in", CircuitBoard.PortNames.DMM_Current, CircuitBoard.PortNames.L2_C);
        digitalTwinManager.CircuitBoard.PlaceCable("DMM_out", CircuitBoard.PortNames.DMM_Ground, CircuitBoard.PortNames.L2_D);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.Current, "0.000"); //DMM display after fuse blown
        VerifySmokeAnimation_IsOn();

        //remove DMM leads
        digitalTwinManager.CircuitBoard.RemoveCable("DMM_in");
        digitalTwinManager.CircuitBoard.RemoveCable("DMM_out");

        //reset DMM
        multimeterComponent.SetFuseOverloaded(false);
        yield return null;
        VerifySmokeAnimation_IsOff();
    }

    /// <summary>
    /// Verify multimeter display shows the correct resistance value.
    /// </summary>
    [UnityTest]
    public IEnumerator Multimeter_Displays_Correct_Resistance()
    {
        digitalTwinManager.MultimeterDial.OnDialValueChanged(4);
        yield return null;

        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.Resistance, OverloadText);

        // To bypass the IsNaN checking
        multimeterComponent.Volts.Voltage = 1f;

        // Get the MultimeterModel
        var model = multimeterComponent.Model;

        model.OhmmeterResistance = -0.5f;
        multimeterComponent.DisplayResistance();
        Assert.AreEqual("-0.500", multimeterComponent.multimeterText.text);

        model.OhmmeterResistance = -10.375f;
        multimeterComponent.DisplayResistance();
        Assert.AreEqual("-10.38", multimeterComponent.multimeterText.text);

        model.OhmmeterResistance = -0.375f;
        multimeterComponent.DisplayResistance();
        Assert.AreEqual("-0.375", multimeterComponent.multimeterText.text);

        model.OhmmeterResistance = 0.375f;
        multimeterComponent.DisplayResistance();
        Assert.AreEqual("0.375", multimeterComponent.multimeterText.text);

        model.OhmmeterResistance = 1f;
        multimeterComponent.DisplayResistance();
        Assert.AreEqual("1.000", multimeterComponent.multimeterText.text);

        model.OhmmeterResistance = 131176f;
        multimeterComponent.DisplayResistance();
        Assert.AreEqual(OverloadText, multimeterComponent.multimeterText.text);

        model.OhmmeterResistance = -103.234f;
        multimeterComponent.DisplayResistance();
        Assert.AreEqual("-103.2", multimeterComponent.multimeterText.text);

        model.OhmmeterResistance = -1000000f;
        multimeterComponent.DisplayResistance();
        Assert.AreEqual(OverloadText, multimeterComponent.multimeterText.text);
    }

    [UnityTest]
    public IEnumerator DMM_Default_Text_Updating_Correctly_Dial()
    {
        string defaultMultimeterSettingText = "0.000";

        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.Off, string.Empty);

        //change dial
        digitalTwinManager.MultimeterDial.OnDialValueChanged(1);
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.ACVoltage, defaultMultimeterSettingText);

        //change dial
        digitalTwinManager.MultimeterDial.OnDialValueChanged(2);
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.DCVoltage, defaultMultimeterSettingText);

        //change dial
        digitalTwinManager.MultimeterDial.OnDialValueChanged(4);
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.Resistance, OverloadText);

        //change dial
        digitalTwinManager.MultimeterDial.OnDialValueChanged(3);
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.ACVoltageMillivolts, defaultMultimeterSettingText);

        //change dial
        digitalTwinManager.MultimeterDial.OnDialValueChanged(5);
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.Capacitance, defaultMultimeterSettingText);

        //change dial
        digitalTwinManager.MultimeterDial.OnDialValueChanged(6);
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.Current, defaultMultimeterSettingText);

        //change dial
        digitalTwinManager.MultimeterDial.OnDialValueChanged(4); //(check again)
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.Resistance, OverloadText);

        //change dial
        digitalTwinManager.MultimeterDial.OnDialValueChanged(2); //(check again)
        yield return null;
        VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings.DCVoltage, defaultMultimeterSettingText);
    }


    /// <summary>
    ///     Similar to the test above but instead uses MultimeterComponent.OnDial
    /// </summary>
    /// <remarks>
    ///     Expects the following mapping for the dial:
    ///     0 - Off
    ///     1 - AC Volts
    ///     2 - DC Volts
    ///     3 - AC VoltageMillivolts
    ///     4 - Resistance
    ///     5 - Capacitance
    ///     6 - Current
    /// </remarks>
    [UnityTest]
    public IEnumerator DMM_Default_Text_Updating_Model_Correctly()
    {
        string defaultMultimeterSettingText = "0.000";

        // Should start in the Off state
        Assert.AreEqual(MultimeterModel.MultimeterState.Off, multimeterComponent.Model.CurrentState);
        Assert.AreEqual(string.Empty, multimeterComponent.multimeterText.text);

        //  Change dial to AC Voltage, no model setting exists for the dial setting, but the display should still read "0.000"
        digitalTwinManager.MultimeterDial.OnDialValueChanged(1);
        yield return null;
        Assert.AreEqual(MultimeterModel.MultimeterState.Undefined, multimeterComponent.Model.CurrentState);
        Assert.AreEqual(defaultMultimeterSettingText, multimeterComponent.multimeterText.text);

        //  Change dial to DC Voltage
        digitalTwinManager.MultimeterDial.OnDialValueChanged(2);
        yield return null;
        Assert.AreEqual(MultimeterModel.MultimeterState.DCVoltage, multimeterComponent.Model.CurrentState);
        Assert.AreEqual(defaultMultimeterSettingText, multimeterComponent.multimeterText.text);

        // Change dial to AC millivolts, no model setting exists
        digitalTwinManager.MultimeterDial.OnDialValueChanged(3);
        yield return null;
        Assert.AreEqual(MultimeterModel.MultimeterState.Undefined, multimeterComponent.Model.CurrentState);
        Assert.AreEqual(defaultMultimeterSettingText, multimeterComponent.multimeterText.text);

        // Change dial to Resistance, show Overload text
        digitalTwinManager.MultimeterDial.OnDialValueChanged(4);
        yield return null;
        Assert.AreEqual(MultimeterModel.MultimeterState.Resistance, multimeterComponent.Model.CurrentState);
        Assert.AreEqual(OverloadText, multimeterComponent.multimeterText.text);

        // Change dial to capacitance, no model setting exists
        digitalTwinManager.MultimeterDial.OnDialValueChanged(5);
        yield return null;
        Assert.AreEqual(MultimeterModel.MultimeterState.Undefined, multimeterComponent.Model.CurrentState);
        Assert.AreEqual(defaultMultimeterSettingText, multimeterComponent.multimeterText.text);

        // Change dial to DC current
        digitalTwinManager.MultimeterDial.OnDialValueChanged(6);
        yield return null;
        Assert.AreEqual(MultimeterModel.MultimeterState.DCCurrent, multimeterComponent.Model.CurrentState);
        Assert.AreEqual(defaultMultimeterSettingText, multimeterComponent.multimeterText.text);

        // Turn off
        digitalTwinManager.MultimeterDial.OnDialValueChanged(0); //(check again)
        yield return null;
        Assert.AreEqual(MultimeterModel.MultimeterState.Off, multimeterComponent.Model.CurrentState);
        Assert.AreEqual(string.Empty, multimeterComponent.multimeterText.text);

        // Back to resistance
        digitalTwinManager.MultimeterDial.OnDialValueChanged(4); //(check again)
        yield return null;
        Assert.AreEqual(MultimeterModel.MultimeterState.Resistance, multimeterComponent.Model.CurrentState);
        Assert.AreEqual(OverloadText, multimeterComponent.multimeterText.text);

        // Back to DC Voltage
        digitalTwinManager.MultimeterDial.OnDialValueChanged(2); //(check again)
        yield return null;
        Assert.AreEqual(MultimeterModel.MultimeterState.DCVoltage, multimeterComponent.Model.CurrentState);
        Assert.AreEqual(defaultMultimeterSettingText, multimeterComponent.multimeterText.text);
    }

    // Helper methods for easier readability
    private void VerifyDivisionSettingAndMultimeterText(MultimeterDialSettings expectedDivisionName, string expectedDmmText)
    {
        Assert.AreEqual(expectedDivisionName, multimeterComponent.multimeterDial.CurrentDivision.setting);
        Assert.AreEqual(expectedDmmText, multimeterComponent.multimeterText.text);
    }

    private void VerifySmokeAnimation_IsOn()
    {
        Assert.IsTrue(multimeterComponent.Animator.GetBool(multimeterComponent.AnimationSmokeKey));
    }

    private void VerifySmokeAnimation_IsOff()
    {
        Assert.IsFalse(multimeterComponent.Animator.GetBool(multimeterComponent.AnimationSmokeKey));
    }

    private void SetMultimeterDialToAmps()
    {
        digitalTwinManager.MultimeterDial.OnDialValueChanged(6);
    }

    private void SetMultimeterDialToVoltsDC()
    {
        digitalTwinManager.MultimeterDial.OnDialValueChanged(2);
    }

    private void MoveDMMVoltageLeadFromOldPortToNewPort(string cable, CircuitBoard.PortNames oldPort, CircuitBoard.PortNames newPort)
    {
        digitalTwinManager.CircuitBoard.RemoveCable(cable);
        digitalTwinManager.CircuitBoard.PlaceCable(cable, CircuitBoard.PortNames.DMM_Voltage, newPort);
        digitalTwinManager.CircuitBoard.SolveCircuit();
    }
}