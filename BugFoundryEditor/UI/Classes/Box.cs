namespace BugFoundry.BugFoundryEditor.UI.Classes
{
    using UnityEngine;

    public class Box
    {
        public Box(Vector2 start, Vector2 end, float lineHeight)
        {
            this.x1 = start.x;
            this.x2 = end.x;
            this.y1 = start.y + lineHeight;
            this.y2 = start.y;
        }

        private readonly float x1;
        private readonly float x2;
        private readonly float y1;
        private readonly float y2;

        public bool Inside(Vector2 pos)
        {
            if (pos.x > this.x1 && pos.x < this.x2 && pos.y < this.y1 && pos.y > this.y2)
                return true;

            return false;
        }
    }
}