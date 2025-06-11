using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;
using VARLab.MPCircuits.Model;

public class LightBulbIntegrationTests : MPCIntegrationTestsSetUpHelper
{
    // variables ---------------------------------------------------------------
    private DigitalTwinManager digitalTwinManager;

    // set up/ tear down --------------------------------------------------------
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        IntegrationTestHelper.ClearScene();
        yield return null;

        digitalTwinManager = SetUpDigitalTwinManager();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        yield return null;
        IntegrationTestHelper.ClearScene();
    }

    // tests --------------------------------------------------------------------
    [UnityTest]
    public IEnumerator Light_Turns_On_When_Circuit_Complete()
    {
        //light off at start
        Assert.AreEqual(0, digitalTwinManager.L1.PointLight.intensity);
        yield return null;

        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 10f;
        digitalTwinManager.CircuitBoard.PlaceCable("cable1", CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.L1_A, false);
        digitalTwinManager.CircuitBoard.PlaceCable("cable2", CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.L1_B, false);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        //light on
        Assert.AreNotEqual(0, digitalTwinManager.L1.PointLight.intensity);
    }
}
