namespace Buggary.BuggaryEditor.TextEditors.OpenEditor
{
    using System;
    using Models;
    using TMPro;
    using UnityEngine;

    [Serializable]
    [CreateAssetMenu(fileName = "AngleBracketsEscapeValidator",
        menuName = "TextMeshPro/Input Validators/AngleBracketsEscape", order = 100)]
    public class AngleBracketsEscapeValidator : TMP_InputValidator
    {
        private OpenEditor editor;

        public void Setup(OpenEditor editorIn)
        {
            this.editor = editorIn;
        }

        public override char Validate(ref string text, ref int pos, char ch)
        {
            if (Input.GetKey(KeyCode.LeftControl))
                return ch;

            if (this.editor.GetEnabledStatus == false && ch == '\n')
                return ch;

            pos += 1;

            if (ch == '<')
                ch = '❮';

            if (ch == '>')
                ch = '❯';

            string textBefore = text;
            text = text.Insert(pos - 1, ch.ToString());

            if (this.editor is not null)
            {
                this.editor.SetValidatorOutput(new ValidatorOutput()
                {
                    Character = ch,
                    Index = pos - 1,
                    TextAfter = text,
                    TextBefore = textBefore,
                });
            }

            return ch;
        }
    }
}