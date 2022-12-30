namespace Projects.Buggary.BuggaryEditor.Roslyn
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Host.Mef;
    using Microsoft.CodeAnalysis.Text;
    using UnityEngine;

    public class DocumentRoslynModule
    {
        private Document document;

        public static MetadataReference[] AssemblyReferences { get; } =
        {
            MetadataReference.CreateFromFile(typeof(Type).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(MonoBehaviour).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Input).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(KeyCode).Assembly.Location),
        };

        public DocumentRoslynModule()
        {
            MefHostServices host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
            AdhocWorkspace workspace = new(host);
            ProjectInfo projectInfo = ProjectInfo
                .Create(ProjectId.CreateNewId(), VersionStamp.Create(), "MyProject", "MyProject", LanguageNames.CSharp)
                .WithMetadataReferences(AssemblyReferences);
            Project project = workspace.AddProject(projectInfo);
            this.document = workspace.AddDocument(project.Id, $"MyFile.cs", SourceText.From(""));
        }

        public void UpdateText(string text) => this.document = this.document.WithText(SourceText.From(text));

        public Document GetDocument() => this.document;
    }
}