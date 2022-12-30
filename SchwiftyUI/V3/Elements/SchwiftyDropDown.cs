namespace SchwiftyUI.V3.Elements
{
    using System.Collections.Generic;
    using Other;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    /// <summary>
    /// Always set dimensions before setting position!
    /// </summary>
    public class SchwiftyDropdown : SchwiftyElement
    {
        private RectTransform templateRT;
        private RectTransform itemRT;
        private RectTransform itemLabelRT;
        private RectTransform parentLabelRT;
        private RectTransform viewPortRT;
        private int elementCount;

        private SchwiftyLabel parentLabel;
        private SchwiftyLabel label;

        public SchwiftyDropdown(SchwiftyElement parent, string name, List<string> options, UnityAction<int> ddEvent, int initValue = 0, float fontSize = 12)
        {
            this.Type = ElementType.Dropdown;
            this.elementCount = options.Count;
            this.CreateElement(options, ddEvent, initValue, name, fontSize);
            this.SetParent(parent);
        }

        public SchwiftyElement CreateElement(IEnumerable<string> options, UnityAction<int> ddEvent, int initValue, string name, float fontSize)
        {
            //########### DROPDOWN PARENT #########################################

            GameObject go = new GameObject(this.Name);
            RectTransform rt = go.AddComponent<RectTransform>();
            CanvasRenderer r = go.AddComponent<CanvasRenderer>();
            Image img = go.AddComponent<Image>();
            TMP_Dropdown dropdown = go.AddComponent<TMP_Dropdown>();

            List<TMP_Dropdown.OptionData> optionsList = new List<TMP_Dropdown.OptionData>();

            foreach (string opt in options)
            {
                optionsList.Add(new TMP_Dropdown.OptionData(opt));
            }

            dropdown.options = optionsList;
            dropdown.value = initValue;

            this.RectTransform = rt;
            // this.Image = img;
            this.gameObject = go;

            //########### DROPDOWN PARENT LABEL #########################################

            this.parentLabel = new SchwiftyLabel(this, null, "Parent Label", fontSize);
            this.parentLabel.gameObject.transform.SetParent(go.transform);
            dropdown.captionText = this.parentLabel.Text;
            this.parentLabelRT = this.parentLabel.RectTransform;

            dropdown.onValueChanged.AddListener(ddEvent);

            //########### DROPDOWN TEMPLATE #########################################

            GameObject goTemplate = new GameObject(this.Name);
            goTemplate.name = "Template";
            this.templateRT = goTemplate.AddComponent<RectTransform>();
            CanvasRenderer cr = goTemplate.AddComponent<CanvasRenderer>();
            Image imgTemplate = goTemplate.AddComponent<Image>();
            cr.cullTransparentMesh = true;
            goTemplate.transform.SetParent(go.transform);
            goTemplate.SetActive(false);

            //########### DROPDOWN VIEWPORT #########################################

            GameObject goViewport = new GameObject(this.Name);
            goViewport.name = "Viewport";
            this.viewPortRT = goViewport.AddComponent<RectTransform>();
            CanvasRenderer crViewport = goViewport.AddComponent<CanvasRenderer>();
            Image imgViewport = goViewport.AddComponent<Image>();

            goViewport.transform.SetParent(goTemplate.transform);

            //########### CONTENT #########################################

            GameObject goContent = new GameObject(this.Name);
            goContent.name = "Content";
            RectTransform rtContent = goContent.AddComponent<RectTransform>();
            goContent.transform.SetParent(goViewport.transform);

            //########### ITEM #########################################

            GameObject goItem = new GameObject(this.Name);
            goItem.name = "Item";
            this.itemRT = goItem.AddComponent<RectTransform>();
            Toggle toggle = goItem.AddComponent<Toggle>();
            goItem.transform.SetParent(goContent.transform);

            //########### ITEM LABEL#########################################

            this.label = new SchwiftyLabel(this, null, "Item Label", fontSize);
            this.label.gameObject.transform.SetParent(goItem.transform);
            this.itemLabelRT = this.label.RectTransform;

            //########### DROPDOWN OTHER #########################################

            dropdown.template = this.templateRT;
            dropdown.itemText = this.label.Text;

            if (name != null)
            {
                this.gameObject.name = name;
            }

            return this;
        }

        public override SchwiftyElement SetDimensionsWithCurrentAnchors(float x, float y, RectTransform rectTransformIn = null)
        {
            // TODO: this probably needs to be redone
            base.SetDimensionsWithCurrentAnchors(x, y, rectTransformIn);
            this.parentLabelRT.sizeDelta = new Vector2(x, y);
            Vector2 dimensions = new Vector2(x, y * this.elementCount);
            this.templateRT.sizeDelta = dimensions;
            this.viewPortRT.sizeDelta = dimensions;
            this.itemRT.sizeDelta = new Vector2(x, y);
            this.itemLabelRT.sizeDelta = new Vector2(x, y);

            return this;
        }

        protected override SchwiftyElement SetTopLeftOffsetParent(float x, float y, RectTransform parent = null, RectTransform rectTransformIn = null, int z = 0)
        {
            base.SetTopLeftOffsetParent(x, y, parent, rectTransformIn, z);
            base.SetTopLeftOffsetParent(x, y, parent, this.parentLabelRT, z);
            base.SetTopLeftOffsetParent(x, y + this.Dimensions.y + 5, parent, this.templateRT, z);
            return this;
        }

        public SchwiftyDropdown SetTextColor(Color c)
        {
            this.parentLabel.SetTextColor(c);
            this.label.SetTextColor(c);
            return this;
        }
    }
}