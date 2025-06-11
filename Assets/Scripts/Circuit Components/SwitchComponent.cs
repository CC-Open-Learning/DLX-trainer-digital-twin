using UnityEngine;
using UnityEngine.Events;
using VARLab.Interactions;

namespace VARLab.MPCircuits
{
    public class SwitchComponent : MonoBehaviour
    {
        [SerializeField] public Interactable switchModel;
        [SerializeField] public Animator switchModelAnimator;
        [SerializeField] public GameObject onSprite, offSprite;
        [SerializeField] public bool startingStateOfSwitch;

        public UnityEvent OnSwitchFlipped;

        private bool isOn = false;

        public bool IsOn => isOn;

        /// <summary>
        /// This start method allows the person to change the starting state of the switch by modifying it in the inspector
        /// </summary>
        public void Start()
        {
            if (startingStateOfSwitch)
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            isOn = !isOn;

            //play switch animation by setting animation controller state
            switchModelAnimator.Play(isOn ? "ON" : "OFF");

            //set the board sprites
            onSprite.SetActive(isOn);
            offSprite.SetActive(!isOn);

            OnSwitchFlipped?.Invoke();
        }
    }
}
