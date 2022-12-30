namespace SchwiftyUI.V3
{
    using System;
    using Containers.Sizers;
    using UnityEngine;

    public static class SchwiftyExtensions
    {
        public static Vector2 GetTopLeft(this RectTransform t)
        {
            Vector2 pivot = t.pivot;
            return new Vector2(t.position.x - t.rect.size.x * pivot.x, t.position.y + t.rect.size.y * (1 - pivot.y));
        }

        public static Vector2 GetSizeAnchorAgnostic(this RectTransform t)
        {
            return new Vector2(t.rect.width, t.rect.height);
        }

        public static float GetYSize(this YSizer sizer, float fixedSideLenght, float parentSide)
        {
            switch (sizer.type)
            {
                case SizerType.Absolute:
                    return sizer.value;
                    break;
                case SizerType.ParentProportional:
                    return sizer.value * parentSide;
                    break;
                case SizerType.ElementFixedSideProportional:
                    return sizer.value * fixedSideLenght;
                    break;
                default: throw new Exception();
            }
        }

        public static void SetTopLeft(this RectTransform rt, float x, float y, float z = 0)
        {
            Vector2 size = rt.rect.size;
            rt.position = new Vector3(x + size.x / 2, y - size.y / 2, z);
        }
    }
}