namespace Projects.Buggary.BuggaryEditor.Infrastructure
{
    using UnityEngine;

    public static class Extensions
    {
        public static RectTransform GetRectTransform(this GameObject input) => input.GetComponent<RectTransform>();

        public static RectTransform GetRectTransform(this Transform input) => (RectTransform)input;

        public static RectTransform GetRectTransform(this MonoBehaviour input) => input.GetComponent<RectTransform>();
    }
}