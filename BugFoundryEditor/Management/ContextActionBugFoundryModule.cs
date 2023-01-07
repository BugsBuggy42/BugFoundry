namespace BugFoundry.BugFoundryEditor.Management
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Commons.Legacy;
    using Contracts;
    using Microsoft.CodeAnalysis;
    using Roslyn;
    using Roslyn.Refactors;
    using SchwiftyUI.BugFoundry.SchwiftyUI.V3.Elements;
    using UI;
    using UnityEngine;

    public class ContextActionBugFoundryModule
    {
        private AddMissingImportRoslynModule missingImportRoslynModule = new();
        private ListSelectionFloatingMenu menu = new();
        private DocumentRoslynModule document;
        private ITextEditor editor;
        private bool active = false;

        public ContextActionBugFoundryModule(
            RectTransform rootParent,
            SchwiftyPanel editorPanel,
            DocumentRoslynModule documentIn,
            ITextEditor editorIn,
            BugFoundryColors colors)
        {
            this.document = documentIn;
            this.editor = editorIn;
            this.menu.Create(rootParent, editorPanel, colors, this.ImportUsing, this.ImportUsing);
            this.menu.SetEnabled(false);
        }

        public void Update(int caretIndex, Vector2 targetPosition, bool caretChanged)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Period))
            {
                this.editor.Disable();
                List<string> candidateNamespaces = this.missingImportRoslynModule
                    .GetCandidateNamespaces(this.document.GetDocument(), caretIndex, CancellationToken.None).Result;
                this.menu.SetEnabled(true);
                this.menu.SetItems(candidateNamespaces);
                this.active = true;
            }

            if (this.active == false) return;

            this.menu.Update(targetPosition);

            if (caretChanged)
            {
                this.menu.SetEnabled(false);
                this.active = false;
                Coroutiner.Instance.StartCoroutine(this.EnableEditor(true));
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.menu.SetEnabled(false);
                this.active = false;
                Coroutiner.Instance.StartCoroutine(this.EnableEditor(true));
            }
        }

        IEnumerator EnableEditor(bool nextFrame)
        {
            if (nextFrame)
                yield return null;

            this.editor.Enable();
        }

        private void ImportUsing(string namespaceStr)
        {
            string textBefore = this.document.GetDocument().GetTextAsync().Result.ToString();
            TextPosition caretPosition = this.GetCaretPosition(textBefore, this.editor.GetCaretIndex());
            Document newDoc = this.missingImportRoslynModule
                .AddSingleUsing(this.document.GetDocument(), namespaceStr, CancellationToken.None).Result;
            string textAfter = newDoc.GetTextAsync().Result.ToString();

            this.editor.SetText(textAfter);
            // Debug.Log($"{textAfter}");
            Coroutiner.Instance.StartCoroutine(this.EnableEditor(true));
            this.editor.SetCaretIndex(this.GetNewCaretPosition(textAfter, caretPosition));
        }

        private TextPosition GetCaretPosition(string str, int caretPos)
        {
            string[] lines = str.Split(new[] { '\n' }, StringSplitOptions.None);
            int currentLineIndex = -1;
            int currentLineLength = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (caretPos >= lines[i].Length)
                {
                    caretPos -= lines[i].Length + 1;
                }
                else
                {
                    currentLineIndex = i;
                    break;
                }
            }

            return new TextPosition()
            {
                Line = currentLineIndex,
                Column = caretPos,
            };
        }

        private int GetNewCaretPosition(string text, TextPosition caretPosition)
        {
            string[] lines = text.Split(new[] { '\n' }, StringSplitOptions.None);

            int newColumn;

            if (lines[caretPosition.Line + 1].Length > caretPosition.Column)
                newColumn = lines[caretPosition.Line + 1].Length;
            else
                newColumn = caretPosition.Column;

            int prevLines = lines.Take(caretPosition.Line + 2).Sum(x => x.Length + 1);
            return prevLines + newColumn;
        }

        private class TextPosition
        {
            public int Line { get; set; }

            public int Column { get; set; }
        }
    }
}