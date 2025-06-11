using System.Collections;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits.Model;
using VARLab.MPCircuits;
using NUnit.Framework;
using System;

public class DebugPanelTests : MPCIntegrationTestsSetUpHelper
{
    // variables ---------------------------------------------------------------
    DebugPanel debugPanel;


    // set up/ tear down --------------------------------------------------------
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        IntegrationTestHelper.ClearScene();
        yield return null;

        debugPanel = SetUpDebugPanel();
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        yield return null;
        IntegrationTestHelper.ClearScene();
    }


    // tests --------------------------------------------------------------------
    [UnityTest]
    public IEnumerator Unity_Port_Object_Display_Values_Match_CircuitBoard_Port_Values()
    {
        string replaceNaNwithDash = "<color=#ED2939>-</color>";
        yield return null;

        //port voltages should be dashes at the start in the debug panel instead of NaN
        Assert.AreEqual(double.NaN, debugPanel.DigitalTwinManager.CircuitBoard.GetVoltage("A")); //testing 1 to verify circuitsolver port

        Assert.AreEqual(replaceNaNwithDash, debugPanel.GetPortVoltage(debugPanel.L1_A));
        Assert.AreEqual(replaceNaNwithDash, debugPanel.GetPortVoltage(debugPanel.L1_B));
        Assert.AreEqual(replaceNaNwithDash, debugPanel.GetPortVoltage(debugPanel.L2_C));
        Assert.AreEqual(replaceNaNwithDash, debugPanel.GetPortVoltage(debugPanel.L2_D));
        Assert.AreEqual(replaceNaNwithDash, debugPanel.GetPortVoltage(debugPanel.F1_Left));
        Assert.AreEqual(replaceNaNwithDash, debugPanel.GetPortVoltage(debugPanel.F1_Right));
        Assert.AreEqual(replaceNaNwithDash, debugPanel.GetPortVoltage(debugPanel.SW1_Left));
        Assert.AreEqual(replaceNaNwithDash, debugPanel.GetPortVoltage(debugPanel.SW1_Right));

        //simulate series circuit with 2 lightbulbs
        debugPanel.DigitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;
        debugPanel.DigitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.F1_Left);
        debugPanel.DigitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.F1_Right, CircuitBoard.PortNames.SW1_Left);
        debugPanel.DigitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.SW1_Right, CircuitBoard.PortNames.L1_A);
        debugPanel.DigitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.L1_B, CircuitBoard.PortNames.L2_C);
        debugPanel.DigitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.L2_D, CircuitBoard.PortNames.Battery_Gnd);
        debugPanel.DigitalTwinManager.CircuitBoard.SW1.IsConnected = true;
        debugPanel.DigitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        //circuit solver port values
        Assert.AreEqual(9.99, Math.Round(debugPanel.DigitalTwinManager.CircuitBoard.GetVoltage("F1_0")), 2);
        Assert.AreEqual(9.99, Math.Round(debugPanel.DigitalTwinManager.CircuitBoard.GetVoltage("F1_1")), 2);
        Assert.AreEqual(9.99, Math.Round(debugPanel.DigitalTwinManager.CircuitBoard.GetVoltage("SW1_0")), 2);
        Assert.AreEqual(9.99, Math.Round(debugPanel.DigitalTwinManager.CircuitBoard.GetVoltage("SW1_1")), 2);
        Assert.AreEqual(9.99, Math.Round(debugPanel.DigitalTwinManager.CircuitBoard.GetVoltage("A")), 2);
        Assert.AreEqual(4.99, Math.Round(debugPanel.DigitalTwinManager.CircuitBoard.GetVoltage("B")), 2);
        Assert.AreEqual(4.99, Math.Round(debugPanel.DigitalTwinManager.CircuitBoard.GetVoltage("C")), 2);
        Assert.AreEqual(0, Math.Round(debugPanel.DigitalTwinManager.CircuitBoard.GetVoltage("D")), 2);

        //the GetPortVoltage method should return a rounded string with the correct voltage value for the board ports
        Assert.AreEqual("10", debugPanel.GetPortVoltage(debugPanel.F1_Left));
        Assert.AreEqual("10", debugPanel.GetPortVoltage(debugPanel.F1_Right));
        Assert.AreEqual("10", debugPanel.GetPortVoltage(debugPanel.SW1_Left));
        Assert.AreEqual("10", debugPanel.GetPortVoltage(debugPanel.SW1_Right));
        Assert.AreEqual("10", debugPanel.GetPortVoltage(debugPanel.L1_A));
        Assert.AreEqual("5", debugPanel.GetPortVoltage(debugPanel.L1_B));
        Assert.AreEqual("5", debugPanel.GetPortVoltage(debugPanel.L2_C));
        Assert.AreEqual("0", debugPanel.GetPortVoltage(debugPanel.L2_D));

        yield return null;
    }
}
