using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VARLab.MPCircuits
{
    public class EnableCursorOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        bool previousCursorEnabled = true;

        public void OnPointerEnter(PointerEventData eventData)
        {
            previousCursorEnabled = Cursor.visible;
            Cursor.visible = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ShowCursor();
        }

        void ShowCursor()
        {
            CableControls c = FindObjectOfType<CableControls>();

            if (c == null) { return; }

            Cursor.visible = !c.CanPlaceCables || !c.IsCableColorSelected;
        }

        public void OnDisable()
        {
            ShowCursor();
        }
    }
}
