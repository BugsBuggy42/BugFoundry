namespace SchwiftyUI.BugFoundry.SchwiftyUI.V3.Containers.Sizers
{
    public class YSizer
    {
        public SizerType type;
        public float value;

        public YSizer(SizerType typeIn, float valueIn)
        {
            this.type = typeIn;
            this.value = valueIn;
        }
    }
}