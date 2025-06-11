using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VARLab.Interactions;

namespace VARLab.MPCircuits
{
    public class RadialSliderKnob: MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RadialSlider Parent;
        [SerializeField] private Slider slider;

        // Called when the knob is being dragged.
        public void OnDrag(PointerEventData eventData)
        {
            Parent.OnDrag(eventData);
        }

        // Called when the knob drag ends.
        public void OnEndDrag(PointerEventData eventData)
        {
            Parent.OnEndDrag(eventData);

            if (!eventData.fullyExited) return;

            if(Parent.gameObject.GetComponent<MultimeterSliderUI>() != null )
            {
                StartCoroutine(WaitForMultimeterToFinishAnimation(Parent.gameObject.GetComponent<MultimeterSliderUI>()));
            }
            else
            {
                Parent.DisableOnMouseExit();
            }
        }

        IEnumerator WaitForMultimeterToFinishAnimation(MultimeterSliderUI multimeterSliderUI)
        {
            yield return new WaitUntil(() => multimeterSliderUI.currentAnimation == null);
            Parent.DisableOnMouseExit();
        }
    }
}