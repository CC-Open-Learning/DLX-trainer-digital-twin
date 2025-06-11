using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VARLab.Interfaces;
using VARLab.MPCircuits;
using Moq;

public class FadingIntegrationTests
{
    private ScreenFadeManager screenFadeManager;

    [SetUp]
    public void Setup()
    {
        IntegrationTestHelper.ClearScene();
    }

    [TearDown]
    public void Teardown()
    {
        GameObject.Destroy(GameObject.FindObjectOfType<ScreenFadeManager>());
        IntegrationTestHelper.ClearScene();
    }

    [UnityTest]
    public IEnumerator Check_If_IsFading_Stops_Multiple_Fading_Calls_When_Changed()
    {
        SetUpScreenFadeManager();

        for (int i = 0; i < 2; i++)
        {
            screenFadeManager.FadeAround(null);
        }

        Assert.AreEqual(true, screenFadeManager.isFading);

        yield return null;
    }

    private void SetUpScreenFadeManager()
    {
        GameObject screenFadeManagerGO = new();
        screenFadeManager = screenFadeManagerGO.AddComponent<ScreenFadeManager>();
        Image image = screenFadeManagerGO.AddComponent<Image>();
        screenFadeManager.fadePanelImage = image;
    }
}
