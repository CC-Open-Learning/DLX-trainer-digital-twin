using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VARLab.MPCircuits;

//*****************************************************************************************************************************************
//comment out the ConditionalIgnore line for normal test runs (otherwise it won't work) and uncomment it for code coverage report test runs 
//so the system tests don't skew line coverage %

//[TestFixture, ConditionalIgnore("IgnoreForCoverage", "This is a system test")]
public class UISystemTests : MonoBehaviour
{
    Camera mainCamera;
    ZoomInAndOut zoomInAndOut;

    // ======================================= SET UP ========================================
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        SceneManager.LoadScene("Digital Twin");
        yield return null; // Scene is loaded on next frame.

        mainCamera = FindObjectsOfType<Camera>().Where(x => x.name.Equals("Main Camera")).DefaultIfEmpty(null).FirstOrDefault();
        zoomInAndOut = FindObjectsOfType<ZoomInAndOut>().Where(x => x.name.Equals("Main Camera")).DefaultIfEmpty(null).FirstOrDefault();

        yield return null;
    }

    // ======================================= TESTS ========================================
    [Test]
    public void ConfirmSmallerScreenFOVIsNotBiggerThanZoomSliderMax()
    {
        //ensure the 8:9 aspect ratio won't reset to a different value after zooming in then fully back out again
        Assert.LessOrEqual(zoomInAndOut.SmallerScreenFOV, zoomInAndOut.ZoomSlider.maxValue);
    }

    [UnityTest]
    public IEnumerator ConfirmFieldOfViewForDifferentAspectRatios()
    {
        //set aspect ratio to 16:9
        mainCamera.aspect = 16f / 9f; //width divided by height
        yield return new WaitForSecondsRealtime(1f);
        Assert.AreEqual(zoomInAndOut.FullScreenFOV, mainCamera.fieldOfView);

        //set aspect ratio to 8:9
        mainCamera.aspect = 8f / 9f; //width divided by height
        yield return new WaitForSecondsRealtime(1f);
        Assert.AreEqual(zoomInAndOut.SmallerScreenFOV, mainCamera.fieldOfView);
    }
}
