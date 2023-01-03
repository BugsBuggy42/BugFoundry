namespace BugFoundry.BugFoundryEditor.Contracts
{
    using System.Collections.Generic;
    using UnityEngine;
    using Range = Roslyn.Highlights.Range;

    public interface ITextEditor
    {
        public string GetText(bool clean);
        public void SetText(string text);
        public Vector2 GetCaretPosition(bool left, bool top);
        public int GetCaretIndex();
        public void SetCaretIndex(int position);
        public void InsertText(int index, string text);
        public void DeleteText(int index, int count);
        public void Disable();
        public void Enable();
        bool ColorSections(string text, List<Range> ranges);
        public char GetLastChar();
        public Vector2 GetCharacterPosition(int index, bool left, bool top);
        public int GetLastCharInd();
        /// <summary>
        /// Should be called every frame once!
        /// </summary>
        public bool GetCharacterChangedThisFrame();
        public float GetScrollYDelta();
        public float GetLineHeight();
    }
}