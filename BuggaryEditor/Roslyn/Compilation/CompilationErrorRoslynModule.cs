namespace Projects.Buggary.BuggaryEditor.Roslyn.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using UnityEngine;

    public class CompilationErrorRoslynModule
    {
        public List<Diagnostic> CheckCompilation(string source)
        {
            try
            {
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
                string assemblyName = Path.GetRandomFileName();
                MetadataReference[] references = DocumentRoslynModule.AssemblyReferences;

                CSharpCompilation compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using (MemoryStream ms = new())
                {
                    EmitResult result = compilation.Emit(ms);

                    if (!result.Success)
                    {
                        IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error);

                        return failures.ToList();
                    }
                }

                return new List<Diagnostic>();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return new List<Diagnostic>();
            }
        }
    }
}