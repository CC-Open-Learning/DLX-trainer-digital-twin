using System;
using UnityEngine;
using UnityEngine.UI;

namespace VARLab.MPCircuits
{
    public class MultimeterSliderOptionButton : MonoBehaviour
    {
        public static Action<MultimeterDialSettings, int> OnOptionButtonSelected;

        [SerializeField] public MultimeterDialSettings setting;
        [SerializeField] public int targetDialZRotation;
        [SerializeField] public GameObject SpriteHighlight;

        public MultimeterDialSettings Setting => setting;
        public int TargetDialZRotation => targetDialZRotation;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        public void OnButtonClick()
        {
            OnOptionButtonSelected?.Invoke(setting, targetDialZRotation);
        }

        public void TurnHighlight(bool isOn)
        {
            if (isOn)
            {
                SpriteHighlight.SetActive(true);
                return;
            }
            SpriteHighlight.SetActive(false);
        }
    }
}
