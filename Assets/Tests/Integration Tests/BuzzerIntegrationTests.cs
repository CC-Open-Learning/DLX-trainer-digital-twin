using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;
using VARLab.MPCircuits.Model;

public class BuzzerIntegrationTests : MPCIntegrationTestsSetUpHelper
{
    Buzzer buzzer;
    DigitalTwinManager digitalTwinManager;

    [UnitySetUp]
    public IEnumerator SetUp() 
    {
        IntegrationTestHelper.ClearScene();

        buzzer = SetUpBuzzer();
        digitalTwinManager = SetUpDigitalTwinManager();
        
        yield return null;
    }

    [UnityTest]
    public IEnumerator Verify_Start_And_Disable_Buzzer_Values()
    {
        buzzer.gameObject.SetActive(true);

        // Verify that variables are being set correctly on start
        Assert.IsNotNull(buzzer.BuzzerModel);
        Assert.IsNotNull(buzzer.BuzzerModel.OnValuesUpdated);
        Assert.AreEqual(1.14f, buzzer.AudioSource.pitch);

        yield return null;

        // Disable buzzer game object to check the "OnDisable" method
        buzzer.gameObject.SetActive(false);

        Assert.IsNull(buzzer.BuzzerModel.OnValuesUpdated);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Verify_On_Port_Value_Changed_Buzzer_Values()
    {
        // Invoke the diode model connected to the buzzer
        buzzer.BuzzerModel.OnValuesUpdated?.Invoke(buzzer.BuzzerModel);

        // Verify the value of the buzzer volume, the volume should be set to 0 because the current is equal to 0
        Assert.AreEqual(0, buzzer.AudioSource.volume);

        // Verify the buzzer audio state, the state should be that the audio is not playing as the volume is set to 0
        Assert.AreEqual(false, buzzer.AudioSource.isPlaying);

        yield return null;

        // Now we set the current to be higher then the forward voltage by creating a circuit to enable the audio
        digitalTwinManager.CircuitBoard.Battery.BoardVoltage = 14f;
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.B1_V);
        digitalTwinManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.B1_Gnd);
        digitalTwinManager.CircuitBoard.SolveCircuit();

        yield return null;

        buzzer.BuzzerModel.OnValuesUpdated?.Invoke(buzzer.BuzzerModel);

        // Verify the value of the buzzer volume, the volume should be higher than 0 because the current is greater then 1
        Assert.AreEqual(0.07, Math.Round(buzzer.AudioSource.volume, 2));

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        UnityEngine.Object.Destroy(UnityEngine.Object.FindObjectOfType<DigitalTwinManager>());

        IntegrationTestHelper.ClearScene();

        yield return null;
    }
}
