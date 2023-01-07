namespace SchwiftyUI.BugFoundry.SchwiftyUI.V3.Containers
{
    using System;
    using System.Linq;
    using Addons;
    using Elements;
    using global::BugFoundry.Commons.CommonHelpers;
    using Inputs;
    using TMPro;
    using UnityEngine;

    public class InfoPanelFloating
    {
        private readonly SchwiftyPanelLabel label;
        private readonly RectTransform constraints;
        private readonly TMP_TextInfo textInfo;
        private readonly MouseOverMono mouseOverMono;

        public bool MouseOverTheElement = false;

        public InfoPanelFloating(SchwiftyElement parent, RectTransform constraintsIn, TMP_FontAsset fontAsset)
        {
            this.constraints = constraintsIn;
            this.label = new SchwiftyPanelLabel(parent, "InfoPanelFloating")
                .WordWrapping(true)
                .SetFontAsset(fontAsset)
                .SetLineSpacing(1)
                .SetTextSize(20)
                .SetTextColor(Color.white)
                .SetBackgroundColor(MyColor.GrayBlend(0.3f))
                .UsePositioner15(new Positioner().SameAsParent())
                .ToPanelLabel6900();

            this.mouseOverMono = this.label.panelElemnent.gameObject.AddComponent<MouseOverMono>();
            this.mouseOverMono.Initialize(() => { }, () => { });
            this.label.labelElemnent.gameObject.AddComponent<LinkOpener>();

            this.textInfo = this.label.Text.textInfo;
            this.label.gameObject.SetActive(false);
        }

        /// TODO: Consider consolidating those two methods
        public void DisplayText(string content, Vector2 position, float margins, Color color)
        {
            Vector2 panelTL = this.constraints.GetTopLeft();
            Vector2 panelSD = this.constraints.GetSizeAnchorAgnostic();
            float rightMargin = 10;
            this.label.gameObject.SetActive(true);
            this.label.SetColor(MyColor.GrayBlend(0.7f));
            this.label.SetDimensionsWithCurrentAnchors(panelSD.x - rightMargin * 2, 100);
            this.label.SetTopLeft20(panelTL.x + rightMargin, panelSD.y - 100);
            this.label.labelElemnent.Text.text = content;
            this.label.labelElemnent.Text.ForceMeshUpdate(true, true);

            TMP_CharacterInfo[] charInfos = this.textInfo.characterInfo.Take(content.Length).ToArray();
            string trimmedChars = string.Join("", charInfos.Select(x => x.character));
            // string allChars = string.Join("", this.textInfo.characterInfo.Select(x => x.character));
            if (content != trimmedChars)
            {
                Debug.LogWarning("Problem");
            }

            float minX = charInfos.Min(x => x.topLeft.x);
            float maxX = charInfos.Max(x => x.topRight.x);
            float minY = charInfos.Min(x => x.bottomLeft.y);
            float maxY = charInfos.Max(x => x.topLeft.y);

            Vector2 labelSize = new(Math.Abs(minX - maxX) + margins, Math.Abs(minY - maxY) + margins);

            this.label.SetDimensionsWithCurrentAnchors(labelSize.x,labelSize.y);

            float y;
            float yUp = position.y + labelSize.y + 35;
            float yDown = position.y - 10;

            if (yUp < panelSD.y)
                y = yUp;
            else
                y = yDown;

            this.label.SetTopLeft20(new Vector2(panelTL.x + rightMargin, y));
        }

        public void DisplayTextMouseOver(string content, Vector2 position, float margins, float lineHeight, bool mouseOverDetection = false)
        {
            Vector2 panelTL = this.constraints.GetTopLeft();
            Vector2 panelSD = this.constraints.GetSizeAnchorAgnostic();
            float rightMargin = 10;
            this.label.gameObject.SetActive(true);
            this.label.SetColor(MyColor.GrayBlend(0.7f));
            this.label.SetDimensionsWithCurrentAnchors(panelSD.x - rightMargin * 2, 100);
            this.label.SetTopLeft20(panelTL.x + rightMargin, panelSD.y - 100);
            this.label.labelElemnent.Text.text = content;
            this.label.labelElemnent.Text.ForceMeshUpdate(true, true);

            TMP_CharacterInfo[] charInfos = this.textInfo.characterInfo.Take(content.Length).ToArray();
            string trimmedChars = string.Join("", charInfos.Select(x => x.character));
            if (content != trimmedChars)
                Debug.LogWarning("Problem");

            float minX = charInfos.Min(x => x.topLeft.x);
            float maxX = charInfos.Max(x => x.topRight.x);
            float minY = charInfos.Min(x => x.bottomLeft.y);
            float maxY = charInfos.Max(x => x.topLeft.y);

            Vector2 labelSize = new(Math.Abs(minX - maxX) + margins, Math.Abs(minY - maxY) + margins);

            this.label.SetDimensionsWithCurrentAnchors(labelSize.x, labelSize.y);

            float y;
            float yUp = position.y + labelSize.y + lineHeight;
            float yDown = position.y - lineHeight;

            if (yUp < panelSD.y)
                y = yUp;
            else
                y = yDown;

            this.label.SetTopLeft20(new Vector2(panelTL.x + rightMargin, y));

            if (mouseOverDetection)
            {
                this.mouseOverMono.Initialize(
                    () =>
                    {
                        this.MouseOverTheElement = true;
                    },

                    () => { this.MouseOverTheElement = false; this.Hide(); });
            }
            else
            {
                this.mouseOverMono.Initialize(() => { }, () => { });
            }
        }

        public void Hide() => this.label.gameObject.SetActive(false);
    }
}