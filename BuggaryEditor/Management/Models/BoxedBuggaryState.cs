namespace Buggary.BuggaryEditor.Management.Models
{
    using BuggaryEditor.Models;

    public class BoxedBuggaryState
    {
        public BoxedBuggaryState(BuggaryState initState)
        {
            this.Value = initState;
        }

        public BuggaryState Value { get; set; }
    }
}