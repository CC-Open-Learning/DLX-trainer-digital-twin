using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VARLab.Interfaces;
using VARLab.MPCircuits;
using VARLab.MPCircuits.Model;


/// <summary>
///     Tests for the <see cref="DualCircuitLEDComponent"/>
/// </summary>
/// <remarks>
/// 
///     The formula for light intensity is as follows:
///     i = C * (Max(c_LO - cT_LO, 0) + Max(c_HI - cT_HI, 0))
///     
///     
///     where
///         i = intensity
///         C = lighting coefficient
///         M = Max(c - cT, 0)
///         c_LO, c_HI = current of LO, HI respectively
///         cT_LO, cT_HI = current threshold of LO, HI = (voltage threshold / resistance)
///     
///     This calculation is performed within the class itself, and is hand-calculated
///     within these tests using default values in order to validate correctness
/// </remarks>
public class DualCircuitLEDIntegrationsTests : MPCIntegrationTestsSetUpHelper
{
    public const float Epsilon = 1e-5f;

    // variables ---------------------------------------------------------------
    public DigitalTwinManager DTManager;


    public static float GetExpectedModifier(float voltage, float resistance, float threshold)
    {
        float current = voltage / resistance;
        float currentThreshold = threshold / resistance;

        return Mathf.Max(current - currentThreshold, 0f);
    }

    // set up/ tear down --------------------------------------------------------
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        IntegrationTestHelper.ClearScene();
        yield return null;

        DTManager = SetUpDigitalTwinManager();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        yield return null;
        IntegrationTestHelper.ClearScene();
    }

    // tests --------------------------------------------------------------------
    [UnityTest]
    public IEnumerator DualCircuitLED_ToggleBulbPlaced_RemovesAndReplaces()
    {
        DualCircuitLEDComponent led = DTManager.L3Dual;

        // Enable objects to ensure their startup callbacks (Awake, Start, OnEnable) execute
        DTManager.gameObject.SetActive(true);
        led.gameObject.SetActive(true);
        yield return null;

        //bulb is not removed at start
        Assert.AreEqual(true, led.IsConnected);
        Assert.AreEqual(true, DTManager.CircuitBoard.L3Low.IsConnected);
        Assert.AreEqual(true, DTManager.CircuitBoard.L3High.IsConnected);
        yield return null;

        //remove bulb
        DTManager.L3Dual.ToggleBulbPlaced();
        Assert.AreEqual(false, led.IsConnected);
        Assert.AreEqual(false, DTManager.CircuitBoard.L3Low.IsConnected);
        Assert.AreEqual(false, DTManager.CircuitBoard.L3High.IsConnected);
        yield return null;

        //replace bulb
        DTManager.L3Dual.ToggleBulbPlaced();
        Assert.AreEqual(true, led.IsConnected);
        Assert.AreEqual(true, DTManager.CircuitBoard.L3Low.IsConnected);
        Assert.AreEqual(true, DTManager.CircuitBoard.L3High.IsConnected);
        yield return null;
    }


    [UnityTest]
    public IEnumerator DualCircuitLED_LowIntensity_10V()
    {
        // ARRANGE
        DualCircuitLEDComponent led = DTManager.L3Dual;
        float sourceVoltage = 10f;

        // expected values:
        float defaultVoltageThreshold = 4.8f;
        float defaultCoefficient = 1f;
        float modelLowResistance = 1500f;

        float modifierExpected = GetExpectedModifier(sourceVoltage, modelLowResistance, defaultVoltageThreshold);
        float intensityExpected = defaultCoefficient * modifierExpected;

        float initialIntensity = led.PointLight.intensity;

        // ACT

        // Enable objects to ensure their startup callbacks (Awake, Start, OnEnable) execute
        DTManager.gameObject.SetActive(true);
        led.gameObject.SetActive(true);
        yield return null;

        DTManager.CircuitBoard.Battery.BoardVoltage = sourceVoltage;

        // Connect LO directly through battery
        DTManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.L3_Lo);
        DTManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.L3_Com);
        DTManager.CircuitBoard.SolveCircuit();
        yield return null;

        float modifierCalculated = DualCircuitLEDComponent.CalculateLightingModifier(led.ModelLow, led.VoltageThreshold);
        float intensityRead = led.PointLight.intensity;


        // ASSERT

        // Light is off on start
        Assert.AreEqual(0, initialIntensity);

        // Default values are as expected
        Assert.AreEqual(defaultVoltageThreshold, led.VoltageThreshold);
        Assert.AreEqual(defaultCoefficient, led.LightingCoefficient);

        // The modifier value calculated by the static function must be within the epsilon range
        Assert.IsTrue(Mathf.Abs(modifierExpected - modifierCalculated) < Epsilon);

        // The intensity value read from the light must be within the epsilon range
        Assert.IsTrue(Mathf.Abs(intensityRead - intensityExpected) < Epsilon);

        yield break;
    }

    [UnityTest]
    public IEnumerator DualCircuitLED_HighIntensity_14V()
    {
        // ARRANGE
        DualCircuitLEDComponent led = DTManager.L3Dual;
        float sourceVoltage = 14f;

        // expected values:
        float defaultVoltageThreshold = 4.8f;
        float defaultCoefficient = 1f;
        float modelHighResistance = 750f;

        float modifierExpected = GetExpectedModifier(sourceVoltage, modelHighResistance, defaultVoltageThreshold);
        float intensityExpected = defaultCoefficient * modifierExpected;


        // ACT

        // Enable objects to ensure their startup callbacks (Awake, Start, OnEnable) execute
        DTManager.gameObject.SetActive(true);
        led.gameObject.SetActive(true);
        yield return null;

        DTManager.CircuitBoard.Battery.BoardVoltage = sourceVoltage;

        // Connect LO directly through battery
        DTManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.L3_Hi);
        DTManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.L3_Com);
        DTManager.CircuitBoard.SolveCircuit();
        yield return null;

        float modifierCalculated = DualCircuitLEDComponent.CalculateLightingModifier(led.ModelHigh, led.VoltageThreshold);
        float intensityRead = led.PointLight.intensity;



        // ASSERT

        // Default values are as expected
        Assert.AreEqual(defaultVoltageThreshold, led.VoltageThreshold);
        Assert.AreEqual(defaultCoefficient, led.LightingCoefficient);

        // The modifier value calculated by the static function must be within the epsilon range
        Assert.IsTrue(Mathf.Abs(modifierExpected - modifierCalculated) < Epsilon);

        // The intensity value read from the light must be within the epsilon range
        Assert.IsTrue(Mathf.Abs(intensityRead - intensityExpected) < Epsilon);

        yield break;
    }


    /// <summary>
    ///     Validates that when either or both the LO or HI ports are connected,
    ///     the bulb is not illuminated if below the voltage threshold
    /// </summary>
    [UnityTest]
    public IEnumerator DualCircuitLED_Both_7V()
    {
        // ARRANGE
        DualCircuitLEDComponent led = DTManager.L3Dual;
        float sourceVoltage = 7f;


        // expected values:
        float defaultVoltageThreshold = 4.8f;
        float defaultCoefficient = 1f;
        float modelHighResistance = 750f;
        float modelLowResistance = 1500f;

        float modifierLowExpected = GetExpectedModifier(sourceVoltage, modelLowResistance, defaultVoltageThreshold);
        float modifierHighExpected = GetExpectedModifier(sourceVoltage, modelHighResistance, defaultVoltageThreshold);

        float intensityExpected = defaultCoefficient * (modifierLowExpected + modifierHighExpected);


        // ACT

        // Enable objects to ensure their startup callbacks (Awake, Start, OnEnable) execute
        DTManager.gameObject.SetActive(true);
        led.gameObject.SetActive(true);
        yield return null;

        DTManager.CircuitBoard.Battery.BoardVoltage = sourceVoltage;

        // Connect LO directly through battery
        DTManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.L3_Lo);
        DTManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.L3_Hi);
        DTManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.L3_Com);
        DTManager.CircuitBoard.SolveCircuit();
        yield return null;

        float modifierLowActual = DualCircuitLEDComponent.CalculateLightingModifier(led.ModelLow, led.VoltageThreshold);
        float modifierHighActual = DualCircuitLEDComponent.CalculateLightingModifier(led.ModelHigh, led.VoltageThreshold);
        float intensityRead = led.PointLight.intensity;



        // ASSERT

        // Default values are as expected
        Assert.AreEqual(defaultVoltageThreshold, led.VoltageThreshold);
        Assert.AreEqual(defaultCoefficient, led.LightingCoefficient);

        // The modifier values calculated by the static function must be within the epsilon range
        Assert.IsTrue(Mathf.Abs(modifierLowExpected - modifierLowActual) < Epsilon);
        Assert.IsTrue(Mathf.Abs(modifierHighExpected - modifierHighActual) < Epsilon);

        // The intensity value read from the light must be within the epsilon range
        Assert.IsTrue(Mathf.Abs(intensityRead - intensityExpected) < Epsilon);

        yield break;

    }



    /// <summary>
    ///     Validates that when either or both the LO or HI ports are connected,
    ///     the bulb is not illuminated if below the voltage threshold
    /// </summary>
    [UnityTest]
    public IEnumerator DualCircuitLED_BelowThreshold_NotIlluminated()
    {
        // ARRANGE
        DualCircuitLEDComponent led = DTManager.L3Dual;
        float sourceVoltage = 4.7999f;


        // Given that the default threshold is 4.8V and the source voltage is 3V,
        // expected modifiers and intensities should all be 0 regardless of
        // component resistances and coefficient

        // expected values:
        float defaultVoltageThreshold = 4.8f;
        float defaultCoefficient = 1f;
        // float modelHighResistance = 750f;
        // float modelLowResistance = 1500f;

        float modifierLowExpected = 0f; // GetExpectedModifier(sourceVoltage, modelLowResistance, defaultVoltageThreshold);
        float modifierHighExpected = 0f; // GetExpectedModifier(sourceVoltage, modelHighResistance, defaultVoltageThreshold);

        float intensityExpected = 0f; // defaultCoefficient * (modifierLowExpected + modifierHighExpected);


        // ACT

        // Enable objects to ensure their startup callbacks (Awake, Start, OnEnable) execute
        DTManager.gameObject.SetActive(true);
        led.gameObject.SetActive(true);
        yield return null;

        DTManager.CircuitBoard.Battery.BoardVoltage = sourceVoltage;

        // Connect LO directly through battery
        DTManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.L3_Lo);
        DTManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_V, CircuitBoard.PortNames.L3_Hi);
        DTManager.CircuitBoard.PlaceCable(CircuitBoard.PortNames.Battery_Gnd, CircuitBoard.PortNames.L3_Com);
        DTManager.CircuitBoard.SolveCircuit();
        yield return null;

        float modifierLowActual = DualCircuitLEDComponent.CalculateLightingModifier(led.ModelLow, led.VoltageThreshold);
        float modifierHighActual = DualCircuitLEDComponent.CalculateLightingModifier(led.ModelHigh, led.VoltageThreshold);
        float intensityRead = led.PointLight.intensity;



        // ASSERT

        // Default values are as expected
        Assert.AreEqual(defaultVoltageThreshold, led.VoltageThreshold);
        Assert.AreEqual(defaultCoefficient, led.LightingCoefficient);

        // The modifier values calculated by the static function must be within the epsilon range
        Assert.IsTrue(Mathf.Abs(modifierLowExpected - modifierLowActual) < Epsilon);
        Assert.IsTrue(Mathf.Abs(modifierHighExpected - modifierHighActual) < Epsilon);

        // The intensity value read from the light must be within the epsilon range
        Assert.IsTrue(Mathf.Abs(intensityRead - intensityExpected) < Epsilon);

        yield break;

    }
}
