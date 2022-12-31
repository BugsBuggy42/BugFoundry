namespace Buggary.BuggaryEditor.Management
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BuggaryEditor.Models;
    using Commons.Legacy;
    using Contracts;
    using Infrastructure;
    using Models;
    using Roslyn;
    using SchwiftyUI.V3.Elements;
    using UI;
    using UnityEngine;

    public class SuggestionBuggaryModule
    {
        private readonly BoxedBuggaryState state;
        private readonly SuggestionRoslynModule roslynModule = new();
        private readonly DocumentRoslynModule documentRoslynModule;
        private string lastSearchTerm;
        private readonly ITextEditor editor;
        private int searchTermStart;
        private CancellationTokenSource source;
        private readonly bool useOpenEditor;
        private readonly List<char> triggers;
        private const string SearchTermDefault = "unicorn69420";
        private bool suggestionActive = false;
        private readonly ListSelectionFloatingMenu menuSuggest = new();
        private int lastCaretIndex;
        private string lastText;

        public SuggestionBuggaryModule(
            BoxedBuggaryState stateIn,
            ITextEditor editorIn,
            bool openEditor,
            DocumentRoslynModule documentRoslynModuleIn,
            RectTransform defaultEditorRoot,
            SchwiftyPanel editorPanel,
            BuggaryColors colors)
        {
            this.state = stateIn;
            this.editor = editorIn;
            this.useOpenEditor = openEditor;
            this.lastSearchTerm = SearchTermDefault;
            this.documentRoslynModule = documentRoslynModuleIn;
            this.triggers = new List<char>() { '.', ' ', '\t' };

            _ = this.menuSuggest.Create(defaultEditorRoot, editorPanel,
                colors, this.HandleEnter, this.HandleEnter);
            this.menuSuggest.SetEnabled(false);
        }

        public void Update(bool indexChanged, bool newCharacterInserted, string text, int caretIndex,
            Vector2 caretPosition)
        {
            this.lastCaretIndex = caretIndex;
            this.lastText = text;

            this.menuSuggest.Update(caretPosition);
            if (indexChanged) this.ConditionalTriggerCompletion(text, caretIndex, newCharacterInserted);
            // ProcessUserInput needs to be before UpdateSearch
            this.ProcessUserInput(text, caretIndex);
            this.UpdateSearch(text, caretIndex);
        }

        /// <summary>
        /// Triggers completion conditionally
        /// </summary>
        /// <returns>The new dot index if there is one or -1 otherwise</returns>
        private void ConditionalTriggerCompletion(string textIn, int indexIn, bool newCharacterInserted)
        {
            char lastCharacter = this.editor.GetLastChar();

            if (newCharacterInserted && this.triggers.Contains(lastCharacter))
            {
                this.source?.Cancel();
                this.DisableDotInsert(false);
                this.DotCompletion(textIn, indexIn, lastCharacter, true);
                return;
            }

            if (this.state.Value == BuggaryState.Completion) return;

            this.DotCompletion(textIn, indexIn, lastCharacter, newCharacterInserted);

            this.AfterCompletion(textIn, indexIn, newCharacterInserted);
        }

        private void AfterCompletion(string textIn, int indexIn, bool newCharacterInserted)
        {
            if (this.AfterDotCompletionCheck(newCharacterInserted, textIn, indexIn))
            {
                // Debug.Log($"AfterCompletion");

                if (this.useOpenEditor)
                    this.searchTermStart = indexIn - 1;
                else
                    this.searchTermStart = indexIn;

                this.source?.Cancel();
                this.source = new CancellationTokenSource();
                this.TriggerCompletion(textIn, this.source.Token);
            }
        }

        private void ForcedCompletion(string textIn, int indexIn)
        {
            Debug.Log($"ForcedCompletion");

            if (this.useOpenEditor)
                this.searchTermStart = indexIn + 0;
            else
                this.searchTermStart = indexIn + 1;

            this.source?.Cancel();
            this.source = new CancellationTokenSource();
            this.TriggerCompletion(textIn, this.source.Token, 1);
        }

        private void DotCompletion(string textIn, int indexIn, char lastCharacter, bool newCharacterInserted)
        {
            if (lastCharacter == '.' && newCharacterInserted)
            {
                // Debug.Log($"DotCompletion");

                if (this.useOpenEditor)
                    this.searchTermStart = indexIn;
                else
                    this.searchTermStart = indexIn + 1;

                this.source?.Cancel();
                this.source = new CancellationTokenSource();
                this.TriggerCompletion(textIn, this.source.Token, -1);
            }
        }

        private void TriggerCompletion(string textIn, CancellationToken token, int offset = 0)
        {
            this.state.Value = BuggaryState.Completion;

            Task.Run(async () =>
            {
                // Debug.Log($"|{textIn[this.searchTermStart + offset]}| {this.searchTermStart + offset}");

                List<string> result =
                    await this.roslynModule.GetSuggestion(this.searchTermStart + offset,
                        this.documentRoslynModule.GetDocument(), token);

                if (token.IsCancellationRequested)
                    return;

                if (result.Count == 0)
                {
                    // Debug.Log($"NoSuggestions");
                    // this.DisableDotInsert(true);
                }

                Dispatcher.Instance.Invoke(() =>
                {
                    this.suggestionActive = true;
                    this.menuSuggest.SetEnabled(true);
                    this.menuSuggest.SetItems(result);
                    this.menuSuggest.ReorderItems(this.lastSearchTerm);
                    this.editor.Disable();
                });
            }, token);
        }

        private bool AfterDotCompletionCheck(bool newCharacterInserted, string textIn, int indexIn)
        {
            if (this.useOpenEditor)
            {
                try
                {
                    return newCharacterInserted && this.triggers.Contains(textIn[indexIn - 2]) &&
                           indexIn - 1 == this.editor.GetLastCharInd();
                }
                catch (Exception e)
                {
                    // This happens because the end of the text is being trimmed for ZWS characters including new lines
                    // so when the cursor is off in trailing space it will throw an exception
                    Debug.LogWarning(e);
                    return false;
                }
            }
            else
            {
                int ind = this.editor.GetLastCharInd();
                if (this.triggers.Contains(this.TryGetCharacter(textIn, ind - 1)))
                    return true;
                else
                    return false;
            }
        }

        private char TryGetCharacter(string text, int ind)
        {
            try
            {
                return text[ind];
            }
            catch
            {
                return '#';
            }
        }

        public void DisableDotInsert(bool nextFrame) =>
            Coroutiner.Instance.StartCoroutine(this.DisableDotInsertCo(nextFrame));

        private IEnumerator DisableDotInsertCo(bool nextFrame)
        {
            this.state.Value = BuggaryState.Edit;

            if (nextFrame)
                yield return null;

            this.source?.Cancel();
            this.menuSuggest.SetEnabled(false);
            this.editor.Enable();
            this.lastSearchTerm = SearchTermDefault;
            this.suggestionActive = false;
        }

        private void UpdateSearch(string text, int currentIndex)
        {
            if (this.state.Value != BuggaryState.Completion) return;

            int length = currentIndex - this.searchTermStart;

            if (this.useOpenEditor == false)
                length += 1;

            if (length < 0)
            {
                this.DisableDotInsert(false);
                // Debug.Log($"length < 0");
                return;
            }

            string searchTerm = new(text.Skip(this.searchTermStart).Take(length).ToArray());

            if (searchTerm != this.lastSearchTerm)
            {
                // if no matches are found, disable the menu
                if (this.suggestionActive && !this.menuSuggest.GetCurrentItems().Any(x =>
                        x.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase)))
                {
                    this.DisableDotInsert(true);
                    return;
                }

                this.lastSearchTerm = searchTerm;
                this.menuSuggest.ReorderItems(searchTerm);
            }
        }

        private void ProcessUserInput(string text, int index)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Space))
                this.ForcedCompletion(text, index);
            this.ProcessEscape();
        }

        private void ProcessEscape()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.menuSuggest.SetEnabled(false);
                this.editor.Disable(); // TODO: Probably a mistake should be enable
                this.state.Value = BuggaryState.Edit;
                this.lastSearchTerm = SearchTermDefault;
            }
        }

        private void HandleEnter(string content)
        {
            if (this.useOpenEditor)
            {
                string textLocal = this.lastText;
                int count = this.lastCaretIndex - this.searchTermStart;
                if (count > 0)
                    textLocal = textLocal.Remove(this.searchTermStart, count);

                textLocal = textLocal.Insert(this.searchTermStart, content);
                this.editor.SetText(textLocal);
                this.editor.SetCaretIndex(this.searchTermStart + content.Length);
            }
            else
            {
                this.editor.DeleteText(this.searchTermStart, this.lastCaretIndex - this.searchTermStart);
                this.editor.InsertText(this.searchTermStart, content);
            }

            this.DisableDotInsert(true);
        }
    }
}