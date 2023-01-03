namespace BugFoundry.BugFoundryEditor.Management.Models
{
    using BugFoundryEditor.Models;

    public class BoxedBugFoundryState
    {
        public BoxedBugFoundryState(BugFoundryState initState)
        {
            this.Value = initState;
        }

        public BugFoundryState Value { get; set; }
    }
}