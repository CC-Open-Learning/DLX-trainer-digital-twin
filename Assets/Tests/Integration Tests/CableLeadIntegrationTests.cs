using EPOOutline;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;

public class CableLeadIntegrationTests : CableIntegrationTestsSetup
{
    CableConnector cableConnector;

    CableLead leadStart;
    CableLead leadEnd;

    CableLead lead2Start;
    CableLead lead2End;

    PortBehaviour portStart;
    PortBehaviour portEnd;

    LineRenderer line;
    Outlinable startOutline;

    [SetUp]
    public void SetUp()
    {
        IntegrationTestHelper.ClearScene();

        leadStart = SetUpCableLead();
        leadEnd = SetUpCableLead();
        lead2Start = SetUpCableLead();
        lead2End = SetUpCableLead();
        cableConnector = SetUpCableConnector(ref leadStart, ref leadEnd, ref line);

        portStart = SetUpPort();
        portEnd = SetUpPort();

        startOutline = leadStart.GetComponent<Outlinable>();
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

        leadStart.DisconnectPort();
        leadEnd.DisconnectPort();
        yield return null;

        Assert.AreEqual(0, portStart.NumberLeadsConnected);
        Assert.AreEqual(0, portEnd.NumberLeadsConnected);

        leadStart.DisconnectPort();
        yield return null;

        Assert.IsNull(leadStart.ConnectedPort);
    }

    [UnityTest]
    public IEnumerator Stack_Leads_Successfully()
    {
        Assert.AreEqual(0, portStart.NumberLeadsConnected);
        Assert.AreEqual(0, portEnd.NumberLeadsConnected);

        leadStart.ConnectPort(portStart);
        leadEnd.ConnectPort(portEnd);
        lead2Start.ConnectPort(portStart);
        lead2End.ConnectPort(portEnd);
        yield return null;

        Assert.AreEqual(2, portStart.NumberLeadsConnected);
        Assert.AreEqual(2, portEnd.NumberLeadsConnected);
    }

    [UnityTest]
    public IEnumerator OnHover_Enable_Disable_Outline()
    {
        leadStart.EnableOutline(true);
        int expectedOutlineLayer = 0;
        yield return null;

        Assert.AreEqual(startOutline.OutlineLayer, expectedOutlineLayer);

        leadStart.EnableOutline(false);
        expectedOutlineLayer = 63;

        yield return null;

        Assert.AreEqual(startOutline.OutlineLayer, expectedOutlineLayer);
    }

    [UnityTest]
    public IEnumerator Enable_Disable_Selection()
    {
        leadStart.EnableSelection(true);
        int expectedOutlineLayer = 0;

        yield return null;

        Assert.IsTrue(leadStart.GetComponent<Collider>().enabled);
        Assert.AreEqual(expectedOutlineLayer, startOutline.OutlineLayer);

        expectedOutlineLayer = 63;

        leadStart.EnableSelection(false);

        yield return null;

        Assert.IsFalse(leadStart.GetComponent<Collider>().enabled);
        Assert.AreEqual(expectedOutlineLayer, startOutline.OutlineLayer);
    }

    [UnityTest]
    public IEnumerator GetOtherLead_Returns_Correct_CableConnector()
    {
        yield return null;

        Assert.AreEqual(leadStart.GetOtherLead(), leadEnd);
        Assert.AreEqual(leadStart.Parent(), cableConnector);
    }
}
