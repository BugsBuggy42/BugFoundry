namespace SchwiftyUI.BugFoundry.SchwiftyUI.V3.Elements
{
    using System;
    using System.Collections;
    using Forks;
    using global::BugFoundry.Commons.Legacy;
    using Other;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Always set dimensions before setting position!
    /// </summary>
    public class SchwiftyInput : SchwiftyElement
    {
        public RectTransform textAreaRT;
        public TextMeshProUGUI Text;
        public Transform Caret;
        public TMP_InputField_Mine InputField;

        private RectTransform placeholderRT;
        private RectTransform textRT;
        private RectTransform caretRT;
        private Camera cam;
        private GameObject textAreaGO;
        private Image image;

        public SchwiftyInput(SchwiftyElement parent, string name, Camera cam, string placeholder = "", string defaultValue = "",
            float fontSize = 20)
        {
            this.Type = ElementType.Input;
            this.Name = name;
            this.cam = cam;

            this.CreateElement(placeholder, defaultValue, fontSize, name);
            this.SetParent(parent);
        }

        public SchwiftyElement CreateElement(string placeholder, string defaultValue, float fontSize,
            string name)
        {
            //########### Input #########################################
            GameObject go = new(name);
            go.SetActive(false);
            RectTransform rt = go.AddComponent<RectTransform>();
            CanvasRenderer cr = go.AddComponent<CanvasRenderer>();
            this.image = go.AddComponent<Image>();
            this.InputField = go.AddComponent<TMP_InputField_Mine>();

            // InputFocusManager.Register(this.InputField); TODO: Fix THIS

            this.RectTransform = rt;
            this.gameObject = go;

            //########### TextArea #########################################
            this.textAreaGO = new("TextArea");
            this.textAreaRT = this.textAreaGO.AddComponent<RectTransform>();
            RectMask2D mask = this.textAreaGO.AddComponent<RectMask2D>();
            this.textAreaGO.transform.SetParent(go.transform);
            this.textAreaRT.anchorMin = new Vector2(0,0);
            this.textAreaRT.anchorMax = new Vector2(1,1);

            //########### Placeholder #######################################
            GameObject goPh = new("Placeholder");
            this.placeholderRT = goPh.AddComponent<RectTransform>();
            CanvasRenderer phCr = goPh.AddComponent<CanvasRenderer>();
            TextMeshProUGUI text = goPh.AddComponent<TextMeshProUGUI>();
            LayoutElement layoutElement = goPh.AddComponent<LayoutElement>();
            text.text = placeholder;
            goPh.transform.SetParent(this.textAreaGO.transform);
            text.fontSize = fontSize;
            text.horizontalAlignment = HorizontalAlignmentOptions.Left;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;

            //########### Text ##############################################
            GameObject goText = new("Text");
            this.textRT = goText.AddComponent<RectTransform>();

            this.textRT.anchorMin = new Vector2(0, 0);
            this.textRT.anchorMax = new Vector2(1, 1);

            goText.transform.SetParent(this.textAreaGO.transform);
            _ = goText.AddComponent<CanvasRenderer>();
            this.Text = goText.AddComponent<TextMeshProUGUI>();
            this.Text.fontSize = fontSize;
            this.Text.horizontalAlignment = HorizontalAlignmentOptions.Left;
            this.Text.verticalAlignment = VerticalAlignmentOptions.Middle;

            this.InputField.textComponent = this.Text;
            this.InputField.textViewport = this.textAreaRT;
            this.InputField.placeholder = text;
            this.InputField.customCaretColor = true;
            this.InputField.caretColor = Color.black;
            this.InputField.caretWidth = 3;
            this.InputField.restoreOriginalTextOnEscape = false;
            this.image.color = Color.grey;

            GameObject.Find("Main").GetComponent<Coroutiner>().StartCoroutine(this.GetCaret());

            go.SetActive(true);
            return this;
        }

        IEnumerator GetCaret()
        {
            yield return null;
            this.Caret = this.textAreaGO.transform.Find("Caret");
            // this.SelectionCaret = this.Caret.GetComponent<TMP_SelectionCaret>();
        }

        public SchwiftyInput SetLineLimit(int lineLimit)
        {
            this.InputField.lineLimit = lineLimit;
            return this;
        }

        public SchwiftyInput MakeMultiline()
        {
            this.InputField.lineType = TMP_InputField_Mine.LineType.MultiLineNewline;
            return this;
        }

        public void SetValue(string value)
        {
            this.InputField.text = value;
        }

        public string GetValue()
        {
            return this.InputField.text;
        }

        public override SchwiftyElement SetDimensionsWithCurrentAnchors(float x, float y, RectTransform rectTransformIn = null)
        {
            base.SetDimensionsWithCurrentAnchors(x, y, rectTransformIn);
            base.SetDimensionsWithCurrentAnchors(x, y, this.textAreaRT);
            base.SetDimensionsWithCurrentAnchors(x, y, this.placeholderRT);
            base.SetDimensionsWithCurrentAnchors(x, y, this.textRT);
            return this;
        }

        protected override SchwiftyElement SetTopLeftOffsetParent(float x, float y, RectTransform parent = null,
            RectTransform rectTransformIn = null, int z = 0)
        {
            base.SetTopLeftOffsetParent(x, y, parent, rectTransformIn, z);
            base.SetTopLeftOffsetParent(x, y, parent, this.textAreaRT, z);
            base.SetTopLeftOffsetParent(x, y, parent, this.placeholderRT, z);
            base.SetTopLeftOffsetParent(x, y, parent, this.textRT, z);
            return this;
        }

        public SchwiftyInput SetColor(Color color)
        {
            this.Text.color = color;
            return this;
        }

        public SchwiftyInput SetHorizontalAlignment(HorizontalAlignmentOptions alignment)
        {
            this.Text.horizontalAlignment = alignment;
            return this;
        }

        public SchwiftyInput SetVerticalAlignment(VerticalAlignmentOptions alignment)
        {
            this.Text.verticalAlignment = alignment;
            return this;
        }

        public SchwiftyInput SetTextWrapping(bool value)
        {
            this.Text.enableWordWrapping = value;
            return this;
        }

        public SchwiftyInput SetCaretColor(Color color)
        {
            this.InputField.caretColor = color;
            return this;
        }

        public SchwiftyInput SetCaretWidth(int caretWidth)
        {
            this.InputField.caretWidth = caretWidth;
            return this;
        }

        public SchwiftyInput SetActions(Action leftRight, Action<float> scroll)
        {
            this.InputField.SetActions(leftRight, scroll);
            return this;
        }

        public SchwiftyInput SetFontAsset(TMP_FontAsset asset)
        {
            this.InputField.fontAsset = asset;
            return this;
        }

        public SchwiftyInput SetTextSize(float size)
        {
            this.Text.fontSize = size;
            return this;
        }

        public SchwiftyInput SetBackgroundColor(Color color)
        {
            this.image.color = color;
            return this;
        }
    }
}