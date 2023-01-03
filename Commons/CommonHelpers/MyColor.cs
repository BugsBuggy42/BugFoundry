namespace BugFoundry.Commons.CommonHelpers
{
    using UnityEngine;

    public class MyColor
    {
        public static Color32 Lime = new Color32(166, 254, 0, 254);
        public static Color32 Green = new Color32(0, 254, 111, 254);
        public static Color32 Aqua = new Color32(0, 201, 254, 254);
        public static Color32 Blue = new Color32(0, 122, 254, 254);
        public static Color32 Navy = new Color32(60, 0, 254, 254);
        public static Color32 Purple = new Color32(143, 0, 254, 254);
        public static Color32 Pink = new Color32(232, 0, 254, 254);
        public static Color32 Red = new Color32(254, 9, 0, 254);
        public static Color32 Orange = new Color32(254, 161, 0, 254);
        public static Color32 Yellow = new Color32(254, 224, 0, 254);
        public static Color32 Brown = new Color32(79, 54, 0, 254);

        public static Color GrayBlend(float black)
        {
            return Color.Lerp(Color.white, Color.black, black);
        }
    }

    public class MyColorTri
    {
        public static Color32 Ground = new Color32(153, 105, 0, 1);
    }
}