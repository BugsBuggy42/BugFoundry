namespace SchwiftyUI.V3.Inputs
{
    using Enums;
    using UnityEngine;

    public class YInput
    {
        private YPosition position;
        private float offset;
        private bool proportional;

        public YInput(YPosition position, float offset, bool proportional = false)
        {
            this.position = position;
            this.offset = offset;
            this.proportional = proportional;
        }

        public float Calculate(RectTransform parent)
        {
            float offsetLocal = this.offset;

            Vector2 sizeDelta = parent.GetSizeAnchorAgnostic();

            if (this.proportional)
                offsetLocal = offsetLocal / 100f * sizeDelta.y;

            if (this.position == YPosition.Top)
                return parent.position.y + sizeDelta.y / 2 - offsetLocal;

            if (this.position == YPosition.Center)
                return parent.position.y - offsetLocal;

            if (this.position == YPosition.Bottom)
                return parent.position.y - sizeDelta.y / 2 - offsetLocal;

            return 0;
        }
    }
}