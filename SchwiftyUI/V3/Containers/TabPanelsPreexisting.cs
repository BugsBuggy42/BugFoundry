namespace Buggary.SchwiftyUI.V3.Containers
{
    using System.Collections.Generic;
    using Elements;
    using Inputs;
    using UnityEngine;

    public class TabPanelsPreexisting
    {
        public void Create(List<SchwiftyButton> tabs, SchwiftyElement panelsParent, Color tabsColor, Color tabsSelectedColor, out List<SchwiftyElement> panelsOut)
        {
            List<SchwiftyElement> panels = new ();

            for (int i = 0; i < tabs.Count; i++)
            {
                SchwiftyElement panel = new SchwiftyTransform(panelsParent, $"tabPanel{i}").UsePositioner15(new Positioner().SameAsParent());

                if (i != 0)
                {
                    panel.SetActive(false);
                }

                panels.Add(panel);

                SchwiftyButton newTab = tabs[i];

                newTab.Button.onClick.AddListener(() =>
                {
                    foreach (var p in panels)
                    {
                        p.SetActive(false);
                    }

                    foreach (var t in tabs)
                    {
                        t.SetBackgroundColor(tabsColor);
                    }

                    panel.SetActive(true);
                    newTab.SetBackgroundColor(tabsSelectedColor);
                });

                if (i == 0)
                {
                    newTab.SetBackgroundColor(tabsSelectedColor);
                }
                else
                {
                    newTab.SetBackgroundColor(tabsColor);
                }
            }

            panelsOut = panels;
        }
    }
}