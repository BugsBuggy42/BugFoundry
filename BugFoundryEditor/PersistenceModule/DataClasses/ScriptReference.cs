namespace BugFoundry.BugFoundryEditor.PersistenceModule.DataClasses
{
    using System.Collections.Generic;

    public class ScriptReference
    {
        public List<string> Paths { get; set; } = new ();

        public string LastPath { get; set; } = "";
    }
}