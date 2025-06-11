using UnityEngine;

namespace VARLab.MPCircuits
{
    /// <summary>
    /// This scriptable object is used to store the build version
    /// for use in the watermark display and other purposes if needed
    /// </summary>
    public class BuildInfoSO : ScriptableObject
    {
        // string to hold the build version as we typically use a 
        // format such as MPC-24W4.0-Digital-Twin instead of just
        // a number
        public string buildVersion;
    }
}

