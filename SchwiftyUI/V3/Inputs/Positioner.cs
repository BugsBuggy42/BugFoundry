namespace SchwiftyUI.V3.Inputs
{
    using System;
    using Elements;
    using UnityEngine;

    public class Positioner
    {
        private XInput x1;
        private XInput x2;
        private float xSize;

        private YInput y1;
        private YInput y2;
        private float ySize;

        private float topBorder;
        private float bottomBorder;
        private float leftBorder;
        private float rightBorder;

        private bool sameAsParent = false;

        private bool doXPercent = false;
        private bool doYPercent = false;
        private bool doBothPercent = false;

        private float percentX;
        private float percentY;

        private float percent;
        private float multy;

        public Positioner SameAsParent()
        {
            this.sameAsParent = true;
            return this;
        }

        public Positioner PercentOfParentX(float percentIn, float multyIn)
        {
            this.doXPercent = true;
            this.percent = percentIn;
            this.multy = multyIn;
            return this;
        }

        public Positioner PercentOfParentY(float percentIn, float multyIn)
        {
            this.doYPercent = true;
            this.percent = percentIn;
            this.multy = multyIn;
            return this;
        }

        public Positioner PercentOfParent(float x, float y)
        {
            this.doBothPercent = true;
            this.percentX = x;
            this.percentY = y;
            return this;
        }

        public Positioner SetX(XInput x1In, XInput x2In)
        {
            this.ClearX();
            this.x1 = x1In;
            this.x2 = x2In;
            return this;
        }

        public Positioner SetX(XInput x1In, float xSizeIn)
        {
            this.ClearX();
            this.x1 = x1In;
            this.xSize = xSizeIn;
            return this;
        }

        public Positioner SetY(YInput y1In, YInput y2In)
        {
            this.ClearY();
            this.y1 = y1In;
            this.y2 = y2In;
            return this;
        }

        public Positioner SetY(YInput y1In, float ySizeIn)
        {
            this.ClearY();
            this.y1 = y1In;
            this.ySize = ySizeIn;
            return this;
        }

        public Positioner SetBorder(int top, int right, int bottom, int left)
        {
            this.topBorder = top;
            this.rightBorder = right;
            this.bottomBorder = bottom;
            this.leftBorder = left;
            return this;
        }

        public void CalculateBox(RectTransform parent, out Vector2 topLeft, out Vector2 sizeDelta)
        {
            if (this.doXPercent)
            {
                topLeft = SchwiftyElement.GetTopLeft(parent);
                Vector2 parentSize = parent.GetSizeAnchorAgnostic();
                float xLength = this.percent * parentSize.x;
                float yLength = this.multy * xLength;
                sizeDelta = new Vector2(xLength, yLength);
            }
            else if (this.doYPercent)
            {
                topLeft = SchwiftyElement.GetTopLeft(parent);
                Vector2 parentSize = parent.GetSizeAnchorAgnostic();
                float yLength = this.percent * parentSize.y;
                float xLength = this.multy * yLength;
                sizeDelta = new Vector2(xLength, yLength);
            }
            else if (this.doBothPercent)
            {
                topLeft = SchwiftyElement.GetTopLeft(parent);
                Vector2 parentSize = parent.GetSizeAnchorAgnostic();
                float xLength = this.percentX * parentSize.x;
                float yLength = this.percentY * parentSize.y;
                sizeDelta = new Vector2(xLength, yLength);
            }
            else if (this.sameAsParent)
            {
                topLeft = SchwiftyElement.GetTopLeft(parent);
                sizeDelta = parent.GetSizeAnchorAgnostic();

                RectTransform t = parent;
                // Debug.Log($"{t.gameObject.name} t.rect.size.y:{t.rect.size.y} t.rect.height:{t.rect.height} t.rect.y:{t.rect.y} t.anchoredPosition.y:{t.anchoredPosition.y} t.sizeDelta.y:{t.sizeDelta.y}");
                // Debug.Log($"{t.gameObject.name} t.rect.size.x:{t.rect.size.x} t.rect.width:{t.rect.width} t.rect.x:{t.rect.x} t.anchoredPosition.x:{t.anchoredPosition.x} t.sizeDelta.x:{t.sizeDelta.x}");

                // Debug.Log($"POS {parent.name} TL:{topLeft.x} {topLeft.y} SD:{sizeDelta.x} {sizeDelta.y}");
            }
            else
            {
                if (this.x1 == null || this.y1 == null)
                    throw new Exception("You must register x and y input");

                float x1Value = this.x1.Calculate(parent);
                float x2Value;

                if (this.x2 == null)
                    x2Value = x1Value + this.xSize;
                else
                    x2Value = this.x2.Calculate(parent);


                if (x1Value > x2Value)
                    (x1Value, x2Value) = (x2Value, x1Value);

                float tlX = x1Value;
                float sizeX = x2Value - x1Value;

                //==========================================================================================

                float y1Value = this.y1.Calculate(parent);
                float y2Value;

                if (this.y2 == null)
                    y2Value = y1Value - this.ySize;
                else
                    y2Value = this.y2.Calculate(parent);

                if (y1Value < y2Value)
                    (y1Value, y2Value) = (y2Value, y1Value);

                float tlY = y1Value;
                float sizeY = y1Value - y2Value;

                topLeft = new Vector2(tlX, tlY);
                sizeDelta = new Vector2(sizeX, sizeY);
            }

            topLeft.x += this.leftBorder;
            topLeft.y -= this.topBorder;
            sizeDelta.x -= (this.leftBorder + this.rightBorder);
            sizeDelta.y -= (this.topBorder + this.bottomBorder);
        }

        private void ClearX()
        {
            this.x1 = null;
            this.x2 = null;
        }

        private void ClearY()
        {
            this.y1 = null;
            this.y2 = null;
        }
    }
}