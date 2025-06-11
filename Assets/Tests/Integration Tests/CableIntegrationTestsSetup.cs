using EPOOutline;
using UnityEngine;
using UnityEngine.UI;
using VARLab.Interactions;
using VARLab.MPCircuits;

public class CableIntegrationTestsSetup
{
    //set up objects/classes -----------------------------------------------------------------------
    public GameObject SetUpTransparentLead()
    {
        GameObject transparentLeadGO = new();
        transparentLeadGO.SetActive(true);
        return transparentLeadGO;
    }

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
        _cableControl.blackCablePrefab = new();
        _cableControl.transparentRedCableLeadPrefab = new(); //instantiated in start() method
        _cableControl.transparentBlackCableLeadPrefab = new();

        _cableControl.interactionManager = SetUpInteractionManager();

        _cableControl.redCableBundle.AddComponent<Outlinable>();
        _cableControl.blackCableBundle.AddComponent<Outlinable>();

        _cableControl.OnCableDeleted = new();
        _cableControl.OnCablePlaced = new();
        _cableControl.OnCableDisconnected = new();

        cableControlGO.SetActive(true);

        return _cableControl;
    }

    public CableConnector SetUpCableConnector(ref CableLead leadStart, ref CableLead leadEnd, ref LineRenderer line)
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

    public PortBehaviour SetUpPort(string name = "New Game Object")
    {
        GameObject portGameObj = new GameObject(name);
        portGameObj.SetActive(false);
        portGameObj.AddComponent<Interactable>();

        PortBehaviour port = portGameObj.AddComponent<PortBehaviour>();
        portGameObj.GetComponent<Interactable>().MouseClick = new();
        portGameObj.GetComponent<Interactable>().MouseEnter = new();
        portGameObj.GetComponent<Interactable>().MouseExit = new();
        portGameObj.SetActive(true);

        return port;
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

    public Interactable SetUpInteractable()
    {
        GameObject interactableGO = new();
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
        _object.gameObject.GetComponent<RadialSlider>().slider = new GameObject().AddComponent<Slider>();
        _object.gameObject.GetComponent<RadialSlider>().slider.value = value;
        _object.gameObject.GetComponent<RadialSlider>().slider.minValue = minValue;
        _object.gameObject.GetComponent<RadialSlider>().slider.maxValue = maxValue;
        _object.gameObject.GetComponent<RadialSlider>().knobRotationRoot = new GameObject().transform;
        _object.gameObject.GetComponent<RadialSlider>().minKnobRotationValue = minKnobRotationValue;
        _object.gameObject.GetComponent<RadialSlider>().maxKnobRotationValue = maxKnobRotationValue;
        _object.gameObject.GetComponent<RadialSlider>().background = new GameObject().AddComponent<Image>();
        _object.gameObject.GetComponent<RadialSlider>().baseUI = new();
        _object.gameObject.GetComponent<RadialSlider>().sliderUI = new();
    }
}
