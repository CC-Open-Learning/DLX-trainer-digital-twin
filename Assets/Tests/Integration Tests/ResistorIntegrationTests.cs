using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;
using VARLab.MPCircuits.Model;

public class ResistorIntegrationTests : MPCIntegrationTestsSetUpHelper
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

    /// <summary>
    ///     Ensures all <see cref="ResistorModel"/> based components are set to be
    ///     Connected on start
    /// </summary>
    [UnityTest]
    public IEnumerator Resistor_Components_Connected()
    {
        Assert.IsTrue(digitalTwinManager.CircuitBoard.R1.IsConnected);
        Assert.IsTrue(digitalTwinManager.CircuitBoard.R2.IsConnected);
        Assert.IsTrue(digitalTwinManager.CircuitBoard.R3.IsConnected);
        Assert.IsTrue(digitalTwinManager.CircuitBoard.R4.IsConnected);
        Assert.IsTrue(digitalTwinManager.CircuitBoard.R5.IsConnected);

        // Lights
        Assert.IsTrue(digitalTwinManager.CircuitBoard.L1.IsConnected);
        Assert.IsTrue(digitalTwinManager.CircuitBoard.L2.IsConnected);
        Assert.IsTrue(digitalTwinManager.CircuitBoard.L3Low.IsConnected);
        Assert.IsTrue(digitalTwinManager.CircuitBoard.L3High.IsConnected);

        yield return null;
    }
}
