namespace BugFoundry.BugFoundryEditor.TextEditors.OpenEditor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Range = Roslyn.Highlights.Range;

    public class EditorTagging
    {
        private readonly EditorTextHelper textHelper = new ();

        private string RemoveExistingTag(string text, int beginOfWord)
        {
            if (beginOfWord > 0 && char.IsLetterOrDigit(text[beginOfWord - 1]))
            {
                Debug.Log("Invalid word!");
                return null;
            }

            int endOfWord = this.textHelper.GetEndOfWord(text, beginOfWord);

            text = CheckRight(text, beginOfWord, endOfWord);

            text = CheckLeft(text, beginOfWord, endOfWord);

            return text;
        }

        private static string CheckLeft(string text, int beginOfWord, int endOfWord)
        {
            if (text[beginOfWord - 1] == '>')
            {
                int beginOfTag = -1;
                for (int i = beginOfWord - 1 - 1; i >= 0; i--)
                    if (text[i] == '<')
                    {
                        beginOfTag = i;
                        break;
                    }

                text = text.Remove(beginOfTag, beginOfWord - beginOfTag);
            }
            else
                Debug.Log($"{text.Substring(beginOfWord, endOfWord - beginOfWord)} after tag else");

            return text;
        }

        private static string CheckRight(string text, int beginOfWord, int endOfWord)
        {
            if (text[endOfWord + 1] == '<')
            {
                int endOfTag = -1;

                for (int i = endOfWord + 1; i < text.Length; i++)
                    if (text[i] == '>')
                    {
                        endOfTag = i;
                        break;
                    }

                text = text.Remove(endOfWord + 1, endOfTag - (endOfWord));
            }
            else
                Debug.Log($"{text.Substring(beginOfWord, endOfWord - beginOfWord)} before tag else");

            return text;
        }

        public string ColorWord(string taggedText, int cleanInd, Color color)
        {
            int ind = this.textHelper.CleanToMarkedIndex(taggedText, cleanInd);
            taggedText = this.RemoveExistingTag(taggedText, ind);
            int beginWord = this.textHelper.CleanToMarkedIndex(taggedText, cleanInd);
            int endWord = this.textHelper.GetEndOfWord(taggedText, beginWord);
            taggedText = this.ColorSection(taggedText, beginWord, endWord + 1, color); // TODO: +1 ??
            return taggedText;
        }

        private string ColorSection(string text, Range range)
        {
            try
            {
                int start = range.TextSpan.Start;
                int end = range.TextSpan.End;
                Color color = range.Color;
                text = this.ColorSection(text, start, end, color);
                return text;
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                return text;
            }
        }

        public string ColorSections(string text, List<Range> ranges)
        {
            text = this.textHelper.TrimEndEditor(text);
            text = this.textHelper.FormatInputBraces(text);

            foreach (Range range in ranges.OrderByDescending(x => x.TextSpan.Start).Where(x => x.Color != default))
                text = this.ColorSection(text, range);

            return text;
        }

        private string ColorSection(string text, int start, int end, Color color)
        {
            try
            {
                if (end == start)
                {
                    Debug.Log($"here333: |{text[end]}|");
                }

                text = text.Insert(end, "</color>");
                text = text.Insert(start, $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>");
                return text;
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                return text;
            }
        }
    }
}