using SpiceSharp;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace VARLab.MPCircuits.Model
{
    class CircuitSolver
    {
        public EventHandler<ExportDataEventArgs> ExportDataEvent;

        private bool isSolvingACircuit;

        private List<CircuitData> solveRequests = new List<CircuitData>();

        public void SolveCircuit(Circuit ckt, float boardVoltageDC, bool ohmMeterConnected, double ohmMeterSourceVoltage)
        {
            var c = new CircuitData { BoardVoltageDC = boardVoltageDC, Circuit = ckt, IsOhmMeterConnected = ohmMeterConnected, OhmmeterSourceVoltage = ohmMeterSourceVoltage};
            solveRequests.Add(c);
            CheckForCircuitToSolve();
        }

        private void CheckForCircuitToSolve()
        {

            if (!isSolvingACircuit)
            {
                if (solveRequests.Count <= 0)
                    return;

                //UnityEngine.Debug.Log(solveRequests[0].BoardVoltageDC);

                CircuitData nextData = solveRequests[0];

                solveRequests.RemoveAt(0);

                SolveCircuitWithSpice(nextData);
            }
        }

        public void SolveCircuitWithSpice(CircuitData data)
        {
            // Create a DC sweep and register to the event for exporting simulation data
            //var dc = new DC("dc", "Battery", data.BoardVoltageDC, data.BoardVoltageDC, 1);

            DC dc;

            if (data.IsOhmMeterConnected)
            {
                dc = new DC("DC 1", new[]
                {
                    new ParameterSweep("Battery", new LinearSweep(data.BoardVoltageDC, data.BoardVoltageDC, 1)),
                    new ParameterSweep("Vohmmeter", new LinearSweep(data.OhmmeterSourceVoltage, data.OhmmeterSourceVoltage, 1))
                });
            } else
            {
                dc = new DC("DC 1", new[]
                {
                    new ParameterSweep("Battery", new LinearSweep(data.BoardVoltageDC, data.BoardVoltageDC, 1)),
                });
            }

            isSolvingACircuit = true;

            dc.ExportSimulationData += ExportDataEvent;
            dc.ExportSimulationData += CircuitSolveEnd;

            // Run the simulation
            try
            {
                dc.Run(data.Circuit);
            }
            catch (ValidationFailedException vfe)
            {
                UnityEngine.Debug.Log("Validation Failed Exception Occurred");
                foreach (var rule in vfe.Rules)
                {
                    UnityEngine.Debug.Log("Rule Violation " + rule.ToString() + " occurred " + 
                        rule.ViolationCount + " times");
                }
                CircuitSolveEnd(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log("Exception Occurred " + ex.ToString());
                CircuitSolveEnd(null, EventArgs.Empty);
            }
        }

        private void CircuitSolveEnd(object s, EventArgs e)
        {
            isSolvingACircuit = false;

            //UnityEngine.Debug.Log(isSolvingACircuit);
            CheckForCircuitToSolve();
        }

        public struct CircuitData
        {
            public Circuit Circuit;
            public float BoardVoltageDC;
            public bool IsOhmMeterConnected;
            public double OhmmeterSourceVoltage;
        }
    }
}