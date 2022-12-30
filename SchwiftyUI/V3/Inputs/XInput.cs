namespace SchwiftyUI.V3.Inputs
{
    using Enums;
    using UnityEngine;

    public class XInput
    {
        private XPosition position;
        private float offset;
        private bool proportional;

        public XInput(XPosition position, float offset, bool proportional = false)
        {
            this.position = position;
            this.offset = offset;
            this.proportional = proportional;
        }

        public float Calculate(RectTransform parent)
        {
            Vector2 sizeDelta = parent.GetSizeAnchorAgnostic();

            float offsetLocal = this.offset;

            if (this.proportional)
                offsetLocal = offsetLocal / 100f * sizeDelta.x;

            if (this.position == XPosition.Left)
                return parent.position.x - sizeDelta.x / 2 + offsetLocal;

            if (this.position == XPosition.Center)
                return parent.position.x + offsetLocal;

            if (this.position == XPosition.Right)
                return parent.position.x + sizeDelta.x / 2 + offsetLocal;

            return 0;
        }
    }
}