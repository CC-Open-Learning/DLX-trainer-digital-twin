using EPOOutline;
using UnityEngine;

namespace VARLab.MPCircuits
{
    public static class HighlightInteractions
    {
        private static Color defaultOutlineColor = Color.yellow;

        [Tooltip("A layer that is included in the Outliner layer mask")]
        public static int VisibleLayer = 0;

        [Tooltip("A layer not included in the Outliner layer mask. " +
            "The Outline will be hidden when set to this layer.")]
        public static int HiddenLayer = 63;    // Default of 63 is the highest layer

        /// <summary>
        ///     If the received <paramref name="obj"/> has an <see cref="Outlinable"/>
        ///     component, the layer of the Outline is changed so that the layer is visible
        /// </summary>
        /// <param name="obj">A relevant GameObject</param>
        public static void ShowOutline(this GameObject obj, bool shown, Color? c = null)
        {
            if (!obj) { return; }

            var outline = obj.GetComponent<Outlinable>();

            if (!outline) { return; }

            outline.OutlineLayer = shown ? VisibleLayer : HiddenLayer;

            outline.OutlineParameters.Color = c.GetValueOrDefault(defaultOutlineColor);
        }

        public static void SetDefaultOutlineColor(Color _color)
        {
            defaultOutlineColor = _color;
        }
    }
}
