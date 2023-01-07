namespace BugFoundry.SchwiftyUI.V3.Containers
{
    using System.Collections.Generic;
    using Behaviours;
    using Elements;
    using UnityEngine;

    public class XSplit : IResizeable
    {
        private SchwiftyElement left;
        private SchwiftyElement right;
        private SchwiftyElement dragBar;
        private SchwiftyElement parent;
        private readonly float margin = 0.005f;
        private float resizeBarWidthCm;

        public void SplitPanel(
            out SchwiftyPanel leftOut,
            out SchwiftyPanel rightOut,
            float proportionIn,
            SchwiftyElement elementIn,
            Texture2D resizeCursor,
            float resizeBarWidthCmIn)
        {
            this.resizeBarWidthCm = resizeBarWidthCmIn;
            this.parent = elementIn;
            RectTransform rt = this.parent.RectTransform;
            Rect rect = rt.rect;

            leftOut = (SchwiftyPanel)new SchwiftyPanel(this.parent, this.parent.Name + " left split")
                .SetBackgroundColor(Color.black)
                .SetAnchors10(new Vector2(0, 0), new Vector2(proportionIn, 1))
                .ZeroOffsets();

            rightOut = (SchwiftyPanel)new SchwiftyPanel(this.parent, this.parent.Name + " right split")
                .SetBackgroundColor(Color.white)
                .SetAnchors10(new Vector2(proportionIn, 0), new Vector2(1, 1))
                .ZeroOffsets();

            this.dragBar = new SchwiftyPanel(this.parent, this.parent.Name + " drag bar")
                .SetBackgroundColor(new Color(0, 0, 0, 0))
                .SetAnchors10(new Vector2(proportionIn - this.margin, 0), new Vector2(proportionIn + this.margin, 1))
                .ZeroOffsets();
            SplitBehaviour beh = this.dragBar.gameObject.AddComponent<SplitBehaviour>();
            beh.Initilize(this.Slide, resizeCursor);

            float dpc = Screen.dpi / 2.54f;
            float dots = dpc * this.resizeBarWidthCm;

            Vector2 resizeBarX = this.dragBar.RectTransform.GetSizeAnchorAgnostic();
            this.dragBar.SetSizeWithCurrentAnchorsSingle(new Vector2(dots, resizeBarX.y));

            this.left = leftOut;
            this.right = rightOut;

            this.parent.RegisterWithRoot(new List<IResizeable>() { this });
        }

        public void Slide()
        {
            Vector3 mousePosition = Input.mousePosition;

            Vector2 tl = this.parent.RectTransform.GetTopLeft();
            Vector2 size = this.parent.RectTransform.GetSizeAnchorAgnostic();

            float x1 = tl.x;
            float x2 = tl.x + size.x;
            float x = mousePosition.x;
            float xx1 = x - x1;
            float xx2 = x2 - x;

            if (xx1 < 0 || xx2 < 0)
            {
                Debug.Log($"less than 0");
                return;
            }

            float ratio = xx1 / (xx1 + xx2);

            if (this.left.Destroyed == false)
                this.left
                    .SetAnchors10(new Vector2(0, 0), new Vector2(ratio, 1))
                    .ZeroOffsets();

            if (this.right.Destroyed == false)
                this.right
                    .SetAnchors10(new Vector2(ratio, 0), new Vector2(1, 1))
                    .ZeroOffsets();

            this.dragBar
                .SetAnchors10(new Vector2(ratio - this.margin, 0), new Vector2(ratio + this.margin, 1))
                .ZeroOffsets();

            Vector2 resizeBarX = this.dragBar.RectTransform.GetSizeAnchorAgnostic();
            float dpc = Screen.dpi / 2.54f;
            float dots = dpc * this.resizeBarWidthCm;
            this.dragBar.SetSizeWithCurrentAnchorsSingle(new Vector2(dots, resizeBarX.y));
        }

        public void Resize()
        {
            Vector2 resizeBarX = this.dragBar.RectTransform.GetSizeAnchorAgnostic();
            float dpc = Screen.dpi / 2.54f;
            float dots = dpc * this.resizeBarWidthCm;
            this.dragBar.SetSizeWithCurrentAnchorsSingle(new Vector2(dots, resizeBarX.y));
        }
    }
}