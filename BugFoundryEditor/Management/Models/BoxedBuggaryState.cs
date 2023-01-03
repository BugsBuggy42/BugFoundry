namespace BugFoundry.BugFoundryEditor.Management.Models
{
    using BugFoundryEditor.Models;

    public class BoxedBuggaryState
    {
        public BoxedBuggaryState(BuggaryState initState)
        {
            this.Value = initState;
        }

        public BuggaryState Value { get; set; }
    }
}