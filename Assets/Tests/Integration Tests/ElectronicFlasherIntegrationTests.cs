using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;

public class ElectronicFlasherIntegrationTests : MPCIntegrationTestsSetUpHelper
{
    ElectronicFlasherComponent electronicFlasher;
    DigitalTwinManager digitalTwinManager;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        IntegrationTestHelper.ClearScene();

        digitalTwinManager = SetUpDigitalTwinManager();
        electronicFlasher = SetUpElectronicFlasherComponent(digitalTwinManager);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Verify_Start_Flasher_Values()
    {
        // Verify that variables are being set correctly on start
        Assert.IsNotNull(electronicFlasher.ElectronicFlasherModel);
        Assert.IsNotNull(electronicFlasher.ElectronicFlasherModel.OnValuesUpdated);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Verify_On_Port_Value_Changed_Flasher_Values()
    {
        //_isActuated should be set by default to false
        Assert.AreEqual(false, electronicFlasher.IsCoroutineRunning);

        // Invoke the capacitor model connected to the flasher
        electronicFlasher.ElectronicFlasherModel.OnValuesUpdated?.Invoke(electronicFlasher.ElectronicFlasherModel);

        //_isActuated should be set to false as well when out of the coroutine
        Assert.AreEqual(false, electronicFlasher.IsCoroutineRunning);

        yield return null;

        // Now we connect the ports to the flasher and set it higher than the default threshold of 8.80 V
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 8.80f;
        digitalTwinManager.CircuitBoard.PlaceCable(VARLab.MPCircuits.Model.CircuitBoard.PortNames.Battery_V, VARLab.MPCircuits.Model.CircuitBoard.PortNames.EF_Bat);
        digitalTwinManager.CircuitBoard.PlaceCable(VARLab.MPCircuits.Model.CircuitBoard.PortNames.Battery_Gnd, VARLab.MPCircuits.Model.CircuitBoard.PortNames.EF_Gnd);
        digitalTwinManager.CircuitBoard.PlaceCable(VARLab.MPCircuits.Model.CircuitBoard.PortNames.EF_Sig, VARLab.MPCircuits.Model.CircuitBoard.PortNames.L1_A);
        digitalTwinManager.CircuitBoard.PlaceCable(VARLab.MPCircuits.Model.CircuitBoard.PortNames.L1_B, VARLab.MPCircuits.Model.CircuitBoard.PortNames.Battery_Gnd);
        digitalTwinManager.CircuitBoard.SolveCircuit();

        yield return null;

        electronicFlasher.ElectronicFlasherModel.OnValuesUpdated?.Invoke(electronicFlasher.ElectronicFlasherModel);

        // audio source volume should be set to 1.0
        Assert.AreEqual(1.0f, electronicFlasher.AudioSource.volume);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        GameObject.Destroy(GameObject.FindObjectOfType<DigitalTwinManager>());

        IntegrationTestHelper.ClearScene();

        yield return null;
    }
}
