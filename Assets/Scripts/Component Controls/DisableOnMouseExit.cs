using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VARLab.MPCircuits
{
    public class DisableOnMouseExit : MonoBehaviour, IPointerExitHandler
    {
        public void OnPointerExit(PointerEventData eventData)
        {
            this.gameObject.SetActive(false);
        }
    }
}
