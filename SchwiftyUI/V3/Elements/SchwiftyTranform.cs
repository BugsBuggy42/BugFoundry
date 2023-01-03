namespace BugFoundry.SchwiftyUI.V3.Elements
{
    using Other;
    using UnityEngine;

    /// <summary>
    /// Always set dimensions before setting position!
    /// </summary>
    public class SchwiftyTransform : SchwiftyElement
    {
        public SchwiftyTransform(SchwiftyElement parent,string name)
        {
            this.Type = ElementType.TransformOnly;
            this.CreateElement(name);
            this.SetParent(parent);
        }

        public SchwiftyElement CreateElement(string name)
        {
            GameObject go = new (this.Name);
            RectTransform rt = go.AddComponent<RectTransform>();

            this.RectTransform = rt;
            this.gameObject = go;

            if (name != null)
            {
                this.gameObject.name = name;
            }

            return this;
        }
    }
}