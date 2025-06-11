using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{
    public class DigitalTwinManager : MonoBehaviour
    {
        /// <summary> Data model responsible for performing circuit calculations </summary>
        private CircuitBoard _circuitBoard;

        [Header("Trainer Board Configuration")]
        public BoardVoltageSettings BoardVoltageSettings;
        public CableControls CableControls;
        public MultimeterComponent Multimeter;
        public MultimeterDial MultimeterDial;


        [Header("Components")]
        public LightbulbComponent L1;
        public LightbulbComponent L2;
        public DualCircuitLEDComponent L3Dual;
        public SwitchComponent ToggleSwitch;
        public SwitchComponent PushButtonSwitch;
        public FuseComponent F1;
        public RelayComponent RL1;
        public ElectronicFlasherComponent EF;
        public PotentiometerComponent POT;


        [Header("Events")]
        public UnityEvent OnCircuitBoardSolved;


        /// <summary>
        ///     Data model responsible for performing circuit calculations.
        /// </summary>
        /// <remarks>
        ///     The <see cref="CircuitBoard"/> is lazy-loaded so that no matter which component tries
        ///     to access it, it will always be available.
        /// </remarks>
        public CircuitBoard CircuitBoard
        {
            get
            {
                // Board voltage defined by BoardVoltageSettings (defaults to 2f)
                _circuitBoard ??= new CircuitBoard(BoardVoltageSettings.BoardVoltage);
                return _circuitBoard;
            }
            set => _circuitBoard = value;
        }


        private IEnumerator Start()
        {
            //allow the user to connect as many cables as possible for now
            CableControls.CanCreateCables(int.MaxValue);

            yield return null; //wait until all ports objects are loaded before solving circuit at start
        }

        private void OnEnable()
        {
            BoardVoltageSettings.OnBoardVoltageChanged.AddListener(TrySolveCircuit);
            POT.OnPOTResistanceChanged.AddListener(TrySolveCircuit);

            Multimeter.OnSettingChanged.AddListener(TrySolveCircuit);
            Multimeter.OnFuseStateChanged.AddListener(TrySolveCircuit);

            CableControls.OnCablePlaced.AddListener(PlaceCable);
            CableControls.OnCableDisconnected.AddListener(DeleteCable);

            F1.OnFusePlaced.AddListener(OnFusePlaced);
            F1.OnFuseBlown.AddListener(OnFuseBlownOrRemoved);
            F1.OnFuseRemoved.AddListener(OnFuseBlownOrRemoved);

            L1.OnLightbulbPlaced.AddListener(OnLightBulbL1Placed);
            L2.OnLightbulbPlaced.AddListener(OnLightBulbL2Placed);
            L3Dual.OnLightbulbPlaced.AddListener(OnLightBulbL3DualFilamentPlaced);

            L1.OnLightbulbRemoved.AddListener(OnLightBulbL1Removed);
            L2.OnLightbulbRemoved.AddListener(OnLightBulbL2Removed);
            L3Dual.OnLightbulbRemoved.AddListener(OnLightBulbL3DualFilamentRemoved);

            CircuitBoard.OnCircuitSolveEventHandler += OnCircuitSolved;

            ToggleSwitch.OnSwitchFlipped.AddListener(FlipToggleSwitch);
            PushButtonSwitch.OnSwitchFlipped.AddListener(PressPushButtonSwitch);

            RL1.OnRelayActuated.AddListener(TrySolveCircuit);

            EF.OnFlasherActuated.AddListener(TrySolveCircuit);
        }

        private void OnDisable()
        {
            BoardVoltageSettings.OnBoardVoltageChanged.RemoveListener(TrySolveCircuit);
            POT.OnPOTResistanceChanged.RemoveListener(TrySolveCircuit);

            Multimeter.OnSettingChanged.RemoveListener(TrySolveCircuit);
            Multimeter.OnFuseStateChanged.RemoveListener(TrySolveCircuit);

            CableControls.OnCablePlaced.RemoveListener(PlaceCable);
            CableControls.OnCableDisconnected.RemoveListener(DeleteCable);

            F1.OnFusePlaced.RemoveListener(OnFusePlaced);
            F1.OnFuseBlown.RemoveListener(OnFuseBlownOrRemoved);
            F1.OnFuseRemoved.RemoveListener(OnFuseBlownOrRemoved);

            L1.OnLightbulbPlaced.RemoveListener(OnLightBulbL1Placed);
            L2.OnLightbulbPlaced.RemoveListener(OnLightBulbL2Placed);
            L3Dual.OnLightbulbPlaced.RemoveListener(OnLightBulbL3DualFilamentPlaced);

            L1.OnLightbulbRemoved.RemoveListener(OnLightBulbL1Removed);
            L2.OnLightbulbRemoved.RemoveListener(OnLightBulbL2Removed);
            L3Dual.OnLightbulbRemoved.RemoveListener(OnLightBulbL3DualFilamentRemoved);

            CircuitBoard.OnCircuitSolveEventHandler -= OnCircuitSolved;

            ToggleSwitch.OnSwitchFlipped.RemoveListener(FlipToggleSwitch);
            PushButtonSwitch.OnSwitchFlipped.RemoveListener(PressPushButtonSwitch);

            RL1.OnRelayActuated.RemoveListener(TrySolveCircuit);

            EF.OnFlasherActuated.RemoveListener(TrySolveCircuit);
        }


        /// <summary>
        ///     "exception handling to come, right now it just stops crashing when spicesharp fails"
        ///     <br />
        ///     Yeah, this function really does need some error handling. 
        ///     See comments within <see cref="CircuitBoard.SolveCircuit"/>
        /// </summary>
        public void TrySolveCircuit()
        {
            try
            {
                CircuitBoard.SolveCircuit();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }


        [Obsolete("Fuse component can directly manipulate the FuseModel, making this redundant")]
        private void OnFusePlaced()
        {
            CircuitBoard.F1.IsConnected = true;

            TrySolveCircuit();
        }

        [Obsolete("Fuse component can directly manipulate the FuseModel, making this redundant")]
        private void OnFuseBlownOrRemoved()
        {
            CircuitBoard.F1.IsConnected = false;

            TrySolveCircuit();
        }

        [Obsolete("Incandescent bulbs can directly manipulate their respective data models, making this redundant")]
        private void OnLightBulbL1Placed()
        {
            CircuitBoard.L1.IsConnected = true;

            TrySolveCircuit();
        }

        [Obsolete("Incandescent bulbs can directly manipulate their respective data models, making this redundant")]
        private void OnLightBulbL2Placed()
        {
            CircuitBoard.L2.IsConnected = true;

            TrySolveCircuit();
        }

        [Obsolete("Dual Circuit bulb can directly manipulate its underlying data models, making this redundant")]
        private void OnLightBulbL3DualFilamentPlaced()
        {
            CircuitBoard.L3High.IsConnected = true;
            CircuitBoard.L3Low.IsConnected = true;

            TrySolveCircuit();
        }

        [Obsolete("Incandescent bulbs can directly manipulate their respective data models, making this redundant")]
        private void OnLightBulbL1Removed()
        {
            CircuitBoard.L1.IsConnected = false;

            TrySolveCircuit();
        }

        [Obsolete("Incandescent bulbs can directly manipulate their respective data models, making this redundant")]
        private void OnLightBulbL2Removed()
        {
            CircuitBoard.L2.IsConnected = false;

            TrySolveCircuit();
        }
        [Obsolete("Dual Circuit bulb can directly manipulate its underlying data models, making this redundant")]
        private void OnLightBulbL3DualFilamentRemoved()
        {
            CircuitBoard.L3Low.IsConnected = false;
            CircuitBoard.L3High.IsConnected = false;

            TrySolveCircuit();
        }

        [Obsolete("Switch component can directly manipulate its model, making this redundant")]
        private void FlipToggleSwitch()
        {
            CircuitBoard.SW1.IsConnected = ToggleSwitch.IsOn;

            TrySolveCircuit();
        }

        [Obsolete("Push button component can directly manipulate its model, making this redundant")]
        private void PressPushButtonSwitch()
        {
            CircuitBoard.PB1.IsConnected = PushButtonSwitch.IsOn;

            TrySolveCircuit();
        }

        private void PlaceCable(CableConnector cable)
        {
            if (cable.cableEnd.ConnectedPort.PortName == CircuitBoard.PortNames.None ||
                cable.cableStart.ConnectedPort.PortName == CircuitBoard.PortNames.None)
            {
                return;
            }

            //action -------------------------------------------------------------------
            CircuitBoard.PlaceCable(cable.CableID.ToString(),
                                  cable.cableStart.ConnectedPort.PortName,
                                  cable.cableEnd.ConnectedPort.PortName,
                                  cable.IsFaulty);

            TrySolveCircuit();
        }

        private void DeleteCable(CableConnector cable)
        {
            if (cable.cableEnd.ConnectedPort == null || cable.cableStart.ConnectedPort == null)
            {
                return;
            }

            if (cable.cableEnd.ConnectedPort.PortName == CircuitBoard.PortNames.None ||
                cable.cableStart.ConnectedPort.PortName == CircuitBoard.PortNames.None)
            {
                return;
            }

            CircuitBoard.RemoveCable(cable.CableID.ToString());

            TrySolveCircuit();
        }


        /// <summary>
        ///     We have a string of events and listeners such as OnCircuitSolved += CircuitSolved
        ///     which makes following the chain of events difficult.
        ///     
        ///     Events should be named for the state after an action has occurred, ie CircuitBoardSolved
        ///     Listeners should be named for their purpose and outcome, not the action they are listening for.
        ///     
        ///     It seems this function and its corresponding event OnCircuitBoardSolved are simply exposing the
        ///     event from the CircuitBoard itself. If this is the case, some renaming is likely required for clarity.
        /// </summary>
        private void OnCircuitSolved()
        {
            OnCircuitBoardSolved?.Invoke();
        }
    }
}
