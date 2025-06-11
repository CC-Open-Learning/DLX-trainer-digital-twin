using System;
using UnityEngine;

namespace VARLab.MPCircuits
{
    public class OutlineComponentOnHover : MonoBehaviour
    {
        public GameObject ObjectToHighlight;

        [NonSerialized] public InteractionManager InteractionManager; //public for testing

        private void Start()
        {
            InteractionManager = FindObjectOfType<InteractionManager>();
        }

        public void OnMouseEnter()
        {
            if (InteractionManager.InteractableComponentsEnabled == true)
            {
                InteractionManager.ShowOutlineForHoveredState(ObjectToHighlight);
            }
        }

        public void OnMouseExit()
        {
            InteractionManager.HideOutline(ObjectToHighlight);
        }
    }
}
