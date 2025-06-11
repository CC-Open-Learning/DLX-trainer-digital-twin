using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using VARLab.SCORM.Editor;

namespace VARLab.MPCircuits
{
    [InitializeOnLoad]
    public class BuildInfoManager : IPreprocessBuildWithReport
    {
        // returns the callballOrder for callbacks.  Not strictly needed here
        // but included in case other build preprocesses are added later
        public int callbackOrder { get { return 0; } }

        // Get the ScriptableObject asset path
        private const string _buildInfoPath = "Assets/Settings/BuildInfo.asset";

        // static constructor to resolve warnings
        static BuildInfoManager()
        {
        }

        /// <summary>
        /// This method gets the current build version (from the build player settings)
        /// and saves it to a scriptable object asset.  If the asset doesn't exist, it 
        /// will also create the asset first.
        /// </summary>
        /// <param name="report"></param>
        public void OnPreprocessBuild(BuildReport report)
        {
            // Load the ScriptableObject asset
            BuildInfoSO buildInfo = AssetDatabase.LoadAssetAtPath<BuildInfoSO>(_buildInfoPath);

            // If the ScriptableObject asset doesn't exist, create a new one
            if (buildInfo == null)
            {
                buildInfo = ScriptableObject.CreateInstance<BuildInfoSO>();
                AssetDatabase.CreateAsset(buildInfo, _buildInfoPath);
            }

            // Update the build version
            if (buildInfo != null)
            {
                buildInfo.buildVersion = PlayerPrefs.GetString(ScormProperties.ManifestIdentifier);
                EditorUtility.SetDirty(buildInfo);
            }
            else
            {
                Debug.LogError("Unable to write to Build Info asset");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


    }
}
