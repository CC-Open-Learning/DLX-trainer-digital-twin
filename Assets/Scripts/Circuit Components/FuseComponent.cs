using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VARLab.Interactions;
using VARLab.MPCircuits.Model;

namespace VARLab.MPCircuits
{
    /// <summary>
    /// Controls the logic for the animations, sounds, and sprites of the fuse component and diagram
    /// </summary>
    public class FuseComponent : MonoBehaviour
    {
        private const float CurrentThresholdAmps = 3f;

        [Tooltip("The interactable script used by the fuse")]
        [SerializeField] private Interactable _interactable;
        [Tooltip("The name of the component")]
        [SerializeField] private CircuitBoard.ComponentNames _componentName;

        [Tooltip("The audio clip that plays when the fuse is blown")]
        public AudioClip BlownFuseSound;
        private float BlownFuseSoundVolume = 0.75f;
        public AudioSource SoundEffectSource;
        [Tooltip("The animation controller for the fuse")]
        public Animator FuseAnimator;
        [Tooltip("The game object for the on sprite of the fuse")]
        public GameObject OnSprite;
        [Tooltip("The game object for the off sprite of the fuse")]
        public GameObject OffSprite;
        [Tooltip("Is fuse component defective at the start of the simulation?")]
        public bool IsDefective;

        [HideInInspector] public bool IsFuseRemoved;
        [HideInInspector] public bool FuseIsBlown = false;

        public FuseModel fuseModel;

        public readonly int PlaceFuseHash = Animator.StringToHash("PlaceFuse");
        public readonly int RemoveFuseHash = Animator.StringToHash("RemoveFuse");
        public readonly int RemoveDefectiveFuseHash = Animator.StringToHash("RemoveBrokenFuse");
        public readonly int BlowFuseHash = Animator.StringToHash("BlowFuse");
        public readonly int FlashFuseHash = Animator.StringToHash("FlashFuse");

        public UnityEvent OnFusePlaced;
        public UnityEvent OnFuseRemoved;
        public UnityEvent OnFuseBlown;

        /// <summary>
        /// Initilizes the fuse model if it is null, checks if the fuse is defective and sets the closed variable correctly, and
        /// adds the method "CheckFuseStatus" from the fuse models action "OnValuesUpdated"
        /// </summary>
        public void Awake()
        {
            if (fuseModel == null)
            {
                fuseModel = FindFirstObjectByType<DigitalTwinManager>().CircuitBoard.GetComponent(_componentName) as FuseModel;
            }

            fuseModel.IsDefective = IsDefective;

            fuseModel.OnValuesUpdated += CheckFuseStatus;
        }

        /// <summary>
        /// Removes the method "CheckFuseStatus" from the fuse models action "OnValuesUpdated"
        /// </summary>
        private void OnDisable()
        {
            fuseModel.OnValuesUpdated -= CheckFuseStatus;
        }

        /// <summary>
        /// Checks the status of the fuse by looking at if the fuse model current is greater then the current threshold amps
        /// and if the fuse is not removed. This will break the fuse and play and update the correct information to reflect that
        /// </summary>
        /// <param name="circuitComponentModel">The model used by the fuse model action "OnValuesUpdated"</param>
        public void CheckFuseStatus(CircuitComponentModel circuitComponentModel)
        {
            if (fuseModel.Current >= CurrentThresholdAmps && !IsFuseRemoved)
            {
                FuseIsBlown = true;
                PlayBlowFuseAnimation();
                Debug.Log("Fuse Broken");
                OnFuseBlown?.Invoke();
            }
        }

        /// <summary>
        /// Plays the blown fuse animation, plays the sound for the blown fuse, and updates the image to be disconnected
        /// </summary>
        public void PlayBlowFuseAnimation()
        {
            StartCoroutine(BlowFuse());
            SoundEffectSource.PlayOneShot(BlownFuseSound, BlownFuseSoundVolume);
            UpdateConnectedFuseImage(false);
        }

        /// <summary>
        /// Plays the animations for the flash and the breaking of the fuse when it is short circuited
        /// </summary>
        /// <returns>This waits 0.1 seconds before transitioning between the flash animation and the blow animation</returns>
        private IEnumerator BlowFuse()
        {
            FuseAnimator.SetTrigger(FlashFuseHash);  // triggers the "flash" effect when a fuse blows
            yield return new WaitForSeconds(0.1f);
            FuseAnimator.SetTrigger(BlowFuseHash);
        }

        /// <summary>
        /// When the fuse collider is clicked, place or remove the fuse. If the fuse is defective then move past the broken fuse 
        /// animations and go to the animation to place the fuse into the jar of shame
        /// </summary>
        public void UpdateFuseState()
        {
            if (!IsFuseRemoved)
            {
                if (IsDefective)
                {
                    FuseAnimator.SetTrigger(RemoveDefectiveFuseHash);
                    IsDefective = false;
                    fuseModel.IsDefective = false;
                }
                else
                {
                    FuseAnimator.SetTrigger(RemoveFuseHash);
                }

                IsFuseRemoved = true;

                Debug.Log("Fuse Removed");
                OnFuseRemoved?.Invoke();
            }
            else
            {
                FuseAnimator.SetTrigger(PlaceFuseHash);
                IsFuseRemoved = false;

                Debug.Log("Fuse Placed");
                OnFusePlaced?.Invoke();
            }

            UpdateConnectedFuseImage(!IsFuseRemoved);

            StartCoroutine(DisableUntilAnimationIsComplete(IsFuseRemoved));
        }

        /// <summary>
        /// Changes the fuse squiggle diagram(below fuse) between broken/unbroken depending on current state
        /// </summary>
        /// <param name="connected">is the fuse connected from the circuit</param>
        public void UpdateConnectedFuseImage(bool connected)
        {
            OnSprite.SetActive(connected ? true : false);  //fuse connected                          -> squiggle connected
            OffSprite.SetActive(connected ? false : true); //fuse is removed/disconnected/broken     -> squiggle disconnected
        }

        /// <summary>
        /// Disables the interactable and waits until the currently playing animation clip is complete before re-enabling it
        /// </summary>
        private IEnumerator DisableUntilAnimationIsComplete(bool isFuseRemoved)
        {
            float timeToWait = FuseAnimator.GetCurrentAnimatorStateInfo(0).length;

            GetComponent<CapsuleCollider>().enabled = false;
            yield return new WaitForSeconds(timeToWait);
            GetComponent<CapsuleCollider>().enabled = true;
        }
    }
}
