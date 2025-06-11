using NUnit.Framework;
using System;
using VARLab.MPCircuits.Model;
using static VARLab.MPCircuits.Model.CircuitBoard;

public class CircuitSolverUnitTests
{
    private CircuitBoard circuitBoard;
    private bool isSolved = false;
    private bool errorThrown = false;

    //===================== Test Set Up =====================//

    [SetUp]
    public void SetUp()
    {
        isSolved = false;

        circuitBoard = new CircuitBoard(10);
        circuitBoard.OnCircuitSolveEventHandler += CircuitSolverResult;
    }

    [TearDown]
    public void TearDown()
    {

    }

    //===================== Test Connections =====================//

    [Test]
    public void OpenCircuitHandledCorrectly()
    {
        //removing the fuse opens the circuit
        circuitBoard.F1.IsConnected = false;

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.F1_Left, false);
        circuitBoard.PlaceCable(PortNames.F1_Right, PortNames.L1_A, false);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd, false);

        WaitForCircuitSolver();

        Assert.AreEqual(10, GetRoundedVoltageOfPort("1", 1)); // Batery at 10 v

        // All disconnected components read NaN since they are
        // not included in the circuit
        Assert.AreEqual(double.NaN, GetRoundedVoltageOfPort("F1_0", 1));
        Assert.AreEqual(double.NaN, GetRoundedVoltageOfPort("A", 1));
        Assert.AreEqual(double.NaN, GetRoundedVoltageOfPort("B", 1));
        Assert.IsFalse(errorThrown);
    }

    [Test]
    public void LooseBranchesPrunedCorrectly()
    {
        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd);
        circuitBoard.PlaceCable(PortNames.L1_A, PortNames.POT_H); //loose branch

        WaitForCircuitSolver();

        // H no longer has voltage when loose, as it is simply not included in the circuit
        Assert.AreEqual(double.NaN, GetRoundedVoltageOfPort("H", 1));

        Assert.AreEqual(10, GetRoundedVoltageOfPort("A", 1));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("B", 1));
        Assert.IsFalse(errorThrown);

        circuitBoard.PlaceCable(PortNames.POT_H, PortNames.L2_C); //create a loose branch with 2 components

        WaitForCircuitSolver();

        // Same with C and H here, previously both ports would read 10 because they were
        // "squashed" with A, but the new traversal algorithm simply ignores these ports and
        // doesnt add them to the circuit.
        Assert.AreEqual(double.NaN, GetRoundedVoltageOfPort("H", 1));
        Assert.AreEqual(double.NaN, GetRoundedVoltageOfPort("C", 1));

        Assert.AreEqual(10, GetRoundedVoltageOfPort("A", 1));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("B", 1));
        Assert.IsFalse(errorThrown);
    }

    //===================== Test Short Circuit =====================//

    [Test]
    public void ShortCircuitHandledCorrectly()
    {
        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.L1_A, false);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd, false);
        circuitBoard.PlaceCable(PortNames.L1_A, PortNames.L1_B, false); //short the circuit

        WaitForCircuitSolver();

        Assert.False(errorThrown);

        Assert.AreEqual(0, GetRoundedVoltageOfPort("1", 1));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("0", 1));
        Assert.AreEqual(10, GetRoundedVoltageOfPort("Rbat-", 1));

        Assert.True(circuitBoard.Battery.IsShorted);
    }

    [Test]
    public void ShortComponentHandledCorrectly()
    {
        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.L1_A, false);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.L2_C, false);
        circuitBoard.PlaceCable(PortNames.L2_D, PortNames.Battery_Gnd, false);
        circuitBoard.PlaceCable(PortNames.L2_D, PortNames.L2_C, false); //short L2

        WaitForCircuitSolver();

        Assert.AreEqual(10, GetRoundedVoltageOfPort("A", 1));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("B", 1));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("C", 1));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("D", 1));
        Assert.IsFalse(errorThrown);
        Assert.False(circuitBoard.Battery.IsShorted);
    }

    //===================== Test Digital Multimeter =====================//

    [Test]
    public void MultimeterSetToReadVoltageButReadingCurrentHandledCorrectly()
    {
        //put an ammeter in parallel with the circuit
        circuitBoard.DMM.CurrentState = MultimeterModel.MultimeterState.DCCurrent;

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.L1_A, false);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd, false);
        circuitBoard.PlaceCable(PortNames.DMM_Current, PortNames.L1_A, false);
        circuitBoard.PlaceCable(PortNames.DMM_Ground, PortNames.L1_B, false);

        WaitForCircuitSolver();

        Assert.IsFalse(errorThrown);

        //shorts the meter
        Assert.IsTrue(circuitBoard.DMM.AmmeterCurrent > MultimeterModel.AmmeterFuseThresholdAmps);
    }

    [Test]
    public void MultimeterSetToReadCurrentButReadingVoltageHandledCorrectly()
    {
        //put the voltmeter in series with the circuit
        circuitBoard.DMM.CurrentState = MultimeterModel.MultimeterState.DCVoltage;

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.DMM_Voltage);
        circuitBoard.PlaceCable(PortNames.DMM_Ground, PortNames.Battery_Gnd);

        WaitForCircuitSolver();

        Assert.IsFalse(errorThrown);

        //opens the circuit, all voltage is dropped across the meter due to its high resistance
        Assert.AreEqual(10, GetRoundedVoltageOfPort("A", 0));
        Assert.AreEqual(10, GetRoundedVoltageOfPort("B", 0));
        Assert.AreEqual(10, GetRoundedVoltageOfPort("Dmm_V", 0));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("0", 0));
    }

    [Test]
    public void Measure_Voltage_Correctly_Across_L1_And_POT_In_Series()
    {
        // Set the DMM to VoltDC
        circuitBoard.DMM.CurrentState = MultimeterModel.MultimeterState.DCVoltage;

        // Connect L1 in series with POT
        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.POT_G);
        circuitBoard.PlaceCable(PortNames.POT_H, PortNames.Battery_Gnd);

        // Connect the DMM to measure voltage at L1
        circuitBoard.PlaceCable(PortNames.DMM_Voltage, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.DMM_Ground, PortNames.L1_B);

        WaitForCircuitSolver();

        // Total voltage accross L1 and POT is equal board voltage
        Assert.AreEqual(circuitBoard.Battery.BoardVoltage, GetRoundedVoltageOfComponentBetweenTwoSharedPorts("A", "H"));

        // It = V / R = Vs / Rt = 10V / (50 + 45) = 0.105A (50ohm: resistance of L1; 45ohm: resistance of POT_GH)
        // Vl1 = Il1 * Rl1 = 0.105 * 50 = 5.25V
        // Vpot = Ipot * Rpot = 0.105 * 45 = 4.725V

        // Measure voltage at L1 with the DMM (round of 5.25V)
        Assert.AreEqual(circuitBoard.Battery.BoardVoltage / 2, Math.Round(circuitBoard.DMM.VoltmeterVoltage, 0));
    }

    //===================== Measure Voltage Drop =====================//

    [Test]
    public void Measure_VoltageDrop_Across_L1_And_POT_In_Series()
    {
        // Set the DMM to VoltDC
        circuitBoard.DMM.CurrentState = MultimeterModel.MultimeterState.DCVoltage;

        // Connect L1 in series with POT with a faulty cable
        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.POT_G);

        circuitBoard.PlaceCable(PortNames.POT_H, PortNames.Battery_Gnd, true);
        //circuitBoard.AddFaultyCable(PortNames.POT_H, PortNames.Battery_Gnd, "001"); // faulty cable

        // Connect the DMM to measure voltage at L1
        circuitBoard.PlaceCable(PortNames.DMM_Voltage, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.DMM_Ground, PortNames.POT_H);

        WaitForCircuitSolver();

        // Total voltage accross L1 and POT is less than board voltage
        Assert.IsTrue(Math.Round(circuitBoard.DMM.VoltmeterVoltage, 0) < circuitBoard.Battery.BoardVoltage);
    }

    [Test]
    public void Measure_VoltageDrop_Across_L1_F1_SW1_In_Series()
    {
        // Set the digital multimeter to Volt DC
        circuitBoard.DMM.CurrentState = MultimeterModel.MultimeterState.DCVoltage;

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.F1_Left);
        circuitBoard.PlaceCable(PortNames.F1_Right, PortNames.SW1_Left);
        circuitBoard.PlaceCable(PortNames.SW1_Right, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.L2_C);

        // Add faulty cable
        circuitBoard.PlaceCable(PortNames.L2_D, PortNames.Battery_Gnd, true);

        circuitBoard.SW1.IsConnected = true;

        circuitBoard.PlaceCable(PortNames.DMM_Voltage, PortNames.L2_C);
        circuitBoard.PlaceCable(PortNames.DMM_Ground, PortNames.Battery_Gnd);

        WaitForCircuitSolver();

        Assert.IsTrue(Math.Round(circuitBoard.DMM.VoltmeterVoltage, 0) < circuitBoard.Battery.BoardVoltage);
        Assert.IsTrue(GetRoundedVoltageOfComponentBetweenTwoSharedPorts("C", "D") < circuitBoard.Battery.BoardVoltage / 2);
    }

    //==================== Relay Testing ==============//
    [Test]
    public void Verify_Relay_Actuates_Correctly()
    {
        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.RL1_30);

        circuitBoard.PlaceCable(PortNames.RL1_87A, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd);
        circuitBoard.PlaceCable(PortNames.RL1_87, PortNames.L2_C);
        circuitBoard.PlaceCable(PortNames.L2_D, PortNames.Battery_Gnd);

        WaitForCircuitSolver();

        //All connections are made aside from 85 and 86, so this is verifying that not acutated state allows voltage through RL87A but not through RL87
        Assert.IsTrue(circuitBoard.L1.Voltage > 0);

        Assert.IsFalse(circuitBoard.RL1.IsActuated);

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.RL1_85);
        circuitBoard.PlaceCable(PortNames.Battery_Gnd, PortNames.RL1_86);

        WaitForCircuitSolver();

        Assert.IsTrue(circuitBoard.RL1.IsActuated);

        // Technically this second circuit solve iteration is needed for the test to pass :/
        WaitForCircuitSolver();

        //Relay is actuated, so L2 should now be the branch that is receiving voltage
        Assert.IsTrue(circuitBoard.L2.Voltage > 0);

        Assert.IsTrue(circuitBoard.RL1.IsActuated);
    }

    [Test]
    public void Verify_Relay_Min_And_Max_Actuate_Threshold()
    {
        circuitBoard.Battery.BoardVoltage = 5;

        circuitBoard.PlaceCable(PortNames.RL1_87, PortNames.L2_C);

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.RL1_30);
        circuitBoard.PlaceCable(PortNames.RL1_87A, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd);
        circuitBoard.PlaceCable(PortNames.RL1_87, PortNames.L2_C);
        circuitBoard.PlaceCable(PortNames.L2_D, PortNames.Battery_Gnd);

        //actuating
        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.RL1_85);
        circuitBoard.PlaceCable(PortNames.RL1_86, PortNames.Battery_Gnd);


        WaitForCircuitSolver();

        //Relay should not have actuated since it hasn't met the current threshold
        Assert.IsFalse(circuitBoard.RL1.IsActuated);
        Assert.IsTrue(circuitBoard.RL1.Current < circuitBoard.RL1.CurrentThresholdAmps);

        //over current threshold
        circuitBoard.Battery.BoardVoltage = 8;

        WaitForCircuitSolver();

        //L1 receives voltage and RL1 is actuated
        Assert.IsTrue(circuitBoard.L1.Voltage > 0);
        Assert.IsTrue(circuitBoard.RL1.IsActuated);

        //Now testing the low threshold where it should deactuate

        circuitBoard.Battery.BoardVoltage = 0f;  //not possible when playing the actual sim unless you route through the potentiometer
        circuitBoard.Battery.OnValuesUpdated?.Invoke(circuitBoard.Battery);
        WaitForCircuitSolver();

        Assert.IsFalse(circuitBoard.RL1.IsActuated);
        Assert.IsTrue(circuitBoard.RL1.Current < circuitBoard.RL1.CurrentThresholdAmpsLow);
    }

    //===================== Test Labs =====================//

    [Test]
    public void Test_PreLab()
    {
        circuitBoard.DMM.CurrentState = MultimeterModel.MultimeterState.DCVoltage;

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.DMM_Voltage, false);
        circuitBoard.PlaceCable(PortNames.Battery_Gnd, PortNames.DMM_Ground, false);

        WaitForCircuitSolver();

        Assert.AreEqual(10, Math.Round(circuitBoard.GetVoltage("Dmm_V"), 4));
        Assert.AreEqual(0, Math.Round(circuitBoard.GetVoltage("Dmm_Gnd"), 4));
        Assert.IsFalse(errorThrown);

    }

    [Test]
    public void Test_Lab_1()
    {
        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.Battery_Gnd, PortNames.L1_B);

        WaitForCircuitSolver();

        Assert.AreEqual(10, GetRoundedVoltageOfPort("A", 1));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("B", 1));
        Assert.IsFalse(errorThrown);
    }

    [Test]
    public void Test_Lab_2()
    {
        circuitBoard.F1.IsConnected = true;
        circuitBoard.SW1.IsConnected = true;

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.SW1_Left);
        circuitBoard.PlaceCable(PortNames.SW1_Right, PortNames.F1_Left);
        circuitBoard.PlaceCable(PortNames.F1_Right, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd);

        WaitForCircuitSolver();

        Assert.AreEqual(10, GetRoundedVoltageOfPort("A", 1));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("B", 1));
        Assert.IsFalse(errorThrown);
    }

    [Test]
    public void Test_Lab_3()
    {
        circuitBoard.F1.IsConnected = true;
        circuitBoard.SW1.IsConnected = false;

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.F1_Left, false);
        circuitBoard.PlaceCable(PortNames.F1_Right, PortNames.SW1_Left, false);
        circuitBoard.PlaceCable(PortNames.SW1_Right, PortNames.L1_A, false);
        circuitBoard.PlaceCable("cable4", PortNames.L1_B, PortNames.Battery_Gnd, false);

        circuitBoard.SW1.IsConnected = true;

        WaitForCircuitSolver();

        Assert.AreEqual(10, GetRoundedVoltageOfPort("A", 0));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("B", 1));
        Assert.IsFalse(errorThrown);

        circuitBoard.SW1.IsConnected = false;

        circuitBoard.RemoveCable("cable4");
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.L2_C, false);
        circuitBoard.PlaceCable(PortNames.L2_D, PortNames.Battery_Gnd, false);

        circuitBoard.SW1.IsConnected = true;

        WaitForCircuitSolver();

        Assert.AreEqual(10, GetRoundedVoltageOfPort("A", 1));
        Assert.AreEqual(5, GetRoundedVoltageOfPort("B", 0));
        Assert.AreEqual(5, GetRoundedVoltageOfPort("C", 0));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("D", 0));
        Assert.IsFalse(errorThrown);
    }

    [Test]
    public void Test_Lab_4()
    {
        circuitBoard.F1.IsConnected = true;
        circuitBoard.SW1.IsConnected = true;

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.SW1_Left, false);
        circuitBoard.PlaceCable(PortNames.SW1_Right, PortNames.F1_Left, false);
        circuitBoard.PlaceCable(PortNames.F1_Right, PortNames.L1_A, false);
        circuitBoard.PlaceCable(PortNames.F1_Right, PortNames.L2_C, false);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd, false);
        circuitBoard.PlaceCable(PortNames.L2_D, PortNames.Battery_Gnd, false);

        WaitForCircuitSolver();

        Assert.AreEqual(10, GetRoundedVoltageOfPort("A", 0));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("B", 1));
        Assert.AreEqual(10, GetRoundedVoltageOfPort("C", 0));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("D", 1));
        Assert.IsFalse(errorThrown);
    }


    /// <summary>
    ///     Connects L1 and L2 in parallel, with the Fuse, Switch, 
    ///     and Potentiometer in series with the parallel bulbs
    /// </summary>
    [Test]
    public void Test_Lab_5()
    {
        circuitBoard.F1.IsConnected = true;
        circuitBoard.SW1.IsConnected = false;

        circuitBoard.POT.HI_Resistance = 75;

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.F1_Left);
        circuitBoard.PlaceCable(PortNames.F1_Right, PortNames.SW1_Left);
        circuitBoard.PlaceCable(PortNames.SW1_Right, PortNames.POT_H);
        circuitBoard.PlaceCable(PortNames.POT_G, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.POT_G, PortNames.L2_C);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd);
        circuitBoard.PlaceCable(PortNames.L2_D, PortNames.Battery_Gnd);

        circuitBoard.SW1.IsConnected = true;

        WaitForCircuitSolver();

        Assert.AreEqual(10, GetRoundedVoltageOfPort("A", 0));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("B", 0));
        Assert.AreEqual(10, GetRoundedVoltageOfPort("C", 0));
        Assert.AreEqual(0, GetRoundedVoltageOfPort("D", 0));
        Assert.AreEqual(10, GetRoundedVoltageOfPort("G", 0));
    }

    [Test]
    public void Test_Lab_6()
    {
        circuitBoard.F1.IsConnected = false;
        circuitBoard.SW1.IsConnected = true;

        circuitBoard.DMM.CurrentState = MultimeterModel.MultimeterState.DCCurrent;

        // Measure current accross the circuit with 1 light bulb
        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.F1_Left);
        circuitBoard.PlaceCable(PortNames.F1_Right, PortNames.SW1_Left);
        circuitBoard.PlaceCable(PortNames.SW1_Right, PortNames.L1_A);
        circuitBoard.PlaceCable("cable_target", PortNames.L1_B, PortNames.Battery_Gnd);

        circuitBoard.PlaceCable(PortNames.DMM_Current, PortNames.F1_Left);
        circuitBoard.PlaceCable(PortNames.DMM_Ground, PortNames.F1_Right);

        WaitForCircuitSolver();

        float totalBranchVoltage = (float)(circuitBoard.GetVoltage("1") - circuitBoard.GetVoltage("0"));
        float totalBranchResistance = (float)circuitBoard.L1.Resistance + CircuitComponentModel.NoResistance * 2; //NO_RESISTANCE: battery, ammeter
        float branchCurrent = totalBranchVoltage / totalBranchResistance;

        Assert.AreEqual(0.3, Math.Round(branchCurrent, 1));
        Assert.IsFalse(errorThrown);

        // Knowledge check, measure current flow across the circuit with 2 light bulbs, confirm current decreases
        circuitBoard.RemoveCable("cable_target");
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.L2_C);
        circuitBoard.PlaceCable(PortNames.L2_D, PortNames.Battery_Gnd);

        WaitForCircuitSolver();

        totalBranchVoltage = (float)(circuitBoard.GetVoltage("1") - circuitBoard.GetVoltage("0"));
        totalBranchResistance = (float)circuitBoard.L1.Resistance * 2 + CircuitComponentModel.NoResistance * 2; //NO_RESISTANCE: battery, ammeter
        branchCurrent = (float)Math.Round((totalBranchVoltage / totalBranchResistance), 1);

        Assert.AreEqual(0.2f, branchCurrent);
        Assert.IsFalse(errorThrown);
    }

    [Test]
    public void Test_Lab_7()
    {
        circuitBoard.F1.IsConnected = false;
        circuitBoard.SW1.IsConnected = true;

        circuitBoard.DMM.CurrentState = MultimeterModel.MultimeterState.DCCurrent;

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.F1_Left);
        circuitBoard.PlaceCable(PortNames.F1_Right, PortNames.SW1_Left);
        circuitBoard.PlaceCable(PortNames.SW1_Right, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd);

        circuitBoard.PlaceCable("DMM_in", PortNames.DMM_Current, PortNames.F1_Left);
        circuitBoard.PlaceCable("DMM_out", PortNames.DMM_Ground, PortNames.F1_Right);


        WaitForCircuitSolver();

        float totalBranchVoltage = (float)(circuitBoard.GetVoltage("A") - circuitBoard.GetVoltage("B"));
        float totalBranchResistance = (float)circuitBoard.L1.Resistance + CircuitComponentModel.NoResistance * 2; //NO_RESISTANCE: battery, ammeter
        float branchCurrent = totalBranchVoltage / totalBranchResistance;

        Assert.AreEqual(0.3, Math.Round(branchCurrent, 1));
        Assert.IsFalse(errorThrown);

        // Measure the current across 1 light bulb (in a parallel circuit)
        circuitBoard.F1.IsConnected = false;

        circuitBoard.RemoveCable("DMM_in");
        circuitBoard.RemoveCable("DMM_out");

        circuitBoard.PlaceCable(PortNames.SW1_Right, PortNames.L2_C);
        circuitBoard.PlaceCable(PortNames.L2_D, PortNames.Battery_Gnd);

        circuitBoard.PlaceCable("DMM_in", PortNames.DMM_Current, PortNames.F1_Left);
        circuitBoard.PlaceCable("DMM_out", PortNames.DMM_Ground, PortNames.F1_Right);

        circuitBoard.F1.IsConnected = true;

        WaitForCircuitSolver();

        totalBranchVoltage = (float)(circuitBoard.GetVoltage("1") - circuitBoard.GetVoltage("0"));
        totalBranchResistance = (float)circuitBoard.L1.Resistance / 2 + CircuitComponentModel.NoResistance * 3; //NO_RESISTANCE: battery, ammeter, fuse
        branchCurrent = totalBranchVoltage / totalBranchResistance;

        Assert.AreEqual(0.5f, Math.Round(branchCurrent, 1));
        Assert.IsFalse(errorThrown);
    }

    //===================== Test Lights =====================//

    [Test]
    public void Light_Adjusts_To_Voltage_Regulator_Changes()
    {
        //board voltage initially set to 10 ---------------------------
        Assert.AreEqual(10, circuitBoard.Battery.BoardVoltage);

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.L1_A, false);
        circuitBoard.PlaceCable(PortNames.Battery_Gnd, PortNames.L1_B, false);

        WaitForCircuitSolver();

        //lightbulb L1 Voltage
        Assert.AreEqual(10, GetRoundedVoltageOfComponentBetweenTwoSharedPorts("A", "B"));

        //board voltage changed to 5 ---------------------------
        circuitBoard.Battery.BoardVoltage = 5;
        WaitForCircuitSolver();

        //lightbulb L1 Voltage
        Assert.AreEqual(5, GetRoundedVoltageOfComponentBetweenTwoSharedPorts("A", "B"));
    }

    [Test]
    public void Light_Turns_OffOrOn_When_Switch_Flipped()
    {
        //confirm board voltage is 10
        Assert.AreEqual(10, circuitBoard.Battery.BoardVoltage);

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.F1_Left, false);
        circuitBoard.PlaceCable(PortNames.F1_Right, PortNames.SW1_Left, false);
        circuitBoard.PlaceCable(PortNames.SW1_Right, PortNames.L1_A, false);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd, false);

        circuitBoard.F1.IsConnected = true;

        //switch set to closed/ON ---------------------------
        circuitBoard.SW1.IsConnected = true;

        WaitForCircuitSolver();

        //light on and has voltage
        Assert.AreEqual(10, GetRoundedVoltageOfComponentBetweenTwoSharedPorts("A", "B"));

        //flip switch to open/OFF ---------------------------
        circuitBoard.SW1.IsConnected = false;
        WaitForCircuitSolver();

        //light voltage should be NaN because the switch is off (no voltage, and ports are not being squashed)
        Assert.AreEqual(double.NaN, GetRoundedVoltageOfComponentBetweenTwoSharedPorts("A", "B"));
    }

    [Test]
    public void Light_Turns_OffOrOn_When_Lead_Completes_Circuit_Or_Disconnects_It()
    {
        //confirm board voltage is 10
        Assert.AreEqual(10, circuitBoard.Battery.BoardVoltage);

        //simple circuit complete with L1 ----------------
        circuitBoard.PlaceCable("cable1", PortNames.Battery_V, PortNames.L1_A, false);
        circuitBoard.PlaceCable("cable2", PortNames.Battery_Gnd, PortNames.L1_B, false);
        WaitForCircuitSolver();

        //light at full brightness/ voltage
        Assert.AreEqual(10, GetRoundedVoltageOfComponentBetweenTwoSharedPorts("A", "B"));

        //disconnect ground cable ---------------
        circuitBoard.RemoveCable("cable2");
        WaitForCircuitSolver();

        //no voltage at light when lead/circuit is disconnected
        Assert.AreEqual(double.NaN, GetRoundedVoltageOfComponentBetweenTwoSharedPorts("A", "B"));

        //connect ground cable again (complete circuit) ---------------
        circuitBoard.PlaceCable(PortNames.Battery_Gnd, PortNames.L1_B, false);
        WaitForCircuitSolver();

        //light at full brightness/ voltage
        Assert.AreEqual(10, GetRoundedVoltageOfComponentBetweenTwoSharedPorts("A", "B"));

        //disconnect battery+ cable ---------------
        circuitBoard.RemoveCable("cable1");
        WaitForCircuitSolver();

        //no voltage at light when lead/circuit is disconnected
        Assert.AreEqual(double.NaN, GetRoundedVoltageOfComponentBetweenTwoSharedPorts("A", "B"));
    }

    [Test]
    public void Light_Adjusts_To_Potentiometer_Resistance()
    {
        //parallel circuit with only L1 connected to the POT (L2 is not)
        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.F1_Left);
        circuitBoard.PlaceCable(PortNames.F1_Right, PortNames.SW1_Left);
        circuitBoard.PlaceCable(PortNames.SW1_Right, PortNames.L2_C);
        circuitBoard.PlaceCable(PortNames.L2_D, PortNames.Battery_Gnd);
        circuitBoard.PlaceCable(PortNames.SW1_Right, PortNames.POT_H);
        circuitBoard.PlaceCable(PortNames.POT_I, PortNames.L1_A);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd);

        circuitBoard.F1.IsConnected = true;
        circuitBoard.SW1.IsConnected = true;

        //full resistance on POT (75 ohms)
        circuitBoard.POT.HI_Resistance = 75;

        WaitForCircuitSolver();

        //confirm board voltage is 10
        Assert.AreEqual(10, circuitBoard.Battery.BoardVoltage);

        //light L2 on and has full voltage
        Assert.AreEqual(10, GetRoundedVoltageOfComponentBetweenTwoSharedPorts("C", "D"));
        Assert.AreEqual(3, GetRoundedVoltageOfComponentBetweenTwoSharedPorts("A", "B"));
    }

    [Test]
    public void Light_Turns_OffOrOn_Depending_On_Fuse_State()
    {
        //confirm board voltage is 10
        Assert.AreEqual(10, circuitBoard.Battery.BoardVoltage);

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.F1_Left, false);
        circuitBoard.PlaceCable(PortNames.F1_Right, PortNames.SW1_Left, false);
        circuitBoard.PlaceCable(PortNames.SW1_Right, PortNames.L1_A, false);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.Battery_Gnd, false);

        //fuse intact and connected ---------------
        circuitBoard.F1.IsConnected = true;
        circuitBoard.SW1.IsConnected = true;

        WaitForCircuitSolver();

        //light is on
        Assert.AreEqual(10, GetRoundedVoltageOfPort("A", 0));

        //fuse blown or removed ---------------
        circuitBoard.F1.IsConnected = false;
        WaitForCircuitSolver();

        //light is off
        Assert.AreEqual(double.NaN, GetRoundedVoltageOfPort("A", 0));
    }

    [Test]
    public void L1_And_L2_Turns_Off_Depending_On_LightBulb_State()
    {
        // Trying with L1 and L2 by default
        circuitBoard.L1.IsConnected = true;
        circuitBoard.L2.IsConnected = true;

        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.L1_A, false);
        circuitBoard.PlaceCable(PortNames.L1_B, PortNames.L2_C, false);
        circuitBoard.PlaceCable(PortNames.Battery_Gnd, PortNames.L2_D, false);

        WaitForCircuitSolver();

        // Half the voltage for this because the two light bulbs are sharing the same connection to battery+ (through L1-A to L1-B and then to L2-C)
        Assert.AreEqual(5, Math.Round(circuitBoard.GetVoltage("B")));
        Assert.AreEqual(5, Math.Round(circuitBoard.GetVoltage("C")));

        // Trying with L1 disconnected and L2 connected
        circuitBoard.L1.IsConnected = false;
        circuitBoard.L2.IsConnected = true;

        WaitForCircuitSolver();

        Assert.AreEqual(double.NaN, Math.Round(circuitBoard.GetVoltage("B")));
        Assert.AreEqual(double.NaN, Math.Round(circuitBoard.GetVoltage("C")));

        // Trying with L1 connected and L2 disconnected
        circuitBoard.L1.IsConnected = true;
        circuitBoard.L2.IsConnected = false;

        WaitForCircuitSolver();

        Assert.AreEqual(double.NaN, Math.Round(circuitBoard.GetVoltage("B")));
        Assert.AreEqual(double.NaN, Math.Round(circuitBoard.GetVoltage("C")));
    }

    [Test]
    public void Resistors_With_L1_Working_Correctly()
    {
        // Source voltage = 10V
        // Bulbs have a default resistance of 38.1, then resistance is updated based on voltage provided from
        // a circuit solve iteration.
        // This means that our calculations will always use the resistance derived from the previous 
        // circuit solve to inform the next one. Can cause L1, L2 to get desynced temporarily in few edge cases that should still be caught

        double epsilon = 0.1;
        int precision = 2;
        string testPort = "A";
        LightbulbModel l1 = circuitBoard.L1;


        //simple circuit -----------------------------------------------------------------------
        circuitBoard.PlaceCable("cable1", PortNames.Battery_V, PortNames.L1_A, false);
        circuitBoard.PlaceCable("cable2", PortNames.Battery_Gnd, PortNames.L1_B, false);

        WaitForCircuitSolver();


        //confirm light at full voltage (set to 10) WITHOUT resistor added
        Assert.AreEqual(10.0, GetRoundedVoltageOfPort(testPort, precision));
        // Confirm resistance in L1 after setting board voltage
        // After calculating, resistance of L1 will be
        // _baseResistance (38.1) + _coefficient (5/7) * (voltage (10) - _referenceVoltage (12))
        // ~= 36.67
        Assert.IsTrue(36.67 - l1.Resistance < epsilon);

        //simple circuit with R5 included -------------------------------------------------------
        MoveOneLeadEndOfCable("cable1", PortNames.Battery_V, PortNames.R5_S);
        circuitBoard.PlaceCable("cable3", PortNames.L1_A, PortNames.R5_R, false);

        WaitForCircuitSolver();


        //new voltage with 2 ohm resister added
        // Calculation here used 36.67 Ohms resistance, giving 94.8% of voltage to bulb
        double difference = Math.Abs(9.48 - GetRoundedVoltageOfPort(testPort, precision));
        Assert.IsTrue(difference < epsilon);

        // Now bulb has 36.3 ohms resistance
        Assert.IsTrue(36.3 - l1.Resistance < epsilon);

        //simple circuit with R4 included --------------------------------------------------------
        MoveOneLeadEndOfCable("cable1", PortNames.Battery_V, PortNames.R4_Q);
        MoveOneLeadEndOfCable("cable3", PortNames.L1_A, PortNames.R4_P);

        WaitForCircuitSolver();



        //new voltage with 20 ohm resister added
        // Calculation here used 36.3, so with 20 ohm resistor we get 64.7% of total voltage
        difference = Math.Abs(6.44 - GetRoundedVoltageOfPort(testPort, precision));
        Assert.IsTrue(difference < epsilon);

        // Now bulb has 34.13 ohms resistance
        Assert.IsTrue(34.13 - l1.Resistance < epsilon);


        //simple circuit with R3 included (same as R4 resistance) --------------------------------
        MoveOneLeadEndOfCable("cable1", PortNames.Battery_V, PortNames.R3_O);
        MoveOneLeadEndOfCable("cable3", PortNames.L1_A, PortNames.R3_N);

        WaitForCircuitSolver();

        //new voltage with 20 ohm resister added
        // Calculation here used 34.13, so percentage is 63.1%
        difference = Math.Abs(6.31 - GetRoundedVoltageOfPort(testPort, precision));
        Assert.IsTrue(difference < epsilon);

        // Now bulb has 34.04 ohms resistance
        Assert.IsTrue(34.04 - l1.Resistance < epsilon);

        //simple circuit with R2 included -------------------------------------------------------
        MoveOneLeadEndOfCable("cable1", PortNames.Battery_V, PortNames.R2_M);
        MoveOneLeadEndOfCable("cable3", PortNames.L1_A, PortNames.R2_L);

        WaitForCircuitSolver();

        //new voltage with 40 ohm resister added
        //Calculation here used 34.04, so percentage is 46.0%
        difference = Math.Abs(4.60 - GetRoundedVoltageOfPort(testPort, precision));
        Assert.IsTrue(difference < epsilon);

        // Now bulb has 32.81 ohms resistance
        Assert.IsTrue(32.81 - l1.Resistance < epsilon);

        //simple circuit with R1 included --------------------------------------------------------
        MoveOneLeadEndOfCable("cable1", PortNames.Battery_V, PortNames.R1_K);
        MoveOneLeadEndOfCable("cable3", PortNames.L1_A, PortNames.R1_J);

        WaitForCircuitSolver();

        //new voltage with 100 ohm resister added
        // resistance is 32.81, percentage voltage is 24.7%
        difference = Math.Abs(2.47 - GetRoundedVoltageOfPort(testPort, precision));
        Assert.IsTrue(difference < epsilon);

        // Final resistance calculation gives 31.29 ohms resistance
        Assert.IsTrue(31.29 - l1.Resistance < epsilon);
    }

    //===================== Test Diode D1, D2 =====================//

    [Test]
    public void Verify_Functionalities_Of_D1()
    {
        // Confirm board voltage is 10V
        Assert.AreEqual(10, circuitBoard.Battery.BoardVoltage);

        // Connect D1 to battery so current go from Annode to Cathode
        circuitBoard.PlaceCable("cable1", PortNames.Battery_V, PortNames.D1_E, false);
        circuitBoard.PlaceCable("cable2", PortNames.Battery_Gnd, PortNames.D1_F, false);

        WaitForCircuitSolver();

        // Verify that current goes from Annode to Cathode -> LED would shine
        Assert.AreEqual(10, GetRoundedVoltageOfPort("D1_E", 1));
        Assert.IsTrue(circuitBoard.Ports["D1_E"].Voltage - circuitBoard.Ports["D1_F"].Voltage >= 1f);

        // Disconnect D1 to battery
        circuitBoard.RemoveCable("cable1");
        circuitBoard.RemoveCable("cable2");
        WaitForCircuitSolver();

        // Connect D1 to battery with the opposite direction
        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.D1_F, false);
        circuitBoard.PlaceCable(PortNames.Battery_Gnd, PortNames.D1_E, false);

        WaitForCircuitSolver();

        // Verify that current won't go through D1
        Assert.AreEqual(0, GetRoundedVoltageOfPort("D1_E", 1));
        Assert.IsTrue(circuitBoard.Ports["D1_E"].Voltage - circuitBoard.Ports["D1_F"].Voltage < 1f);
    }

    [Test]
    public void Diode_CanReadResistance()
    {
        Assert.AreEqual(DiodeModel.InternalLEDResistance, circuitBoard.D1.Resistance);
    }

    [Test]
    public void Verify_Functionalities_Of_D2()
    {
        // Confirm board voltage is 10V
        Assert.AreEqual(10, circuitBoard.Battery.BoardVoltage);

        // Connect D2 to battery so current go from Annode to Cathode
        circuitBoard.PlaceCable("cable1", PortNames.Battery_V, PortNames.D2_A, false);
        circuitBoard.PlaceCable("cable2", PortNames.Battery_Gnd, PortNames.D2_K, false);

        WaitForCircuitSolver();

        // Verify that current goes from Annode to Cathode
        Assert.AreEqual(10, GetRoundedVoltageOfPort("D2_A", 1));

        // Disconnect D2 to battery
        circuitBoard.RemoveCable("cable1");
        circuitBoard.RemoveCable("cable2");

        WaitForCircuitSolver();

        // Connect D2 to battery with the opposite direction
        circuitBoard.PlaceCable(PortNames.Battery_V, PortNames.D2_K, false);
        circuitBoard.PlaceCable(PortNames.Battery_Gnd, PortNames.D2_A, false);

        WaitForCircuitSolver();

        // Verify that current won't go through D2
        Assert.AreEqual(0, GetRoundedVoltageOfPort("D2_A", 1));
    }

    //===================== Helper methods =====================//
    private void MoveOneLeadEndOfCable(string cableID, PortNames newStartPort, PortNames newEndPort)
    {
        circuitBoard.RemoveCable(cableID);
        circuitBoard.PlaceCable(cableID, newStartPort, newEndPort, false); //(not faulty)
    }

    private double GetRoundedVoltageOfPort(string port, int decimalPlaces)
    {
        return Math.Round(circuitBoard.GetVoltage(port), decimalPlaces);
    }

    private void WaitForCircuitSolver()
    {
        isSolved = false;
        errorThrown = false;
        try
        {
            circuitBoard.SolveCircuit();
            // I suppose we are expecting that CircuitSolverResult() will be called here after a successful solve
        }
        catch (Exception)
        {
            errorThrown = true;
        }

        UnityEngine.Debug.Log($"Circuit solved? {isSolved}");
    }

    private double GetRoundedVoltageOfComponentBetweenTwoSharedPorts(string port1, string port2)
    {
        double voltage = Math.Max(circuitBoard.GetVoltage(port1), circuitBoard.GetVoltage(port2)) - Math.Min(circuitBoard.GetVoltage(port1), circuitBoard.GetVoltage(port2));

        return Math.Round(voltage);
    }

    private void CircuitSolverResult()
    {
        UnityEngine.Debug.Log("Inside circuit solver callback, setting 'isSolved' to 'true'");
        isSolved = true;

    }
}
