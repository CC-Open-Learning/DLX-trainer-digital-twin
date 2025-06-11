using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using VARLab.DeveloperTools;
using VARLab.SCORM;

namespace VARLab.MPCircuits
{
    public class AnalyticsLogin : MonoBehaviour
    {
        public const string BuildNumberKey = "BuildNumber";
        public const string SceneNameKey = "Scene";
        public const string ProductionEnvKey = "Production";


        public BuildInfoSO BuildInfo;

        public string Username = "Development";
        public string DisplayName = "Development";

        private bool loginAttempt = false;
        private bool loginSuccess = false;

        /// <summary>
        ///     Initialize console commands for debugging
        /// </summary>
        public void Start()
        {
            CommandInterpreter.Instance?.Add(new ScormInfoCommand(GetInfo));
            CommandInterpreter.Instance?.Add(new AnalyticsDebugCommand(DebugLoginAttempt));
        }

        public void SuccessCallback(LoginResult result)
        {
            Debug.Log(result.ToString());
            loginSuccess = true;
        }

        public void ErrorCallback(PlayFabError error)
        {
            Debug.LogWarning(error.ToString());
        }

        public void HandleScormLogin(ScormManager.Event response)
        {
#if DEVELOPMENT_BUILD
            Debug.LogWarning("Cannot send Analytics events through the " +
                "SCORM login callback in 'development' context. Use 'Developer Console'" +
                "to manually fire events");
            return;
#endif

            // Must be an 'Initialized' event to load username
            if (response != ScormManager.Event.Initialized) { return; }

            AssignLoginID();
            FireLoginEvent();
        }


        public void AssignLoginID()
        {
            try
            {
                if (ScormManager.Initialized)
                {
                    Username = ScormManager.GetLearnerId();
                    DisplayName = ScormManager.GetLearnerName();
                    // Reading user's DisplayName from ScormManager but not including it in the event payload
                }
            }
            catch (NullReferenceException)
            {
                Debug.Log("SCORM was not able to properly initialize");
            }
        }

        public void FireLoginEvent(bool production = true)
        {
            LoginWithCustomIDRequest request = new()
            {
                CustomId = Username,
                CreateAccount = true,
                CustomTags = new()
                {
                    {BuildNumberKey, BuildInfo.buildVersion},
                    {SceneNameKey, SceneManager.GetActiveScene().name },
                    {ProductionEnvKey, production.ToString() }
                }
            };

            loginAttempt = true;
            PlayFabClientAPI.LoginWithCustomID(request, SuccessCallback, ErrorCallback, null, null);
        }

        /// <summary>
        ///     Returns a formatted string with information about 
        ///     the SCORM and Analytics context
        /// </summary>
        /// <returns>Formatted string with SCORM info, learner ID and name</returns>
        public string GetInfo()
        {
            StringBuilder builder = new();
            builder.Append($"Analytics:\n");
            builder.Append($"\tSent: {loginAttempt}\tReceived: {loginSuccess}\n\n");
            builder.Append($"SCORM:\t{ScormManager.Initialized}\n");
            builder.Append($"Learner:\t{Username}\n");
            builder.Append($"Name:\t{DisplayName}\n");
            return builder.ToString();
        }

        protected void DebugLoginAttempt(bool repeat = false)
        {
            // Only want to call repeated login attempts if
            // 'repeat' is set to true
            if (loginAttempt && !repeat)
            {
                Debug.LogWarning("Analytics login event will not be repeated.");
                return;
            }

            AssignLoginID();
            FireLoginEvent(production: false);
        }
    }
}
