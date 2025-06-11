using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VARLab.Interactions;

namespace VARLab.MPCircuits
{
    public class InteractionManager : MonoBehaviour
    {
        public List<Interactable> ports, multimeter, boardVoltage, switchToggle, fuse, potentiometers, cableBundles, lights;
        public Interactable boardVoltageSlider, potSlider, multimeterDial;
        public Color InteractableHoverColor, SelectedObjectOutlineColor;

        [NonSerialized] public bool InteractableComponentsEnabled = true;
        [NonSerialized] public List<Interactable> OutlinableComponents = new List<Interactable>();

        private void Start()
        {
            EnableAllInteractions(true);
            HighlightInteractions.SetDefaultOutlineColor(SelectedObjectOutlineColor);
            OutlinableComponents = FindObjectsOfType<Interactable>().Where(x => x.GetComponent<OutlineComponentOnHover>() != null).ToList();
        }

        public void ShowOutlineForHoveredState(GameObject gameObject)
        {
            gameObject.ShowOutline(true, InteractableHoverColor);
        }

        public void ShowOutlineForSelectedState(GameObject gameObject)
        {
            gameObject.ShowOutline(true, SelectedObjectOutlineColor);
        }

        public void HideOutline(GameObject gameObject)
        {
            gameObject.ShowOutline(false);
        }

        public void ToggleComponentInteractions(bool enabled)
        {
            InteractableComponentsEnabled = enabled;

            foreach (var component in OutlinableComponents)
            {
                component.enabled = enabled;
            }
        }

        public void EnableAllInteractions(bool enabled)
        {
            EnablePortInteractions(enabled);
            EnableBoardVoltageInteractions(enabled);
            EnableMultimeterInteractions(enabled);
            EnableSwitchInteractions(enabled);
            EnableFuseInteractions(enabled);
            EnablePOTInteractions(enabled);
        }

        public void EnableSwitchInteractions(bool enabled)
        {
            foreach (var sw in switchToggle)
            {
                sw.enabled = enabled;
            }
        }

        public void EnableFuseInteractions(bool enabled)
        {
            foreach (var f in fuse)
            {
                f.enabled = enabled;
            }
        }

        public void EnablePOTInteractions(bool enabled)
        {
            foreach (var f in potentiometers)
            {
                potSlider.gameObject.SetActive(true);
                f.enabled = enabled;
            }

            if (!enabled)
            {
                potSlider.enabled = false;
                potSlider.gameObject.GetComponent<RadialSlider>().SetUIActiveState(false);
            }
        }

        public void EnablePortInteractions(bool enabled)
        {
            foreach (var port in ports)
            {
                port.GetComponent<PortBehaviour>().EnableInteraction(enabled);
            }

            foreach (var bundle in cableBundles)
            {
                bundle.enabled = enabled;
            }
        }

        public void EnableBoardVoltageInteractions(bool enabled)
        {
            foreach (var boardVoltages in boardVoltage)
            {
                boardVoltageSlider.gameObject.SetActive(true);
                boardVoltages.enabled = enabled;
            }

            if (!enabled)
            {
                boardVoltageSlider.enabled = false;
                boardVoltageSlider.gameObject.GetComponent<RadialSlider>().SetUIActiveState(false);
            }
        }

        public void EnableMultimeterInteractions(bool enabled)
        {
            foreach (var multimeters in multimeter)
            {
                //raycast is enabled and disabled to make sure ports under multimeter are accessible
                multimeterDial.gameObject.GetComponent<Image>().raycastTarget = true;
                multimeters.enabled = enabled;
            }

            if (!enabled)
            {
                multimeterDial.enabled = false;
                multimeterDial.gameObject.GetComponent<RadialSlider>().SetUIActiveState(false);
                multimeterDial.gameObject.GetComponent<Image>().raycastTarget = false;
            }
        }
    }
}
