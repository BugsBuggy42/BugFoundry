namespace BugFoundry.Commons.CommonHelpers
{
    using UnityEngine;

    public class ColorUtil
    {
        private Color c1;
        private Color c2;
        private Color c3;

        public ColorUtil(Color c1, Color c2, Color c3)
        {
            this.c1 = c1;
            this.c2 = c2;
            this.c3 = c3;
        }

        public Color GetBlend(float prop1, float prop2)
        {
            Color result = Color.Lerp(this.c1, this.c2, prop1);
            result = Color.Lerp(result, this.c3, prop2);
            return result;
        }

        public Color GetBlend(float prop1, float prop2, Color color3)
        {
            Color result = Color.Lerp(this.c1, this.c2, prop1);
            result = Color.Lerp(result, color3, prop2);
            return result;
        }
    }
}