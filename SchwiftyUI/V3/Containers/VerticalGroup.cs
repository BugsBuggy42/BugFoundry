namespace BugFoundry.SchwiftyUI.V3.Containers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Elements;
    using Sizers;
    using UnityEngine;
    using UnityEngine.UI;
    using Object = UnityEngine.Object;

    public class VerticalGroup : IResizeable
    {
        private SchwiftyScrollbarApplier scrolling = new();
        private List<SchwiftyElement> ignoreList = new();
        private SchwiftyElement parent;
        private SchwiftyElement contentParent;
        private float ppYMargin;
        private float ppXMargin;
        private YSizer ySizer;
        private List<SchwiftyElement> elements;
        private bool applyScroll;
        private ScrollRect maskScrollRect;

        private List<SchwiftyElement> GetIgnoreList() => this.elements.Concat(this.ignoreList).ToList();
        public SchwiftyElement GetContentParent() => this.contentParent;
        public ScrollRect GetScrollRect() => this.maskScrollRect;

        public void Create(
            SchwiftyElement parentIn,
            float ppYMarginIn,
            float ppXMarginIn,
            YSizer containerYSizerIn,
            List<SchwiftyElement> elementsIn,
            bool applyScrollIn = true,
            bool expandWithChildrenIn = false,
            Color? contentPanelColor = null,
            Color? scrollbarBody = null,
            Color? scrollbarHandle= null)
        {
            this.parent = parentIn;
            this.ppYMargin = ppYMarginIn;
            this.ppXMargin = ppXMarginIn;
            this.ySizer = containerYSizerIn;
            this.elements = elementsIn;
            this.applyScroll = applyScrollIn;
            contentPanelColor ??= Color.gray;

            float yMargin = ppYMarginIn;
            float xMargin = ppXMarginIn;

            float xDimension = Math.Abs(parentIn.RectTransform.rect.width) - xMargin * 2;

            if (xDimension <= 0)
                throw new Exception();

            float yDimension = this.ySizer.GetYSize(xDimension, this.parent.RectTransform.GetSizeAnchorAgnostic().y);

            float lenght = this.elements.Count * (yMargin + yDimension) + yMargin;

            if (this.contentParent != null)
                Object.Destroy(this.contentParent.gameObject);

            this.contentParent = new SchwiftyPanel(this.parent, $"{this.parent.Name} contentParent")
                .SetBackgroundColor(contentPanelColor.Value)
                .SetDimensionsWithCurrentAnchors(this.parent.RectTransform.GetSizeAnchorAgnostic().x, lenght)
                .SetTopLeft20(this.parent.RectTransform.GetTopLeft());

            this.ignoreList.Add(this.contentParent);

            Vector2 topLeftP = this.contentParent.RectTransform.GetTopLeft();

            for (int i = 0; i < this.elements.Count; i++)
            {
                SchwiftyElement element = this.elements[i];

                element
                    .SetParent(this.contentParent)
                    .SetDimensionsWithCurrentAnchors(xDimension, yDimension)
                    .SetTopLeft20(topLeftP.x + xMargin, topLeftP.y - i * (yMargin + yDimension) - yMargin);
            }

            if (this.applyScroll)
            {
                this.scrolling.Create(
                    parentIn,
                    this.contentParent,
                    15,
                    scrollbarBody,
                    scrollbarHandle);
                this.maskScrollRect = this.scrolling.GetScrollRect();
            }

            this.parent.RegisterWithRoot(new List<IResizeable>() { this });
        }

        public void Resize()
        {
            // Debug.Log("RESIZEEEEE");

            float yMargin = this.ppYMargin;
            float xMargin = this.ppXMargin;

            float xDimension = Math.Abs(this.parent.RectTransform.rect.width) - xMargin * 2;

            if (xDimension <= 0)
            {
                Debug.Log(
                    $"rect.width: {this.parent.RectTransform.rect.width} sizeDelta.x: {this.parent.RectTransform.sizeDelta.x} margin * 2: {xMargin * 2}");
                throw new Exception();
            }

            float yDimension = this.ySizer.GetYSize(xDimension, this.parent.RectTransform.GetSizeAnchorAgnostic().y);

            float lenght = this.elements.Count * (yMargin + yDimension) + yMargin;

            this.contentParent
                .SetDimensionsWithCurrentAnchors(this.parent.RectTransform.GetSizeAnchorAgnostic().x, lenght)
                .SetTopLeft20(this.parent.RectTransform.GetTopLeft());

            Vector2 topLeftP = this.parent.RectTransform.GetTopLeft(); // TODO: top left should be content parent

            for (int i = 0; i < this.elements.Count; i++)
            {
                SchwiftyElement element = this.elements[i];

                element
                    .SetDimensionsWithCurrentAnchors(xDimension, yDimension)
                    .SetTopLeft20(topLeftP.x + xMargin, topLeftP.y - i * (yMargin + yDimension) - yMargin);
            }
        }

        public void ReorderExistingElements()
        {
            float yMargin = this.ppYMargin;
            float xMargin = this.ppXMargin;
            float xDimension = Math.Abs(this.parent.RectTransform.rect.width) - xMargin * 2;
            if (xDimension <= 0)
            {
                Debug.Log(
                    $"rect.width: {this.parent.RectTransform.rect.width} sizeDelta.x: {this.parent.RectTransform.sizeDelta.x} margin * 2: {xMargin * 2}");
                throw new Exception();
            }

            float yDimension = this.ySizer.GetYSize(xDimension, this.parent.RectTransform.GetSizeAnchorAgnostic().y);
            Vector2 topLeftP = this.contentParent.RectTransform.GetTopLeft();

            for (int i = 0; i < this.elements.Count; i++)
            {
                SchwiftyElement element = this.elements[i];
                element.SetTopLeft20(topLeftP.x + xMargin, topLeftP.y - i * (yMargin + yDimension) - yMargin); }
        }

        public void Destroy()
        {
            this.scrolling.Destroy();
            this.contentParent.Destroy();
        }
    }
}