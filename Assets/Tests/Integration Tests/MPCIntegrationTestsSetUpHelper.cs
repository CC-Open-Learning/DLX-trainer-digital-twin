using EPOOutline;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;
using VARLab.Interactions;
using VARLab.MPCircuits;
using VARLab.MPCircuits.Model;
using static UnityEngine.UI.Slider;

public class MPCIntegrationTestsSetUpHelper
{
    // ==========================================================================================================
    //                                            Digital Twin Related
    // ==========================================================================================================
    public DigitalTwinManager SetUpDigitalTwinManager()
    {
        GameObject digitalTwinGO = new();
        digitalTwinGO.SetActive(false);

        DigitalTwinManager digitalTwinManager = digitalTwinGO.AddComponent<DigitalTwinManager>();

        CircuitBoard c = new CircuitBoard(10);

        digitalTwinManager.CircuitBoard = c;

        digitalTwinManager.BoardVoltageSettings = SetUpBoardVoltageSettings();
        digitalTwinManager.CableControls = SetUpCableControl();
        digitalTwinManager.F1 = SetUpFuseComponent(digitalTwinManager);
        digitalTwinManager.MultimeterDial = SetUpMultimeterDial();
        digitalTwinManager.Multimeter = SetUpMultimeterComponent(digitalTwinManager, digitalTwinManager.MultimeterDial);
        digitalTwinManager.L1 = SetUpLightBulb(digitalTwinManager);
        digitalTwinManager.L2 = SetUpLightBulb(digitalTwinManager);
        digitalTwinManager.L3Dual = SetUpDualCircuitComponent(digitalTwinManager);
        digitalTwinManager.ToggleSwitch = SetUpSwitchComponent();
        digitalTwinManager.PushButtonSwitch = SetUpSwitchComponent();
        digitalTwinManager.RL1 = SetUpRelayComponent(digitalTwinManager);
        digitalTwinManager.EF = SetUpElectronicFlasherComponent(digitalTwinManager);
        digitalTwinManager.POT = SetUpPotentiometerComponent();

        digitalTwinManager.OnCircuitBoardSolved = new UnityEvent();

        digitalTwinGO.SetActive(true);

        return digitalTwinManager;
    }

    public CircuitImporter SetUpCircuitImporter()
    {
        GameObject importerGO = new GameObject();
        CircuitImporter importer;
        importer = importerGO.AddComponent<CircuitImporter>();
        importer.Cables = new List<CircuitImporter.Cable>();
        importer.Controls = SetUpCableControl();
        return importer;
    }

    public DebugPanel SetUpDebugPanel()
    {
        GameObject gameObject = new();
        gameObject.SetActive(false);

        DebugPanel debugPanel = gameObject.AddComponent<DebugPanel>();
        debugPanel.DebugCanvasBackground = new();
        debugPanel.DebugText = SetUpTextMeshProUGUI();
        debugPanel.OnOffToggle = new GameObject().AddComponent<Toggle>();
        debugPanel.Camera = new GameObject().AddComponent<Camera>();
        debugPanel.Camera.transform.position = Vector3.zero;

        debugPanel.L1_A = SetUpPort(CircuitBoard.PortNames.L1_A);
        debugPanel.L1_B = SetUpPort(CircuitBoard.PortNames.L1_B);
        debugPanel.L2_C = SetUpPort(CircuitBoard.PortNames.L2_C);
        debugPanel.L2_D = SetUpPort(CircuitBoard.PortNames.L2_D);
        debugPanel.POT_G = SetUpPort(CircuitBoard.PortNames.POT_G);
        debugPanel.POT_H = SetUpPort(CircuitBoard.PortNames.POT_H);
        debugPanel.POT_I = SetUpPort(CircuitBoard.PortNames.POT_I);
        debugPanel.F1_Left = SetUpPort(CircuitBoard.PortNames.F1_Left);
        debugPanel.F1_Right = SetUpPort(CircuitBoard.PortNames.F1_Right);
        debugPanel.SW1_Left = SetUpPort(CircuitBoard.PortNames.SW1_Left);
        debugPanel.SW1_Right = SetUpPort(CircuitBoard.PortNames.SW1_Right);
        debugPanel.DMM_Voltage = SetUpPort(CircuitBoard.PortNames.DMM_Voltage);
        debugPanel.DMM_Current = SetUpPort(CircuitBoard.PortNames.DMM_Current);
        debugPanel.DMM_Ground = SetUpPort(CircuitBoard.PortNames.DMM_Ground);
        debugPanel.R1_J = SetUpPort(CircuitBoard.PortNames.R1_J);
        debugPanel.R1_K = SetUpPort(CircuitBoard.PortNames.R1_K);
        debugPanel.R2_L = SetUpPort(CircuitBoard.PortNames.R2_L);
        debugPanel.R2_M = SetUpPort(CircuitBoard.PortNames.R2_M);
        debugPanel.R3_N = SetUpPort(CircuitBoard.PortNames.R3_N);
        debugPanel.R3_O = SetUpPort(CircuitBoard.PortNames.R3_O);
        debugPanel.R4_P = SetUpPort(CircuitBoard.PortNames.R4_P);
        debugPanel.R4_Q = SetUpPort(CircuitBoard.PortNames.R4_Q);
        debugPanel.R5_R = SetUpPort(CircuitBoard.PortNames.R5_R);
        debugPanel.R5_S = SetUpPort(CircuitBoard.PortNames.R5_S);
        debugPanel.L3_Com = SetUpPort(CircuitBoard.PortNames.L3_Com);
        debugPanel.L3_Lo = SetUpPort(CircuitBoard.PortNames.L3_Lo);
        debugPanel.L3_Hi = SetUpPort(CircuitBoard.PortNames.L3_Hi);
        debugPanel.D1_E = SetUpPort(CircuitBoard.PortNames.D1_E);
        debugPanel.D1_F = SetUpPort(CircuitBoard.PortNames.D1_F);
        debugPanel.D2_A = SetUpPort(CircuitBoard.PortNames.D2_A);
        debugPanel.D2_K = SetUpPort(CircuitBoard.PortNames.D2_K);
        debugPanel.B1_V = SetUpPort(CircuitBoard.PortNames.B1_V);
        debugPanel.B1_Gnd = SetUpPort(CircuitBoard.PortNames.B1_Gnd);
        debugPanel.PB1_Left = SetUpPort(CircuitBoard.PortNames.PB1_Left);
        debugPanel.PB1_Right = SetUpPort(CircuitBoard.PortNames.PB1_Right);
        debugPanel.RL1_85 = SetUpPort(CircuitBoard.PortNames.RL1_85);
        debugPanel.RL1_86 = SetUpPort(CircuitBoard.PortNames.RL1_86);
        debugPanel.RL1_87 = SetUpPort(CircuitBoard.PortNames.RL1_87);
        debugPanel.RL1_87A = SetUpPort(CircuitBoard.PortNames.RL1_87A);
        debugPanel.RL1_30 = SetUpPort(CircuitBoard.PortNames.RL1_30);

        debugPanel.DigitalTwinManager = SetUpDigitalTwinManager();

        gameObject.SetActive(true);

        return debugPanel;
    }


    // ==========================================================================================================
    //                                           Circuit Components 
    // ==========================================================================================================

    public LightbulbComponent SetUpLightBulb(DigitalTwinManager d)
    {
        GameObject lightGameObj = new();
        lightGameObj.SetActive(false);

        LightbulbComponent light = lightGameObj.AddComponent<LightbulbComponent>();
        light.PointLight = SetUpLightObject();
        light.LightBulbGO = new();
        light.LightBulbGO.AddComponent<MeshRenderer>();
        light.LightBulbGO.GetComponent<MeshRenderer>().materials = new Material[3] { SetUpMaterial("EmisiveJar"), SetUpMaterial("Hotwire"), SetUpMaterial("FIlaments") };

        light.Model = d.CircuitBoard.L1;

        light.OnLightbulbPlaced = new UnityEvent();
        light.OnLightbulbRemoved = new UnityEvent();

        light.EmissionColor = Color.yellow;
        light.EmissionColorFilament = Color.yellow;
        light.EmissionCurve = AnimationCurve.EaseInOut(0, 0, 0.9f, 0.9f);
        light.EmissionCurveFilament = AnimationCurve.EaseInOut(0, 0, 0.9f, 0.9f);

        lightGameObj.SetActive(true);

        return light;
    }

    public Light SetUpLightObject()
    {
        GameObject lightGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lightGO.SetActive(false);

        Light light = lightGO.AddComponent<Light>();
        light.intensity = 0;

        lightGO.SetActive(true);

        return light;
    }

    public SwitchComponent SetUpSwitchComponent()
    {
        GameObject switchObj = new();
        switchObj.SetActive(false);

        switchObj.AddComponent<Interactable>();
        switchObj.GetComponent<Interactable>().MouseClick = new();
        switchObj.GetComponent<Interactable>().MouseEnter = new();
        switchObj.GetComponent<Interactable>().MouseExit = new();
        switchObj.GetComponent<Interactable>().MouseDown = new();

        SwitchComponent switchClass = switchObj.AddComponent<SwitchComponent>();

        switchClass.switchModelAnimator = switchObj.AddComponent<Animator>();
        switchClass.switchModelAnimator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Toggle Switch/Toggle Switch.controller");
        switchClass.onSprite = new GameObject();
        switchClass.offSprite = new GameObject();
        switchClass.OnSwitchFlipped = new UnityEvent();
        switchClass.switchModel = switchObj.GetComponent<Interactable>();
        UnityEventTools.AddPersistentListener(switchClass.switchModel.MouseClick);

        switchObj.SetActive(true);

        return switchClass;
    }

    public FuseComponent SetUpFuseComponent(DigitalTwinManager d)
    {
        GameObject obj = new();
        obj.SetActive(false);

        obj.AddComponent<Interactable>();
        obj.GetComponent<Interactable>().MouseClick = new();
        obj.GetComponent<Interactable>().MouseEnter = new();
        obj.GetComponent<Interactable>().MouseExit = new();
        obj.GetComponent<Interactable>().MouseDown = new();

        obj.AddComponent<CapsuleCollider>();

        FuseComponent fuse = obj.AddComponent<FuseComponent>();

        fuse.fuseModel = d.CircuitBoard.F1;

        fuse.OffSprite = new GameObject();
        fuse.OnSprite = new GameObject();

        fuse.FuseAnimator = obj.AddComponent<Animator>();
        fuse.FuseAnimator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Fuse/Fuse.controller");

        fuse.BlownFuseSound = SetUpAudioClip();

        fuse.OnFusePlaced = new UnityEvent();
        fuse.OnFuseBlown = new UnityEvent();
        fuse.OnFuseRemoved = new UnityEvent();

        obj.SetActive(true);

        return fuse;
    }

    public DualCircuitLEDComponent SetUpDualCircuitComponent(DigitalTwinManager d)
    {
        GameObject obj = new();
        obj.SetActive(false);

        obj.AddComponent<Interactable>();
        obj.GetComponent<Interactable>().MouseClick = new();
        obj.GetComponent<Interactable>().MouseEnter = new();
        obj.GetComponent<Interactable>().MouseExit = new();
        obj.GetComponent<Interactable>().MouseDown = new();

        DualCircuitLEDComponent led = obj.AddComponent<DualCircuitLEDComponent>();

        led.PointLight = obj.AddComponent<Light>();
        led.PointLight.intensity = 0f;

        led.Animator = obj.AddComponent<Animator>();
        led.Animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Lightbulbs/Dual Circuit/Dual Circuit.controller");

        led.OnLightbulbPlaced = new UnityEvent();
        led.OnLightbulbRemoved = new UnityEvent();

        // obj.SetActive(true);

        return led;
    }

    public RelayComponent SetUpRelayComponent(DigitalTwinManager d)
    {
        GameObject obj = new();
        obj.SetActive(false);

        RelayComponent relay = obj.AddComponent<RelayComponent>();

        relay.OnRelayActuated = new UnityEvent();

        obj.SetActive(false);
        return relay;
    }

    public ElectronicFlasherComponent SetUpElectronicFlasherComponent(DigitalTwinManager d)
    {
        GameObject obj = new();
        obj.SetActive(false);

        obj.AddComponent<AudioSource>();

        ElectronicFlasherComponent electronicFlasherClass = obj.AddComponent<ElectronicFlasherComponent>();

        electronicFlasherClass.ComponentName = CircuitBoard.ComponentNames.EF;
        electronicFlasherClass.AudioSource = obj.GetComponent<AudioSource>();

        electronicFlasherClass.OnFlasherActuated = new UnityEvent();

        obj.SetActive(true);

        return electronicFlasherClass;
    }

    public PotentiometerComponent SetUpPotentiometerComponent()
    {
        GameObject obj = new();
        obj.SetActive(false);

        PotentiometerComponent POT = obj.AddComponent<PotentiometerComponent>();

        POT.OnPOTResistanceChanged = new UnityEvent();

        obj.SetActive(true);
        return POT;
    }

    public Buzzer SetUpBuzzer()
    {
        GameObject buzzerGO = new();
        buzzerGO.SetActive(false);

        Buzzer buzzer = buzzerGO.AddComponent<Buzzer>();
        buzzer.AudioSource = buzzerGO.AddComponent<AudioSource>();
        buzzer.ComponentName = CircuitBoard.ComponentNames.B1;

        buzzerGO.SetActive(true);
        return buzzer;
    }

    public MotorComponent SetUpMotor()
    {
        GameObject motorGO = new();
        motorGO.SetActive(false);

        MotorComponent motor = motorGO.AddComponent<MotorComponent>();
        motor.MotorTransform = new GameObject().transform;
        motor.SoundEffect = SetUpAudioSource();

        motorGO.SetActive(true);

        return motor;
    }

    // ==========================================================================================================
    //                                              Cables / Ports
    // ==========================================================================================================

    public CableControls SetUpCableControl()
    {
        GameObject cableControlGO = new();
        cableControlGO.SetActive(false);

        // Set up for Camera.main
        GameObject camera;
        camera = new GameObject("MainCamera");
        camera.AddComponent<Camera>();
        camera.tag = "MainCamera";
        Camera.main.nearClipPlane = 1f;

        CableControls _cableControl = cableControlGO.AddComponent<CableControls>();

        cableControlGO.AddComponent<Interactable>();
        cableControlGO.AddComponent<MeshRenderer>();

        _cableControl.redCableBundle = new();
        _cableControl.blackCableBundle = new();
        _cableControl.redCablePrefab = new();
        _cableControl.redCablePrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Cables/Cable/Red Cable.prefab", typeof(GameObject));
        _cableControl.blackCablePrefab = new();
        _cableControl.blackCablePrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Cables/Cable/Black Cable.prefab", typeof(GameObject));
        _cableControl.transparentRedCableLeadPrefab = new(); //instantiated in start() method
        _cableControl.transparentBlackCableLeadPrefab = new();

        _cableControl.interactionManager = SetUpInteractionManager();

        _cableControl.redCableBundle.AddComponent<Outlinable>();
        _cableControl.redCableBundle.GetComponent<Outlinable>().OutlineLayer = 63;

        _cableControl.redCableBundle.AddComponent<Interactable>();
        _cableControl.redCableBundle.GetComponent<Interactable>().MouseEnter = new();
        _cableControl.redCableBundle.GetComponent<Interactable>().MouseExit = new();
        _cableControl.redCableBundle.GetComponent<Interactable>().MouseDown = new();
        _cableControl.redCableBundle.GetComponent<Interactable>().MouseUp = new();
        _cableControl.redCableBundle.GetComponent<Interactable>().MouseClick = new();
        _cableControl.redCableBundle.GetComponent<Interactable>().enabled = true;

        _cableControl.blackCableBundle.AddComponent<Outlinable>();
        _cableControl.blackCableBundle.GetComponent<Outlinable>().OutlineLayer = 63;

        _cableControl.blackCableBundle.AddComponent<Interactable>();
        _cableControl.blackCableBundle.GetComponent<Interactable>().MouseEnter = new();
        _cableControl.blackCableBundle.GetComponent<Interactable>().MouseExit = new();
        _cableControl.blackCableBundle.GetComponent<Interactable>().MouseDown = new();
        _cableControl.blackCableBundle.GetComponent<Interactable>().MouseUp = new();
        _cableControl.blackCableBundle.GetComponent<Interactable>().MouseClick = new();
        _cableControl.blackCableBundle.GetComponent<Interactable>().enabled = true;

        _cableControl.OnCableDeleted = new();
        _cableControl.OnCablePlaced = new();
        _cableControl.OnCableDisconnected = new();

        cableControlGO.SetActive(true);

        return _cableControl;
    }

    public CableConnector SetUpCableConnector(CableLead leadStart, CableLead leadEnd, LineRenderer line)
    {
        GameObject cableConnectorGO = new GameObject("Cable Connector 1");
        cableConnectorGO.AddComponent<MeshRenderer>();
        cableConnectorGO.SetActive(false);

        CableConnector _cableConnector = cableConnectorGO.AddComponent<CableConnector>();
        leadStart.transform.parent = cableConnectorGO.transform;
        leadEnd.transform.parent = cableConnectorGO.transform;
        _cableConnector.cableStart = leadStart;
        _cableConnector.cableEnd = leadEnd;
        _cableConnector.startPoint = leadStart.transform;
        _cableConnector.endPoint = leadEnd.transform;
        _cableConnector.deleteIcon = SetUpInteractable();
        _cableConnector.deleteIcon.MouseClick = new();

        cableConnectorGO.AddComponent<Outlinable>();

        line = cableConnectorGO.AddComponent<LineRenderer>();

        _cableConnector.InitLineRenderer();

        _cableConnector.capsuleCollider = new GameObject().AddComponent<CapsuleCollider>();

        _cableConnector.points = new CablePhysics[5] {
            SetUpCablePhysics(),
            SetUpCablePhysics(),
            SetUpCablePhysics(),
            SetUpCablePhysics(),
            SetUpCablePhysics()};

        return _cableConnector;
    }

    public CablePhysics SetUpCablePhysics()
    {
        GameObject cablePhysicsGO = new();
        cablePhysicsGO.SetActive(false);

        CablePhysics cablePhysics = cablePhysicsGO.AddComponent<CablePhysics>();
        cablePhysics._position = new();
        cablePhysics._oldPosition = new();

        cablePhysicsGO.SetActive(true);

        return cablePhysics;
    }

    public CableLead SetUpCableLead()
    {
        GameObject cableLeadGO = new GameObject();
        cableLeadGO.SetActive(false);

        cableLeadGO.AddComponent<Interactable>();
        cableLeadGO.GetComponent<Interactable>().MouseClick = new();
        cableLeadGO.GetComponent<Interactable>().MouseEnter = new();
        cableLeadGO.GetComponent<Interactable>().MouseExit = new();
        cableLeadGO.AddComponent<Outlinable>();
        cableLeadGO.AddComponent<BoxCollider>();
        cableLeadGO.AddComponent<MeshRenderer>();
        CableLead cableLead = cableLeadGO.AddComponent<CableLead>();

        cableLeadGO.SetActive(true);

        return cableLead;
    }

    public PortBehaviour SetUpPort(CircuitBoard.PortNames portName = CircuitBoard.PortNames.None)
    {
        GameObject portGameObj = new();
        portGameObj.SetActive(false);

        portGameObj.AddComponent<Interactable>();

        PortBehaviour port = portGameObj.AddComponent<PortBehaviour>();
        port.PortName = portName;

        portGameObj.GetComponent<Interactable>().MouseClick = new();
        portGameObj.GetComponent<Interactable>().MouseEnter = new();
        portGameObj.GetComponent<Interactable>().MouseExit = new();
        port.OnPortClicked = new UnityEvent<PortBehaviour>();
        port.OnPortHovered = new UnityEvent<PortBehaviour>();
        port.OnPortHoveredOff = new UnityEvent<PortBehaviour>();

        portGameObj.SetActive(true);

        return port;
    }

    // ==========================================================================================================
    //                                           Sliders / Dials / Inputs
    // ==========================================================================================================

    public MultimeterComponent SetUpMultimeterComponent(DigitalTwinManager d, MultimeterDial dial)
    {
        GameObject multimeterComponentGO = new();
        multimeterComponentGO.SetActive(false);

        MultimeterComponent multimeterComponent = multimeterComponentGO.AddComponent<MultimeterComponent>();
        multimeterComponent.multimeterDial = dial;
        multimeterComponent.Volts = SetUpPort(CircuitBoard.PortNames.DMM_Voltage);
        multimeterComponent.Amps = SetUpPort(CircuitBoard.PortNames.DMM_Current);
        multimeterComponent.Ground = SetUpPort(CircuitBoard.PortNames.DMM_Ground);
        multimeterComponent.Model = d.CircuitBoard.DMM;
        multimeterComponent.multimeterText = SetUpTextMeshPro();
        multimeterComponent.OnSettingChanged = new();
        multimeterComponent.OnFuseStateChanged = new();
        multimeterComponent.Animator = multimeterComponentGO.AddComponent<Animator>();
        multimeterComponent.Animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Smoke/Multimeter.controller");

        multimeterComponentGO.SetActive(true);

        return multimeterComponent;
    }

    public MultimeterDial SetUpMultimeterDial()
    {
        GameObject multimeterDialGameObj = new();
        multimeterDialGameObj.SetActive(false);

        MultimeterDial multimeterDial = multimeterDialGameObj.AddComponent<MultimeterDial>();
        multimeterDial.OnDivisionChanged = new();

        multimeterDialGameObj.SetActive(true);

        return multimeterDial;
    }

    public MultimeterSliderUI SetUpMultimeterSliderUI()
    {
        GameObject multimeterSliderUIGameObj = new();
        multimeterSliderUIGameObj.SetActive(false);

        MultimeterSliderUI multimeterSliderUI = multimeterSliderUIGameObj.AddComponent<MultimeterSliderUI>();
        multimeterSliderUI.dial = new GameObject().AddComponent<RectTransform>();
        multimeterSliderUI.settingSwitchSound = SetUpAudioClip();
        multimeterSliderUI.settingSwitchSource = SetUpAudioSource();
        multimeterSliderUI.currentSetting = MultimeterDialSettings.Off;
        multimeterSliderUI.dialSettings = new MultimeterSliderOptionButton[7]
            {
                SetUpMultimeterSliderOptionButton(MultimeterDialSettings.Off, 110),
                SetUpMultimeterSliderOptionButton(MultimeterDialSettings.ACVoltage, 80),
                SetUpMultimeterSliderOptionButton(MultimeterDialSettings.DCVoltage, 45),
                SetUpMultimeterSliderOptionButton(MultimeterDialSettings.ACVoltageMillivolts, 15),
                SetUpMultimeterSliderOptionButton(MultimeterDialSettings.Resistance, -15),
                SetUpMultimeterSliderOptionButton(MultimeterDialSettings.Capacitance, -45),
                SetUpMultimeterSliderOptionButton(MultimeterDialSettings.Current, -75)
            };
        multimeterSliderUI.radialSlider = SetUpRadialSlider(1f, 7.5f, 0f, -180f);
        multimeterSliderUI.multimeterSlider = multimeterSliderUI.radialSlider.slider;
        multimeterSliderUI.currentZRotation = 0;

        multimeterSliderUIGameObj.SetActive(true);

        return multimeterSliderUI;
    }

    public MultimeterSliderOptionButton SetUpMultimeterSliderOptionButton(MultimeterDialSettings setting, int targetDialZRotation)
    {
        GameObject multimeterSliderOptionButtonObj = new();
        multimeterSliderOptionButtonObj.SetActive(false);

        MultimeterSliderOptionButton multimeterSliderOptionButton = multimeterSliderOptionButtonObj.AddComponent<MultimeterSliderOptionButton>();
        multimeterSliderOptionButton.setting = setting;
        multimeterSliderOptionButton.targetDialZRotation = targetDialZRotation;
        multimeterSliderOptionButton.SpriteHighlight = new();

        Button button = multimeterSliderOptionButtonObj.AddComponent<Button>();

        multimeterSliderOptionButtonObj.SetActive(true);
        return multimeterSliderOptionButton;
    }

    public RadialSlider SetUpRadialSlider(float minValue, float maxValue, float minKnobRotationValue, float maxKnobRotationValue)
    {
        GameObject radialSliderGO = new();
        radialSliderGO.SetActive(false);

        RadialSlider radialSlider = radialSliderGO.AddComponent<RadialSlider>();
        radialSlider.parent = radialSliderGO.AddComponent<Interactable>();

        radialSlider.knobRotationRoot = new GameObject().transform;
        radialSlider.minKnobRotationValue = minKnobRotationValue;
        radialSlider.maxKnobRotationValue = maxKnobRotationValue;

        radialSlider.slider = SetUpSlider();
        radialSlider.slider.value = minValue;
        radialSlider.slider.minValue = minValue;
        radialSlider.slider.maxValue = maxValue;

        radialSlider.sliderUI = new();
        radialSlider.buttonUI = new();
        radialSlider.baseUI = new();

        radialSlider.background = SetUpImage();

        radialSlider.OnValueSelected = new UnityEvent<float>();

        radialSliderGO.SetActive(true);

        return radialSlider;
    }

    public Slider SetUpSlider()
    {
        GameObject sliderGO = new();
        sliderGO.SetActive(false);

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.onValueChanged = new SliderEvent();

        sliderGO.SetActive(true);

        return slider;
    }

    public SettingsInputMethods SetUpInputMethods()
    {
        GameObject settingsInputMethodsGameObj = new();
        settingsInputMethodsGameObj.SetActive(false);

        SettingsInputMethods settingsInputMethods = settingsInputMethodsGameObj.AddComponent<SettingsInputMethods>();
        settingsInputMethods.InputMethods = SetUpInputMethodList();
        settingsInputMethodsGameObj.SetActive(true);

        return settingsInputMethods;
    }

    public List<SettingsInputMethods.InputMethod> SetUpInputMethodList()
    {
        List<SettingsInputMethods.InputMethod> inputMethods = new List<SettingsInputMethods.InputMethod>();

        SettingsInputMethods.InputMethod sliderInputMethod = new();
        sliderInputMethod.InputMethodName = "Slider";
        sliderInputMethod.InputMethodType = SettingsInputMethods.InputMethodTypes.Slider;

        SettingsInputMethods.InputMethod incrementDecrementButtonsInputMethod = new();
        incrementDecrementButtonsInputMethod.InputMethodName = "Increment & Decrement";
        incrementDecrementButtonsInputMethod.InputMethodType = SettingsInputMethods.InputMethodTypes.IncrementAndDecrement;

        inputMethods.Add(sliderInputMethod);
        inputMethods.Add(incrementDecrementButtonsInputMethod);

        return inputMethods;
    }

    public SettingsInputMethods.InputMethod SetUpCurrentInputMethod(SettingsInputMethods.InputMethod inputMethodToSet, SettingsInputMethods.InputMethodTypes inputTypeToSet, string InputMethodName)
    {
        inputMethodToSet.InputMethodType = inputTypeToSet;
        inputMethodToSet.InputMethodName = InputMethodName;

        return inputMethodToSet;
    }

    // ==========================================================================================================
    //                                           Lab Functionality / Control
    // ==========================================================================================================

    public BoardVoltageSettings SetUpBoardVoltageSettings()
    {
        GameObject boardVoltageSettingsGO = new();
        boardVoltageSettingsGO.SetActive(false);

        BoardVoltageSettings boardVoltageSettings = boardVoltageSettingsGO.AddComponent<BoardVoltageSettings>();
        boardVoltageSettings.BoardVoltage = BoardVoltageSettings.DefaultBoardVoltage;

        boardVoltageSettings.BoardVoltageSlider = SetUpRadialSlider(2f, 14f, 15f, -300f);
        boardVoltageSettings.OnBoardVoltageChanged = new UnityEvent();

        boardVoltageSettingsGO.SetActive(true);

        return boardVoltageSettings;
    }





    // ==========================================================================================================
    //                                            Managers / Singleton
    // ==========================================================================================================

    ////------------- You can use this set up for singletons in your specific test class as needed ------------------ (plus what else you need)
    //GameObject singletonGO;
    //Singleton singleton;

    //[UnitySetUp]
    //public IEnumerator SetUp()
    //{
    //    //using [UnitySetUp], IEnumerator, & yield return null after the Singleton awake method calls to ensure the Singleton
    //    //static INSTANCE is destroyed before the next test. The frame may need to advance before the garbage collector officially 
    //    //destroys it.
    //    IntegrationTestHelper.ClearScene();

    //    singletonGO = SetUpSingletonGO();
    //    yield return null;
    //    singletonGO.SetActive(false);

    //    singleton = singletonGO.GetComponent<Singleton>();

    //    singleton.VoiceOverManager = SetUpVoiceOverManager();
    //    singleton.SFXManager = SetUpSFXManager();
    //    singleton.FeedbackPanelManager = SetUpFeedbackPanelManager();
    //    singleton.InteractionManager = SetUpInteractionManager();
    //    singleton.PanelManager = SetUpPanelManager();
    //    singleton.SettingsManager = SetUpSettingsManager();

    //    singletonGO.SetActive(true);
    //}

    //[UnityTearDown]
    //public IEnumerator TearDown()
    //{
    //    //using [UnityTearDown], IEnumerator, & yield return null to ensure the Singleton static INSTANCE is destroyed before the next test
    //    GameObject.Destroy(GameObject.FindObjectOfType<Singleton>());
    //    yield return null;

    //    IntegrationTestHelper.ClearScene();
    //}

    public GameObject SetUpSingletonGO()
    {
        GameObject singletonGO = new();
        singletonGO.SetActive(false);
        singletonGO.AddComponent<Singleton>();
        singletonGO.SetActive(true);

        return singletonGO;
    }

    public MixerGroupManager SetUpMixerGroupManager(MixerGroupManager.AudioManagerType managerType, string volumeParameter, string mixerGroupName, AudioSource mainAudioSource)
    {
        GameObject mixerGroupGO = new();
        mixerGroupGO.SetActive(false);
        MixerGroupManager mixerGroupManager = mixerGroupGO.AddComponent<MixerGroupManager>();
        mixerGroupGO.AddComponent<AudioSource>();

        mixerGroupManager.ManagerType = managerType;
        mixerGroupManager.VolumeParameter = volumeParameter;
        mixerGroupManager.Mixer = SetUpAudioMixer();
        mixerGroupManager.Group = SetUpAudioMixerGroup(mixerGroupManager.Mixer, mixerGroupName);
        mixerGroupManager.Source = mainAudioSource;

        mixerGroupGO.SetActive(true);

        return mixerGroupManager;
    }

    public AudioMixer SetUpAudioMixer()
    {
        AudioMixer mixer = Resources.Load<AudioMixer>("Audio/MainMixer");
        return mixer;
    }

    public AudioMixerGroup SetUpAudioMixerGroup(AudioMixer mixer, string groupName)
    {
        return mixer.FindMatchingGroups(groupName)[0];
    }

    public InteractionManager SetUpInteractionManager()
    {
        GameObject interactionManagerGO = new();
        interactionManagerGO.SetActive(false);

        InteractionManager interactionManager = interactionManagerGO.AddComponent<InteractionManager>();

        interactionManager.ports = new();
        interactionManager.multimeter = new();
        interactionManager.boardVoltage = new();
        interactionManager.switchToggle = new();
        interactionManager.fuse = new();
        interactionManager.potentiometers = new();
        interactionManager.cableBundles = new();
        interactionManager.lights = new();

        interactionManager.boardVoltageSlider = SetUpInteractable();
        SetUpInteractableRadialSlider(interactionManager.boardVoltageSlider, 10f, 2f, 14f, 15f, -300f);

        interactionManager.potSlider = SetUpInteractable();
        SetUpInteractableRadialSlider(interactionManager.potSlider, 30f, 0f, 75f, 15f, -300f);

        interactionManager.multimeterDial = SetUpInteractable();
        SetUpInteractableRadialSlider(interactionManager.multimeterDial, 1f, 1f, 7.5f, 0f, -180f);
        interactionManager.multimeterDial.gameObject.AddComponent<Image>();

        interactionManager.InteractableHoverColor = Color.gray;
        interactionManager.SelectedObjectOutlineColor = Color.yellow;

        interactionManagerGO.SetActive(true);

        return interactionManager;
    }

    // Helper method to avoid duplicate code above ^
    public void SetUpInteractableRadialSlider(Interactable _object, float value, float minValue, float maxValue, float minKnobRotationValue, float maxKnobRotationValue)
    {
        _object.gameObject.AddComponent<RadialSlider>();
        _object.gameObject.GetComponent<RadialSlider>().slider = SetUpSlider();
        _object.gameObject.GetComponent<RadialSlider>().slider.value = value;
        _object.gameObject.GetComponent<RadialSlider>().slider.minValue = minValue;
        _object.gameObject.GetComponent<RadialSlider>().slider.maxValue = maxValue;
        _object.gameObject.GetComponent<RadialSlider>().knobRotationRoot = SetUpGameObject().transform;
        _object.gameObject.GetComponent<RadialSlider>().minKnobRotationValue = minKnobRotationValue;
        _object.gameObject.GetComponent<RadialSlider>().maxKnobRotationValue = maxKnobRotationValue;
        _object.gameObject.GetComponent<RadialSlider>().background = SetUpImage();
        _object.gameObject.GetComponent<RadialSlider>().baseUI = new();
        _object.gameObject.GetComponent<RadialSlider>().sliderUI = new();
    }

    public PanelManager SetUpPanelManager()
    {
        GameObject panelManagerGO = new();
        panelManagerGO.SetActive(false);

        PanelManager panelManager = panelManagerGO.AddComponent<PanelManager>();

        panelManagerGO.SetActive(true);

        return panelManager;
    }

    public SettingsManager SetUpSettingsManager()
    {
        GameObject settingsManagerGO = new();
        settingsManagerGO.SetActive(false);

        SettingsManager settingsManager = settingsManagerGO.AddComponent<SettingsManager>();
        settingsManager.VoiceOverText = SetUpTextMeshProUGUI();
        settingsManager.SoundEffectsText = SetUpTextMeshProUGUI();
        settingsManager.InputMethodText = SetUpTextMeshProUGUI();
        settingsManager.VoiceOverIncrementButton = SetUpButton();
        settingsManager.VoiceOverDecrementButton = SetUpButton();
        settingsManager.SoundEffectsIncrementButton = SetUpButton();
        settingsManager.SoundEffectsDecrementButton = SetUpButton();
        settingsManager.InputMethodIncrementButton = SetUpButton();
        settingsManager.InputMethodDecrementButton = SetUpButton();
        settingsManager.OpenButton = SetUpButton();
        settingsManager.CloseButton = SetUpButton();
        settingsManager.VoiceOverSlider = SetUpSlider();
        settingsManager.SoundEffectsSlider = SetUpSlider();
        settingsManager.CloseButtonBorder = new GameObject();
        settingsManager.StartPanelButton = SetUpStartPanelButton();

        settingsManager.InputMethods = SetUpInputMethods();
        settingsManager.CurrentInputMethod = SetUpCurrentInputMethod(settingsManager.InputMethods.InputMethods[0], SettingsInputMethods.InputMethodTypes.Slider, "Slider");
        settingsManager.InputMethodsCount = settingsManager.InputMethods.InputMethods.Count;

        settingsManagerGO.SetActive(true);

        return settingsManager;
    }

    // Not part of singleton class but operates similarily
    public void SetUpScreenFadeManager()
    {
        GameObject screenFadeManagerGO = new();
        screenFadeManagerGO.SetActive(false);

        ScreenFadeManager screenFadeManager = screenFadeManagerGO.AddComponent<ScreenFadeManager>();
        screenFadeManager.fadePanelImage = SetUpImage();
        screenFadeManager.FadeTime = 0;

        screenFadeManagerGO.SetActive(true);
    }


    // ==========================================================================================================
    //                                                UI / Buttons
    // ==========================================================================================================

    public StartPanelButton SetUpStartPanelButton()
    {
        GameObject gameObject = new();
        gameObject.SetActive(false);

        StartPanelButton startPanelButton = gameObject.AddComponent<StartPanelButton>();
        startPanelButton.baseColour = Color.black;  //temp color for setup
        startPanelButton.accentColour = Color.white; //temp color for setup
        startPanelButton.buttonMain = SetUpImage();
        startPanelButton.buttonBorder = SetUpImage();
        startPanelButton.buttonText = SetUpTextMeshProUGUI();

        gameObject.SetActive(true);

        return startPanelButton;
    }

    public BoardVoltageDisplay SetUpBoardVoltageDisplay()
    {
        GameObject gameObject = new();
        gameObject.SetActive(false);

        BoardVoltageDisplay boardVoltageDisplay = gameObject.AddComponent<BoardVoltageDisplay>();
        boardVoltageDisplay.BoardVoltageValueText = SetUpTextMeshPro();

        gameObject.SetActive(true);

        return boardVoltageDisplay;
    }


    // ==========================================================================================================
    //                                           Basic Unity Elements 
    // ==========================================================================================================

    public void SetUpMainCameraAudioListener()
    {
        GameObject mainCameraGameObj = new();
        mainCameraGameObj.SetActive(false);
        mainCameraGameObj.AddComponent<AudioListener>();
        mainCameraGameObj.SetActive(true);
    }

    public AudioSource SetUpAudioSource()
    {
        GameObject audioSourceGO = new();
        audioSourceGO.SetActive(false);

        AudioSource audioSource = audioSourceGO.AddComponent<AudioSource>();
        audioSourceGO.SetActive(true);

        return audioSource;
    }

    public TextMeshPro SetUpTextMeshPro()
    {
        GameObject multimeterTextGameObj = new();
        multimeterTextGameObj.SetActive(false);

        TextMeshPro multimeterText = multimeterTextGameObj.AddComponent<TextMeshPro>();
        multimeterText.text = string.Empty;
        multimeterTextGameObj.SetActive(true);

        return multimeterText;
    }

    public TextMeshProUGUI SetUpTextMeshProUGUI()
    {
        GameObject textGO = new();
        textGO.SetActive(false);

        TextMeshProUGUI textMeshPro = textGO.AddComponent<TextMeshProUGUI>();
        textGO.SetActive(true);

        return textMeshPro;
    }

    public Interactable SetUpInteractable()
    {
        GameObject interactableGO = new GameObject();
        interactableGO.SetActive(false);

        Interactable interactable = interactableGO.AddComponent<Interactable>();
        interactable.MouseEnter = new();
        interactable.MouseExit = new();
        interactable.MouseDown = new();
        interactable.MouseUp = new();
        interactable.MouseClick = new();
        interactable.enabled = true;

        interactableGO.SetActive(true);
        return interactable;
    }

    public GameObject SetUpInteractableComponentGameObject()
    {
        GameObject interactableComponentGO = new GameObject();
        interactableComponentGO.SetActive(false);

        OutlineComponentOnHover interactableComponent = interactableComponentGO.AddComponent<OutlineComponentOnHover>();
        interactableComponent.ObjectToHighlight = SetUpOutlinableGO();

        Interactable interactable = interactableComponentGO.AddComponent<Interactable>();
        interactable.MouseEnter = new();
        interactable.MouseExit = new();
        interactable.MouseDown = new();
        interactable.MouseUp = new();
        interactable.MouseClick = new();
        interactable.enabled = true;

        interactableComponentGO.SetActive(true);

        return interactableComponentGO;
    }

    public Image SetUpImage()
    {
        GameObject imageGO = new();
        imageGO.SetActive(false);

        Image image = imageGO.AddComponent<Image>();
        imageGO.SetActive(true);

        return image;
    }

    public Button SetUpButton()
    {
        GameObject buttonGO = new();
        buttonGO.SetActive(false);

        Button button = buttonGO.AddComponent<Button>();
        buttonGO.SetActive(true);

        return button;
    }

    public GameObject SetUpButtonGO()
    {
        GameObject buttonGO = new();
        buttonGO.SetActive(false);

        Button button = buttonGO.AddComponent<Button>();
        buttonGO.SetActive(true);

        return buttonGO;
    }

    public AudioClip SetUpAudioClip()
    {
        AudioClip audioClip = AudioClip.Create("sampleForTesting", 100, 1, 1000, false);
        return audioClip;
    }

    public RectTransform SetUpRectTransform()
    {
        GameObject rectTransformGO = new();
        rectTransformGO.SetActive(false);

        RectTransform rectTransform = rectTransformGO.AddComponent<RectTransform>();
        rectTransformGO.SetActive(true);

        return rectTransform;
    }

    private GameObject SetUpOutlinableGO()
    {
        GameObject outlineGO = new();
        outlineGO.SetActive(false);

        outlineGO.AddComponent<Outlinable>();
        outlineGO.GetComponent<Outlinable>().OutlineLayer = 63;

        outlineGO.SetActive(true);

        return outlineGO;
    }

    public GameObject SetUpGameObject()
    {
        GameObject gameObject = new();
        gameObject.SetActive(true);

        return gameObject;
    }

    public Material SetUpMaterial(string name)
    {
        Material material = new Material(SetUpShader());
        material.name = name;

        return material;
    }

    public Shader SetUpShader()
    {
        return Shader.Find("Universal Render Pipeline/Lit");
    }
}