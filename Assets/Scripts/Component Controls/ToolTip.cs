using UnityEngine;
using UnityEngine.UI;

namespace VARLab.MPCircuits
{
    public class ToolTip : MonoBehaviour
    {
        [SerializeField] Image enlargedImage;
        [SerializeField] Sprite spriteToShow;

        [SerializeField] SpriteRenderer minimizedSpriteRenderer;
        [SerializeField] Sprite spriteToMinimize;


        private void Awake()
        {
            enlargedImage = GetComponent<Image>();
            spriteToShow = enlargedImage.sprite;
            enlargedImage.sprite = null;
            SetImageAlpha(enlargedImage);

            if (minimizedSpriteRenderer == null) return;
            spriteToMinimize = minimizedSpriteRenderer.sprite;
            minimizedSpriteRenderer.sprite = null;
        }

        /// <summary>
        /// disables the sprite to minimize and enables the enlarged sprite. Used to replace a sprite that is too small with a larger copy
        /// </summary>
        /// <param name="enabled"></param>
        public void MinimizeAndEnlargeTargetSprites(bool enabled)
        {
            enlargedImage.sprite = null;
            minimizedSpriteRenderer.sprite = spriteToMinimize;

            SetImageAlpha(enlargedImage);
            if (enabled == false) return;

            enlargedImage.sprite = spriteToShow;
            SetImageAlpha(enlargedImage, 1);
            minimizedSpriteRenderer.sprite = null;
        }

        /// <summary>
        /// Enables a target sprite, more traditional tool tip
        /// </summary>
        /// <param name="enabled"></param>
        public void ShowSprite(bool enabled)
        {
            enlargedImage.sprite = null;
            if (enabled == false) return;

            enlargedImage.sprite = spriteToShow;
        }

        void SetImageAlpha(Image image, float alpha = 0)
        {
            Color tempColor = image.color;
            tempColor.a = alpha;
            image.color = tempColor;
        }

        public void SetToolTipState(bool enabled)
        {
            enlargedImage.enabled = !enabled;

            if (enabled == true)
            {
                minimizedSpriteRenderer.sprite = null;
            }
            else
            {
                minimizedSpriteRenderer.sprite = spriteToMinimize;
            }
        }
    }
}
