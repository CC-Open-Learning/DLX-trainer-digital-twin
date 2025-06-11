using EPOOutline;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;

public class CableControlsIntegrationTests : CableIntegrationTestsSetup
{
    CableControls cableControls;
    CableConnector cableConnector;

    CableLead leadStart;
    CableLead leadEnd;

    CableLead lead2Start;
    CableLead lead2End;

    CableLead selectedLead;

    PortBehaviour portStart;
    PortBehaviour portEnd;
    PortBehaviour portConnectedTo;

    LineRenderer line;
    LineRenderer line2;

    [SetUp]
    public void SetUp()
    {
        IntegrationTestHelper.ClearScene();

        leadStart = SetUpCableLead();
        leadEnd = SetUpCableLead();

        lead2Start = SetUpCableLead();
        lead2End = SetUpCableLead();

        selectedLead = SetUpCableLead();

        portStart = SetUpPort("portStart");
        portEnd = SetUpPort("portEnd");
        portConnectedTo = SetUpPort("portConnectedTo");

        cableControls = SetUpCableControl();
        cableControls.CurrentlySelectedTransparentCableLead = SetUpTransparentLead();
        cableControls.currentlySelectedCable = SetUpCableConnector(ref leadStart, ref leadEnd, ref line);
        cableControls.currentlySelectedCable.InitLineRenderer();
        cableControls.currentlySelectedLead = selectedLead;

        cableConnector = SetUpCableConnector(ref lead2Start, ref lead2End, ref line2);
        cableConnector.InitLineRenderer();
    }

    [TearDown]
    public void TearDown()
    {
        IntegrationTestHelper.ClearScene();
    }

    [UnityTest]
    public IEnumerator RemoveTransparentLeadAfterObjectivePanelReset()
    {
        Assert.AreEqual(true, cableControls.CurrentlySelectedTransparentCableLead.activeInHierarchy);
        yield return null;

        cableControls.RemoveAllCables(); //listener method for objective panel reset
        yield return null;

        Assert.AreEqual(false, cableControls.CurrentlySelectedTransparentCableLead.activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator Connect_Disconnect_A_Cable_Successfully()
    {
        Assert.AreEqual(0, portStart.NumberLeadsConnected);
        Assert.AreEqual(0, portEnd.NumberLeadsConnected);

        leadStart.ConnectPort(portStart);
        leadEnd.ConnectPort(portEnd);
        yield return null;

        Assert.AreEqual(1, portStart.NumberLeadsConnected);
        Assert.AreEqual(1, portEnd.NumberLeadsConnected);

        leadStart.DisconnectPort();
        leadEnd.DisconnectPort();
        yield return null;

        Assert.AreEqual(0, portStart.NumberLeadsConnected);
        Assert.AreEqual(0, portEnd.NumberLeadsConnected);
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
    public IEnumerator Check_Lead_Rotation()
    {
        leadStart.transform.position = new Vector3(5, 11, 6);
        leadEnd.transform.position = new Vector3(0, 11, 5);

        // Checking to see if the position of the currently selected cable is the same as the lead start and lead end
        Assert.AreEqual(leadStart.transform.position, cableControls.currentlySelectedCable.cableStart.transform.position);
        Assert.AreEqual(leadEnd.transform.position, cableControls.currentlySelectedCable.cableEnd.transform.position);

        yield return null;

        Vector3 target = leadStart.transform.position - leadEnd.transform.position;

        var lookTowardsStart = Quaternion.LookRotation(-target);
        var lookTowardsEnd = Quaternion.LookRotation(target);

        //Checking to see if the angles are updated correctly on the currently selected cable by redoing the calculation
        Assert.AreEqual(lookTowardsEnd.eulerAngles, cableControls.currentlySelectedCable.cableStart.transform.eulerAngles);
        Assert.AreEqual(lookTowardsStart.eulerAngles, cableControls.currentlySelectedCable.cableEnd.transform.eulerAngles);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Check_Hover_Collider_Position()
    {
        lead2Start.transform.position = new Vector3(5, 11, 6);
        lead2End.transform.position = new Vector3(0, 11, 5);

        cableConnector.InitLineRenderer();
        cableConnector.CreateMesh();

        Vector3 distanceBetween = (cableConnector.cableStart.transform.position - cableConnector.cableEnd.transform.position);
        Vector3 middlePosition = cableConnector.endPoint.position + (cableConnector.startPoint.position - cableConnector.endPoint.position) / 2;

        // Checking to see if the distance between the lead start and end is correct and that the middle position is correctly calculated to be
        // the middle of the capsule collider
        Assert.AreEqual(Vector3Int.RoundToInt(distanceBetween.normalized), Vector3Int.RoundToInt(cableConnector.capsuleCollider.gameObject.transform.forward.normalized));
        Assert.AreEqual(middlePosition, cableConnector.capsuleCollider.gameObject.transform.position);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Verify_OnPortHoverOff()
    {
        cableControls.OnPortHoverOff(portStart);
        yield return null;

        Assert.AreEqual(HighlightInteractions.HiddenLayer, portStart.gameObject.GetComponent<Outlinable>().OutlineLayer);
    }

    [UnityTest]
    public IEnumerator Verify_OnPortHover()
    {
        // For setting maxCableNum
        cableControls.CanCreateCables(1);

        // To set currently selected lead to null
        cableControls.currentlySelectedLead = null;

        // For setting currentlySelectedColor to black
        cableControls.OnCableBundleClicked(true);
        yield return null;

        cableControls.OnPortHover(portStart);
        yield return null;

        Assert.AreEqual(HighlightInteractions.VisibleLayer, portStart.gameObject.GetComponent<Outlinable>().OutlineLayer);
    }

    [UnityTest]
    public IEnumerator Verify_OnLeadHovered()
    {
        // For setting maxCableNum
        cableControls.CanCreateCables(1);

        // To set currently selected lead to null
        cableControls.currentlySelectedLead = null;

        // For setting currentlySelectedColor to black
        cableControls.OnCableBundleClicked(true);

        yield return null;

        cableControls.currentlySelectedLead = selectedLead;

        cableControls.currentlySelectedLead.ConnectPort(portConnectedTo);

        cableControls.OnLeadHovered(cableControls.currentlySelectedLead);

        yield return null;

        Assert.AreEqual(HighlightInteractions.VisibleLayer, cableControls.currentlySelectedLead.ConnectedPort.gameObject.GetComponent<Outlinable>().OutlineLayer);
    }

    [UnityTest]
    public IEnumerator Verify_OnLeadHoverOff()
    {
        cableControls.currentlySelectedLead.ConnectPort(portConnectedTo);
        cableControls.OnLeadHoveredOff(cableControls.currentlySelectedLead);
        yield return null;

        Assert.AreEqual(HighlightInteractions.HiddenLayer, cableControls.currentlySelectedLead.ConnectedPort.gameObject.GetComponent<Outlinable>().OutlineLayer);
    }

    [UnityTest]
    public IEnumerator Verify_OnCableHovered_On()
    {
        cableControls.currentlySelectedCable = null;

        // Set up a cable connector
        lead2Start.transform.position = new Vector3(5, 11, 6);
        lead2End.transform.position = new Vector3(0, 11, 5);
        cableConnector.InitLineRenderer();
        cableConnector.CreateMesh();

        // To set Cursor.visible to true
        cableControls.EnableTransparentLeadByType();

        cableControls.OnCableHovered(cableConnector);
        yield return null;

        Assert.AreEqual(HighlightInteractions.VisibleLayer, cableConnector.gameObject.GetComponent<Outlinable>().OutlineLayer);
        Assert.IsTrue(cableConnector.deleteIcon.gameObject.activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator Verify_OnCableHovered_Off()
    {
        cableControls.currentlySelectedCable = null;

        // Set up a cable connector
        lead2Start.transform.position = new Vector3(5, 11, 6);
        lead2End.transform.position = new Vector3(0, 11, 5);
        cableConnector.InitLineRenderer();
        cableConnector.CreateMesh();

        // To set maxCableNum
        cableControls.CanCreateCables(1);

        // To set currently selected lead to null
        cableControls.currentlySelectedLead = null;

        // To set currentlySelectedColor to black
        cableControls.OnCableBundleClicked(true);

        // To set Cursor.visible to false
        cableControls.EnableTransparentLeadByType();

        cableControls.OnCableHovered(cableConnector);
        yield return null;

        Assert.AreEqual(HighlightInteractions.HiddenLayer, cableConnector.gameObject.GetComponent<Outlinable>().OutlineLayer);
        Assert.IsFalse(cableConnector.deleteIcon.gameObject.activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator Verify_OnCableHoveredOff()
    {
        cableControls.currentlySelectedCable = null;

        // Set up a cable connector
        lead2Start.transform.position = new Vector3(5, 11, 6);
        lead2End.transform.position = new Vector3(0, 11, 5);
        cableConnector.InitLineRenderer();
        cableConnector.CreateMesh();

        // To set maxCableNum
        cableControls.CanCreateCables(1);

        // To set currently selected lead to null
        cableControls.currentlySelectedLead = null;

        // To set currentlySelectedColor to black
        cableControls.OnCableBundleClicked(true);

        // To set Cursor.visible to false
        cableControls.EnableTransparentLeadByType();

        cableControls.OnCableHoveredOff(cableConnector);
        yield return null;

        Assert.AreEqual(HighlightInteractions.HiddenLayer, cableConnector.gameObject.GetComponent<Outlinable>().OutlineLayer);
        Assert.IsFalse(cableConnector.deleteIcon.gameObject.activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator Verify_OnLeadClicked_to_DisconnectAndSelectLead()
    {
        leadStart.ConnectPort(portStart);

        cableControls.currentlySelectedCable = null;

        cableControls.OnLeadClicked(leadStart);
        yield return null;

        // DisconnectAndSelectLead() gets called
        Assert.AreEqual(false, Cursor.visible);
    }

    [UnityTest]
    public IEnumerator Verify_OnLeadClicked_to_FinishCableConnection()
    {
        leadStart.ConnectPort(portStart);
        leadEnd.ConnectPort(portEnd);

        // Set up Parent()
        GameObject cableConnectorGO = new("Cable Connector 2");
        cableConnectorGO.SetActive(false);

        CableConnector _cableConnector = cableConnectorGO.AddComponent<CableConnector>();
        cableControls.currentlySelectedLead.transform.parent = _cableConnector.transform;
        cableControls.currentlySelectedLead.Parent().cableStart = lead2Start;
        cableControls.currentlySelectedLead.Parent().cableEnd = lead2End;
        cableControls.currentlySelectedLead.Parent().deleteIcon = SetUpInteractable();
        cableControls.currentlySelectedLead.Parent().deleteIcon.MouseClick = new();
        cableControls.currentlySelectedLead.Parent().startPoint = leadStart.transform;
        cableControls.currentlySelectedLead.Parent().endPoint = leadEnd.transform;
        cableControls.currentlySelectedLead.Parent().gameObject.AddComponent<LineRenderer>();
        cableControls.currentlySelectedLead.Parent().InitLineRenderer();
        cableControls.currentlySelectedLead.GetOtherLead().ConnectPort(portConnectedTo);
        cableControls.currentlySelectedLead.Parent().points = new CablePhysics[5] {
            SetUpCablePhysics(),
            SetUpCablePhysics(),
            SetUpCablePhysics(),
            SetUpCablePhysics(),
            SetUpCablePhysics()};

        cableControls.currentlySelectedCable.deleteIcon = SetUpInteractable();
        cableControls.currentlySelectedCable.deleteIcon.MouseClick = new();
        cableControls.currentlySelectedCable.InitLineRenderer();

        cableConnectorGO.SetActive(true);

        // Function called
        cableControls.OnLeadClicked(leadStart);
        yield return null;

        // OnPortClick() gets called, then FinishCableConnection()
        Assert.IsTrue(leadStart.GetComponent<Collider>().enabled);
        Assert.IsNull(cableControls.currentlySelectedCable);
        Assert.IsNull(cableControls.currentlySelectedLead);
    }

    [UnityTest]
    public IEnumerator Verify_OnDeleteIconClicked_BlackCable()
    {
        // Set up a cable connector
        lead2Start.transform.position = new Vector3(5, 11, 6);
        lead2End.transform.position = new Vector3(0, 11, 5);
        cableConnector.InitLineRenderer();
        cableConnector.CreateMesh();

        // To set maxCableNum
        cableControls.CanCreateCables(1);

        // To set currently selected lead to null
        cableControls.currentlySelectedLead = null;

        // To set currentlySelectedColor to black
        cableControls.OnCableBundleClicked(true);

        // Function called
        cableControls.OnDeleteIconClicked(cableConnector);
        yield return null;

        Assert.AreEqual(HighlightInteractions.VisibleLayer, cableControls.blackCableBundle.GetComponent<Outlinable>().OutlineLayer);
        Assert.IsTrue(cableControls.transparentBlackCableLeadPrefab.activeInHierarchy);
        Assert.IsFalse(Cursor.visible);
        Assert.IsTrue(cableConnector == null);
    }

    [UnityTest]
    public IEnumerator Verify_OnDeleteIconClicked_RedCable()
    {
        // Set up a cable connector
        lead2Start.transform.position = new Vector3(5, 11, 6);
        lead2End.transform.position = new Vector3(0, 11, 5);
        cableConnector.InitLineRenderer();
        cableConnector.CreateMesh();

        // To set maxCableNum
        cableControls.CanCreateCables(1);

        // To set currently selected lead to null
        cableControls.currentlySelectedLead = null;

        // To set currentlySelectedColor to red
        cableControls.OnCableBundleClicked(false);

        // Function called
        cableControls.OnDeleteIconClicked(cableConnector);
        yield return null;

        Assert.AreEqual(HighlightInteractions.VisibleLayer, cableControls.redCableBundle.GetComponent<Outlinable>().OutlineLayer);
        Assert.IsTrue(cableControls.transparentRedCableLeadPrefab.activeInHierarchy);
        Assert.IsFalse(Cursor.visible);
        Assert.IsTrue(cableConnector == null);
    }

    [UnityTest]
    public IEnumerator Verify_ResetCableBundleState()
    {
        // To set maxCableNum
        cableControls.CanCreateCables(1);

        // To set currently selected lead to null
        cableControls.currentlySelectedLead = null;

        // To set currentlySelectedColor to red
        cableControls.OnCableBundleClicked(false);

        // Function called
        cableControls.ResetCableBundleState();
        yield return null;

        Assert.AreEqual(HighlightInteractions.HiddenLayer, cableControls.blackCableBundle.GetComponent<Outlinable>().OutlineLayer);
        Assert.AreEqual(HighlightInteractions.HiddenLayer, cableControls.redCableBundle.GetComponent<Outlinable>().OutlineLayer);
    }

    [UnityTest]
    public IEnumerator Verify_EnableSelectedCableBundleOutline()
    {
        // To set maxCableNum
        cableControls.CanCreateCables(1);

        // To set currently selected lead to null
        cableControls.currentlySelectedLead = null;

        // To set currentlySelectedColor to black
        cableControls.OnCableBundleClicked(true);

        // Function called
        cableControls.EnableSelectedCableBundleOutline(true);
        yield return null;

        Assert.AreEqual(HighlightInteractions.VisibleLayer, cableControls.blackCableBundle.GetComponent<Outlinable>().OutlineLayer);
        Assert.AreEqual(HighlightInteractions.HiddenLayer, cableControls.redCableBundle.GetComponent<Outlinable>().OutlineLayer);

        // To set currentlySelectedColor to red
        cableControls.OnCableBundleClicked(false);

        // Function called
        cableControls.EnableSelectedCableBundleOutline(true);
        yield return null;

        Assert.AreEqual(HighlightInteractions.HiddenLayer, cableControls.blackCableBundle.GetComponent<Outlinable>().OutlineLayer);
        Assert.AreEqual(HighlightInteractions.VisibleLayer, cableControls.redCableBundle.GetComponent<Outlinable>().OutlineLayer);
    }
}
