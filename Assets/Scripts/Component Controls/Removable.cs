using UnityEngine;
using UnityEngine.Events;

namespace VARLab.MPCircuits
{

    /// <summary>
    ///     Provides functionality for an object to be 'removable'. 
    /// </summary>
    /// <remarks>
    ///     This class uses the two animation parameters 'Place' and 'Remove' in order 
    ///     to trigger the appropriate animations, and exposes events which notify when
    ///     the state of the object has changed between the 'Placed' and 'Removed' states
    /// </remarks>    
    public class Removable : MonoBehaviour
    {
        // Marked as static readonly since they are calculated values but need only be set once.
        // All animations should use these keys
        public static readonly int AnimationRemoveKey = Animator.StringToHash("Remove");
        public static readonly int AnimationPlaceKey = Animator.StringToHash("Place");

        public bool IsPlaced = true;

        public Animator Animator;

        [Header("Events")]
        public UnityEvent Placed;
        public UnityEvent Removed;


        /// <summary>
        ///     Ensure the object starts in the correct state according to
        ///     the <see cref="IsPlaced"/> boolean
        /// </summary>
        public void Start()
        {
            Animate();
            NotifyStateChanged();
        }


        public void Toggle()
        {
            IsPlaced = !IsPlaced;
            Animate();
            NotifyStateChanged();
        }

        /// <summary>
        ///     Sets the appropriate animation trigger based on the current "removed"
        ///     state of the object, causing the animation for placing or removing the
        ///     object to be played.
        /// </summary>
        public void Animate()
        {
            Animator.SetTrigger(IsPlaced ? AnimationPlaceKey : AnimationRemoveKey);
        }

        /// <summary>
        ///     Invoke the appropriate event based on the current "removed" state 
        ///     of the removable object.
        /// </summary>
        public void NotifyStateChanged()
        {
            (IsPlaced ? Placed : Removed)?.Invoke();
        }
    }
}
