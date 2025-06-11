using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;


public class BuildMultipleScenes : EditorWindow
{
    static string projectIdentifier = "MPC";
    static string sprintNumber;
    static string projectTitle;
    static bool developmentBuild = false;

    // Regex to exrtact the scene name from the scene path
    static string pattern = @"/([^/]+).unity$";
    static Match match;

    // Properties used to build the scene name
    static string sceneName;
    static string dlxName;
    static string dt;

    // PlayerPerf keys
    const string manifestIdentifier = "Manifest_Identifier";
    const string courseTitle = "Course_Title";
    const string courseDescription = "Course_Description";
    const string scoTitle = "SCO_Title";

    /// <summary>
    /// Displays the Projects Settings window when the menu button is selected.
    /// </summary>
    [MenuItem("Build/Build Multiple Scenes")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<BuildMultipleScenes>("Project Setting");
    }

    /// <summary>
    /// Builds the Project Settings window in the editor where the project information
    /// will be captured.
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.Space();

        GUILayout.Label("Set up the project information", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        projectIdentifier = EditorGUILayout.TextField("Project Identifier: ", projectIdentifier);
        GUILayout.Label("Example: MPC", EditorStyles.miniLabel);

        EditorGUILayout.Space();

        sprintNumber = EditorGUILayout.TextField("Sprint Number: ", sprintNumber);
        GUILayout.Label("Example: 23S1.0", EditorStyles.miniLabel);

        EditorGUILayout.Space();

        developmentBuild = EditorGUILayout.Toggle("Development Build ", developmentBuild);

        EditorGUILayout.Space();

        if (GUILayout.Button("Continue"))
        {
            SceneBuild();
        }
    }

    /// <summary>
    /// Builds all the scenes that have been added to the build settings as individual
    /// scorm modules.
    /// </summary>
    public static void SceneBuild()
    {
        // List of scenes in the Build Settings
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

        // Set project name and sprint number
        dlxName = $"{projectIdentifier}-{sprintNumber.ToUpper()}";

        // Set the date of the build
        dt = DateTime.Today.ToString("dMMMyyyy", CultureInfo.CreateSpecificCulture("en-CA")).ToUpper();

        // Disables all the scenes in the build settings to ensure only one scene is build at a time
        foreach (var scene in scenes)
        {
            scene.enabled = false;
        }

        // Set up scene and build
        foreach (var scene in scenes)
        {
            SetSceneName(scene);
            SetPlayerPrefs();
            SetUpPlayer(scene);
        }
    }

    /// <summary>
    /// Set up the player and build the current scene
    /// </summary>
    /// <param name="scene"> current scene </param>
    private static void SetUpPlayer(EditorBuildSettingsScene scene)
    {
        BuildPlayerOptions buildPlayerOptions = new()
        {
            scenes = new[] { scene.path },
            locationPathName = "Builds/" + sceneName,
            target = BuildTarget.WebGL,
            options = developmentBuild ? BuildOptions.Development : BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"{sceneName} build succeeded: {summary.totalSize} bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }

        // Disable current scene before moving to next scene
        scene.enabled = false;
    }

    /// <summary>
    /// Set up the player prefs for the current scene
    /// </summary>
    private static void SetPlayerPrefs()
    {
        PlayerPrefs.SetString(manifestIdentifier, sceneName);
        PlayerPrefs.SetString(courseTitle, projectTitle);
        PlayerPrefs.SetString(courseDescription, "A CORE projected generated for WebGL");
        PlayerPrefs.SetString(scoTitle, "");
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Set up the sceneName of the scene that will be built
    /// </summary>
    /// <param name="scene"> current scene </param>
    /// <returns></returns>
    private static void SetSceneName(EditorBuildSettingsScene scene)
    {
        // enable scene that will be built
        scene.enabled = true;

        // Extract the scene name from the scene path
        match = Regex.Match(scene.path, pattern);

        projectTitle = match.Groups[1].Value;

        // Build the Scene name
        sceneName = $"{dlxName}-{dt}-{match.Groups[1].Value}";

        if (developmentBuild)
        {
            sceneName += "-Development";
        }
    }
}

