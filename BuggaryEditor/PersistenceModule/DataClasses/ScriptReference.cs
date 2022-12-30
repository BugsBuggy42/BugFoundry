namespace Projects.Buggary.BuggaryEditor.PersistenceModule.DataClasses
{
    using System.Collections.Generic;

    public class ScriptReference
    {
        public List<string> Paths { get; set; } = new ();

        public string LastPath { get; set; } = "";
    }
}