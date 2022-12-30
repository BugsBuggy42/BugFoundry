namespace Buggary.SchwiftyUI.V3.Elements
{
    using System;
    using Other;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    /// <summary>
    /// Always set dimensions before setting position!
    /// </summary>
    public class SchwiftyButton : SchwiftyElement
    {
        private TextMeshProUGUI text;
        private Button btn;
        private Image image;

        public SchwiftyButton(SchwiftyElement parent, string name, out MonoBehaviour mono, string buttonName = null, Type buttonBeh = null, UnityAction<SchwiftyButton> act = null, float fontSize = 20)
        {
            this.Type = ElementType.Button;
            this.CreateElement(act, buttonName, buttonBeh, fontSize, name, out mono);
            this.SetParent(parent);
        }

        public SchwiftyElement CreateElement(UnityAction<SchwiftyButton> act, string buttonName, Type buttonBeh, float fontSize, string name, out MonoBehaviour mono)
        {
            //########### BUTTON #########################################

            GameObject go = new (name);
            RectTransform rt = go.AddComponent<RectTransform>();
            CanvasRenderer cr = go.AddComponent<CanvasRenderer>();
            this.image = go.AddComponent<Image>();
            this.btn = go.AddComponent<Button>();

            if (act != null)
            {
                this.btn.onClick.AddListener(() => act(this));
            }

            if (buttonBeh != null)
            {
                ButtonBaseBehaviour t = (ButtonBaseBehaviour)go.AddComponent(buttonBeh);
                t.Setup(); // TODO: add BBB setup here
                mono = t;
            }
            else
            {
                mono = null;
            }

            this.RectTransform = rt;
            this.gameObject = go;

            //########### TEXT #########################################

            GameObject goText = new GameObject(this.Name + "text");
            this.text = goText.AddComponent<TextMeshProUGUI>();
            RectTransform rtB = goText.GetComponent<RectTransform>();
            //var crB = goText.GetComponent<CanvasRenderer>();
            goText.transform.SetParent(go.transform);
            Vector3 p = goText.transform.position;
            goText.transform.position = new Vector3(p.x, p.y, 0.01f);

            this.text.alignment = TextAlignmentOptions.Center;

            this.text.text = name;

            if (buttonName != null)
            {
                this.text.text = buttonName;
            }

            this.text.color = Color.black;
            this.text.fontSize = fontSize;

            this.ButtonTextRT = rtB;
            this.Button = this.btn;

            if (name != null)
            {
                this.gameObject.name = name;
            }

            return this;
        }

        public SchwiftyButton AddSprite(Sprite sprite)
        {
            this.image.sprite = sprite;
            return this;
        }

        public SchwiftyButton SetTextColor(Color c)
        {
            this.text.color = c;
            return this;
        }

        public SchwiftyButton SetText(string text)
        {
            this.text.text = text;
            return this;
        }

        public string GetText()
        {
            return this.text.text;
        }

        public SchwiftyButton SetAct(UnityAction<SchwiftyButton> act)
        {
            this.btn.onClick.RemoveAllListeners();
            this.btn.onClick.AddListener(() => act(this));
            return this;
        }

        public SchwiftyButton SetBackgroundColor(Color color)
        {
            this.image.color = color;
            return this;
        }
    }

    public class ButtonBaseBehaviour : MonoBehaviour
    {
        protected Image Image { get; set; }

        private void Awake()
        {
            this.Image = this.GetComponent<Image>();
        }

        public void Setup()
        {
        }
    }
}