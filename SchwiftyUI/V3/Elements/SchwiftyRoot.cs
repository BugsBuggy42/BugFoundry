namespace BugFoundry.SchwiftyUI.V3.Elements
{
    using System;
    using System.Collections.Generic;
    using Containers;
    using global::SchwiftyUI.BugFoundry.SchwiftyUI.V3;
    using UnityEngine;

    public class SchwiftyRoot : SchwiftyElement
    {
        private Vector2 screen;
        private List<IResizeable> Resizers { get; set; } = new();
        private string name;

        public SchwiftyRoot(RectTransform rootElement, string name = null)
        {
            this.Name = name;
            this.RectTransform = rootElement;
            this.gameObject = rootElement.gameObject;
            this.gameObject.name = $"{name} Root";
            this.SetupMono(this.gameObject, this.Update, name);
        }

        private void Update()
        {
            if (Math.Abs(this.screen.x - Screen.height) < 0.002f && Math.Abs(this.screen.y - Screen.width) < 0.002f)
                return;

            // Debug.Log(this.Name + " Screen Size Changed");

            Debug.Log($"HERE445");

            this.screen = new Vector2(Screen.height, Screen.width);

            foreach (IResizeable item in this.Resizers) item.Resize();
        }

        private void SetupMono(GameObject rootElement, Action act, string name)
        {
            SchwiftyCanvasMono mono = rootElement.AddComponent<SchwiftyCanvasMono>();
            mono.Setup(act);
        }

        public void AddToResizeCollections(List<IResizeable> resizers)
        {
            this.Resizers.AddRange(resizers);
        }
    }
}