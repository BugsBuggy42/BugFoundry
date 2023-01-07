namespace BugFoundry.BugFoundryEditor.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Management;
    using SchwiftyUI.BugFoundry.SchwiftyUI.V3;
    using SchwiftyUI.BugFoundry.SchwiftyUI.V3.Containers;
    using SchwiftyUI.BugFoundry.SchwiftyUI.V3.Containers.Sizers;
    using SchwiftyUI.BugFoundry.SchwiftyUI.V3.Elements;
    using SchwiftyUI.BugFoundry.SchwiftyUI.V3.Inputs;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class ListSelectionFloatingMenu
    {
        private bool Enabled { get; set; } = false;

        private Canvas canvas;
        private SchwiftyElement menuParent;
        private List<SchwiftyElement> labelList = new();
        private List<string> currentItems = new();
        private SchwiftyElement contentParent;
        private ScrollRect maskScrollRect;
        private VerticalGroup vertGroup;
        private SchwiftyPanel editorPanel;
        private BugFoundryColors colors;

        private const float FontSize = 15;
        private readonly Color labelNotSelected = Color.Lerp(Color.white, Color.black, 0.3f);
        private Dictionary<string, SchwiftyElement> dict = new();
        private Action<string> itemClickedAction;
        private Action<string> enterAction;
        private string selectedLabel = "";
        private readonly float downPosOffset = 10;
        private readonly float upPosOffset = 35;

        public List<string> GetCurrentItems() => this.currentItems;

        public SchwiftyElement Create(
            RectTransform rootParent,
            SchwiftyPanel editorPanelIn,
            BugFoundryColors colorsIn,
            Action<string> itemClickedActionIn,
            Action<string> enterActionIn = null)
        {
            this.colors = colorsIn;
            this.editorPanel = editorPanelIn;
            this.itemClickedAction = itemClickedActionIn;
            this.enterAction = enterActionIn;
            SchwiftyRoot root = new(rootParent);
            this.menuParent = new SchwiftyPanel(root, "ListSelectionFloatingMenu")
                .SetBackgroundColor(Color.gray)
                .UsePositioner15(new Positioner().PercentOfParentY(0.4f, 0.7f));

            return this.menuParent;
        }

        public void Update(Vector2 targetPosition)
        {
            if (this.Enabled == false) return;

            this.UpdateTransform(targetPosition);

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                int cIndex = this.labelList.IndexOf(this.GetSelectedLabel());
                if (cIndex + 1 >= this.labelList.Count) return;
                SchwiftyElement newElement = this.labelList[cIndex + 1];
                this.SetSelected(((SchwiftyPanelLabel)newElement).Text.text, true);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                int cIndex = this.labelList.IndexOf(this.GetSelectedLabel());
                if (cIndex - 1 < 0) return;
                SchwiftyElement newElement = this.labelList[cIndex - 1];
                this.SetSelected(((SchwiftyPanelLabel)newElement).Text.text, true);
            }

            if (this.enterAction != null && Input.GetKeyDown(KeyCode.Return))
                this.enterAction.Invoke(this.selectedLabel);
        }

        public void SetEnabled(bool enabledIn)
        {
            this.Enabled = enabledIn;
            this.menuParent.gameObject.SetActive(this.Enabled);
        }

        private void UpdateTransform(Vector2 targetPosition)
        {
            this.menuParent.UsePositioner15(new Positioner().PercentOfParentY(0.4f, 0.7f));
            Vector2 editorPanelSize = this.editorPanel.RectTransform.GetSizeAnchorAgnostic();
            Vector2 editorPanelTopLeft = this.editorPanel.RectTransform.GetTopLeft();
            Vector2 menuParentSize = this.menuParent.RectTransform.GetSizeAnchorAgnostic();
            Vector2 newPosition =
                this.GetBoxPosition(targetPosition, editorPanelSize, editorPanelTopLeft, menuParentSize);
            this.menuParent?.SetTopLeft20(newPosition);
        }

        /// <summary>
        /// </summary>
        /// <param name="targetPosition">This is in local space, so relative to the editorPanelSize</param>
        /// <param name="editorPanelSize"></param>
        /// <param name="editorPanelTopLeft"></param>
        /// <param name="menuSize"></param>
        /// <returns></returns>
        public Vector2 GetBoxPosition(Vector2 targetPosition, Vector2 editorPanelSize, Vector2 editorPanelTopLeft,
            Vector2 menuSize)
        {
            string diag = "";
            float x;
            float xRight = targetPosition.x;
            float xLeft = targetPosition.x - menuSize.x;

            if (xRight + menuSize.x > editorPanelSize.x)
                x = xLeft;
            else
                x = xRight;

            float y;
            float yUp = targetPosition.y + menuSize.y + this.upPosOffset;
            float yDown = targetPosition.y - this.downPosOffset;

            // Assumption: the editor takes the entire y dimension;
            if (yUp < editorPanelSize.y)
                y = yUp;
            else
                y = yDown;

            return new Vector2(x + editorPanelTopLeft.x, y);
        }

        public void SetItems(List<string> elements)
        {
            this.currentItems = elements;

            if (elements.Count == 0)
            {
                // Debug.Log("No Elements...");
                return;
            }

            foreach (SchwiftyElement item in this.labelList)
                item.Destroy();

            this.labelList = elements.Distinct().Select(x =>
                    new SchwiftyPanelLabel(this.menuParent, $"selectItem-{x}", x)
                        .SetMargin(new Vector4(5, 0, 0, 0))
                        .SetHorizontalAlignment(HorizontalAlignmentOptions.Left)
                        .SetTextSize(FontSize)
                        .SetTextColor(Color.black))
                .Cast<SchwiftyElement>()
                .ToList();

            this.dict.Clear();
            this.dict = this.labelList.ToDictionary(x => ((SchwiftyPanelLabel)x).Text.text, x => x);

            this.vertGroup?.Destroy();
            this.vertGroup = new();
            this.vertGroup.Create(
                this.menuParent,
                5,
                5,
                new YSizer(SizerType.ElementFixedSideProportional, 0.1f),
                this.labelList,
                scrollbarBody: this.colors.scrollbarBody,
                scrollbarHandle: this.colors.scrollbarHandle);

            this.contentParent = this.vertGroup.GetContentParent();
            this.maskScrollRect = this.vertGroup.GetScrollRect();

            foreach (SchwiftyPanelLabel element in this.labelList.Cast<SchwiftyPanelLabel>())
                element.SetBackgroundColor(this.labelNotSelected);

            (this.labelList[0] as SchwiftyPanelLabel)?.SetColor(Color.white);
            this.selectedLabel = (this.labelList[0] as SchwiftyPanelLabel)?.Text.text;
            this.SetSelected((this.labelList[0] as SchwiftyPanelLabel)?.Text.text, true);

            foreach (SchwiftyElement item in this.labelList)
            {
                SchwiftyPanel some = ((SchwiftyPanelLabel)item).panelElemnent;
                some.gameObject.name = ((SchwiftyPanelLabel)item).Text.text;
                Clicky clicky = some.gameObject.AddComponent<Clicky>();
                clicky.Setup(() =>
                {
                    string text = ((SchwiftyPanelLabel)item).Text.text;
                    if (this.selectedLabel == text)
                        this.itemClickedAction(text);
                    else
                        this.SetSelected(text, true, false);
                });
            }
        }

        public void ReorderItems(string searchTerm)
        {
            if (this.labelList.Count == 0 || this.labelList == null) return;

            this.ColorDeselected(this.dict[this.selectedLabel]);
            this.labelList.Sort((a, b) => this.SortFunction(a, b, searchTerm));
            this.vertGroup.ReorderExistingElements();
            this.selectedLabel = ((SchwiftyPanelLabel)this.labelList[0]).Text.text;
            this.SetSelected(((SchwiftyPanelLabel)this.labelList[0])?.Text.text, true);
        }

        private int SortFunction(SchwiftyElement a, SchwiftyElement b, string searchTerm)
        {
            SchwiftyPanelLabel aa = (SchwiftyPanelLabel)a;
            SchwiftyPanelLabel bb = (SchwiftyPanelLabel)b;
            string aaText = aa.Text.text;
            string bbText = bb.Text.text;
            int aaRank = this.RankMatch(aaText, searchTerm);
            int bbRank = this.RankMatch(bbText, searchTerm);
            if (aaRank == bbRank)
            {
                if (aaText.Length < bbText.Length) return -1;
                if (aaText.Length > bbText.Length) return 1;
                return 0;
            }

            if (aaRank > bbRank) return 1;
            if (aaRank < bbRank) return -1;
            return 0;
        }

        private int RankMatch(string input, string search)
        {
            int index = input.IndexOf(search, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1)
                index = 1000;
            return index;
        }

        private void SetSelected(string selected, bool setSelected, bool snap = true)
        {
            this.ColorDeselected(this.GetSelectedLabel());

            if (setSelected)
                this.selectedLabel = selected;

            this.ColorSelected(this.GetLabel(selected));

            if (snap)
                this.SnapTo(this.contentParent.RectTransform, this.GetLabel(selected).RectTransform,
                    this.maskScrollRect);
        }

        private void ColorSelected(SchwiftyElement element)
            => (element as SchwiftyPanelLabel)?.SetColor(Color.white);

        private void ColorDeselected(SchwiftyElement element)
            => (element as SchwiftyPanelLabel)?.SetColor(this.labelNotSelected);

        public SchwiftyPanelLabel GetSelectedLabel()
        {
            try
            {
                return this.dict[this.selectedLabel] as SchwiftyPanelLabel;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return null;
            }
        }

        private SchwiftyPanelLabel GetLabel(string key)
        {
            try
            {
                return (this.dict[key] as SchwiftyPanelLabel);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                return null;
            }
        }

        private void SnapTo(RectTransform contentPanel, RectTransform target, ScrollRect scrollRect)
        {
            Canvas.ForceUpdateCanvases();
            contentPanel.anchoredPosition =
                (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
                - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
        }
    }
}