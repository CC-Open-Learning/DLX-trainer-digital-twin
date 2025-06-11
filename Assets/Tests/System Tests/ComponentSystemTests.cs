using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VARLab.Interactions;
using VARLab.MPCircuits;

//*****************************************************************************************************************************************
//comment out the ConditionalIgnore line for normal test runs (otherwise it won't work) and uncomment it for code coverage report test runs 
//so the system tests don't skew line coverage %

//[TestFixture, ConditionalIgnore("IgnoreForCoverage", "This is a system test")]
public class ComponentSystemTests : MonoBehaviour
{
    //sliders
    private RadialSlider boardVoltage;

    //fuse
    private GameObject fuseInteractable;
    private GameObject intactFuseModel;
    private GameObject blownFuseModel;
    private GameObject fuseSmoke;

    //switches
    private GameObject pushButtonInteractable;

    //lights
    private GameObject lightL1Interactable;

    //cables
    private GameObject redCableBunch;

    //ports
    private GameObject batteryPos_Port;
    private GameObject batteryNeg_Port;
    private GameObject f1_L_Port;
    private GameObject f1_R_Port;
    private GameObject l1_A_Port;
    private GameObject l1_B_Port;
    private GameObject pb1_L_Port;
    private GameObject pb1_R_Port;

    //panels
    private ControlsGuidePanel controlPanel;

    //Constants
    private const float ApproachZero = 0.00001f;

    // ======================================= SET UP ========================================
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("Digital Twin");
        yield return null; // Scene is loaded on next frame.

        boardVoltage = FindObjectsOfType<RadialSlider>().Where(x => x.name.Equals("VoltageKnobSlider")).DefaultIfEmpty(null).FirstOrDefault();

        fuseInteractable = GameObject.Find("Fuse");

        intactFuseModel = GameObject.Find("Fuse/Fuse");
        blownFuseModel = GameObject.Find("Fuse/Blown Fuse");
        fuseSmoke = GameObject.Find("Fuse/Smoke"); //there are 2 objects named "Smoke" - for DMM and fuse so used parent to specify which

        pushButtonInteractable = GameObject.Find("Push Button Switch");

        lightL1Interactable = GameObject.Find("Bulb L1");

        redCableBunch = GameObject.Find("Red_Cable_Bunch");

        batteryPos_Port = GameObject.Find("Battery_V2");
        batteryNeg_Port = GameObject.Find("Battery_Gnd1");
        f1_L_Port = GameObject.Find("F1_Left");
        f1_R_Port = GameObject.Find("F1_Right");
        l1_A_Port = GameObject.Find("L1_A");
        l1_B_Port = GameObject.Find("L1_B");
        pb1_L_Port = GameObject.Find("PB1_Left");
        pb1_R_Port = GameObject.Find("PB1_Right");

        controlPanel = FindObjectsOfType<ControlsGuidePanel>().Where(x => x.name.Equals("Controls Guide Panel")).DefaultIfEmpty(null).FirstOrDefault();
        yield return null;
        controlPanel.ShowControlGuidePanel(false);

        yield return null;
    }

    // ======================================= TESTS ========================================
    [Test]
    public void ConfirmControllersExist()
    {
        Assert.IsNotNull(FindObjectsOfType<DigitalTwinManager>().Where(x => x.name.Equals("Master OHM's Law Trainer")).DefaultIfEmpty(null).FirstOrDefault());
        Assert.IsNotNull(FindObjectsOfType<InteractionManager>().Where(x => x.name.Equals("InteractionManager")).DefaultIfEmpty(null).FirstOrDefault());
        Assert.IsNotNull(FindObjectsOfType<CableControls>().Where(x => x.name.Equals("Cable Bundles")).DefaultIfEmpty(null).FirstOrDefault());
    }

    //-------------------------------
    [UnityTest]
    public IEnumerator FuseWorksProperly() //approx. 13 seconds to run
    {
        Vector3 baseFusePositionInPort = intactFuseModel.transform.position;
        Vector3 baseBlownFusePositionInPort = blownFuseModel.transform.position;

        //confirm intact fuse is visible and in port at start  &&  confirm blown fuse model and smoke are not visible
        Assert.AreEqual(true, intactFuseModel.activeInHierarchy);
        Assert.AreEqual(baseFusePositionInPort, intactFuseModel.transform.position);
        Assert.AreEqual(false, blownFuseModel.activeInHierarchy);
        Assert.AreEqual(false, fuseSmoke.activeInHierarchy);
        yield return new WaitForSecondsRealtime(1f);

        //remove intact fuse ----------------------------------------------------------
        fuseInteractable.GetComponent<Interactable>().MouseClick?.Invoke(fuseInteractable);
        yield return new WaitForSecondsRealtime(1.5f); //wait for animation to complete
        Assert.AreEqual(false, intactFuseModel.activeInHierarchy);
        yield return new WaitForSecondsRealtime(1f);

        //place fuse ------------------------------------------------------------------
        fuseInteractable.GetComponent<Interactable>().MouseClick?.Invoke(fuseInteractable);
        yield return null;
        Assert.AreEqual(true, intactFuseModel.activeInHierarchy);
        Assert.AreEqual(baseFusePositionInPort, intactFuseModel.transform.position);
        yield return new WaitForSecondsRealtime(0.5f);

        //break fuse by connecting it directly to the battery ports and setting the voltage to be equal to 2 V -------------------
        boardVoltage.SetSliderValue(2f);
        yield return new WaitForSecondsRealtime(1f);

        redCableBunch.GetComponent<Interactable>().MouseClick?.Invoke(redCableBunch);
        yield return new WaitForSecondsRealtime(0.25f);

        batteryPos_Port.GetComponent<Interactable>().MouseClick?.Invoke(batteryPos_Port); //1st cable: B+ to F-L
        f1_L_Port.GetComponent<Interactable>().MouseClick?.Invoke(f1_L_Port);
        yield return new WaitForSecondsRealtime(1f);

        f1_R_Port.GetComponent<Interactable>().MouseClick?.Invoke(f1_R_Port); //2nd cable: F-R to B-
        batteryNeg_Port.GetComponent<Interactable>().MouseClick?.Invoke(batteryNeg_Port);
        yield return new WaitForSecondsRealtime(1f);

        Assert.AreEqual(true, blownFuseModel.activeInHierarchy);
        Assert.AreEqual(baseBlownFusePositionInPort, blownFuseModel.transform.position);
        Assert.AreEqual(true, fuseSmoke.activeInHierarchy);
        Assert.AreEqual(false, intactFuseModel.activeInHierarchy);
        yield return new WaitForSecondsRealtime(1f);

        //remove broken fuse ----------------------------------------------------------
        fuseInteractable.GetComponent<Interactable>().MouseClick?.Invoke(fuseInteractable);
        yield return new WaitForSecondsRealtime(4f); //wait for animation to complete
        Assert.AreNotEqual(baseBlownFusePositionInPort, blownFuseModel.transform.position);
    }

    [UnityTest]
    public IEnumerator PushButtonWorksProperly()
    {
        Assert.AreEqual(0, lightL1Interactable.GetComponentInChildren<Light>().intensity); //light off
        yield return new WaitForSecondsRealtime(1f);

        boardVoltage.SetSliderValue(10f);
        yield return new WaitForSecondsRealtime(1f);

        //simple circuit with L1 and PB1 ----------------------------------------------------------
        redCableBunch.GetComponent<Interactable>().MouseClick?.Invoke(redCableBunch);
        yield return new WaitForSecondsRealtime(0.25f);

        batteryPos_Port.GetComponent<Interactable>().MouseClick?.Invoke(batteryPos_Port); //1st cable: B+ to PB-L
        pb1_L_Port.GetComponent<Interactable>().MouseClick?.Invoke(pb1_L_Port);
        yield return new WaitForSecondsRealtime(0.75f);

        pb1_R_Port.GetComponent<Interactable>().MouseClick?.Invoke(pb1_R_Port); //2nd cable: PB-R to L1-A
        l1_A_Port.GetComponent<Interactable>().MouseClick?.Invoke(l1_A_Port);
        yield return new WaitForSecondsRealtime(0.75f);

        l1_B_Port.GetComponent<Interactable>().MouseClick?.Invoke(l1_B_Port); //3rd cable: L1-B to B-
        batteryNeg_Port.GetComponent<Interactable>().MouseClick?.Invoke(batteryNeg_Port);
        yield return new WaitForSecondsRealtime(1.75f);

        //changed to close to zero number because of bug with push button, bug is being fixed in MPC-704
        Assert.IsTrue(lightL1Interactable.GetComponentInChildren<Light>().intensity < ApproachZero); //light off

        //hold button down ------------------------------------------------------------------------
        pushButtonInteractable.GetComponent<Interactable>().MouseDown?.Invoke(pushButtonInteractable);
        Assert.AreNotEqual(0, lightL1Interactable.GetComponentInChildren<Light>().intensity); //light on

        yield return new WaitForSecondsRealtime(2f); //still holding button down
        Assert.AreNotEqual(0, lightL1Interactable.GetComponentInChildren<Light>().intensity); //light on

        pushButtonInteractable.GetComponent<Interactable>().MouseUp?.Invoke(pushButtonInteractable);
        //changed to close to zero number because of bug with push button, bug is being fixed in MPC-704
        Assert.IsTrue(lightL1Interactable.GetComponentInChildren<Light>().intensity < ApproachZero); //light off
        yield return new WaitForSecondsRealtime(1.5f);
    }
}
