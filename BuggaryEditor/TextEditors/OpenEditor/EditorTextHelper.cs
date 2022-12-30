namespace Projects.Buggary.BuggaryEditor.TextEditors.OpenEditor
{
    using System.Text.RegularExpressions;
    using UnityEngine;

    public class EditorTextHelper
    {
        public int GetEndOfWord(string text, int beginWord)
        {
            for (int i = beginWord; i < text.Length; i++)
            {
                if (char.IsLetterOrDigit(text[i]))
                    continue;

                return i - 1;
            }

            return -1;
        }

        public string TrimEndEditor(string text)
        {
            text = text.TrimEnd(' ', '\n', '\r', (char)8203);
            text += " \n ";
            return text;
        }

        public bool HasInvisibleSymbol(string text)
        {
            foreach (char chr in text)
                if (8203 == chr)
                    return true;

            return false;
        }

        public int CleanToMarkedIndex(string markedUpText, int cleanIndex)
        {
            bool inATag = false;
            int counter = 0;
            int result = -1;

            for (int markIndex = 0; markIndex < markedUpText.Length; markIndex++)
            {
                if (markedUpText[markIndex] == '<')
                    inATag = true;

                if (markedUpText[markIndex] == '>')
                {
                    counter--;
                    inATag = false;
                }

                if (inATag == false)
                    counter++;

                if (counter == cleanIndex)
                {
                    result = markIndex;
                    break;
                }
            }

            return result;
        }

        // ReSharper disable once CognitiveComplexity
        public int MarkedToCleanIndex(string markedUpText, int markedIndexTarget)
        {
            if (this.InsideATag(markedUpText, markedIndexTarget))
            {
                Debug.Log($"InATag {markedUpText[markedIndexTarget]}");
            }

            bool inATag = false;
            int counter = 0;
            int result = -1;

            for (int markIndex = 0; markIndex < markedUpText.Length; markIndex++)
            {
                if (markedUpText[markIndex] == '<')
                    inATag = true;

                if (markedUpText[markIndex] == '>')
                {
                    counter--;
                    inATag = false;
                }

                if (inATag == false)
                    counter++;

                if (markIndex == markedIndexTarget)
                {
                    result = counter - 1;
                    break;
                }
            }

            return result;
        }

        public string FormatInputBraces(string text)
        {
            text = text
                .Replace("<", "❮")
                .Replace(">", "❯");

            return text;
        }

        private int FindClosingTag(string text, int ind)
        {
            for (int i = ind; i < text.Length; i++)
            {
                if (text[i] == '>')
                    return i;

                if (text[i] == '<' && i != ind)
                    return -1;
            }

            return -1;
        }

        private int FindOpeningTag(string text, int ind)
        {
            for (int i = ind; i >= 0; i--)
            {
                if (text[i] == '<')
                    return i;

                if (text[i] == '>' && i != ind)
                    return -1;
            }

            return -1;
        }

        public bool InsideATag(string text, int ind) =>
            this.FindOpeningTag(text, ind) != -1 && this.FindClosingTag(text, ind) != -1;

        public string CleanOutput(string text)
        {
            const string pattern = "<.+?>";
            text = Regex.Replace(text, pattern, "");

            text = text
                .Replace("❮", "<")
                .Replace("❯", ">");

            return text;
        }
    }
}