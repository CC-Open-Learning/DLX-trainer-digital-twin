using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace VARLab.MPCircuits
{
    public class ScreenFadeManager : MonoBehaviour
    {
        [SerializeField] public Image fadePanelImage;
        [SerializeField] private float fadeTime = 1f;
        public bool isFading;

        public float FadeTime { get => fadeTime; set => fadeTime = value; }

        public static ScreenFadeManager Instance { get; private set; }
        public void FadeAround(Action a)
        {
            if (!isFading)
            {
                isFading = true;
                StartCoroutine(FadeAroundCoroutine(a));
            }
        }

        public IEnumerator FadeAroundCoroutine(Action a)
        {
            yield return StartCoroutine(FadeCoroutine(true));
            a();
            yield return new WaitForSeconds(0.25f);
            yield return StartCoroutine(FadeCoroutine(false));
        }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        IEnumerator FadeCoroutine(bool fadingToBlack)
        {

            fadePanelImage.raycastTarget = true;

            float elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                fadePanelImage.color = new Color(
                    fadePanelImage.color.r,
                    fadePanelImage.color.g,
                    fadePanelImage.color.b,
                    Mathf.Lerp(0, 1, fadingToBlack ? elapsedTime / fadeTime : 1 - elapsedTime / fadeTime)
                );
                yield return null;
            };

            if (!fadingToBlack)
                isFading = false;
            
            fadePanelImage.raycastTarget = false;

            yield return 0;
        }

    }
}
