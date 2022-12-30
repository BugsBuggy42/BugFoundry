namespace SchwiftyUI.V3.Elements
{
    using UnityEngine;

    public static class ElementExtensions
    {
        public static SchwiftyButton ToButton6900(this SchwiftyElement element)
        {
            return (SchwiftyButton)element;
        }

        public static SchwiftyLabel ToLabel6900(this SchwiftyElement element)
        {
            return (SchwiftyLabel)element;
        }

        public static SchwiftyPanelLabel ToPanelLabel6900(this SchwiftyElement element)
        {
            return (SchwiftyPanelLabel)element;
        }

        public static SchwiftyPanel ToPanel6900(this SchwiftyElement element)
        {
            return (SchwiftyPanel)element;
        }

        public static SchwiftyScrollbar ToScrollBar6900(this SchwiftyElement element)
        {
            return (SchwiftyScrollbar)element;
        }

        public static SchwiftyTransform ToTransform6900(this SchwiftyElement element)
        {
            return (SchwiftyTransform)element;
        }

        public static SchwiftyInput ToInput6900(this SchwiftyElement element)
        {
            return (SchwiftyInput)element;
        }

        public static SchwiftyRoot ToRoot(this Canvas canvas) => new (canvas.GetComponent<RectTransform>());

        public static SchwiftyRoot ToRoot(this GameObject panel) => new (panel.GetComponent<RectTransform>());
    }
}