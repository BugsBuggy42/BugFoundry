namespace BugFoundry.BugFoundryEditor.Roslyn.Highlights
{
    using Microsoft.CodeAnalysis.Classification;
    using Microsoft.CodeAnalysis.Text;
    using UnityEngine;

    public class Range
    {
        public Color Color { get; set; }
        public ClassifiedSpan ClassifiedSpan { get; private set; }
        public string Text { get; private set; }

        public Range(string classification, TextSpan span, SourceText text) :
            this(classification, span, text.GetSubText(span).ToString())
        {
        }

        public Range(string classification, TextSpan span, string text) :
            this(new ClassifiedSpan(classification, span), text)
        {
        }

        public Range(ClassifiedSpan classifiedSpan, string text)
        {
            this.ClassifiedSpan = classifiedSpan;
            this.Text = text;
        }

        public string ClassificationType => this.ClassifiedSpan.ClassificationType;

        public TextSpan TextSpan => this.ClassifiedSpan.TextSpan;
    }
}