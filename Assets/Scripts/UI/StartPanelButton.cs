using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VARLab.MPCircuits
{
    public class StartPanelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] public Color baseColour;
        [SerializeField] public Color accentColour;

        [SerializeField] public Image buttonMain;
        [SerializeField] public Image buttonBorder;
        [SerializeField] public TextMeshProUGUI buttonText;
        public void OnPointerEnter(PointerEventData eventData)
        {
            buttonMain.color = accentColour;
            buttonBorder.color = baseColour;

            buttonText.color = baseColour;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            buttonMain.color = baseColour;
            buttonBorder.color = accentColour;

            buttonText.color = accentColour;
        }
    }
}
