using NUnit.Framework;
using System.Collections;
using EPOOutline;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;
using UnityEngine;

public class CableConnectorIntegrationTests : CableIntegrationTestsSetup
{
    CableConnector cableConnector;

    CableLead leadStart;
    CableLead leadEnd;

    PortBehaviour portStart;
    PortBehaviour portEnd;

    LineRenderer line;
    Outlinable startOutline;

    [SetUp]
    public void SetUp()
    {
        IntegrationTestHelper.ClearScene();

        portStart = SetUpPort();
        portEnd = SetUpPort();

        leadStart = SetUpCableLead();
        leadEnd = SetUpCableLead();
        cableConnector = SetUpCableConnector(ref leadStart, ref leadEnd, ref line);
        cableConnector.deleteIcon = cableConnector.gameObject.AddComponent<VARLab.Interactions.Interactable>();
        cableConnector.deleteIcon.MouseClick = new UnityEngine.Events.UnityEvent<GameObject>();
    }

    [TearDown]
    public void TearDown()
    {
        IntegrationTestHelper.ClearScene();
    }

    [UnityTest]
    public IEnumerator Connect_Disconnect_A_Cable_Successfully()
    {
        Assert.AreEqual(0, portStart.NumberLeadsConnected);
        Assert.AreEqual(0, portEnd.NumberLeadsConnected);
        Assert.IsNull(leadStart.ConnectedPort);

        leadStart.ConnectPort(portStart);
        leadEnd.ConnectPort(portEnd);
        yield return null;

        Assert.AreEqual(1, portStart.NumberLeadsConnected);
        Assert.AreEqual(1, portEnd.NumberLeadsConnected);
        Assert.IsNotNull(leadStart.ConnectedPort);


        cableConnector.Disconnect();
        yield return null;

        Assert.AreEqual(0, portStart.NumberLeadsConnected);
        Assert.AreEqual(0, portEnd.NumberLeadsConnected);

        Assert.IsNull(leadStart.ConnectedPort);
        Assert.IsNull(leadEnd.ConnectedPort);

        Assert.IsTrue(cableConnector == null);

        yield return null;
        
    }

    [UnityTest]
    public IEnumerator HasEnoughCablePhysicsParticles()
    {
        cableConnector.InitCableParticles();
        yield return null;

        //we have the expected number of segments
        Assert.AreEqual(cableConnector.segments + 1, cableConnector.points.Length);

        //points on both ends are bound
        Assert.IsTrue(cableConnector.points[0].IsBound());
        Assert.IsTrue(cableConnector.points[cableConnector.segments].IsBound());

        //points in between are free
        bool pointsAreFree = true;
        for (int i = 1; i < cableConnector.segments; i++)
        {
            if (cableConnector.points[i].IsBound())
                pointsAreFree = false;
        }

        Assert.IsTrue(pointsAreFree);
    }
}
