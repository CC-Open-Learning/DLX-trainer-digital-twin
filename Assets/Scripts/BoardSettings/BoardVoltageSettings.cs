using UnityEngine;
using UnityEngine.Events;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{
    public class BoardVoltageSettings : MonoBehaviour
    {
        /// <summary> 
        ///     The default board voltage value. 
        ///     All simulations will use this unless otherwise specified
        /// </summary>
        public const float DefaultBoardVoltage = 2f;


        public float BoardVoltage = DefaultBoardVoltage;

        public RadialSlider BoardVoltageSlider;

        public UnityEvent OnBoardVoltageChanged = new();

        private BatteryModel model;

        private void Start()
        {
            // Only one Battery, so it can be referenced directly.
            // In theory, could be injected by DigitalTwinManager
            model ??= FindFirstObjectByType<DigitalTwinManager>().CircuitBoard.Battery;

            if (BoardVoltageSlider)
            {
                BoardVoltageSlider.SetSliderValue(BoardVoltage);
            }
        }

        public void SetBoardVoltage(float boardVoltage)
        {
            if (model == null) { return; }

            model.BoardVoltage = boardVoltage;
            OnBoardVoltageChanged?.Invoke();
        }
    }
}
