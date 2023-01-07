namespace BugFoundry.BugFoundryEditor.UI
{
    using Commons.CommonHelpers;
    using SchwiftyUI.BugFoundry.SchwiftyUI.V3.Elements;
    using UnityEngine;

    public class BottomRightInfoPanel
    {
        private readonly SchwiftyPanelLabel label;

        public BottomRightInfoPanel(RectTransform parent)
        {
            SchwiftyRoot schRoot = new(parent);
            Vector2 sd = schRoot.RectTransform.sizeDelta;
            this.label = new SchwiftyPanelLabel(schRoot, "BottomRightInfoPanel")
                .SetBackgroundColor(MyColor.GrayBlend(0.7f))
                .SetDimensionsWithCurrentAnchors(600, 60)
                .SetTopLeft20(sd.x - 600, 60)
                .ToPanelLabel6900();

            this.label.SetActive(false);
        }

        public void SetText(string text)
        {
            this.label.labelElemnent.Text.text = text;
        }
    }
}