namespace BugFoundry.SchwiftyUI.V3.Elements
{
    using System;
    using System.Collections.Generic;
    using Containers;
    using UnityEngine;

    public class SchwiftyRoot : SchwiftyElement
    {
        private Vector2 screen;
        private List<IResizeable> Resizers { get; set; } = new();

        public SchwiftyRoot(RectTransform rootElement, string name = null)
        {
            this.RectTransform = rootElement;
            this.gameObject = rootElement.gameObject;
            this.gameObject.name = $"{name} Root";
            this.SetupMono(this.gameObject, this.Update);
        }

        private void Update()
        {
            if (Math.Abs(this.screen.x - Screen.height) < 0.002f && Math.Abs(this.screen.y - Screen.width) < 0.002f)
                return;

            this.screen = new Vector2(Screen.height, Screen.width);

            foreach (IResizeable item in this.Resizers) item.Resize();
        }

        private void SetupMono(GameObject rootElement, Action act)
        {
            SchwiftyCanvasMono mono = rootElement.AddComponent<SchwiftyCanvasMono>();
            mono.Setup(act);
        }

        public void AddToToResizeCollections(List<IResizeable> resizers)
        {
            this.Resizers.AddRange(resizers);
        }
    }
}