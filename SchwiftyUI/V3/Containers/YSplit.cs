namespace SchwiftyUI.BugFoundry.SchwiftyUI.V3.Containers
{
    using System.Collections.Generic;
    using Behaviours;
    using Elements;
    using UnityEngine;

    public class YSplit : IResizeable
    {
        private SchwiftyElement top;
        private SchwiftyElement bottom;
        private SchwiftyElement dragBar;
        private SchwiftyElement parent;
        private float proportion;
        private readonly float margin = 0.005f;
        private float resizeBarWidthCm;

        public void SplitPanel(
            out SchwiftyPanel topOut,
            out SchwiftyPanel bottomOut,
            float proportionIn,
            SchwiftyElement elementIn,
            Texture2D resizeCursor,
            float resizeBarWidthCmIn)
        {
            this.resizeBarWidthCm = resizeBarWidthCmIn;
            this.parent = elementIn;
            RectTransform rt = this.parent.RectTransform;
            Rect rect = rt.rect;

            topOut = (SchwiftyPanel)new SchwiftyPanel(this.parent, this.parent.Name + " left split")
                .SetBackgroundColor(Color.black)
                .SetAnchors10(new Vector2(0, 0), new Vector2(1, proportionIn))
                .ZeroOffsets();

            bottomOut = (SchwiftyPanel)new SchwiftyPanel(this.parent, this.parent.Name + " right split")
                .SetBackgroundColor(Color.white)
                .SetAnchors10(new Vector2(0, proportionIn), new Vector2(1, 1))
                .ZeroOffsets();

            this.dragBar = new SchwiftyPanel(this.parent, this.parent.Name + " drag bar")
                .SetBackgroundColor(new Color(0, 0, 0, 0))
                .SetAnchors10(new Vector2(0, proportionIn - this.margin), new Vector2(1, proportionIn + this.margin))
                .ZeroOffsets();
            SplitBehaviour beh = this.dragBar.gameObject.AddComponent<SplitBehaviour>();
            beh.Initilize(this.Slide, resizeCursor);

            float dpc = Screen.dpi / 2.54f;
            float dots = dpc * this.resizeBarWidthCm;

            Vector2 resizeBarX = this.dragBar.RectTransform.GetSizeAnchorAgnostic();
            this.dragBar.SetSizeWithCurrentAnchorsSingle(new Vector2(resizeBarX.x, dots));

            this.top = topOut;
            this.bottom = bottomOut;
            this.proportion = proportionIn;

            this.parent.RegisterWithRoot(new List<IResizeable>() { this });
        }

        public void Slide()
        {
            Vector3 mousePosition = Input.mousePosition;

            Vector2 tl = this.parent.RectTransform.GetTopLeft();
            Vector2 size = this.parent.RectTransform.GetSizeAnchorAgnostic();

            float y1 = tl.y;
            float y2 = tl.y - size.y;
            float y = mousePosition.y;
            float yy1 = y1 - y;
            float yy2 =  y - y2;

            if (yy1 < 0 || yy2 < 0)
            {
                Debug.Log($"less than 0");
                return;
            }

            float ratio = yy2 / (yy1 + yy2);

            this.top
                .SetAnchors10(new Vector2(0, 0), new Vector2(1, ratio))
                .ZeroOffsets();

            this.bottom
                .SetAnchors10(new Vector2(0, ratio), new Vector2(1, 1))
                .ZeroOffsets();

            this.dragBar
                .SetAnchors10(new Vector2(0, ratio - this.margin), new Vector2(1, ratio + this.margin))
                .ZeroOffsets();

            Vector2 resizeBarX = this.dragBar.RectTransform.GetSizeAnchorAgnostic();
            float dpc = Screen.dpi / 2.54f;
            float dots = dpc * this.resizeBarWidthCm;
            this.dragBar.SetSizeWithCurrentAnchorsSingle(new Vector2(resizeBarX.x, dots));
        }

        public void Resize()
        {
            Vector2 resizeBarX = this.dragBar.RectTransform.GetSizeAnchorAgnostic();
            float dpc = Screen.dpi / 2.54f;
            float dots = dpc * this.resizeBarWidthCm;
            this.dragBar.SetSizeWithCurrentAnchorsSingle(new Vector2(resizeBarX.x, dots));
        }
    }
}