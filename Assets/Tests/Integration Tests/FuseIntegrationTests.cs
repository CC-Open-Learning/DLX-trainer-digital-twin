using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;
using VARLab.MPCircuits.Model;

public class FuseIntegrationTests : MPCIntegrationTestsSetUpHelper
{
    private FuseComponent fuse;
    private DigitalTwinManager digitalTwinManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        IntegrationTestHelper.ClearScene();
        yield return null;
        digitalTwinManager = SetUpDigitalTwinManager();
        yield return null;
        fuse = SetUpFuseComponent(digitalTwinManager);
    }

    [UnityTest]
    public IEnumerator OnAwake_FuseModel_Updated_Correctly()
    {
        // Check to see if the fuse model is set to the circuit board F1 variable
        Assert.AreEqual(digitalTwinManager.CircuitBoard.F1, fuse.fuseModel);

        yield return null;

        // This should be false at the start as IsDefective is set to false at the start
        Assert.AreEqual(false, fuse.fuseModel.IsDefective);

        yield return null;

        // Make sure the action "OnValuesUpdated" is set in the Awake method
        Assert.AreEqual("CheckFuseStatus", fuse.fuseModel.OnValuesUpdated.Method.Name);

        yield return null;

        // Call awake again to see if fuse model variable "DefectiveFusse" is set to true when "IsDefective" is set to true
        fuse.IsDefective = true;

        fuse.Awake();

        Assert.AreEqual(true, fuse.fuseModel.IsDefective);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Invoke_Action_Check_Fuse_Status()
    {
        // Invoke "OnValuesUpdated to check the status of the fuse when the fuse is not short circuited and short circuited
        fuse.fuseModel.OnValuesUpdated.Invoke(fuse.fuseModel);

        Assert.AreEqual(false, fuse.FuseIsBlown);

        yield return null;

        // Setting board voltage to two so that F1 blows as it is connected to both battery + and battery -
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 2;
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V,
            CircuitBoard.PortNames.F1_Right);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_Gnd,
            CircuitBoard.PortNames.F1_Left);
        digitalTwinManager.CircuitBoard.SolveCircuit();

        yield return null;

        Assert.AreEqual(true, digitalTwinManager.F1.FuseIsBlown);

        // Setting the board voltage to fourteen so that R5 does break the fuse when trying to short circuit at a higher voltage
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 14f;
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V,
            CircuitBoard.PortNames.F1_Left);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.F1_Right,
            CircuitBoard.PortNames.R5_S);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.R5_R,
            CircuitBoard.PortNames.Battery_Gnd);
        digitalTwinManager.CircuitBoard.SolveCircuit();

        yield return null;

        Assert.AreEqual(true, digitalTwinManager.F1.FuseIsBlown);
    }

    [UnityTest]
    public IEnumerator OnClick_Fuse_Is_Removed_And_Added()
    {
        // Check to see if removing the non-defective fuse sets the correct booleans
        fuse.UpdateFuseState();

        Assert.AreEqual(true, fuse.IsFuseRemoved);
        Assert.AreEqual(false, fuse.IsDefective);

        yield return null;

        // Check to see if adding back the fuse sets the correct boolean
        fuse.UpdateFuseState();

        Assert.AreEqual(false, fuse.IsFuseRemoved);

        yield return null;

        // Check to see if removing the defective fuse sets the correct booleans
        fuse.IsDefective = true;

        fuse.UpdateFuseState();

        Assert.AreEqual(true, fuse.IsFuseRemoved);
        Assert.AreEqual(false, fuse.IsDefective);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator Teardown()
    {
        Object.Destroy(Object.FindObjectOfType<DigitalTwinManager>());
        Object.Destroy(Object.FindObjectOfType<Singleton>());

        IntegrationTestHelper.ClearScene();
        yield return null;
    }
}
