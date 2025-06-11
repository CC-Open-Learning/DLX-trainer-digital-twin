using System;
using VARLab.DeveloperTools;

namespace VARLab.MPCircuits
{
    public class AnalyticsDebugCommand : ICommand
    {
        public const string LoginKeyword = "login";
        public const string ForceFlag = "-f";

        public readonly Action<bool> Callback;

        public string Name => "analytics";

        public string Usage => $"{Name} {LoginKeyword} [{ForceFlag}]";

        public string Description =>
            "Allows developers to initiate analytics events from the Unity Editor or Development builds";

        public AnalyticsDebugCommand(Action<bool> callback)
        {
            Callback = callback;
        }


        /// <summary>
        ///     Expecting the command "analytics login" 
        ///     in the command line
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool Execute(CommandEventArgs e)
        {
            if (e.Args.Length < 2)
            {
                e.Response = this.ErrorResponse();
                return false;
            }

            if (Callback == null)
            {
                e.Response = "No valid function available for Analytics login";
                return false;
            }

            // Attempt login event
            if (e.Args.Length == 2 
                && e.Args[1].Equals(LoginKeyword)) 
            {
                Callback(false);
                e.Response = $"Requested Analytics login. See Unity console for response";
                return true;
            }

            // Attempt login event with 'force' flag
            if (e.Args.Length == 3 
                && e.Args[1].Equals(LoginKeyword)
                && e.Args[2].Equals(ForceFlag))
            {
                Callback(true);
                e.Response = $"Requested Analytics login with force. See Unity console for response";
                return true;
            }

            e.Response = this.ErrorResponse();
            return false;
        }
    }
}
