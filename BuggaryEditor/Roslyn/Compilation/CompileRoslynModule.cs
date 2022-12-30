namespace Buggary.BuggaryEditor.Roslyn.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using UnityEngine;

    public class CompileRoslynModule
    {
        public Type Parse(string source)
        {
            try
            {
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
                string assemblyName = Path.GetRandomFileName();
                MetadataReference[] references = new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(MonoBehaviour).Assembly.Location)
                };

                CSharpCompilation compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using MemoryStream ms = new();
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                        Debug.Log($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    return this.HandleParsedAssembly(Assembly.Load(ms.ToArray()));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return null;
        }

        private Type HandleParsedAssembly(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            Type type = types.SingleOrDefault(x => typeof(MonoBehaviour).IsAssignableFrom(x));
            // Debug.Log($"{type?.Name??"null"} {string.Join(", ", types.Select(x => x.Name))}");
            return type;
        }
    }
}