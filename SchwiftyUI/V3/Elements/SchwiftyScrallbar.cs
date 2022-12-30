namespace Buggary.SchwiftyUI.V3.Elements
{
    using Other;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Always set dimensions before setting position!
    /// </summary>
    public class SchwiftyScrollbar : SchwiftyElement
    {
        public Scrollbar ScrollBar;
        private Image handleImage;
        private Image parentImage;

        public SchwiftyScrollbar(SchwiftyElement parent, string name,
            Scrollbar.Direction direction = Scrollbar.Direction.BottomToTop)
        {
            this.Type = ElementType.ScrollBar;
            // this.stillColor = stillColor;
            // this.activeColor = activeColor;
            this.Name = name;
            this.CreateElement(direction);
            this.SetParent(parent);
        }

        private SchwiftyElement CreateElement(Scrollbar.Direction direction)
        {
            //########### SCROLLBAR PARENT #########################################

            GameObject go = new(this.Name);
            RectTransform rt = go.AddComponent<RectTransform>();
            this.parentImage = go.AddComponent<Image>();
            this.ScrollBar = go.AddComponent<Scrollbar>();
            this.parentImage.color = Color.cyan;

            this.RectTransform = rt;
            this.gameObject = go;
            // this.Scrollbar = scrollBar; TODO: clean scrollbar from generic element;

            //########### SIDING AREA #########################################

            GameObject se = new(this.Name + " Sliding Area");
            RectTransform rtse = se.AddComponent<RectTransform>();
            se.transform.SetParent(go.transform);
            rtse.anchorMin = new Vector2(0, 0);
            rtse.anchorMax = new Vector2(1, 1);
            rtse.offsetMin = new Vector2(0, 0);
            rtse.offsetMax = new Vector2(0, 0);

            this.SlidingAreaRT = rtse;

            //########### Handle #########################################

            GameObject handle = new(this.Name + " Handle");
            this.HandleRT = handle.AddComponent<RectTransform>();
            this.handleImage = handle.AddComponent<Image>();
            handle.transform.SetParent(se.transform);
            this.handleImage.color = Color.green;

            this.HandleRT.anchorMin = new Vector2(0, 0);
            this.HandleRT.anchorMax = new Vector2(0, 0.1f);
            this.HandleRT.offsetMin = new Vector2(0, 0);
            this.HandleRT.offsetMax = new Vector2(0, 0);

            //########### SETUP #########################################

            this.ScrollBar.handleRect = this.HandleRT;
            this.ScrollBar.targetGraphic = this.handleImage;
            this.ScrollBar.direction = direction;

            if (this.Name != null)
                this.gameObject.name = this.Name;

            return this;
        }

        //Color? stillColor = null, Color? activeColor = null,

        public SchwiftyScrollbar SetHandleColor(Color c)
        {
            this.handleImage.color = c;
            return this;
        }

        public SchwiftyScrollbar SetBodyColor(Color c)
        {
            this.parentImage.color = c;
            return this;
        }

        public SchwiftyScrollbar SetHandleColorsOld(Color stillColor, Color activeColor)
        {
            ColorBlock colorBlock = this.ScrollBar.colors;
            colorBlock.normalColor = stillColor;
            colorBlock.selectedColor = stillColor;
            colorBlock.pressedColor = activeColor;
            this.ScrollBar.colors = colorBlock;

            return this;
        }
    }
}