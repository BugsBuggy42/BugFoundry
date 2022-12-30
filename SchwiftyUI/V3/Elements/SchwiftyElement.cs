namespace SchwiftyUI.V3.Elements
{
    using System;
    using System.Collections.Generic;
    using Containers;
    using Inputs;
    using JetBrains.Annotations;
    using Other;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Always set dimensions before setting position!
    /// </summary>
    public class SchwiftyElement
    {
        /// TODO: Pear down the variables here and place then in the elements that actually have them
        protected ElementType Type { get; set; }

        public Vector3 TopLeft { get; set; } = Vector3.zero;
        protected Vector2 Dimensions { get; private set; } = Vector2.zero;
        public SchwiftyElement Parent { get; set; } = null;
        public List<SchwiftyElement> Children { get; set; } = new List<SchwiftyElement>();
        public GameObject gameObject { get; protected set; } = null;
        public string Name { get; set; } = "Unnamed";
        private Color Color { get; set; } = Color.white;
        public RectTransform RectTransform { get; set; }
        protected RectTransform ButtonTextRT { get; set; }
        public Button Button { get; protected set; }
        public int CurrentPage { get; set; } = 0;
        protected RectTransform SlidingAreaRT { get; set; }
        protected RectTransform HandleRT { get; set; }
        public bool Destroyed { get; set; } = false;

        public SchwiftyElement CopyRT(RectTransform rt, out GameObject current)
        {
            current = this.gameObject;
            this.RectTransform.position = rt.position;
            this.RectTransform.sizeDelta = rt.sizeDelta;
            this.gameObject = rt.gameObject;
            return this;
        }

        private SchwiftyRoot GetRootElement()
        {
            SchwiftyElement element = this;

            do
            {
                if (element is SchwiftyRoot root)
                    return root;
                element = element.Parent;
            } while (element != null);

            throw new Exception("Root element not found");
        }

        public void RegisterWithRoot(List<IResizeable> resizeable)
        {
            SchwiftyRoot root = this.GetRootElement();
            root.AddToToResizeCollections(resizeable);
        }

        /// <summary>
        /// Sets anchor too!
        /// does it?
        /// </summary>
        public SchwiftyElement UsePositioner15(Positioner positioner)
        {
            positioner.CalculateBox(this.Parent.RectTransform, out Vector2 topLeft, out Vector2 sizeDelta);

            this
                .SetDimensionsWithCurrentAnchors(sizeDelta.x, sizeDelta.y)
                .SetTopLeft20(topLeft.x, topLeft.y);

            return this;
        }

        public SchwiftyElement SetGoNameAndEleName(string name)
        {
            this.Name = name;
            this.gameObject.name = name;

            return this;
        }

        public SchwiftyElement AddMask()
        {
            // TODO: restring to the elements that need it
            this.gameObject.AddComponent<Mask>();
            return this;
        }

        public SchwiftyElement SetParent(SchwiftyElement parent)
        {
            if (parent == null) return this;

            this.gameObject.transform.SetParent(parent.gameObject.transform);
            parent.Children.Add(this);
            this.Parent = parent;
            this.SetAnchors10(new Vector2(0, 0), new Vector2(1, 1));
            return this;
        }

        /// <summary>
        /// Do this after positioning!
        /// https://answers.unity.com/questions/792642/unity-46-beta-changing-anchors-position-through-sc.html
        /// </summary>
        public SchwiftyElement SetAnchors10(Vector2 anchorMin, Vector2 anchorMax)
        {
            this.RectTransform.anchorMax = anchorMax;
            this.RectTransform.anchorMin = anchorMin;
            return this;
        }

        // public virtual SchwiftyElement SetDimensionsWithCurrentAnchors(Vector2 size,
        //     RectTransform rectTransformIn = null) =>
        //     this.SetDimensionsWithCurrentAnchors(size.x, size.y, rectTransformIn);

        public virtual SchwiftyElement SetDimensionsWithCurrentAnchors(float x, float y,
            RectTransform rectTransformIn = null)
        {
            rectTransformIn = rectTransformIn == null ? this.RectTransform : rectTransformIn;
            this.Dimensions = new Vector2(x, y);
            this.SetSizeWithCurrentAnchorsSingle(rectTransformIn, x, y);

            // TODO: Extract those in their own classes;
            if (this.Type == ElementType.Button)
                this.SetSizeWithCurrentAnchorsSingle(this.ButtonTextRT, x, y);

            if (this.Type == ElementType.ScrollBar)
            {
                this.SetSizeWithCurrentAnchorsSingle(this.SlidingAreaRT, x, y);
                // this.SlidingAreaRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,x);
                this.SetSizeWithCurrentAnchorsSingle(this.HandleRT, x, y / 3);
            }

            return this;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void SetSizeWithCurrentAnchorsSingle(RectTransform transform, float x, float y)
        {
            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x);
            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y);
        }

        public void SetSizeWithCurrentAnchorsSingle(Vector2 size)
        {
            this.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            this.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }

        public static Vector2 GetTopLeft(RectTransform t)
        {
            Vector2 pivot = t.pivot;
            //Debug.Log($"HERE: pos:{t.position} rPos:{t.rect.position}");
            return new Vector2(t.position.x - t.rect.size.x * pivot.x, t.position.y + t.rect.size.y * (1 - pivot.y));
        }

        public SchwiftyElement SetActive(bool active)
        {
            this.gameObject.SetActive(active);
            return this;
        }

        public SchwiftyElement SetTopLeft20(float x, float y, float z = 0)
        {
            Vector2 size = this.RectTransform.rect.size;
            this.RectTransform.position = new Vector3(x + size.x / 2, y - size.y / 2, z);
            return this;
        }

        public SchwiftyElement SetTopLeft20(Vector2 size, float z = 0) => this.SetTopLeft20(size.x, size.y, z);


        public SchwiftyElement ZeroOffsets()
        {
            this.RectTransform.offsetMax = new Vector2(0, 0);
            this.RectTransform.offsetMin = new Vector2(0, 0);
            return this;
        }

        public SchwiftyElement SetOffsets(Vector2 min, Vector2 max)
        {
            this.RectTransform.offsetMax = max;
            this.RectTransform.offsetMin = min;
            return this;
        }

        public SchwiftyElement SetPivot(Vector2 pivot)
        {
            this.RectTransform.pivot = pivot;
            return this;
        }

        public SchwiftyElement SetRightBarConstSize(float widthInCentimeter)
        {
            this
                .SetPivot(new Vector2(1,1))
                .SetAnchors10(new Vector2(1, 0), new Vector2(1, 1))
                .ZeroOffsets();

            float dots = Screen.dpi / 2.54f * widthInCentimeter;
            this.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, dots);
            return this;
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(this.gameObject);
            this.Destroyed = true;
        }

        // consider replacing this
        protected virtual SchwiftyElement SetTopLeftOffsetParent(float x, float y,
            [CanBeNull] RectTransform parent = null,
            RectTransform rectTransformIn = null, int z = 0)
        {
            if (this.Parent != null)
            {
                parent = this.Parent.RectTransform;
            }

            rectTransformIn = rectTransformIn == null ? this.RectTransform : rectTransformIn;

            if (parent == null)
            {
                Debug.LogWarning("parent null");
                return this;
            }

            float pLeft = parent.position.x - parent.sizeDelta.x / 2;
            float pTop = parent.position.y + parent.sizeDelta.y / 2;
            rectTransformIn.position = new Vector3(pLeft + x + (float)Math.Round(rectTransformIn.sizeDelta.x / 2),
                pTop - y - (float)Math.Round(rectTransformIn.sizeDelta.y / 2), z);
            return this;
        }
    }
}