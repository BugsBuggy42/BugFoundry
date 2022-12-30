namespace Buggary.BuggaryEditor.Roslyn.Refactors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.FindSymbols;
    using UnityEngine;

    public class AddMissingImportRoslynModule
    {
        public async Task<Document> AddSingleUsing(Document document, string newNamespace, CancellationToken cancellationToken)
        {
            UsingDirectiveSyntax newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(newNamespace));
            SyntaxTree st = await document.GetSyntaxTreeAsync();
            SyntaxTree newSyntaxTree = this.UpdateUsingDirectives(st, new [] {newUsing});
            SyntaxNode newRoot = newSyntaxTree.GetRoot();
            Document newDoc = document.WithSyntaxRoot(newRoot);
            return newDoc;
        }

        private SyntaxTree UpdateUsingDirectives(SyntaxTree originalTree, UsingDirectiveSyntax[] newUsings)
        {
            CompilationUnitSyntax rootNode = originalTree.GetRoot() as CompilationUnitSyntax;
            rootNode = rootNode.AddUsings(newUsings).NormalizeWhitespace();
            return rootNode.SyntaxTree;
        }

        public async Task<List<string>> GetCandidateNamespaces(Document document, int caretPosition, CancellationToken cancellationToken)
        {
            List<string> result = new ();

            try
            {
                SemanticModel semanticModel = document.GetSemanticModelAsync(cancellationToken).Result;

                IEnumerable<IdentifierNameSyntax> unresolved = (await document.GetSyntaxRootAsync()).DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Where(x => semanticModel.GetSymbolInfo(x).Symbol == null);

                unresolved = unresolved.Where(x =>
                    x.Identifier.Span.Start <= caretPosition && x.Identifier.Span.End >= caretPosition);

                foreach (IdentifierNameSyntax identifier in unresolved)
                {
                    IEnumerable<ISymbol> candidateUsings = await SymbolFinder
                        .FindDeclarationsAsync(document.Project, identifier.Identifier.ValueText, ignoreCase: false);

                    foreach (ISymbol item in candidateUsings)
                        result.Add(item.ContainingNamespace.ToDisplayString());
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return result;
        }

        private void LogText(Document doc, string id) => Debug.Log($"{id}\n{doc.GetTextAsync().Result.ToString()}");
    }
}