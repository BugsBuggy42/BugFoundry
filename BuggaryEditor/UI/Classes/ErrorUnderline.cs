namespace Buggary.BuggaryEditor.UI.Classes
{
    using Microsoft.CodeAnalysis;
    using SchwiftyUI.V3.Elements;

    public class ErrorUnderline
    {
        public ErrorUnderline(SchwiftyPanel panel, Box box, Diagnostic diagnostic)
        {
            this.Underline = panel;
            this.Box = box;
            this.Diagnostic = diagnostic;
        }

        public SchwiftyPanel Underline { get; }
        public Box Box { get; }
        public Diagnostic Diagnostic { get; }
    }
}