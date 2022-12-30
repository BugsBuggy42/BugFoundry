namespace Projects.Buggary.BuggaryEditor.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Classes;
    using Contracts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using SchwiftyUI.V3;
    using SchwiftyUI.V3.Containers;
    using SchwiftyUI.V3.Elements;
    using TMPro;
    using UnityEngine;

    public class ErrorBuggaryModule
    {
        private readonly SchwiftyRoot schRoot;
        private readonly List<ErrorUnderline> errorUnderlines = new();
        private readonly ITextEditor editor;
        private List<Diagnostic> diagnostics = new();
        private readonly InfoPanelFloating infoPanel;
        private readonly Color panelColor;
        private readonly SchwiftyPanel editorPanel;

        public ErrorBuggaryModule(RectTransform parent, SchwiftyPanel editorPanelIn, ITextEditor editor, Color panelColorIn, TMP_FontAsset asset)
        {
            this.editorPanel = editorPanelIn;
            this.panelColor = panelColorIn;
            this.editor = editor;
            this.schRoot = new SchwiftyRoot(parent);
            this.infoPanel = new InfoPanelFloating(new SchwiftyRoot(parent), this.editorPanel.RectTransform, asset);
            this.infoPanel.Hide();
        }

        public void Update()
        {
            Vector3 mp = Input.mousePosition;
            List<ErrorUnderline> localMatches = new();

            foreach (ErrorUnderline underline in this.errorUnderlines)
            {
                if (underline.Box.Inside(mp))
                {
                    localMatches.Add(underline);
                }
            }

            this.DisplayDiagnosticData(localMatches);
        }

        private ErrorUnderline GetRedUnderline(Vector2 start, Vector2 end, Diagnostic diagnostic)
        {
            float length = Math.Abs(end.x - start.x);

            SchwiftyPanel underline = new SchwiftyPanel(this.schRoot, $"undeline {diagnostic.Descriptor.Description.ToString().Take(10)}")
                .SetBackgroundColor(Color.red)
                .SetDimensionsWithCurrentAnchors(length, 5)
                .SetTopLeft20(start)
                .ToPanel6900();

            return new ErrorUnderline(underline, new Box(start, end, this.editor.GetLineHeight()), diagnostic);
        }

        public void SetErrorRedUnderlines(List<Diagnostic> diagnosticsIn)
        {
            this.diagnostics = diagnosticsIn;

            foreach (SchwiftyPanel panel in this.errorUnderlines.Select(x => x.Underline))
                panel.Destroy();

            this.errorUnderlines.Clear();

            Vector2 sd = this.editorPanel.RectTransform.GetSizeAnchorAgnostic();

            foreach (Diagnostic diagnostic in diagnosticsIn)
            {
                TextSpan span = diagnostic.Location.SourceSpan;
                this.errorUnderlines.Add(this.GetRedUnderline(
                    this.editor.GetCharacterPosition(span.Start, true, false) +
                    new Vector2(sd.x / 2, -this.editor.GetScrollYDelta()),
                    this.editor.GetCharacterPosition(span.End, false, false) +
                    new Vector2(sd.x / 2, -this.editor.GetScrollYDelta()), diagnostic));
            }
        }

        public bool DisplayDiagnosticData(int index)
        {
            List<Diagnostic> diagnosticsLocal = this.diagnostics.Where(x =>
                    x.Location.SourceSpan.Start <= index &&
                    x.Location.SourceSpan.End >= index)
                .ToList();

            if (diagnosticsLocal.Count > 0)
            {
                this.infoPanel.DisplayText(string.Join("\n", diagnosticsLocal.Select(x => x.GetMessage())),
                    this.editor.GetCaretPosition(true, false) + (Vector2.down * this.editor.GetScrollYDelta()), 10,
                    this.panelColor);
                return true;
            }

            this.infoPanel.Hide();

            return false;
        }

        private void  DisplayDiagnosticData(List<ErrorUnderline> underlines)
        {
            if (underlines.Count > 0)
            {
                this.infoPanel.DisplayTextMouseOver(string.Join("\n", underlines.Select(x => x.Diagnostic.GetMessage())),
                    Input.mousePosition, 10, this.editor.GetLineHeight());
                return;
            }

            this.infoPanel.Hide();
        }

        public bool RepositionDiagnosticData(int index) => this.DisplayDiagnosticData(index); // this could be optimized
    }
}