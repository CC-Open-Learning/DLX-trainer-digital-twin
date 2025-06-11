using UnityEngine;
using UnityEngine.Events;

namespace VARLab.MPCircuits.Model
{
    public class RelayComponent : MonoBehaviour
    {
        public RelayModel relayModel;
        public UnityEvent OnRelayActuated;

        public void Start()
        {
            //assigns the relay model on start by accessing the created instance through the CircuitBoard
            if (relayModel == null)
            {
                relayModel = FindFirstObjectByType<DigitalTwinManager>()?.CircuitBoard.GetComponent(CircuitBoard.ComponentNames.RL1) as RelayModel;
            }

            //whenever the relay model port values are updated, trys to solve the circuit
            relayModel.OnValuesUpdated += VerifyActuatedState;
        }

        private void OnDisable()
        {
            relayModel.OnValuesUpdated -= VerifyActuatedState;
        }

        private void VerifyActuatedState(CircuitComponentModel model)
        {
            OnRelayActuated?.Invoke();  //Invocation calls TrySolveCircuit in DigitalTwinManager.cs
        }

    }
}
