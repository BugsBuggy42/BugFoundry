namespace Buggary.BuggaryEditor.TextEditors.OpenEditor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Management;
    using Models;
    using SchwiftyUI.V3;
    using SchwiftyUI.V3.Elements;
    using SchwiftyUI.V3.Forks;
    using SchwiftyUI.V3.Inputs;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using Range = Roslyn.Highlights.Range;

    public class OpenEditor : MonoBehaviour, ITextEditor
    {
        [SerializeField] private Camera cam;
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private AngleBracketsEscapeValidator validator;
        [SerializeField] private Color backgroundColor;
        [SerializeField] private Color caretColor;
        [SerializeField] private int caretWith;
        [SerializeField] private float scrollSensitivity;
        [SerializeField] private BuggaryColors colors;

        private SchwiftyInput input;
        private Transform caret;
        private readonly EditorTextHelper textHelper = new();
        private readonly EditorTagging tagging = new();
        private ValidatorOutput vOutput;
        private bool newCharacterThisFrame = false;
        private bool initialized = false;
        private float baseY;
        private float currentY;
        private Action<float> scrollAction;
        private SchwiftyPanel editorPanel;
        private float xPosition = -1;

        public void Initialize(
            SchwiftyElement parent,
            Action leftRightArrowAction,
            Action<float> scrollActionIn,
            SchwiftyPanel editorPanelIn)
        {
            this.editorPanel = editorPanelIn;
            this.vOutput = new ValidatorOutput()
            {
                Character = 'n',
                Index = -1,
                TextAfter = "",
                TextBefore = "",
            };

            this.scrollAction = scrollActionIn;
            this.validator.Setup(this);
            this.input = new SchwiftyInput(parent, "Open Editor", this.cam)
                .SetFontAsset(this.font)
                .SetActions(leftRightArrowAction, _ =>
                {
                    this.currentY = this.input.InputField.textComponent.transform.position.y;
                    scrollActionIn.Invoke(-1);
                })
                .SetCaretColor(this.caretColor)
                .SetCaretWidth(this.caretWith)
                .SetTextWrapping(false)
                .SetVerticalAlignment(VerticalAlignmentOptions.Top)
                .SetHorizontalAlignment(HorizontalAlignmentOptions.Left)
                .MakeMultiline()
                .SetLineLimit(500)
                .SetBackgroundColor(this.backgroundColor)
                .UsePositioner15(new Positioner().SameAsParent())
                .SetGoNameAndEleName("Simple Editor Input")
                .ToInput6900();

            this.input.InputField.isRichTextEditingAllowed = true;
            this.input.InputField.richText = true;
            this.input.InputField.isRichTextEditingAllowed = false;
            this.input.InputField.characterValidation = TMP_InputField_Mine.CharacterValidation.CustomValidator;
            this.input.InputField.inputValidator = this.validator;
            this.input.InputField.scrollSensitivity = this.scrollSensitivity;
            this.baseY = this.input.textAreaRT.position.y;
            this.currentY = this.baseY;
            SchwiftyScrollbar sb =
                new SchwiftyScrollbar(this.input, "OpenEditorScrollbar", direction: Scrollbar.Direction.TopToBottom)
                    .SetBodyColor(this.colors.scrollbarBody)
                    .SetHandleColor(this.colors.scrollbarHandle)
                    .SetRightBarConstSize(0.3f)
                    .ToScrollBar6900();

            this.input.InputField.verticalScrollbar = sb.ScrollBar;
            this.initialized = true;
        }

        public void Update()
        {
            if (this.initialized == false)
            {
                this.enabled = false;
                return;
            }

            float newPosition = this.editorPanel.RectTransform.GetTopLeft().x;

            if (Math.Abs(this.xPosition - newPosition) > 0.001f)
            {
                this.xPosition = newPosition;
                this.scrollAction?.Invoke(-1);
            }

            if (Math.Abs(this.res2.x - Screen.height) < 0.002f && Math.Abs(this.res2.y - Screen.width) < 0.002f)
                return;
            this.res2 = new Vector2(Screen.height, Screen.width);
            this.baseY = this.input.textAreaRT.position.y;
            this.currentY = this.input.InputField.textComponent.transform.position.y;
            this.scrollAction?.Invoke(-1);
        }

        public string GetText(bool clean)
        {
            string text = this.input.Text.text;
            text = this.textHelper.TrimEndEditor(text);
            if (clean) text = this.textHelper.CleanOutput(text);
            text = this.textHelper.TrimEndEditor(text);
            return text;
        }

        public void SetText(string text)
        {
            text = this.textHelper.TrimEndEditor(text);
            this.input.InputField.text = text;
            this.input.Text.ForceMeshUpdate(true);
        }

        /// <summary>
        /// This is in local space
        /// </summary>
        public Vector2 GetCaretPosition(bool left, bool top)
        {
            Vector2 sd = this.input.RectTransform.GetSizeAnchorAgnostic();
            Vector2 tl = this.input.RectTransform.GetTopLeft();
            TMP_CharacterInfo info = this.input.Text.textInfo.characterInfo[this.input.InputField.caretPosition];
            Vector3 result = info.topLeft;
            // Debug.Log($"result: {result.x}, sd: {sd.x}, tl: {tl.x}");
            result.x += sd.x /
                        2; // this brings the char position to the the right value if the input's x started at 0 in screen space
            result.y += sd.y / 2; // the same for Y
            // result.x += tl.x;
            // result.y -= tl.y; // TODO: for now the input screen is always full height, fix this to fit on any RT
            return result;
        }

        public Vector2 GetCharacterPosition(int index, bool left, bool top)
        {
            try
            {
                index--;
                TMP_CharacterInfo info = this.input.Text.textInfo.characterInfo[index];
                Vector2 position;

                // Debug.Log($"### {index} |{info.character}| |{(int)info.character}| {info.bottomLeft.x} {info.bottomRight.x}");

                if (left)
                {
                    if (top)
                        position = info.topLeft;
                    else // bottom
                        position = info.bottomLeft;
                }
                else // right
                {
                    if (top)
                        position = info.topRight;
                    else // bottom
                        position = info.bottomRight;
                }

                Vector2 tl = this.editorPanel.RectTransform.GetTopLeft();
                Vector2 sd = this.editorPanel.RectTransform.GetSizeAnchorAgnostic();

                position.y += sd.y / 2; // TODO: this might break if the input is not full height
                position.x += tl.x;

                Debug.Log($"{tl.x} {tl.y}");

                return position;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return new Vector2(0, 0);
            }
        }

        public int GetCaretIndex() => this.input.InputField.caretPosition;

        public void SetCaretIndex(int index) => this.input.InputField.caretPosition = index;

        public void InsertText(int index, string insert)
        {
            string current = this.GetText(false);
            int markIndex = this.textHelper.CleanToMarkedIndex(current, index);
            this.SetText(current.Insert(markIndex, insert));
            this.input.InputField.caretPosition = index + insert.Length - 1;
        }

        public void DeleteText(int index, int count)
        {
            string current = this.GetText(false);
            int markIndex = this.textHelper.CleanToMarkedIndex(current, index);
            this.SetText(current.Remove(markIndex, count));
        }

        public bool ColorSections(string text, List<Range> ranges)
        {
            if (text == this.GetText(true))
            {
                // Debug.Log("coloring");
                text = this.tagging.ColorSections(text, ranges);
                int caretPosition = this.GetCaretIndex();
                this.SetText(text);
                this.SetCaretIndex(caretPosition);
                return true;
            }

            return false;
        }

        public void Disable()
        {
            // Debug.Log($"Editor Disabled {Time.frameCount}");
            this.input.InputField.CompletionServiceOn = true;
        }

        public void Enable()
        {
            // Debug.Log($"Editor Enabled {Time.frameCount}");
            this.input.InputField.CompletionServiceOn = false;
        }

        public bool GetEnabledStatus => !this.input.InputField.CompletionServiceOn;

        public void SetValidatorOutput(ValidatorOutput v)
        {
            this.newCharacterThisFrame = true;
            this.vOutput = v;
        }

        /// <summary>
        /// Should be called every frame once!
        /// </summary>
        public bool GetCharacterChangedThisFrame()
        {
            bool value = this.newCharacterThisFrame;
            this.newCharacterThisFrame = false;
            return value;
        }

        public float GetScrollYDelta() => this.baseY - this.currentY;

        public float GetLineHeight()
        {
            TMP_LineInfo[] lines = this.input.Text.textInfo.lineInfo;

            if (lines.Length == 0)
                Debug.Log($"Open GetLineHeight no lines");
            else
                return this.input.Text.textInfo.lineInfo.FirstOrDefault().lineHeight;

            return 50;
        }

        public char GetLastChar() => this.vOutput.Character;

        public int GetLastCharInd() => this.textHelper.MarkedToCleanIndex(this.vOutput.TextAfter, this.vOutput.Index);

        private Vector2 res2;
    }
}