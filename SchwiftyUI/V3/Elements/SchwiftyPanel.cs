namespace Buggary.SchwiftyUI.V3.Elements
{
    using Other;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Always set dimensions before setting position!
    /// </summary>
    public class SchwiftyPanel : SchwiftyElement
    {
        private Image image;

        public SchwiftyPanel(SchwiftyElement parent, string name)
        {
            this.Type = ElementType.Panel;
            this.CreateElement(name);
            this.SetParent(parent);
        }

        public SchwiftyElement CreateElement(string name)
        {
            GameObject go = new (this.Name);
            RectTransform rt = go.AddComponent<RectTransform>();
            CanvasRenderer cr = go.AddComponent<CanvasRenderer>();
            this.image = go.AddComponent<Image>();

            this.RectTransform = rt;
            this.gameObject = go;

            if (name != null)
            {
                this.gameObject.name = name;
            }

            return this;
        }

        public Image GetImage() => this.image;

        public SchwiftyPanel SetBackgroundColor(Color color)
        {
            this.image.color = color;
            return this;
        }
    }
}