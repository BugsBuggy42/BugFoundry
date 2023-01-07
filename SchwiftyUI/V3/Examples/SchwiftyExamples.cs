namespace SchwiftyUI.BugFoundry.SchwiftyUI.V3.Examples
{
    using System.Collections.Generic;
    using System.Linq;
    using Containers;
    using Containers.Sizers;
    using Elements;
    using global::BugFoundry.Commons.CommonHelpers;
    using Inputs;
    using UnityEngine;

    public class SchwiftyExamples : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] Texture2D dragCursor;

        private Vector2 res2;
        private SchwiftyRoot parent;
        private ColorUtil grayBlue;
        private float grayMix = 0.6f;
        private float colorMixAmount = 0.0f;
        private Canvas canvas;

        private void Start()
        {
            this.grayBlue = new ColorUtil(Color.white, Color.black, Color.green);
            this.res2 = new Vector2(Screen.height, Screen.width);
            this.RootSidebar(out List<SchwiftyElement> tabsies);

            this.XYSplitTest(tabsies[0]);

            this.VerticalGroupMulti(tabsies[1], this.cam);

            this.YSplitTest(tabsies[2]);

            this.PanelLabelTest(tabsies[3], this.cam);

            SchwiftyElement test3 = new SchwiftyPanel(tabsies[3], "")
                .SetBackgroundColor(Color.black)
                .UsePositioner15(new Positioner().SameAsParent());
        }

        private void GenerateSplitTest()
        {
            this.canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            SchwiftyRoot parentLocal = new(this.canvas.GetComponent<RectTransform>());
            this.parent = parentLocal;
            parentLocal.SetGoNameAndEleName("parentCanv");
            parentLocal.Name = "parentCanv";

            XSplit xSplit = new();
            xSplit.SplitPanel(out SchwiftyPanel ele1, out SchwiftyPanel ele2, 0.3f, parentLocal, this.dragCursor, 0.1f);

            // this.resizers.Add(split);
            // this.ignoreList.AddRange(new[] { ele1, ele2 });

            SchwiftyElement left = new SchwiftyPanel(ele1, "")
                .SetBackgroundColor(Color.white)
                .SetGoNameAndEleName("left")
                .UsePositioner15(new Positioner()
                    .SameAsParent()
                    .SetBorder(4, 4, 4, 4));

            left.Name = "left";
            left.SetGoNameAndEleName("left");

            SchwiftyElement some =
                new SchwiftyButton(left, "", out MonoBehaviour xRayBeh, "", null, x => { Debug.Log("some"); }, 0f)
                    .SetBackgroundColor(Color.green)
                    .UsePositioner15(new Positioner()
                        .SameAsParent()
                        .SetBorder(10, 10, 30, 10));

            some.Name = "some";
            some.SetGoNameAndEleName("some");

            SchwiftyElement right = new SchwiftyPanel(ele2, "")
                .SetBackgroundColor(MyColor.Orange)
                .SetGoNameAndEleName("left")
                .UsePositioner15(new Positioner()
                    .SameAsParent()
                    .SetBorder(4, 4, 4, 4));

            right.Name = "right";
            right.SetGoNameAndEleName("right");
        }

        private void RootSidebar(out List<SchwiftyElement> testPanels)
        {
            SchwiftyRoot parentLocal = new(GameObject.Find("Canvas").GetComponent<RectTransform>());
            this.parent = parentLocal;
            parentLocal.SetGoNameAndEleName("parentCanv");
            parentLocal.Name = "parentCanv";

            XSplit xSplitOne = new();

            xSplitOne.SplitPanel(out SchwiftyPanel ele1, out SchwiftyPanel ele2, 0.1f, parentLocal, this.dragCursor,
                0.1f);

            SchwiftyElement left = new SchwiftyPanel(ele1, "")
                .SetBackgroundColor(Color.white)
                .SetGoNameAndEleName("left")
                .UsePositioner15(new Positioner()
                    .SameAsParent()
                    .SetBorder(4, 4, 4, 4));

            SchwiftyElement right = new SchwiftyPanel(ele2, "")
                .SetBackgroundColor(MyColor.Orange)
                .SetGoNameAndEleName("right")
                .UsePositioner15(new Positioner()
                    .SameAsParent()
                    .SetBorder(4, 4, 4, 4));

            List<SchwiftyButton> tabs = new()
            {
                new SchwiftyButton(null, "", out _, "0")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab1").ToButton6900(),
                new SchwiftyButton(null, "", out _, "1")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab2").ToButton6900(),
                new SchwiftyButton(null, "", out _, "2")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab3").ToButton6900(),
                new SchwiftyButton(null, "", out _, "3")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab4").ToButton6900(),
                new SchwiftyButton(null, "", out _, "4")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab5").ToButton6900(),
                new SchwiftyButton(null, "", out _, "5")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab6").ToButton6900(),
                new SchwiftyButton(null, "", out _, "6")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab7").ToButton6900(),
                new SchwiftyButton(null, "", out _, "7")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab8").ToButton6900(),
                new SchwiftyButton(null, "", out _, "8")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab9").ToButton6900(),
                new SchwiftyButton(null, "", out _, "9")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab10").ToButton6900(),
            };

            VerticalGroup vertGroup = new();
            vertGroup.Create(left, 5, 5, new YSizer(SizerType.ElementFixedSideProportional, 0.25f),
                tabs.Cast<SchwiftyElement>().ToList());
            TabPanelsPreexisting tabsiesMaker = new TabPanelsPreexisting();
            tabsiesMaker.Create(tabs, right, Color.gray, Color.cyan, out List<SchwiftyElement> panelsies);
            testPanels = panelsies;
        }

        private void YSplitTest(SchwiftyElement parent)
        {
            YSplit s = new();
            s.SplitPanel(out SchwiftyPanel top, out SchwiftyPanel bottom, 0.3f, parent, this.dragCursor, 0.3f);
            top.SetBackgroundColor(Color.blue);
            bottom.SetBackgroundColor(Color.yellow);
        }

        private void XYSplitTest(SchwiftyElement parent)
        {
            XSplit x = new();
            YSplit y = new();
            YSplit y2 = new();

            x.SplitPanel(out SchwiftyPanel left, out SchwiftyPanel right, 0.5f, parent, this.dragCursor, 0.3f);
            y.SplitPanel(out SchwiftyPanel top, out SchwiftyPanel bottom, 0.3f, left, this.dragCursor, 0.3f);
            y2.SplitPanel(out SchwiftyPanel top2, out SchwiftyPanel bottom2, 0.3f, right, this.dragCursor, 0.3f);

            top.SetBackgroundColor(Color.blue);
            bottom.SetBackgroundColor(Color.yellow);
            top2.SetBackgroundColor(Color.black);
            bottom2.SetBackgroundColor(Color.white);
        }

        private void VerticalGroupMulti(SchwiftyElement parentIn, Camera cam)
        {
            List<SchwiftyElement> tabs1 = new()
            {
                new SchwiftyButton(null, "", out _, "+")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab1").ToButton6900(),
                new SchwiftyButton(null, "", out _, "+")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab2").ToButton6900(),
            };

            List<SchwiftyElement> tabs2 = new()
            {
                new SchwiftyButton(null, "", out _, "-")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab1").ToButton6900(),
                new SchwiftyButton(null, "", out _, "-")
                    .SetBackgroundColor(this.grayBlue.GetBlend(this.grayMix, this.colorMixAmount))
                    .SetGoNameAndEleName("tab2").ToButton6900(),
            };

            List<SchwiftyElement> tabs3 = new()
            {
                new SchwiftyInput(null, "", cam).SetGoNameAndEleName("input1").ToInput6900(),
                new SchwiftyInput(null, "", cam).SetGoNameAndEleName("input2").ToInput6900(),
            };

            for (int i = 0; i < 2; i++)
            {
                SchwiftyElement tPlus = tabs1[i].ToButton6900();
                SchwiftyElement tMinus = tabs2[i].ToButton6900();
                SchwiftyInput input = tabs3[i].ToInput6900();

                int value = 0;
                input.InputField.text = value.ToString();

                tPlus.Button.onClick.AddListener(() =>
                {
                    value++;
                    input.InputField.text = value.ToString();
                });

                tMinus.Button.onClick.AddListener(() =>
                {
                    value--;
                    input.InputField.text = value.ToString();
                });
            }

            List<List<SchwiftyElement>> elements = new() { tabs1, tabs2, tabs3 };

            VerticalGroupMulti mg = new();
            mg.CreateWithScroll(parentIn, 0, 0, new List<float>() { 2, 2, 3 }, elements);
        }

        private void PanelLabelTest(SchwiftyElement parentIn, Camera cam)
        {
            float y = parentIn.RectTransform.sizeDelta.y;

            Vector2 tl = SchwiftyElement.GetTopLeft(parentIn.RectTransform);

            SchwiftyPanelLabel pl = new SchwiftyPanelLabel(parentIn, "Value", "Value")
                .SetTextColor(Color.red)
                .SetBackgroundColor(Color.blue)
                .SetDimensionsWithCurrentAnchors(500, 100)
                .SetTopLeft20(tl.x + 150, tl.y - 150)
                .ToPanelLabel6900();
        }

        private void RecAnchor0011(SchwiftyElement element, List<SchwiftyElement> ignoreListRec)
        {
            // Debug.Log($"Rec for element {element.Name} with children count: {element.Children.Count}");
            foreach (SchwiftyElement child in element.Children)
            {
                this.RecAnchor0011(child, ignoreListRec);

                if (ignoreListRec.Contains(child))
                    continue;

                child.SetAnchors10(new Vector2(0, 0), new Vector2(1, 1));
                // Debug.Log($"Set anchor for element {child.Name}");
            }
        }

        private void RecAnchor05(SchwiftyElement element, List<SchwiftyElement> ignoreListRec)
        {
            // Debug.Log($"Rec for element {element.Name} with children count: {element.Children.Count}");
            foreach (SchwiftyElement child in element.Children)
            {
                this.RecAnchor0011(child, ignoreListRec);

                if (ignoreListRec.Contains(child))
                    continue;

                child.SetAnchors10(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                // Debug.Log($"Set anchor for element {child.Name}");
            }
        }
    }
}