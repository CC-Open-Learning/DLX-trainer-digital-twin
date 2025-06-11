using NUnit.Framework;
using VARLab.MPCircuits;
using VARLab.MPCircuits.Model;

public class CircuitImporterTest : MPCIntegrationTestsSetUpHelper
{
    CircuitImporter importer;
    CircuitImporter.Cable cable;

    [SetUp]
    public void SetUp()
    {
        importer = SetUpCircuitImporter();

        cable = new CircuitImporter.Cable();

        cable.startPort = SetUpPort(CircuitBoard.PortNames.Battery_V);
        cable.endPort = SetUpPort(CircuitBoard.PortNames.L1_A);
        cable.color = CableControls.CableColors.Red;
        cable.isFaulty = false;
    }

    [Test]
    public void Add_One_Cable_On_Circuit_Board()
    {
        importer.Cables.Add(cable);
        importer.StartCircuitImport();

        Assert.AreEqual(1, importer.Controls.currentCablesPlaced);
    }
}
