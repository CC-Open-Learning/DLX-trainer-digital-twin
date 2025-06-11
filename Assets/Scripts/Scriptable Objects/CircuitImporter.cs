using System;
using System.Collections.Generic;
using UnityEngine;
using static VARLab.MPCircuits.CableControls;

namespace VARLab.MPCircuits
{
    public class CircuitImporter : MonoBehaviour
    {
        // Allow for the circuit to be imported on start if set to true in the inspector
        public bool ImportCircuitOnStart = false;

        // Import cable controls script to add cables
        public CableControls Controls;

        // List of cables to be placed onto circuitboard
        public List<Cable> Cables;

        private void Start()
        {
            // Start import of cables to circuit
            if (ImportCircuitOnStart)
            {
                StartCircuitImport();
            }
        }

        /// <summary>
        /// This method starts importing cables onto the circuitboard from a list of cables
        /// </summary>
        public void StartCircuitImport()
        {
            // Remove restrictions to create as many cables as needed
            Controls.CanCreateCables(int.MaxValue);

            //for every cable in cables
            foreach (Cable c in Cables)
            {
                // Set cable color
                Controls.SetCableBundleColor(c.color);
                // Connect cable to starting port
                Controls.CreateAndSelectNewCable(c.startPort, c.isFaulty);
                // Finish cable connection on ending port
                Controls.FinishCableConnection(c.endPort);
            }
            // Remove currently seleted lead and show cursor
            Controls.CurrentlySelectedTransparentCableLead.SetActive(false);
            Controls.CurrentlySelectedTransparentCableLead = null;
            Cursor.visible = true;

            // Reset cable bundles
            Controls.ResetCableBundleState();
        }

        // Cable class with all required information to make a cable
        [Serializable]
        public struct Cable
        {
            public PortBehaviour startPort;
            public PortBehaviour endPort;
            public CableColors color;
            public bool isFaulty;
        }
    }
}
