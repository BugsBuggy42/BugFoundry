namespace SchwiftyUI.V3.Containers
{
    using System.Collections.Generic;
    using Elements;
    using Inputs;
    using UnityEngine;
    using UnityEngine.UI;

    public class SchwiftyScrollbarApplier : IResizeable
    {
        private SchwiftyElement parent;
        private SchwiftyElement content;
        private ScrollRect maskScrollRect;
        private SchwiftyScrollbar scrollBar;
        private float thickness;
        private SchwiftyElement mask;

        public ScrollRect GetScrollRect() => this.maskScrollRect;

        public void Create(
            SchwiftyElement parentIn,
            SchwiftyElement contentIn,
            float thicknessIn,
            Color? stillColorIn = null,
            Color? activeColorIn = null,
            ScrollRect.MovementType movementType = ScrollRect.MovementType.Clamped,
            bool showMaskGraphic = false)
        {
            this.parent = parentIn;
            this.content = contentIn;
            this.thickness = thicknessIn;

            Vector2 parentTL = this.parent.RectTransform.GetTopLeft();
            Vector2 parentSD = this.parent.RectTransform.GetSizeAnchorAgnostic();

            this.mask = new SchwiftyPanel(this.parent, $"{this.parent.Name} mask")
                .UsePositioner15(new Positioner().SameAsParent())
                .AddMask();

            this.content.SetParent(this.mask);

            this.maskScrollRect = this.mask.gameObject.AddComponent<ScrollRect>();
            this.maskScrollRect.movementType = movementType;

            this.scrollBar = (Elements.SchwiftyScrollbar)new SchwiftyScrollbar(this.mask, "SB")
                .SetGoNameAndEleName($"{this.parent.Name} scrollBar")
                .SetDimensionsWithCurrentAnchors(thicknessIn, parentSD.y)
                .SetTopLeft20(parentTL.x + parentSD.x - thicknessIn, parentTL.y);

            if (stillColorIn != null && activeColorIn != null)
                this.scrollBar.SetHandleColorsOld(stillColorIn.Value, activeColorIn.Value);

            this.maskScrollRect.content = this.content.RectTransform;
            this.maskScrollRect.verticalScrollbar = this.scrollBar.ScrollBar;
            this.maskScrollRect.normalizedPosition = new Vector2(0, 1);

            this.mask.gameObject.GetComponent<Mask>().showMaskGraphic = showMaskGraphic;

            this.parent.RegisterWithRoot(new List<IResizeable>() { this });
        }

        public void Resize()
        {
            Vector2 tl = this.parent.RectTransform.GetTopLeft();
            Vector2 sd = this.parent.RectTransform.GetSizeAnchorAgnostic();

            this.scrollBar
                .SetDimensionsWithCurrentAnchors(this.thickness, sd.y)
                .SetTopLeft20(tl.x + sd.x - this.thickness, tl.y);

            this.maskScrollRect.content = this.content.RectTransform;
            this.maskScrollRect.normalizedPosition = new Vector2(0, 1);
        }

        public void Destroy()
        {
            this.mask.Destroy();
        }
    }
}