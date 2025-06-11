using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;
using VARLab.MPCircuits.Model;

public class MotorIntegrationTests : MPCIntegrationTestsSetUpHelper
{
    private DigitalTwinManager digitalTwinManager;
    private MotorComponent motor;

    [SetUp]
    public void SetUp()
    {
        IntegrationTestHelper.ClearScene();

        digitalTwinManager = SetUpDigitalTwinManager();
        motor = SetUpMotor();
    }

    [TearDown]
    public void TearDown()
    {
        IntegrationTestHelper.ClearScene();
    }

    [UnityTest]
    public IEnumerator Verify_Motor_Is_Spinning()
    {
        float oldRotationAxisY = motor.MotorTransform.rotation.y; //initial value on start
        Debug.Log($"motor.MotorModel.rotation.y:  {motor.MotorTransform.rotation.y}  |  oldRotationAxisY:  {oldRotationAxisY}");

        //build simple circuit with M1 component
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 4f;
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.M1_Pos);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.M1_Neg);
        digitalTwinManager.CircuitBoard.SolveCircuit();

        yield return new WaitForSecondsRealtime(0.1f);
        Debug.Log($"motor.MotorModel.rotation.y:  {motor.MotorTransform.rotation.y}  |  oldRotationAxisY:  {oldRotationAxisY}");
        Assert.AreNotEqual(motor.MotorTransform.rotation.y, oldRotationAxisY);
        oldRotationAxisY = motor.MotorTransform.rotation.y;

        //
        yield return new WaitForSecondsRealtime(0.1f);
        Debug.Log($"motor.MotorModel.rotation.y:  {motor.MotorTransform.rotation.y}  |  oldRotationAxisY:  {oldRotationAxisY}");
        Assert.AreNotEqual(motor.MotorTransform.rotation.y, oldRotationAxisY);
        oldRotationAxisY = motor.MotorTransform.rotation.y;

        //
        yield return new WaitForSecondsRealtime(0.1f);
        Debug.Log($"motor.MotorModel.rotation.y:  {motor.MotorTransform.rotation.y}  |  oldRotationAxisY:  {oldRotationAxisY}");
        Assert.AreNotEqual(motor.MotorTransform.rotation.y, oldRotationAxisY);
        oldRotationAxisY = motor.MotorTransform.rotation.y;
    }

    [UnityTest]
    public IEnumerator Verify_Motor_Speed_Gets_Faster_With_Voltage_Increase()
    {
        float oldRotatingSpeed;

        Assert.AreEqual(0, motor.CurrentRotatingSpeed); //0 at start with nothing connected

        //build simple circuit with M1 component -- 2V
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 2f;
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.M1_Pos);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.M1_Neg);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;
        Assert.AreEqual(0, motor.CurrentRotatingSpeed);
        oldRotatingSpeed = motor.CurrentRotatingSpeed;

        ChangeBoardVoltage(3f);
        yield return null;
        Assert.Greater(motor.CurrentRotatingSpeed, oldRotatingSpeed);
        oldRotatingSpeed = motor.CurrentRotatingSpeed;

        ChangeBoardVoltage(7f);
        yield return null;
        Assert.Greater(motor.CurrentRotatingSpeed, oldRotatingSpeed);
        oldRotatingSpeed = motor.CurrentRotatingSpeed;

        ChangeBoardVoltage(10f);
        yield return null;
        Assert.Greater(motor.CurrentRotatingSpeed, oldRotatingSpeed);
        oldRotatingSpeed = motor.CurrentRotatingSpeed;

        ChangeBoardVoltage(14f);
        yield return null;
        Assert.Greater(motor.CurrentRotatingSpeed, oldRotatingSpeed);
        oldRotatingSpeed = motor.CurrentRotatingSpeed;
    }

    [UnityTest]
    public IEnumerator Verify_Motor_Reverses_Direction_When_Pos_And_Neg_Ports_Reversed()
    {
        Assert.AreEqual(0, motor.CurrentRotatingSpeed); //0 at start with nothing connected

        //build simple circuit with M1 component (regular)
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 4f;
        digitalTwinManager.CircuitBoard.PlaceCable("M1_in", CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.M1_Pos);
        digitalTwinManager.CircuitBoard.PlaceCable("M1_out", CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.M1_Neg);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        Assert.Greater(motor.CurrentRotatingSpeed, 0);

        digitalTwinManager.CircuitBoard.RemoveCable("M1_in");
        digitalTwinManager.CircuitBoard.RemoveCable("M1_out");

        //build simple circuit with M1 component with Pos And Neg Ports Reversed
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.M1_Neg);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.M1_Pos);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        Assert.Less(motor.CurrentRotatingSpeed, 0);
    }

    [UnityTest]
    public IEnumerator Verify_Motor_Does_Not_Spin_When_Circuit_Incomplete()
    {
        Assert.AreEqual(0, motor.CurrentRotatingSpeed); //0 at start with nothing connected

        //build incomplete circuit (not connected at R3/R5) --------------------------------------------------------
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 4f;
        digitalTwinManager.CircuitBoard.PlaceCable("M1_cable_in", CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.M1_Neg);
        digitalTwinManager.CircuitBoard.PlaceCable("R3_cable", CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.R3_O);
        digitalTwinManager.CircuitBoard.PlaceCable("R5_cable", CircuitBoard.PortNames.R5_R, CircuitBoard.PortNames.M1_Pos);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        Assert.AreEqual(0, motor.CurrentRotatingSpeed);

        digitalTwinManager.CircuitBoard.RemoveCable("R3_cable");
        digitalTwinManager.CircuitBoard.RemoveCable("R5_cable");

        //build complete circuit with M1 and SW1 (closed) component --------------------------------------------------
        digitalTwinManager.CircuitBoard.SW1.IsConnected = true;
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.SW1_Left);
        digitalTwinManager.CircuitBoard.PlaceCable("M1_cable_out", CircuitBoard.PortNames.SW1_Right, CircuitBoard.PortNames.M1_Pos);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        Assert.Greater(motor.CurrentRotatingSpeed, 0);

        OpenSwitch(); //incomplete circuit
        yield return null;

        Assert.AreEqual(0, motor.CurrentRotatingSpeed);

        digitalTwinManager.CircuitBoard.RemoveCable("M1_cable_in");
        digitalTwinManager.CircuitBoard.RemoveCable("M1_cable_out");

        //build complete circuit with M1 and SW1 (closed) component but ports reversed --------------------------------
        digitalTwinManager.CircuitBoard.SW1.IsConnected = true;
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.M1_Pos);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.SW1_Right, CircuitBoard.PortNames.M1_Neg);
        digitalTwinManager.CircuitBoard.SolveCircuit();
        yield return null;

        Assert.Less(motor.CurrentRotatingSpeed, 0);

        //---------------------------------------------------------------------------------------------------------------
        //Commenting out the section below as the switch seems to have a bug of its own and it's not related to the motor 
        //functionality.  When that is fixed in another ticket, this can be uncommented again for future confirmation
        //---------------------------------------------------------------------------------------------------------------

        //OpenSwitch(); //incomplete circuit
        //yield return null;

        //Assert.AreEqual(0, motor.CurrentRotatingSpeed);
    }


    //helper methods
    private void ChangeBoardVoltage(float value)
    {
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = value;
        digitalTwinManager.CircuitBoard.SolveCircuit();
    }

    private void OpenSwitch()
    {
        digitalTwinManager.CircuitBoard.SW1.IsConnected = false;
        digitalTwinManager.CircuitBoard.SolveCircuit();
    }
}
