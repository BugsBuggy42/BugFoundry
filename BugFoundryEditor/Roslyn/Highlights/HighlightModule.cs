namespace BugFoundry.BugFoundryEditor.Roslyn.Highlights
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Management;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Classification;
    using Microsoft.CodeAnalysis.Text;
    using UnityEngine;

    public class HighlightModule
    {
        private readonly BuggaryColors colors;

        public HighlightModule(BuggaryColors colorsIn)
        {
            this.colors = colorsIn;
        }

        // ReSharper disable once CognitiveComplexity
        public async Task<List<Highlights.Range>> Highlight(string textIn)
        {
            AdhocWorkspace workspace = new();
            Solution solution = workspace.CurrentSolution;
            Project project = solution.AddProject("projectName", "assemblyName", LanguageNames.CSharp)
                .WithMetadataReferences(DocumentRoslynModule.AssemblyReferences);
            Document document = project.AddDocument("name.cs", textIn);
            SourceText text = await document.GetTextAsync();
            IEnumerable<ClassifiedSpan> classifiedSpans =
                await Classifier.GetClassifiedSpansAsync(document, TextSpan.FromBounds(0, text.Length));
            IEnumerable<Highlights.Range> ranges = classifiedSpans.Select(classifiedSpan =>
                new Highlights.Range(classifiedSpan, text.GetSubText(classifiedSpan.TextSpan).ToString()));
            ranges = this.FillGaps(text, ranges);
            List<Highlights.Range> listRanges = ranges.ToList();

            foreach (Highlights.Range range in listRanges)
            {
                switch (range.ClassificationType)
                {
                    case "keyword":
                        range.Color = this.colors.keyword;
                        break;
                    case "class name":
                        range.Color = this.colors.className;
                        break;
                    case "identifier":
                        range.Color = this.colors.identifier;
                        break;
                    case "string":
                        range.Color = this.colors.stringLiteral;
                        break;
                    case "method name":
                        range.Color = this.colors.methodName;
                        break;
                    case "interface name":
                        range.Color = this.colors.interfaceName;
                        break;
                    case "namespace name":
                        range.Color = this.colors.namespaceName;
                        break;
                    case "parameter name":
                        range.Color = this.colors.parameterName;
                        break;
                    case "static symbol":
                        range.Color = this.colors.staticSymbol;
                        break;
                    case "keyword - control":
                        range.Color = this.colors.keywordControl;
                        break;
                    case "local name":
                        range.Color = range.Color = this.colors.defaultColor;
                        break;
                    case "property name":
                        range.Color = this.colors.propertyName;
                        break;
                    case "struct name":
                        range.Color = this.colors.structName;
                        break;
                    case "punctuation":
                        range.Color = this.colors.defaultColor;
                        break;
                    case "operator":
                        range.Color = this.colors.defaultColor;
                        break;
                    case "enum member name":
                        range.Color = this.colors.enumMemberName;
                        break;
                    case "enum name":
                        range.Color = this.colors.enumName;
                        break;
                    case "delegate name":
                        range.Color = this.colors.delegateName;
                        break;
                    case "field name":
                        range.Color = this.colors.fieldName;
                        break;
                    case "comment":
                        range.Color = this.colors.comment;
                        break;
                    case "number":
                        range.Color = this.colors.number;
                        break;
                    case "operator - overloaded":
                        range.Color = this.colors.operatorOverloaded;
                        break;
                    case "":
                    case null:
                        range.Color = this.colors.defaultColor;
                        break;
                    default:
                        Debug.Log($"|{range.Text}| |{range.ClassificationType}| Unrecognized");
                        range.Color = this.colors.defaultColor;
                        break;
                }
            }

            return listRanges;
        }

        private IEnumerable<Highlights.Range> FillGaps(SourceText text, IEnumerable<Highlights.Range> ranges)
        {
            const string whitespaceClassification = null;
            int current = 0;
            Highlights.Range previous = null;

            foreach (Highlights.Range range in ranges)
            {
                int start = range.TextSpan.Start;
                if (start > current)
                {
                    yield return new Highlights.Range(whitespaceClassification, TextSpan.FromBounds(current, start), text);
                }

                if (previous == null || range.TextSpan != previous.TextSpan)
                {
                    yield return range;
                }

                previous = range;
                current = range.TextSpan.End;
            }

            if (current < text.Length)
            {
                yield return new Highlights.Range(whitespaceClassification, TextSpan.FromBounds(current, text.Length), text);
            }
        }
    }
}