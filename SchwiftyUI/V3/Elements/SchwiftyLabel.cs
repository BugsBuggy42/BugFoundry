namespace SchwiftyUI.BugFoundry.SchwiftyUI.V3.Elements
{
    using Other;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Always set dimensions before setting position!
    /// </summary>
    public class SchwiftyLabel : SchwiftyElement
    {
        public TextMeshProUGUI Text { get; set; }

        public SchwiftyLabel(SchwiftyElement parent, string name, string value, float fontSize = 12)
        {
            this.Type = ElementType.Label;
            this.CreateElement(value, name, fontSize);
            this.SetParent(parent);
        }

        public SchwiftyElement CreateElement(string value, string name, float fontSize)
        {
            //########### LABEL PARENT #########################################

            GameObject go = new (this.Name);
            RectTransform rt = go.AddComponent<RectTransform>();
            CanvasRenderer cr = go.AddComponent<CanvasRenderer>();
            this.Text = go.AddComponent<TextMeshProUGUI>();
            this.Text.alignment = TextAlignmentOptions.Midline;
            this.Text.fontSize = fontSize;
            this.Text.text = value;

            this.RectTransform = rt;
            this.gameObject = go;

            if (name != null)
            {
                this.gameObject.name = name;
            }

            return this;
        }

        public SchwiftyLabel EnableAutoSizing(bool value)
        {
            this.Text.enableAutoSizing = value;
            return this;
        }

        public SchwiftyLabel SetMargin(Vector4 margin)
        {
            this.Text.margin = margin;
            return this;
        }

        public SchwiftyLabel SetTextSize(float size)
        {
            this.Text.fontSize = size;
            return this;
        }

        public SchwiftyLabel SetTextColor(Color c)
        {
            this.Text.color = c;
            return this;
        }

        public SchwiftyLabel SetHorizontalAlignment(HorizontalAlignmentOptions alignment)
        {
            this.Text.horizontalAlignment = alignment;
            return this;
        }

        public SchwiftyLabel SetVerticalAlignment(VerticalAlignmentOptions alignment)
        {
            this.Text.verticalAlignment = alignment;
            return this;
        }

        public SchwiftyLabel SetLineSpacing(float spacing)
        {
            this.Text.lineSpacing = spacing;
            return this;
        }

        public SchwiftyLabel SetFontAsset(TMP_FontAsset asset)
        {
            this.Text.font = asset;
            return this;
        }

        public SchwiftyLabel WordWrapping(bool value)
        {
            this.Text.enableWordWrapping = value;
            return this;
        }
    }
}