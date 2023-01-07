namespace BugFoundry.BugFoundryEditor.Management
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;
    using Roslyn;
    using SchwiftyUI.BugFoundry.SchwiftyUI.V3.Containers;
    using SchwiftyUI.BugFoundry.SchwiftyUI.V3.Elements;
    using TMPro;
    using UnityEngine;

    public class OverloadBugFoundryModule
    {
        private readonly InfoPanelFloating floatingPanel;

        public OverloadBugFoundryModule(RectTransform parent, SchwiftyPanel editorPanel, TMP_FontAsset font)
        {
            this.floatingPanel = new InfoPanelFloating(new SchwiftyRoot(parent), editorPanel.RectTransform, font);
        }

        public void Update(int caretIndex, bool caretChanged, string text, Vector2 caretPosition)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                this.floatingPanel.Hide();

            if (caretChanged && caretIndex >= 0 && caretIndex < text.Length)
            {
                List<string> overloads = this.GetAllOverloads(text, caretIndex);
                if (overloads.Count > 0)
                    this.floatingPanel.DisplayText(string.Join("\n", overloads), caretPosition, 10, Color.white);
                else
                    this.floatingPanel.Hide();
            }
        }

        private List<string> GetAllOverloads(string codeIn, int position)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeIn);
            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = DocumentRoslynModule.AssemblyReferences;

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            SyntaxToken theToken = syntaxTree.GetRoot().FindToken(position);
            if (theToken.Value == null)
            {
                Debug.Log("theToken.Value == null");
                return null;
            }

            TextSpan span = theToken.Span;
            SyntaxNode theNode = syntaxTree.GetRoot().FindNode(span);
            SymbolInfo info = semanticModel.GetSymbolInfo(theNode);

            List<string> result = new();

            foreach (ISymbol candidateSymbol in info.CandidateSymbols)
            {
                ImmutableArray<ITypeParameterSymbol> pars = candidateSymbol.ContainingType.TypeParameters;

                foreach (ITypeParameterSymbol typeParameterSymbol in pars)
                    Debug.Log(typeParameterSymbol.ToDisplayString());

                IMethodSymbol symbol = (IMethodSymbol)candidateSymbol;
                ITypeSymbol returnType = symbol.ReturnType;
                result.Add($"{returnType.ToDisplayString()} {symbol.ToDisplayString()}");
            }

            return result;
        }
    }
}