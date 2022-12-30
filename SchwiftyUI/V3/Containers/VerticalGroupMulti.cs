namespace Buggary.SchwiftyUI.V3.Containers
{
    using System.Collections.Generic;
    using System.Linq;
    using Elements;
    using Sizers;
    using UnityEngine;

    public class VerticalGroupMulti : IResizeable
    {
        private SchwiftyScrollbarApplier scrolling = new();
        private List<SchwiftyElement> ignoreList = new();
        private List<SchwiftyElement> columns = new();
        private SchwiftyElement parent;
        private SchwiftyElement contentParent;
        private float ppYMargin;
        private float ppXMargin;
        private List<float> proportions;
        private List<List<SchwiftyElement>> elements;
        private bool expandWithChildren;
        private YSizer ySizer;

        public void CreateWithScroll(
            SchwiftyElement parentIn,
            float ppYMarginIn,
            float ppXMarginIn,
            List<float> proportionsIn,
            List<List<SchwiftyElement>> elementsIn,
            bool expandWithChildrenIn = false)
        {
            this.parent = parentIn;
            this.ppYMargin = ppYMarginIn;
            this.ppXMargin = ppXMarginIn;
            this.proportions = proportionsIn;
            this.elements = elementsIn;
            this.expandWithChildren = expandWithChildrenIn;

            RectTransform rt = this.parent.RectTransform;
            Rect rect = rt.rect;
            Vector2 topLeft = this.parent.RectTransform.GetTopLeft();

            float whole = this.proportions.Sum();
            List<float> xSizes = new();

            foreach (float prop in this.proportions)
                xSizes.Add((prop / whole) * rect.size.x);

            this.ySizer = new YSizer(SizerType.ParentProportional, 0.1f);

            for (int i = 0; i < xSizes.Count; i++)
            {
                float leadingX = xSizes.Take(i).Sum();
                float xSize = xSizes[i];
                SchwiftyTransform column = new(this.parent, "column");
                this.columns.Add(column);
                column.SetDimensionsWithCurrentAnchors(xSize, this.parent.RectTransform.GetSizeAnchorAgnostic().y);
                column.SetTopLeft20(topLeft.x + leadingX, topLeft.y);

                VerticalGroup vertGroup = new();
                vertGroup.Create(column, 5, 5, this.ySizer, this.elements[i], false);
            }

            this.parent.RegisterWithRoot(new List<IResizeable>(){this});
        }

        public void Resize()
        {
            RectTransform rt = this.parent.RectTransform;
            Rect rect = rt.rect;
            Vector2 topLeft = this.parent.RectTransform.GetTopLeft();

            float whole = this.proportions.Sum();
            List<float> xSizes = new();

            foreach (float prop in this.proportions)
                xSizes.Add((prop / whole) * rect.size.x);

            this.ySizer = new YSizer(SizerType.ParentProportional, 0.1f);

            for (int i = 0; i < xSizes.Count; i++)
            {
                float leadingX = xSizes.Take(i).Sum();
                float xSize = xSizes[i];
                SchwiftyElement column = this.columns[i];
                column.SetDimensionsWithCurrentAnchors(xSize, this.parent.RectTransform.GetSizeAnchorAgnostic().y);
                column.SetTopLeft20(topLeft.x + leadingX, topLeft.y);
            }
        }
    }
}