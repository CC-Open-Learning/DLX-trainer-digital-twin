using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;

public class POTIntegrationTests : MPCIntegrationTestsSetUpHelper
{
    private DigitalTwinManager digitalTwinManager;

    [SetUp]
    public void SetUp()
    {
        IntegrationTestHelper.ClearScene();
        SetUpInteractionManager();
    }

    [TearDown]
    public void TearDown()
    {
        IntegrationTestHelper.ClearScene();
    }


    [UnityTest]
    public IEnumerator SetPOTResistance_Verify_CircuitBoard_Is_Updated()
    {
        digitalTwinManager = SetUpDigitalTwinManager();

        yield return null;

        // GH and HI resistance should equal up to 75 (max value of potentiometer)
        float potResistance = digitalTwinManager.CircuitBoard.POT.GH_Resistance + digitalTwinManager.CircuitBoard.POT.HI_Resistance;
        Assert.AreEqual(75, potResistance);

        yield return null;
    }
}
