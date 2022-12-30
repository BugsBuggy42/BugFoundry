namespace Buggary.SchwiftyUI.V3.Elements
{
    using Other;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Always set dimensions before setting position!
    /// </summary>
    public class SchwiftyPanelLabel : SchwiftyElement
    {
        public SchwiftyLabel labelElemnent { get; set; }
        public SchwiftyPanel panelElemnent { get; set; }
        public TextMeshProUGUI Text { get; set; }

        public SchwiftyPanelLabel(SchwiftyElement parent, string name, string value = null, float fontSize = 12)
        {
            this.Type = ElementType.PanelLabel;
            this.CreateElement(parent, value, name, fontSize);
        }

        public SchwiftyElement CreateElement(SchwiftyElement parent, string value, string name, float fontSize)
        {
            this.panelElemnent = new SchwiftyPanel(parent, name + " Panel");
            this.labelElemnent = new SchwiftyLabel(this.panelElemnent, name + " Label", value, fontSize);
            this.labelElemnent.SetSizeWithCurrentAnchorsSingle(this.panelElemnent.RectTransform.GetSizeAnchorAgnostic());
            this.labelElemnent.SetTopLeft20(GetTopLeft(this.panelElemnent.RectTransform));

            this.labelElemnent.SetAnchors10(new Vector2(0, 0), new Vector2(1, 1));
            this.RectTransform = this.panelElemnent.RectTransform;
            this.gameObject = this.panelElemnent.gameObject;
            this.Text = this.labelElemnent.Text;
            // this.Image = this.panelElemnent.Image;
            this.Parent = parent;
            return this;
        }

        public SchwiftyPanelLabel EnableAutoSizing(bool value)
        {
            this.labelElemnent.Text.enableAutoSizing = value;
            return this;
        }

        public SchwiftyPanelLabel SetMargin(Vector4 margin)
        {
            this.labelElemnent.Text.margin = margin;
            return this;
        }

        public SchwiftyPanelLabel SetColor(Color color)
        {
            this.panelElemnent.SetBackgroundColor(color);
            return this;
        }

        public SchwiftyPanelLabel SetTextSize(float size)
        {
            this.labelElemnent.Text.fontSize = size;
            return this;
        }

        public SchwiftyPanelLabel SetTextColor(Color c)
        {
            this.labelElemnent.Text.color = c;
            return this;
        }

        public SchwiftyPanelLabel SetHorizontalAlignment(HorizontalAlignmentOptions alignment)
        {
            this.labelElemnent.Text.horizontalAlignment = alignment;
            return this;
        }

        public SchwiftyPanelLabel SetVerticalAlignment(VerticalAlignmentOptions alignment)
        {
            this.labelElemnent.Text.verticalAlignment = alignment;
            return this;
        }

        public SchwiftyPanelLabel SetFontAsset(TMP_FontAsset asset)
        {
            this.labelElemnent.SetFontAsset(asset);
            return this;
        }

        public SchwiftyPanelLabel SetLineSpacing(float value)
        {
            this.labelElemnent.SetLineSpacing(value);
            return this;
        }

        public SchwiftyPanelLabel WordWrapping(bool value)
        {
            this.labelElemnent.WordWrapping(value);
            return this;
        }

        public SchwiftyPanelLabel SetBackgroundColor(Color color)
        {
            this.panelElemnent.SetBackgroundColor(color);
            return this;
        }
    }
}