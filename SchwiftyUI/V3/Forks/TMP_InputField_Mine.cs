//#define TMP_DEBUG_MODE

namespace Buggary.SchwiftyUI.V3.Forks
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;
    using System.Threading;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// Editable text input field.
    /// </summary>
    [AddComponentMenu("UI/TextMeshPro - Input Field", 11)]
    public class TMP_InputField_Mine : Selectable,
        IUpdateSelectedHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler,
        ISubmitHandler,
        ICanvasElement,
        ILayoutElement,
        IScrollHandler
    {
        public bool CompletionServiceOn { get; set; } = false;
        private Action leftRightAction;
        private Action<float> scrollAction;

        public void SetActions(Action leftRightActionIn, Action<float> scrollActionIn)
        {
            this.leftRightAction = leftRightActionIn;
            this.scrollAction = scrollActionIn;
        }

        // Setting the content type acts as a shortcut for setting a combination of InputType, CharacterValidation, LineType, and TouchScreenKeyboardType
        public enum ContentType
        {
            Standard,
            Autocorrected,
            IntegerNumber,
            DecimalNumber,
            Alphanumeric,
            Name,
            EmailAddress,
            Password,
            Pin,
            Custom
        }

        public enum InputType
        {
            Standard,
            AutoCorrect,
            Password,
        }

        public enum CharacterValidation
        {
            None,
            Digit,
            Integer,
            Decimal,
            Alphanumeric,
            Name,
            Regex,
            EmailAddress,
            CustomValidator
        }

        public enum LineType
        {
            SingleLine,
            MultiLineSubmit,
            MultiLineNewline
        }

        public delegate char OnValidateInput(string text, int charIndex, char addedChar);

        [Serializable]
        public class SubmitEvent : UnityEvent<string>
        {
        }

        [Serializable]
        public class OnChangeEvent : UnityEvent<string>
        {
        }

        [Serializable]
        public class SelectionEvent : UnityEvent<string>
        {
        }

        [Serializable]
        public class TextSelectionEvent : UnityEvent<string, int, int>
        {
        }

        [Serializable]
        public class TouchScreenKeyboardEvent : UnityEvent<TouchScreenKeyboard.Status>
        {
        }

        protected TouchScreenKeyboard m_SoftKeyboard;
        static private readonly char[] kSeparators = { ' ', '.', ',', '\t', '\r', '\n' };

        #region Exposed properties

        /// <summary>
        /// Text Text used to display the input's value.
        /// </summary>
        protected RectTransform m_RectTransform;

        [SerializeField] protected RectTransform m_TextViewport;

        protected RectMask2D m_TextComponentRectMask;

        protected RectMask2D m_TextViewportRectMask;
        private Rect m_CachedViewportRect;

        [SerializeField] protected TMP_Text m_TextComponent;

        protected RectTransform m_TextComponentRectTransform;

        [SerializeField] protected Graphic m_Placeholder;

        [SerializeField] protected Scrollbar m_VerticalScrollbar;

        [SerializeField] protected TMP_ScrollbarEventHandler m_VerticalScrollbarEventHandler;
        //private bool m_ForceDeactivation;

        private bool m_IsDrivenByLayoutComponents = false;
        [SerializeField] private LayoutGroup m_LayoutGroup;

        private IScrollHandler m_IScrollHandlerParent;

        /// <summary>
        /// Used to keep track of scroll position
        /// </summary>
        private float m_ScrollPosition;

        /// <summary>
        ///
        /// </summary>
        [SerializeField] protected float m_ScrollSensitivity = 1.0f;

        //[SerializeField]
        //protected TMP_Text m_PlaceholderTextComponent;

        [SerializeField] private ContentType m_ContentType = ContentType.Standard;

        /// <summary>
        /// Type of data expected by the input field.
        /// </summary>
        [SerializeField] private InputType m_InputType = InputType.Standard;

        /// <summary>
        /// The character used to hide text in password field.
        /// </summary>
        [SerializeField] private char m_AsteriskChar = '*';

        /// <summary>
        /// Keyboard type applies to mobile keyboards that get shown.
        /// </summary>
        [SerializeField] private TouchScreenKeyboardType m_KeyboardType = TouchScreenKeyboardType.Default;

        [SerializeField] private LineType m_LineType = LineType.SingleLine;

        /// <summary>
        /// Should hide mobile input field part of the virtual keyboard.
        /// </summary>
        [SerializeField] private bool m_HideMobileInput = false;

        /// <summary>
        /// Should hide soft / virtual keyboard.
        /// </summary>
        [SerializeField] private bool m_HideSoftKeyboard = false;

        /// <summary>
        /// What kind of validation to use with the input field's data.
        /// </summary>
        [SerializeField] private CharacterValidation m_CharacterValidation = CharacterValidation.None;

        /// <summary>
        /// The Regex expression used for validating the text input.
        /// </summary>
        [SerializeField] private string m_RegexValue = string.Empty;

        /// <summary>
        /// The point sized used by the placeholder and input text object.
        /// </summary>
        [SerializeField] private float m_GlobalPointSize = 14;

        /// <summary>
        /// Maximum number of characters allowed before input no longer works.
        /// </summary>
        [SerializeField] private int m_CharacterLimit = 0;

        /// <summary>
        /// Event delegates triggered when the input field submits its data.
        /// </summary>
        [SerializeField] private SubmitEvent m_OnEndEdit = new SubmitEvent();

        /// <summary>
        /// Event delegates triggered when the input field submits its data.
        /// </summary>
        [SerializeField] private SubmitEvent m_OnSubmit = new SubmitEvent();

        /// <summary>
        /// Event delegates triggered when the input field is focused.
        /// </summary>
        [SerializeField] private SelectionEvent m_OnSelect = new SelectionEvent();

        /// <summary>
        /// Event delegates triggered when the input field focus is lost.
        /// </summary>
        [SerializeField] private SelectionEvent m_OnDeselect = new SelectionEvent();

        /// <summary>
        /// Event delegates triggered when the text is selected / highlighted.
        /// </summary>
        [SerializeField] private TextSelectionEvent m_OnTextSelection = new TextSelectionEvent();

        /// <summary>
        /// Event delegates triggered when text is no longer select / highlighted.
        /// </summary>
        [SerializeField] private TextSelectionEvent m_OnEndTextSelection = new TextSelectionEvent();

        /// <summary>
        /// Event delegates triggered when the input field changes its data.
        /// </summary>
        [SerializeField] private OnChangeEvent m_OnValueChanged = new OnChangeEvent();

        /// <summary>
        /// Event delegates triggered when the status of the TouchScreenKeyboard changes.
        /// </summary>
        [SerializeField]
        private TouchScreenKeyboardEvent m_OnTouchScreenKeyboardStatusChanged = new TouchScreenKeyboardEvent();

        /// <summary>
        /// Custom validation callback.
        /// </summary>
        [SerializeField] private OnValidateInput m_OnValidateInput;

        [SerializeField] private Color m_CaretColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        [SerializeField] private bool m_CustomCaretColor = false;

        [SerializeField] private Color m_SelectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);

        /// <summary>
        /// Input field's value.
        /// </summary>
        [SerializeField] [TextArea(5, 10)] protected string m_Text = string.Empty;

        [SerializeField] [Range(0f, 4f)] private float m_CaretBlinkRate = 0.85f;

        [SerializeField] [Range(1, 5)] private int m_CaretWidth = 1;

        [SerializeField] private bool m_ReadOnly = false;

        [SerializeField] private bool m_RichText = true;

        #endregion

        protected int m_StringPosition = 0;
        protected int m_StringSelectPosition = 0;
        protected int m_CaretPosition = 0;
        protected int m_CaretSelectPosition = 0;

        private RectTransform caretRectTrans = null;
        protected UIVertex[] m_CursorVerts = null;
        private CanvasRenderer m_CachedInputRenderer;
        private Vector2 m_LastPosition;

        [NonSerialized] protected Mesh m_Mesh;

        private bool m_AllowInput = false;

        //bool m_HasLostFocus = false;
        private bool m_ShouldActivateNextUpdate = false;
        private bool m_UpdateDrag = false;
        private bool m_DragPositionOutOfBounds = false;
        private const float kHScrollSpeed = 0.05f;
        private const float kVScrollSpeed = 0.10f;
        protected bool m_CaretVisible;
        private Coroutine m_BlinkCoroutine = null;
        private float m_BlinkStartTime = 0.0f;
        private Coroutine m_DragCoroutine = null;
        private string m_OriginalText = "";
        private bool m_WasCanceled = false;
        private bool m_HasDoneFocusTransition = false;
        private WaitForSecondsRealtime m_WaitForSecondsRealtime;
        private bool m_PreventCallback = false;

        private bool m_TouchKeyboardAllowsInPlaceEditing = false;

        private bool m_IsTextComponentUpdateRequired = false;

        private bool m_isLastKeyBackspace = false;
        private float m_PointerDownClickStartTime;
        private float m_KeyDownStartTime;
        private float m_DoubleClickDelay = 0.5f;

        // Doesn't include dot and @ on purpose! See usage for details.
        const string kEmailSpecialCharacters = "!#$%&'*+-/=?^_`{|}~";

        private BaseInput inputSystem
        {
            get
            {
                if (EventSystem.current && EventSystem.current.currentInputModule)
                    return EventSystem.current.currentInputModule.input;
                return null;
            }
        }

        private string compositionString
        {
            get { return this.inputSystem != null ? this.inputSystem.compositionString : Input.compositionString; }
        }

        private bool m_IsCompositionActive = false;
        private bool m_ShouldUpdateIMEWindowPosition = false;
        private int m_PreviousIMEInsertionLine = 0;

        private int compositionLength
        {
            get
            {
                if (this.m_ReadOnly)
                    return 0;

                return this.compositionString.Length;
            }
        }


        protected TMP_InputField_Mine()
        {
            this.SetTextComponentWrapMode();
        }

        protected Mesh mesh
        {
            get
            {
                if (this.m_Mesh == null)
                    this.m_Mesh = new Mesh();
                return this.m_Mesh;
            }
        }

        /// <summary>
        /// Should the mobile keyboard input be hidden.
        /// </summary>
        public bool shouldHideMobileInput
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.tvOS:
                        return this.m_HideMobileInput;
                    default:
                        return true;
                }
            }

            set
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.tvOS:
                        SetPropertyUtility.SetStruct(ref this.m_HideMobileInput, value);
                        break;
                    default:
                        this.m_HideMobileInput = true;
                        break;
                }
            }
        }

        public bool shouldHideSoftKeyboard
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.tvOS:
                    case RuntimePlatform.WSAPlayerX86:
                    case RuntimePlatform.WSAPlayerX64:
                    case RuntimePlatform.WSAPlayerARM:
                    case RuntimePlatform.Stadia:
#if UNITY_2020_2_OR_NEWER
                    case RuntimePlatform.PS4:
#if !(UNITY_2020_2_1 || UNITY_2020_2_2)
                    case RuntimePlatform.PS5:
#endif
#endif
                    case RuntimePlatform.Switch:
                        return this.m_HideSoftKeyboard;
                    default:
                        return true;
                }
            }

            set
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.tvOS:
                    case RuntimePlatform.WSAPlayerX86:
                    case RuntimePlatform.WSAPlayerX64:
                    case RuntimePlatform.WSAPlayerARM:
                    case RuntimePlatform.Stadia:
#if UNITY_2020_2_OR_NEWER
                    case RuntimePlatform.PS4:
#if !(UNITY_2020_2_1 || UNITY_2020_2_2)
                    case RuntimePlatform.PS5:
#endif
#endif
                    case RuntimePlatform.Switch:
                        SetPropertyUtility.SetStruct(ref this.m_HideSoftKeyboard, value);
                        break;
                    default:
                        this.m_HideSoftKeyboard = true;
                        break;
                }

                if (this.m_HideSoftKeyboard == true && this.m_SoftKeyboard != null && TouchScreenKeyboard.isSupported &&
                    this.m_SoftKeyboard.active)
                {
                    this.m_SoftKeyboard.active = false;
                    this.m_SoftKeyboard = null;
                }
            }
        }

        private bool isKeyboardUsingEvents()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.tvOS:
#if UNITY_2020_2_OR_NEWER
                case RuntimePlatform.PS4:
#if !(UNITY_2020_2_1 || UNITY_2020_2_2)
                case RuntimePlatform.PS5:
#endif
#endif
                case RuntimePlatform.Switch:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Input field's current text value. This is not necessarily the same as what is visible on screen.
        /// </summary>
        /// <remarks>
        /// Note that null is invalid value  for InputField.text.
        /// </remarks>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public InputField mainInputField;
        ///
        ///     public void Start()
        ///     {
        ///         mainInputField.text = "Enter Text Here...";
        ///     }
        /// }
        /// </code>
        /// </example>
        public string text
        {
            get { return this.m_Text; }
            set { this.SetText(value); }
        }

        /// <summary>
        /// Set Input field's current text value without invoke onValueChanged. This is not necessarily the same as what is visible on screen.
        /// </summary>
        public void SetTextWithoutNotify(string input)
        {
            this.SetText(input, false);
        }

        void SetText(string value, bool sendCallback = true)
        {
            if (this.text == value)
                return;

            if (value == null)
                value = "";

            value = value.Replace("\0", string.Empty); // remove embedded nulls

            this.m_Text = value;

            /*
            if (m_LineType == LineType.SingleLine)
                value = value.Replace("\n", "").Replace("\t", "");

            // If we have an input validator, validate the input and apply the character limit at the same time.
            if (onValidateInput != null || characterValidation != CharacterValidation.None)
            {
                m_Text = "";
                OnValidateInput validatorMethod = onValidateInput ?? Validate;
                m_CaretPosition = m_CaretSelectPosition = value.Length;
                int charactersToCheck = characterLimit > 0 ? Math.Min(characterLimit, value.Length) : value.Length;
                for (int i = 0; i < charactersToCheck; ++i)
                {
                    char c = validatorMethod(m_Text, m_Text.Length, value[i]);
                    if (c != 0)
                        m_Text += c;
                }
            }
            else
            {
                m_Text = characterLimit > 0 && value.Length > characterLimit ? value.Substring(0, characterLimit) : value;
            }
            */

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                this.SendOnValueChangedAndUpdateLabel();
                return;
            }
#endif

            if (this.m_SoftKeyboard != null)
                this.m_SoftKeyboard.text = this.m_Text;

            if (this.m_StringPosition > this.m_Text.Length)
                this.m_StringPosition = this.m_StringSelectPosition = this.m_Text.Length;
            else if (this.m_StringSelectPosition > this.m_Text.Length)
                this.m_StringSelectPosition = this.m_Text.Length;

            this.m_forceRectTransformAdjustment = true;

            this.m_IsTextComponentUpdateRequired = true;
            this.UpdateLabel();

            if (sendCallback)
                this.SendOnValueChanged();
        }


        public bool isFocused
        {
            get { return this.m_AllowInput; }
        }

        public float caretBlinkRate
        {
            get { return this.m_CaretBlinkRate; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref this.m_CaretBlinkRate, value))
                {
                    if (this.m_AllowInput)
                        this.SetCaretActive();
                }
            }
        }

        public int caretWidth
        {
            get { return this.m_CaretWidth; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref this.m_CaretWidth, value)) this.MarkGeometryAsDirty();
            }
        }

        public RectTransform textViewport
        {
            get { return this.m_TextViewport; }
            set { SetPropertyUtility.SetClass(ref this.m_TextViewport, value); }
        }

        public TMP_Text textComponent
        {
            get { return this.m_TextComponent; }
            set
            {
                if (SetPropertyUtility.SetClass(ref this.m_TextComponent, value))
                {
                    this.SetTextComponentWrapMode();
                }
            }
        }

        //public TMP_Text placeholderTextComponent { get { return m_PlaceholderTextComponent; } set { SetPropertyUtility.SetClass(ref m_PlaceholderTextComponent, value); } }

        public Graphic placeholder
        {
            get { return this.m_Placeholder; }
            set { SetPropertyUtility.SetClass(ref this.m_Placeholder, value); }
        }

        public Scrollbar verticalScrollbar
        {
            get { return this.m_VerticalScrollbar; }
            set
            {
                if (this.m_VerticalScrollbar != null)
                    this.m_VerticalScrollbar.onValueChanged.RemoveListener(this.OnScrollbarValueChange);

                SetPropertyUtility.SetClass(ref this.m_VerticalScrollbar, value);

                if (this.m_VerticalScrollbar)
                {
                    this.m_VerticalScrollbar.onValueChanged.AddListener(this.OnScrollbarValueChange);
                }
            }
        }

        public float scrollSensitivity
        {
            get { return this.m_ScrollSensitivity; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref this.m_ScrollSensitivity, value)) this.MarkGeometryAsDirty();
            }
        }

        public Color caretColor
        {
            get { return this.customCaretColor ? this.m_CaretColor : this.textComponent.color; }
            set
            {
                if (SetPropertyUtility.SetColor(ref this.m_CaretColor, value)) this.MarkGeometryAsDirty();
            }
        }

        public bool customCaretColor
        {
            get { return this.m_CustomCaretColor; }
            set
            {
                if (this.m_CustomCaretColor != value)
                {
                    this.m_CustomCaretColor = value;
                    this.MarkGeometryAsDirty();
                }
            }
        }

        public Color selectionColor
        {
            get { return this.m_SelectionColor; }
            set
            {
                if (SetPropertyUtility.SetColor(ref this.m_SelectionColor, value)) this.MarkGeometryAsDirty();
            }
        }

        public SubmitEvent onEndEdit
        {
            get { return this.m_OnEndEdit; }
            set { SetPropertyUtility.SetClass(ref this.m_OnEndEdit, value); }
        }

        public SubmitEvent onSubmit
        {
            get { return this.m_OnSubmit; }
            set { SetPropertyUtility.SetClass(ref this.m_OnSubmit, value); }
        }

        public SelectionEvent onSelect
        {
            get { return this.m_OnSelect; }
            set { SetPropertyUtility.SetClass(ref this.m_OnSelect, value); }
        }

        public SelectionEvent onDeselect
        {
            get { return this.m_OnDeselect; }
            set { SetPropertyUtility.SetClass(ref this.m_OnDeselect, value); }
        }

        public TextSelectionEvent onTextSelection
        {
            get { return this.m_OnTextSelection; }
            set { SetPropertyUtility.SetClass(ref this.m_OnTextSelection, value); }
        }

        public TextSelectionEvent onEndTextSelection
        {
            get { return this.m_OnEndTextSelection; }
            set { SetPropertyUtility.SetClass(ref this.m_OnEndTextSelection, value); }
        }

        public OnChangeEvent onValueChanged
        {
            get { return this.m_OnValueChanged; }
            set { SetPropertyUtility.SetClass(ref this.m_OnValueChanged, value); }
        }

        public TouchScreenKeyboardEvent onTouchScreenKeyboardStatusChanged
        {
            get { return this.m_OnTouchScreenKeyboardStatusChanged; }
            set { SetPropertyUtility.SetClass(ref this.m_OnTouchScreenKeyboardStatusChanged, value); }
        }

        public OnValidateInput onValidateInput
        {
            get { return this.m_OnValidateInput; }
            set { SetPropertyUtility.SetClass(ref this.m_OnValidateInput, value); }
        }

        public int characterLimit
        {
            get { return this.m_CharacterLimit; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref this.m_CharacterLimit, Math.Max(0, value)))
                {
                    this.UpdateLabel();
                    if (this.m_SoftKeyboard != null)
                        this.m_SoftKeyboard.characterLimit = value;
                }
            }
        }

        //public bool isInteractableControl { set { if ( } }

        /// <summary>
        /// Set the point size on both Placeholder and Input text object.
        /// </summary>
        public float pointSize
        {
            get { return this.m_GlobalPointSize; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref this.m_GlobalPointSize, Math.Max(0, value)))
                {
                    this.SetGlobalPointSize(this.m_GlobalPointSize);
                    this.UpdateLabel();
                }
            }
        }

        /// <summary>
        /// Sets the Font Asset on both Placeholder and Input child objects.
        /// </summary>
        public TMP_FontAsset fontAsset
        {
            get { return this.m_GlobalFontAsset; }
            set
            {
                if (SetPropertyUtility.SetClass(ref this.m_GlobalFontAsset, value))
                {
                    this.SetGlobalFontAsset(this.m_GlobalFontAsset);
                    this.UpdateLabel();
                }
            }
        }

        [SerializeField] protected TMP_FontAsset m_GlobalFontAsset;

        /// <summary>
        /// Determines if the whole text will be selected when focused.
        /// </summary>
        public bool onFocusSelectAll
        {
            get { return this.m_OnFocusSelectAll; }
            set { this.m_OnFocusSelectAll = value; }
        }

        [SerializeField] protected bool m_OnFocusSelectAll = false;
        protected bool m_isSelectAll;

        /// <summary>
        /// Determines if the text and caret position as well as selection will be reset when the input field is deactivated.
        /// </summary>
        public bool resetOnDeActivation
        {
            get { return this.m_ResetOnDeActivation; }
            set { this.m_ResetOnDeActivation = value; }
        }

        [SerializeField] protected bool m_ResetOnDeActivation = true;
        private bool m_SelectionStillActive = false;
        private bool m_ReleaseSelection = false;

        private GameObject m_PreviouslySelectedObject;

        /// <summary>
        /// Controls whether the original text is restored when pressing "ESC".
        /// </summary>
        public bool restoreOriginalTextOnEscape
        {
            get { return this.m_RestoreOriginalTextOnEscape; }
            set { this.m_RestoreOriginalTextOnEscape = value; }
        }

        [SerializeField] private bool m_RestoreOriginalTextOnEscape = true;

        /// <summary>
        /// Is Rich Text editing allowed?
        /// </summary>
        public bool isRichTextEditingAllowed
        {
            get { return this.m_isRichTextEditingAllowed; }
            set { this.m_isRichTextEditingAllowed = value; }
        }

        [SerializeField] protected bool m_isRichTextEditingAllowed = false;


        // Content Type related
        public ContentType contentType
        {
            get { return this.m_ContentType; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref this.m_ContentType, value)) this.EnforceContentType();
            }
        }

        public LineType lineType
        {
            get { return this.m_LineType; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref this.m_LineType, value))
                {
                    this.SetToCustomIfContentTypeIsNot(ContentType.Standard, ContentType.Autocorrected);
                    this.SetTextComponentWrapMode();
                }
            }
        }

        /// <summary>
        /// Limits the number of lines of text in the Input Field.
        /// </summary>
        public int lineLimit
        {
            get { return this.m_LineLimit; }
            set
            {
                if (this.m_LineType == LineType.SingleLine)
                    this.m_LineLimit = 1;
                else
                    SetPropertyUtility.SetStruct(ref this.m_LineLimit, value);
            }
        }

        [SerializeField] protected int m_LineLimit = 0;

        public InputType inputType
        {
            get { return this.m_InputType; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref this.m_InputType, value)) this.SetToCustom();
            }
        }

        public TouchScreenKeyboardType keyboardType
        {
            get { return this.m_KeyboardType; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref this.m_KeyboardType, value))
                    this.SetToCustom();
            }
        }

        public CharacterValidation characterValidation
        {
            get { return this.m_CharacterValidation; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref this.m_CharacterValidation, value)) this.SetToCustom();
            }
        }

        /// <summary>
        /// Sets the Input Validation to use a Custom Input Validation script.
        /// </summary>
        public TMP_InputValidator inputValidator
        {
            get { return this.m_InputValidator; }
            set
            {
                if (SetPropertyUtility.SetClass(ref this.m_InputValidator, value))
                    this.SetToCustom(CharacterValidation.CustomValidator);
            }
        }

        [SerializeField] protected TMP_InputValidator m_InputValidator = null;

        public bool readOnly
        {
            get { return this.m_ReadOnly; }
            set { this.m_ReadOnly = value; }
        }

        public bool richText
        {
            get { return this.m_RichText; }
            set
            {
                this.m_RichText = value;
                this.SetTextComponentRichTextMode();
            }
        }

        // Derived property
        public bool multiLine
        {
            get { return this.m_LineType == LineType.MultiLineNewline || this.lineType == LineType.MultiLineSubmit; }
        }

        // Not shown in Inspector.
        public char asteriskChar
        {
            get { return this.m_AsteriskChar; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref this.m_AsteriskChar, value)) this.UpdateLabel();
            }
        }

        public bool wasCanceled
        {
            get { return this.m_WasCanceled; }
        }


        protected void ClampStringPos(ref int pos)
        {
            if (pos < 0)
                pos = 0;
            else if (pos > this.text.Length)
                pos = this.text.Length;
        }

        protected void ClampCaretPos(ref int pos)
        {
            if (pos < 0)
                pos = 0;
            else if (pos > this.m_TextComponent.textInfo.characterCount - 1)
                pos = this.m_TextComponent.textInfo.characterCount - 1;
        }

        /// <summary>
        /// Current position of the cursor.
        /// Getters are public Setters are protected
        /// </summary>

        protected int caretPositionInternal
        {
            get { return this.m_CaretPosition + this.compositionLength; }
            set
            {
                this.m_CaretPosition = value;
                this.ClampCaretPos(ref this.m_CaretPosition);
            }
        }

        protected int stringPositionInternal
        {
            get { return this.m_StringPosition + this.compositionLength; }
            set
            {
                this.m_StringPosition = value;
                this.ClampStringPos(ref this.m_StringPosition);
            }
        }

        protected int caretSelectPositionInternal
        {
            get { return this.m_CaretSelectPosition + this.compositionLength; }
            set
            {
                this.m_CaretSelectPosition = value;
                this.ClampCaretPos(ref this.m_CaretSelectPosition);
            }
        }

        protected int stringSelectPositionInternal
        {
            get { return this.m_StringSelectPosition + this.compositionLength; }
            set
            {
                this.m_StringSelectPosition = value;
                this.ClampStringPos(ref this.m_StringSelectPosition);
            }
        }

        private bool hasSelection
        {
            get { return this.stringPositionInternal != this.stringSelectPositionInternal; }
        }

        private bool m_isSelected;
        private bool m_IsStringPositionDirty;
        private bool m_IsCaretPositionDirty;
        private bool m_forceRectTransformAdjustment;

        /// <summary>
        /// Get: Returns the focus position as thats the position that moves around even during selection.
        /// Set: Set both the anchor and focus position such that a selection doesn't happen
        /// </summary>
        public int caretPosition
        {
            get { return this.caretSelectPositionInternal; }
            set
            {
                this.selectionAnchorPosition = value;
                this.selectionFocusPosition = value;
                this.m_IsStringPositionDirty = true;
            }
        }

        /// <summary>
        /// Get: Returns the fixed position of selection
        /// Set: If compositionString is 0 set the fixed position
        /// </summary>
        public int selectionAnchorPosition
        {
            get { return this.caretPositionInternal; }

            set
            {
                if (this.compositionLength != 0)
                    return;

                this.caretPositionInternal = value;
                this.m_IsStringPositionDirty = true;
            }
        }

        /// <summary>
        /// Get: Returns the variable position of selection
        /// Set: If compositionString is 0 set the variable position
        /// </summary>
        public int selectionFocusPosition
        {
            get { return this.caretSelectPositionInternal; }
            set
            {
                if (this.compositionLength != 0)
                    return;

                this.caretSelectPositionInternal = value;
                this.m_IsStringPositionDirty = true;
            }
        }


        /// <summary>
        ///
        /// </summary>
        public int stringPosition
        {
            get { return this.stringSelectPositionInternal; }
            set
            {
                this.selectionStringAnchorPosition = value;
                this.selectionStringFocusPosition = value;
                this.m_IsCaretPositionDirty = true;
            }
        }


        /// <summary>
        /// The fixed position of the selection in the raw string which may contains rich text.
        /// </summary>
        public int selectionStringAnchorPosition
        {
            get { return this.stringPositionInternal; }

            set
            {
                if (this.compositionLength != 0)
                    return;

                this.stringPositionInternal = value;
                this.m_IsCaretPositionDirty = true;
            }
        }


        /// <summary>
        /// The variable position of the selection in the raw string which may contains rich text.
        /// </summary>
        public int selectionStringFocusPosition
        {
            get { return this.stringSelectPositionInternal; }
            set
            {
                if (this.compositionLength != 0)
                    return;

                this.stringSelectPositionInternal = value;
                this.m_IsCaretPositionDirty = true;
            }
        }


#if UNITY_EDITOR
        // Remember: This is NOT related to text validation!
        // This is Unity's own OnValidate method which is invoked when changing values in the Inspector.
        protected override void OnValidate()
        {
            base.OnValidate();
            this.EnforceContentType();

            this.m_CharacterLimit = Math.Max(0, this.m_CharacterLimit);

            //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (!this.IsActive())
                return;

            this.SetTextComponentRichTextMode();

            this.UpdateLabel();

            if (this.m_AllowInput)
                this.SetCaretActive();
        }
#endif // if UNITY_EDITOR

        protected override void OnEnable()
        {
            //Debug.Log("*** OnEnable() *** - " + this.name);

            base.OnEnable();

            if (this.m_Text == null)
                this.m_Text = string.Empty;

            // Check if Input Field is driven by any layout components
            ILayoutController layoutController = this.GetComponent<ILayoutController>();

            if (layoutController != null)
            {
                this.m_IsDrivenByLayoutComponents = true;
                this.m_LayoutGroup = this.GetComponent<LayoutGroup>();
            }
            else
                this.m_IsDrivenByLayoutComponents = false;

            if (Application.isPlaying)
            {
                if (this.m_CachedInputRenderer == null && this.m_TextComponent != null)
                {
                    GameObject go = new GameObject("Caret", typeof(TMP_SelectionCaret));

                    go.hideFlags = HideFlags.DontSave;
                    go.transform.SetParent(this.m_TextComponent.transform.parent);
                    go.transform.SetAsFirstSibling();
                    go.layer = this.gameObject.layer;

                    this.caretRectTrans = go.GetComponent<RectTransform>();
                    this.m_CachedInputRenderer = go.GetComponent<CanvasRenderer>();
                    this.m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);

                    // Needed as if any layout is present we want the caret to always be the same as the text area.
                    go.AddComponent<LayoutElement>().ignoreLayout = true;

                    this.AssignPositioningIfNeeded();
                }
            }

            this.m_RectTransform = this.GetComponent<RectTransform>();

            // Check if parent component has IScrollHandler
            IScrollHandler[] scrollHandlers = this.GetComponentsInParent<IScrollHandler>();
            if (scrollHandlers.Length > 1)
                this.m_IScrollHandlerParent = scrollHandlers[1] as ScrollRect;

            // Get a reference to the RectMask 2D on the Viewport Text Area object.
            if (this.m_TextViewport != null)
            {
                this.m_TextViewportRectMask = this.m_TextViewport.GetComponent<RectMask2D>();

                this.UpdateMaskRegions();
            }

            // If we have a cached renderer then we had OnDisable called so just restore the material.
            if (this.m_CachedInputRenderer != null)
                this.m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);

            if (this.m_TextComponent != null)
            {
                this.m_TextComponent.RegisterDirtyVerticesCallback(this.MarkGeometryAsDirty);
                this.m_TextComponent.RegisterDirtyVerticesCallback(this.UpdateLabel);

                // Cache reference to Vertical Scrollbar RectTransform and add listener.
                if (this.m_VerticalScrollbar != null)
                {
                    this.m_VerticalScrollbar.onValueChanged.AddListener(this.OnScrollbarValueChange);
                }

                this.UpdateLabel();
            }

            // Subscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(this.ON_TEXT_CHANGED);
        }

        protected override void OnDisable()
        {
            // the coroutine will be terminated, so this will ensure it restarts when we are next activated
            this.m_BlinkCoroutine = null;

            this.DeactivateInputField();
            if (this.m_TextComponent != null)
            {
                this.m_TextComponent.UnregisterDirtyVerticesCallback(this.MarkGeometryAsDirty);
                this.m_TextComponent.UnregisterDirtyVerticesCallback(this.UpdateLabel);

                if (this.m_VerticalScrollbar != null)
                    this.m_VerticalScrollbar.onValueChanged.RemoveListener(this.OnScrollbarValueChange);
            }

            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            // Clear needs to be called otherwise sync never happens as the object is disabled.
            if (this.m_CachedInputRenderer != null)
                this.m_CachedInputRenderer.Clear();

            if (this.m_Mesh != null)
                DestroyImmediate(this.m_Mesh);

            this.m_Mesh = null;

            // Unsubscribe to event triggered when text object has been regenerated
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(this.ON_TEXT_CHANGED);

            base.OnDisable();
        }


        /// <summary>
        /// Method used to update the tracking of the caret position when the text object has been regenerated.
        /// </summary>
        /// <param name="obj"></param>
        private void ON_TEXT_CHANGED(UnityEngine.Object obj)
        {
            bool isThisObject = obj == this.m_TextComponent;

            if (isThisObject)
            {
                if (Application.isPlaying && this.compositionLength == 0)
                {
                    this.caretPositionInternal = this.GetCaretPositionFromStringIndex(this.stringPositionInternal);
                    this.caretSelectPositionInternal = this.GetCaretPositionFromStringIndex(this.stringSelectPositionInternal);

#if TMP_DEBUG_MODE
                    Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
                }

                if (this.m_VerticalScrollbar)
                    this.UpdateScrollbar();
            }
        }


        IEnumerator CaretBlink()
        {
            // Always ensure caret is initially visible since it can otherwise be confusing for a moment.
            this.m_CaretVisible = true;
            yield return null;

            while ((this.isFocused || this.m_SelectionStillActive) && this.m_CaretBlinkRate > 0)
            {
                // the blink rate is expressed as a frequency
                float blinkPeriod = 1f / this.m_CaretBlinkRate;

                // the caret should be ON if we are in the first half of the blink period
                bool blinkState = (Time.unscaledTime - this.m_BlinkStartTime) % blinkPeriod < blinkPeriod / 2;
                if (this.m_CaretVisible != blinkState)
                {
                    this.m_CaretVisible = blinkState;
                    if (!this.hasSelection)
                        this.MarkGeometryAsDirty();
                }

                // Then wait again.
                yield return null;
            }

            this.m_BlinkCoroutine = null;
        }

        void SetCaretVisible()
        {
            if (!this.m_AllowInput)
                return;

            this.m_CaretVisible = true;
            this.m_BlinkStartTime = Time.unscaledTime;
            this.SetCaretActive();
        }

        // SetCaretActive will not set the caret immediately visible - it will wait for the next time to blink.
        // However, it will handle things correctly if the blink speed changed from zero to non-zero or non-zero to zero.
        void SetCaretActive()
        {
            if (!this.m_AllowInput)
                return;

            if (this.m_CaretBlinkRate > 0.0f)
            {
                if (this.m_BlinkCoroutine == null)
                    this.m_BlinkCoroutine = this.StartCoroutine(this.CaretBlink());
            }
            else
            {
                this.m_CaretVisible = true;
            }
        }

        protected void OnFocus()
        {
            if (this.m_OnFocusSelectAll)
                this.SelectAll();
        }

        protected void SelectAll()
        {
            this.m_isSelectAll = true;
            this.stringPositionInternal = this.text.Length;
            this.stringSelectPositionInternal = 0;
        }

        /// <summary>
        /// Move to the end of the text.
        /// </summary>
        /// <param name="shift"></param>
        public void MoveTextEnd(bool shift)
        {
            if (this.m_isRichTextEditingAllowed)
            {
                int position = this.text.Length;

                if (shift)
                {
                    this.stringSelectPositionInternal = position;
                }
                else
                {
                    this.stringPositionInternal = position;
                    this.stringSelectPositionInternal = this.stringPositionInternal;
                }
            }
            else
            {
                int position = this.m_TextComponent.textInfo.characterCount - 1;

                if (shift)
                {
                    this.caretSelectPositionInternal = position;
                    this.stringSelectPositionInternal = this.GetStringIndexFromCaretPosition(position);
                }
                else
                {
                    this.caretPositionInternal = this.caretSelectPositionInternal = position;
                    this.stringSelectPositionInternal = this.stringPositionInternal = this.GetStringIndexFromCaretPosition(position);
                }
            }

            this.UpdateLabel();
        }

        /// <summary>
        /// Move to the start of the text.
        /// </summary>
        /// <param name="shift"></param>
        public void MoveTextStart(bool shift)
        {
            if (this.m_isRichTextEditingAllowed)
            {
                int position = 0;

                if (shift)
                {
                    this.stringSelectPositionInternal = position;
                }
                else
                {
                    this.stringPositionInternal = position;
                    this.stringSelectPositionInternal = this.stringPositionInternal;
                }
            }
            else
            {
                int position = 0;

                if (shift)
                {
                    this.caretSelectPositionInternal = position;
                    this.stringSelectPositionInternal = this.GetStringIndexFromCaretPosition(position);
                }
                else
                {
                    this.caretPositionInternal = this.caretSelectPositionInternal = position;
                    this.stringSelectPositionInternal = this.stringPositionInternal = this.GetStringIndexFromCaretPosition(position);
                }
            }

            this.UpdateLabel();
        }


        /// <summary>
        /// Move to the end of the current line of text.
        /// </summary>
        /// <param name="shift"></param>
        public void MoveToEndOfLine(bool shift, bool ctrl)
        {
            // Get the line the caret is currently located on.
            int currentLine = this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal].lineNumber;

            // Get the last character of the given line.
            int characterIndex = ctrl == true
                ? this.m_TextComponent.textInfo.characterCount - 1
                : this.m_TextComponent.textInfo.lineInfo[currentLine].lastCharacterIndex;

            int position = this.m_TextComponent.textInfo.characterInfo[characterIndex].index;

            if (shift)
            {
                this.stringSelectPositionInternal = position;

                this.caretSelectPositionInternal = characterIndex;
            }
            else
            {
                this.stringPositionInternal = position;
                this.stringSelectPositionInternal = this.stringPositionInternal;

                this.caretSelectPositionInternal = this.caretPositionInternal = characterIndex;
            }

            this.UpdateLabel();
        }

        /// <summary>
        /// Move to the start of the current line of text.
        /// </summary>
        /// <param name="shift"></param>
        public void MoveToStartOfLine(bool shift, bool ctrl)
        {
            // Get the line the caret is currently located on.
            int currentLine = this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal].lineNumber;

            // Get the first character of the given line.
            int characterIndex = ctrl == true ? 0 : this.m_TextComponent.textInfo.lineInfo[currentLine].firstCharacterIndex;

            int position = 0;
            if (characterIndex > 0)
                position = this.m_TextComponent.textInfo.characterInfo[characterIndex - 1].index +
                           this.m_TextComponent.textInfo.characterInfo[characterIndex - 1].stringLength;

            if (shift)
            {
                this.stringSelectPositionInternal = position;

                this.caretSelectPositionInternal = characterIndex;
            }
            else
            {
                this.stringPositionInternal = position;
                this.stringSelectPositionInternal = this.stringPositionInternal;

                this.caretSelectPositionInternal = this.caretPositionInternal = characterIndex;
            }

            this.UpdateLabel();
        }


        static string clipboard
        {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        private bool InPlaceEditing()
        {
            if (Application.platform == RuntimePlatform.WSAPlayerX86 ||
                Application.platform == RuntimePlatform.WSAPlayerX64 ||
                Application.platform == RuntimePlatform.WSAPlayerARM)
                return !TouchScreenKeyboard.isSupported || this.m_TouchKeyboardAllowsInPlaceEditing;

            if (TouchScreenKeyboard.isSupported && this.shouldHideSoftKeyboard)
                return true;

            if (TouchScreenKeyboard.isSupported && this.shouldHideSoftKeyboard == false && this.shouldHideMobileInput == false)
                return false;

            return true;
        }

        void UpdateStringPositionFromKeyboard()
        {
            // TODO: Might want to add null check here.
            var selectionRange = this.m_SoftKeyboard.selection;

            //if (selectionRange.start == 0 && selectionRange.length == 0)
            //    return;

            var selectionStart = selectionRange.start;
            var selectionEnd = selectionRange.end;

            var stringPositionChanged = false;

            if (this.stringPositionInternal != selectionStart)
            {
                stringPositionChanged = true;
                this.stringPositionInternal = selectionStart;

                this.caretPositionInternal = this.GetCaretPositionFromStringIndex(this.stringPositionInternal);
            }

            if (this.stringSelectPositionInternal != selectionEnd)
            {
                this.stringSelectPositionInternal = selectionEnd;
                stringPositionChanged = true;

                this.caretSelectPositionInternal = this.GetCaretPositionFromStringIndex(this.stringSelectPositionInternal);
            }

            if (stringPositionChanged)
            {
                this.m_BlinkStartTime = Time.unscaledTime;

                this.UpdateLabel();
            }
        }

        /// <summary>
        /// Update the text based on input.
        /// </summary>
        // TODO: Make LateUpdate a coroutine instead. Allows us to control the update to only be when the field is active.
        protected virtual void LateUpdate()
        {
            // Only activate if we are not already activated.
            if (this.m_ShouldActivateNextUpdate)
            {
                if (!this.isFocused)
                {
                    this.ActivateInputFieldInternal();
                    this.m_ShouldActivateNextUpdate = false;
                    return;
                }

                // Reset as we are already activated.
                this.m_ShouldActivateNextUpdate = false;
            }

            // Handle double click to reset / deselect Input Field when ResetOnActivation is false.
            if (!this.isFocused && this.m_SelectionStillActive)
            {
                GameObject selectedObject =
                    EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

                if (selectedObject == null && this.m_ResetOnDeActivation)
                {
                    this.ReleaseSelection();
                    return;
                }

                if (selectedObject != null && selectedObject != this.gameObject)
                {
                    if (selectedObject == this.m_PreviouslySelectedObject)
                        return;

                    this.m_PreviouslySelectedObject = selectedObject;

                    // Special handling for Vertical Scrollbar
                    if (this.m_VerticalScrollbar && selectedObject == this.m_VerticalScrollbar.gameObject)
                    {
                        // Do not release selection
                        return;
                    }

                    // Release selection for all objects when ResetOnDeActivation is true
                    if (this.m_ResetOnDeActivation)
                    {
                        this.ReleaseSelection();
                        return;
                    }

                    // Release current selection of selected object is another Input Field
                    if (selectedObject.GetComponent<TMP_InputField>() != null)
                        this.ReleaseSelection();

                    return;
                }

#if ENABLE_INPUT_SYSTEM
                if (this.m_ProcessingEvent != null && this.m_ProcessingEvent.rawType == EventType.MouseDown &&
                    this.m_ProcessingEvent.button == 0)
                {
                    // Check for Double Click
                    bool isDoubleClick = false;
                    float timeStamp = Time.unscaledTime;

                    if (this.m_KeyDownStartTime + this.m_DoubleClickDelay > timeStamp)
                        isDoubleClick = true;

                    this.m_KeyDownStartTime = timeStamp;

                    if (isDoubleClick)
                    {
                        //m_StringPosition = m_StringSelectPosition = 0;
                        //m_CaretPosition = m_CaretSelectPosition = 0;
                        //m_TextComponent.rectTransform.localPosition = m_DefaultTransformPosition;

                        //if (caretRectTrans != null)
                        //    caretRectTrans.localPosition = Vector3.zero;

                        this.ReleaseSelection();

                        return;
                    }
                }
#else
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    // Check for Double Click
                    bool isDoubleClick = false;
                    float timeStamp = Time.unscaledTime;

                    if (m_KeyDownStartTime + m_DoubleClickDelay > timeStamp)
                        isDoubleClick = true;

                    m_KeyDownStartTime = timeStamp;

                    if (isDoubleClick)
                    {
                        //m_StringPosition = m_StringSelectPosition = 0;
                        //m_CaretPosition = m_CaretSelectPosition = 0;
                        //m_TextComponent.rectTransform.localPosition = m_DefaultTransformPosition;

                        //if (caretRectTrans != null)
                        //    caretRectTrans.localPosition = Vector3.zero;

                        ReleaseSelection();

                        return;
                    }
                }
#endif
            }

            this.UpdateMaskRegions();

            if (this.InPlaceEditing() && this.isKeyboardUsingEvents() || !this.isFocused)
            {
                return;
            }

            this.AssignPositioningIfNeeded();

            if (this.m_SoftKeyboard == null || this.m_SoftKeyboard.status != TouchScreenKeyboard.Status.Visible)
            {
                if (this.m_SoftKeyboard != null)
                {
                    if (!this.m_ReadOnly)
                        this.text = this.m_SoftKeyboard.text;

                    if (this.m_SoftKeyboard.status == TouchScreenKeyboard.Status.LostFocus)
                        this.SendTouchScreenKeyboardStatusChanged();

                    if (this.m_SoftKeyboard.status == TouchScreenKeyboard.Status.Canceled)
                    {
                        this.m_ReleaseSelection = true;
                        this.m_WasCanceled = true;
                        this.SendTouchScreenKeyboardStatusChanged();
                    }

                    if (this.m_SoftKeyboard.status == TouchScreenKeyboard.Status.Done)
                    {
                        this.m_ReleaseSelection = true;
                        this.OnSubmit(null);
                        this.SendTouchScreenKeyboardStatusChanged();
                    }
                }

                this.OnDeselect(null);
                return;
            }

            string val = this.m_SoftKeyboard.text;

            if (this.m_Text != val)
            {
                if (this.m_ReadOnly)
                {
                    this.m_SoftKeyboard.text = this.m_Text;
                }
                else
                {
                    this.m_Text = "";

                    for (int i = 0; i < val.Length; ++i)
                    {
                        char c = val[i];

                        if (c == '\r' || c == 3)
                            c = '\n';

                        if (this.onValidateInput != null)
                            c = this.onValidateInput(this.m_Text, this.m_Text.Length, c);
                        else if (this.characterValidation != CharacterValidation.None)
                            c = this.Validate(this.m_Text, this.m_Text.Length, c);

                        if (this.lineType == LineType.MultiLineSubmit && c == '\n')
                        {
                            this.m_SoftKeyboard.text = this.m_Text;

                            this.OnSubmit(null);
                            this.OnDeselect(null);
                            return;
                        }

                        if (c != 0)
                            this.m_Text += c;
                    }

                    if (this.characterLimit > 0 && this.m_Text.Length > this.characterLimit)
                        this.m_Text = this.m_Text.Substring(0, this.characterLimit);

                    this.UpdateStringPositionFromKeyboard();

                    // Set keyboard text before updating label, as we might have changed it with validation
                    // and update label will take the old value from keyboard if we don't change it here
                    if (this.m_Text != val)
                        this.m_SoftKeyboard.text = this.m_Text;

                    this.SendOnValueChangedAndUpdateLabel();
                }
            }
            else if (this.m_HideMobileInput && Application.platform == RuntimePlatform.Android)
            {
                this.UpdateStringPositionFromKeyboard();
            }

            //else if (m_HideMobileInput) // m_Keyboard.canSetSelection
            //{
            //    int length = stringPositionInternal < stringSelectPositionInternal ? stringSelectPositionInternal - stringPositionInternal : stringPositionInternal - stringSelectPositionInternal;
            //    m_SoftKeyboard.selection = new RangeInt(stringPositionInternal < stringSelectPositionInternal ? stringPositionInternal : stringSelectPositionInternal, length);
            //}
            //else if (!m_HideMobileInput) // m_Keyboard.canGetSelection)
            //{
            //    UpdateStringPositionFromKeyboard();
            //}

            if (this.m_SoftKeyboard != null && this.m_SoftKeyboard.status != TouchScreenKeyboard.Status.Visible)
            {
                if (this.m_SoftKeyboard.status == TouchScreenKeyboard.Status.Canceled)
                    this.m_WasCanceled = true;

                this.OnDeselect(null);
            }
        }

        private bool MayDrag(PointerEventData eventData)
        {
            return this.IsActive() &&
                   this.IsInteractable() &&
                   eventData.button == PointerEventData.InputButton.Left &&
                   this.m_TextComponent != null &&
                   (this.m_SoftKeyboard == null || this.shouldHideSoftKeyboard || this.shouldHideMobileInput);
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!this.MayDrag(eventData))
                return;

            this.m_UpdateDrag = true;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!this.MayDrag(eventData))
                return;

            CaretPosition insertionSide;

            int insertionIndex = TMP_TextUtilities.GetCursorIndexFromPosition(this.m_TextComponent, eventData.position,
                eventData.pressEventCamera, out insertionSide);

            if (this.m_isRichTextEditingAllowed)
            {
                if (insertionSide == CaretPosition.Left)
                {
                    this.stringSelectPositionInternal = this.m_TextComponent.textInfo.characterInfo[insertionIndex].index;
                }
                else if (insertionSide == CaretPosition.Right)
                {
                    this.stringSelectPositionInternal = this.m_TextComponent.textInfo.characterInfo[insertionIndex].index +
                                                        this.m_TextComponent.textInfo.characterInfo[insertionIndex].stringLength;
                }
            }
            else
            {
                if (insertionSide == CaretPosition.Left)
                {
                    this.stringSelectPositionInternal = insertionIndex == 0
                        ? this.m_TextComponent.textInfo.characterInfo[0].index
                        : this.m_TextComponent.textInfo.characterInfo[insertionIndex - 1].index +
                          this.m_TextComponent.textInfo.characterInfo[insertionIndex - 1].stringLength;
                }
                else if (insertionSide == CaretPosition.Right)
                {
                    this.stringSelectPositionInternal = this.m_TextComponent.textInfo.characterInfo[insertionIndex].index +
                                                        this.m_TextComponent.textInfo.characterInfo[insertionIndex].stringLength;
                }
            }

            this.caretSelectPositionInternal = this.GetCaretPositionFromStringIndex(this.stringSelectPositionInternal);

            this.MarkGeometryAsDirty();

            this.m_DragPositionOutOfBounds =
                !RectTransformUtility.RectangleContainsScreenPoint(this.textViewport, eventData.position,
                    eventData.pressEventCamera);
            if (this.m_DragPositionOutOfBounds && this.m_DragCoroutine == null)
                this.m_DragCoroutine = this.StartCoroutine(this.MouseDragOutsideRect(eventData));

            eventData.Use();

#if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
        }

        IEnumerator MouseDragOutsideRect(PointerEventData eventData)
        {
            while (this.m_UpdateDrag && this.m_DragPositionOutOfBounds)
            {
                Vector2 localMousePos;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(this.textViewport, eventData.position,
                    eventData.pressEventCamera, out localMousePos);

                Rect rect = this.textViewport.rect;

                if (this.multiLine)
                {
                    if (localMousePos.y > rect.yMax)
                        this.MoveUp(true, true);
                    else if (localMousePos.y < rect.yMin)
                        this.MoveDown(true, true);
                }
                else
                {
                    if (localMousePos.x < rect.xMin)
                        this.MoveLeft(true, false);
                    else if (localMousePos.x > rect.xMax)
                        this.MoveRight(true, false);
                }

                this.UpdateLabel();

                float delay = this.multiLine ? kVScrollSpeed : kHScrollSpeed;

                if (this.m_WaitForSecondsRealtime == null)
                    this.m_WaitForSecondsRealtime = new WaitForSecondsRealtime(delay);
                else
                    this.m_WaitForSecondsRealtime.waitTime = delay;

                yield return this.m_WaitForSecondsRealtime;
            }

            this.m_DragCoroutine = null;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!this.MayDrag(eventData))
                return;

            this.m_UpdateDrag = false;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!this.MayDrag(eventData))
                return;

            EventSystem.current.SetSelectedGameObject(this.gameObject, eventData);

            bool hadFocusBefore = this.m_AllowInput;
            base.OnPointerDown(eventData);

            if (this.InPlaceEditing() == false)
            {
                if (this.m_SoftKeyboard == null || !this.m_SoftKeyboard.active)
                {
                    this.OnSelect(eventData);
                    return;
                }
            }

#if ENABLE_INPUT_SYSTEM
            Event.PopEvent(this.m_ProcessingEvent);
            bool shift = this.m_ProcessingEvent != null && (this.m_ProcessingEvent.modifiers & EventModifiers.Shift) != 0;
#else
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif

            // Check for Double Click
            bool isDoubleClick = false;
            float timeStamp = Time.unscaledTime;

            if (this.m_PointerDownClickStartTime + this.m_DoubleClickDelay > timeStamp)
                isDoubleClick = true;

            this.m_PointerDownClickStartTime = timeStamp;

            // Only set caret position if we didn't just get focus now.
            // Otherwise it will overwrite the select all on focus.
            if (hadFocusBefore || !this.m_OnFocusSelectAll)
            {
                CaretPosition insertionSide;

                int insertionIndex = TMP_TextUtilities.GetCursorIndexFromPosition(this.m_TextComponent, eventData.position,
                    eventData.pressEventCamera, out insertionSide);

                if (shift)
                {
                    if (this.m_isRichTextEditingAllowed)
                    {
                        if (insertionSide == CaretPosition.Left)
                        {
                            this.stringSelectPositionInternal = this.m_TextComponent.textInfo.characterInfo[insertionIndex].index;
                        }
                        else if (insertionSide == CaretPosition.Right)
                        {
                            this.stringSelectPositionInternal =
                                this.m_TextComponent.textInfo.characterInfo[insertionIndex].index + this.m_TextComponent.textInfo
                                    .characterInfo[insertionIndex].stringLength;
                        }
                    }
                    else
                    {
                        if (insertionSide == CaretPosition.Left)
                        {
                            this.stringSelectPositionInternal = insertionIndex == 0
                                ? this.m_TextComponent.textInfo.characterInfo[0].index
                                : this.m_TextComponent.textInfo.characterInfo[insertionIndex - 1].index +
                                  this.m_TextComponent.textInfo.characterInfo[insertionIndex - 1].stringLength;
                        }
                        else if (insertionSide == CaretPosition.Right)
                        {
                            this.stringSelectPositionInternal =
                                this.m_TextComponent.textInfo.characterInfo[insertionIndex].index + this.m_TextComponent.textInfo
                                    .characterInfo[insertionIndex].stringLength;
                        }
                    }
                }
                else
                {
                    if (this.m_isRichTextEditingAllowed)
                    {
                        if (insertionSide == CaretPosition.Left)
                        {
                            this.stringPositionInternal = this.stringSelectPositionInternal =
                                this.m_TextComponent.textInfo.characterInfo[insertionIndex].index;
                        }
                        else if (insertionSide == CaretPosition.Right)
                        {
                            this.stringPositionInternal = this.stringSelectPositionInternal =
                                this.m_TextComponent.textInfo.characterInfo[insertionIndex].index + this.m_TextComponent.textInfo
                                    .characterInfo[insertionIndex].stringLength;
                        }
                    }
                    else
                    {
                        if (insertionSide == CaretPosition.Left)
                        {
                            this.stringPositionInternal = this.stringSelectPositionInternal = insertionIndex == 0
                                ? this.m_TextComponent.textInfo.characterInfo[0].index
                                : this.m_TextComponent.textInfo.characterInfo[insertionIndex - 1].index +
                                  this.m_TextComponent.textInfo.characterInfo[insertionIndex - 1].stringLength;
                        }
                        else if (insertionSide == CaretPosition.Right)
                        {
                            this.stringPositionInternal = this.stringSelectPositionInternal =
                                this.m_TextComponent.textInfo.characterInfo[insertionIndex].index + this.m_TextComponent.textInfo
                                    .characterInfo[insertionIndex].stringLength;
                        }
                    }
                }


                if (isDoubleClick)
                {
                    int wordIndex = TMP_TextUtilities.FindIntersectingWord(this.m_TextComponent, eventData.position,
                        eventData.pressEventCamera);

                    if (wordIndex != -1)
                    {
                        // TODO: Should behavior be different if rich text editing is enabled or not?

                        // Select current word
                        this.caretPositionInternal = this.m_TextComponent.textInfo.wordInfo[wordIndex].firstCharacterIndex;
                        this.caretSelectPositionInternal =
                            this.m_TextComponent.textInfo.wordInfo[wordIndex].lastCharacterIndex + 1;

                        this.stringPositionInternal = this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal].index;
                        this.stringSelectPositionInternal =
                            this.m_TextComponent.textInfo.characterInfo[this.caretSelectPositionInternal - 1].index +
                            this.m_TextComponent.textInfo.characterInfo[this.caretSelectPositionInternal - 1].stringLength;
                    }
                    else
                    {
                        // Select current character
                        this.caretPositionInternal = insertionIndex;
                        this.caretSelectPositionInternal = this.caretPositionInternal + 1;

                        this.stringPositionInternal = this.m_TextComponent.textInfo.characterInfo[insertionIndex].index;
                        this.stringSelectPositionInternal = this.stringPositionInternal +
                                                            this.m_TextComponent.textInfo.characterInfo[insertionIndex]
                                                                .stringLength;
                    }
                }
                else
                {
                    this.caretPositionInternal = this.caretSelectPositionInternal =
                        this.GetCaretPositionFromStringIndex(this.stringPositionInternal);
                }

                this.m_isSelectAll = false;
            }

            this.UpdateLabel();
            eventData.Use();

#if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
        }

        protected enum EditState
        {
            Continue,
            Finish
        }

        protected EditState KeyPressed(Event evt)
        {
            var currentEventModifiers = evt.modifiers;
            bool ctrl = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX
                ? (currentEventModifiers & EventModifiers.Command) != 0
                : (currentEventModifiers & EventModifiers.Control) != 0;
            bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
            bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
            bool ctrlOnly = ctrl && !alt && !shift;

            switch (evt.keyCode)
            {
                case KeyCode.Backspace:
                {
                    this.Backspace();
                    return EditState.Continue;
                }

                case KeyCode.Delete:
                {
                    this.DeleteKey();
                    return EditState.Continue;
                }

                case KeyCode.Home:
                {
                    this.MoveToStartOfLine(shift, ctrl);
                    return EditState.Continue;
                }

                case KeyCode.End:
                {
                    this.MoveToEndOfLine(shift, ctrl);
                    return EditState.Continue;
                }

                // Select All
                case KeyCode.A:
                {
                    if (ctrlOnly)
                    {
                        this.SelectAll();
                        return EditState.Continue;
                    }

                    break;
                }

                // Copy
                case KeyCode.C:
                {
                    if (ctrlOnly)
                    {
                        if (this.inputType != InputType.Password)
                            clipboard = this.GetSelectedString();
                        else
                            clipboard = "";
                        return EditState.Continue;
                    }

                    break;
                }

                // Paste
                case KeyCode.V:
                {
                    if (ctrlOnly)
                    {
                        // TODO: Shinanigans Here
                        this.characterValidation = CharacterValidation.None;
                        this.Append(clipboard);
                        this.characterValidation = CharacterValidation.CustomValidator;
                        return EditState.Continue;
                    }

                    break;
                }

                // Cut
                case KeyCode.X:
                {
                    if (ctrlOnly)
                    {
                        if (this.inputType != InputType.Password)
                            clipboard = this.GetSelectedString();
                        else
                            clipboard = "";
                        this.Delete();
                        this.UpdateTouchKeyboardFromEditChanges();
                        this.SendOnValueChangedAndUpdateLabel();
                        return EditState.Continue;
                    }

                    break;
                }

                case KeyCode.LeftArrow:
                {
                    this.leftRightAction?.Invoke();
                    this.MoveLeft(shift, ctrl);
                    return EditState.Continue;
                }

                case KeyCode.RightArrow:
                {
                    this.leftRightAction?.Invoke();
                    this.MoveRight(shift, ctrl);
                    return EditState.Continue;
                }

                case KeyCode.UpArrow:
                {
                    if (this.CompletionServiceOn) break;

                    this.MoveUp(shift);
                    return EditState.Continue;
                }

                case KeyCode.DownArrow:
                {
                    if (this.CompletionServiceOn) break;
                    this.MoveDown(shift);
                    return EditState.Continue;
                }

                case KeyCode.PageUp:
                {
                    this.MovePageUp(shift);
                    return EditState.Continue;
                }

                case KeyCode.PageDown:
                {
                    this.MovePageDown(shift);
                    return EditState.Continue;
                }

                // Submit
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                {
                    if (this.CompletionServiceOn) break;

                    if (this.lineType != LineType.MultiLineNewline)
                    {
                        this.m_ReleaseSelection = true;
                        return EditState.Finish;
                    }

                    break;
                }

                case KeyCode.Escape:
                {
                    this.m_ReleaseSelection = true;
                    this.m_WasCanceled = true;
                    return EditState.Finish;
                }
            }

            char c = evt.character;

            // Don't allow return chars or tabulator key to be entered into single line fields.
            if (!this.multiLine && (c == '\t' || c == '\r' || c == 10))
                return EditState.Continue;

            // Convert carriage return and end-of-text characters to newline.
            if (c == '\r' || (int)c == 3)
                c = '\n';

            // Convert Shift Enter to Vertical tab
            if (shift && c == '\n')
                c = '\v';

            if (this.IsValidChar(c))
            {
                this.Append(c);
            }

            if (c == 0)
            {
                if (this.compositionLength > 0)
                {
                    this.UpdateLabel();
                }
            }

            return EditState.Continue;
        }

        protected virtual bool IsValidChar(char c)
        {
            // Null character
            if (c == 0)
                return false;

            // Delete key on mac
            if (c == 127)
                return false;

            // Accept newline and tab
            if (c == '\t' || c == '\n')
                return true;

            return true;

            // With the addition of Dynamic support, I think this will best be handled by the text component.
            //return m_TextComponent.font.HasCharacter(c, true);
        }

        /// <summary>
        /// Handle the specified event.
        /// </summary>
        private Event m_ProcessingEvent = new Event();

        public void ProcessEvent(Event e)
        {
            this.KeyPressed(e);
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnUpdateSelected(BaseEventData eventData)
        {
            if (!this.isFocused)
                return;

            bool consumedEvent = false;
            EditState shouldContinue;

            while (Event.PopEvent(this.m_ProcessingEvent))
            {
                //Debug.Log("Event: " + m_ProcessingEvent.ToString() + "  IsCompositionActive= " + m_IsCompositionActive + "  Composition Length: " + compositionLength);

                switch (this.m_ProcessingEvent.rawType)
                {
                    case EventType.KeyUp:
                        // TODO: Figure out way to handle navigation during IME Composition.

                        break;


                    case EventType.KeyDown:
                        consumedEvent = true;

                        // Special handling on OSX which produces more events which need to be suppressed.
                        if (this.m_IsCompositionActive && this.compositionLength == 0)
                        {
                            //if (m_ProcessingEvent.keyCode == KeyCode.Backspace && m_ProcessingEvent.modifiers == EventModifiers.None)
                            //{
                            //    int eventCount = Event.GetEventCount();

                            //    // Suppress all subsequent events
                            //    for (int i = 0; i < eventCount; i++)
                            //        Event.PopEvent(m_ProcessingEvent);

                            //    break;
                            //}

                            // Suppress other events related to navigation or termination of composition sequence.
                            if (this.m_ProcessingEvent.character == 0 && this.m_ProcessingEvent.modifiers == EventModifiers.None)
                                break;
                        }

                        shouldContinue = this.KeyPressed(this.m_ProcessingEvent);
                        if (shouldContinue == EditState.Finish)
                        {
                            if (!this.m_WasCanceled)
                                this.SendOnSubmit();

                            this.DeactivateInputField();
                            break;
                        }

                        this.m_IsTextComponentUpdateRequired = true;
                        this.UpdateLabel();

                        break;

                    case EventType.ValidateCommand:
                    case EventType.ExecuteCommand:
                        switch (this.m_ProcessingEvent.commandName)
                        {
                            case "SelectAll":
                                this.SelectAll();
                                consumedEvent = true;
                                break;
                        }

                        break;
                }
            }

            if (consumedEvent)
                this.UpdateLabel();

            eventData.Use();
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnScroll(PointerEventData eventData)
        {
            // Return if Single Line
            if (this.m_LineType == LineType.SingleLine)
            {
                if (this.m_IScrollHandlerParent != null)
                    this.m_IScrollHandlerParent.OnScroll(eventData);

                return;
            }

            if (this.m_TextComponent.preferredHeight < this.m_TextViewport.rect.height)
                return;

            float scrollDirection = -eventData.scrollDelta.y;

            // Determine the current scroll position of the text within the viewport
            this.m_ScrollPosition = this.GetScrollPositionRelativeToViewport();

            this.m_ScrollPosition += (1f / this.m_TextComponent.textInfo.lineCount) * scrollDirection * this.m_ScrollSensitivity;

            this.m_ScrollPosition = Mathf.Clamp01(this.m_ScrollPosition);

            this.AdjustTextPositionRelativeToViewport(this.m_ScrollPosition);

            if (this.m_VerticalScrollbar)
            {
                this.m_VerticalScrollbar.value = this.m_ScrollPosition;
            }

            this.scrollAction?.Invoke(this.m_ScrollPosition);

            //Debug.Log(GetInstanceID() + "- Scroll Position:" + m_ScrollPosition);
        }

        float GetScrollPositionRelativeToViewport()
        {
            // Determine the current scroll position of the text within the viewport
            Rect viewportRect = this.m_TextViewport.rect;

            float scrollPosition =
                (this.m_TextComponent.textInfo.lineInfo[0].ascender - viewportRect.yMax +
                 this.m_TextComponent.rectTransform.anchoredPosition.y) /
                (this.m_TextComponent.preferredHeight - viewportRect.height);

            scrollPosition = (int)((scrollPosition * 1000) + 0.5f) / 1000.0f;

            return scrollPosition;
        }

        private string GetSelectedString()
        {
            if (!this.hasSelection)
                return "";

            int startPos = this.stringPositionInternal;
            int endPos = this.stringSelectPositionInternal;

            // Ensure pos is always less then selPos to make the code simpler
            if (startPos > endPos)
            {
                int temp = startPos;
                startPos = endPos;
                endPos = temp;
            }

            //for (int i = m_CaretPosition; i < m_CaretSelectPosition; i++)
            //{
            //    Debug.Log("Character [" + m_TextComponent.textInfo.characterInfo[i].character + "] using Style [" + m_TextComponent.textInfo.characterInfo[i].style + "] has been selected.");
            //}


            return this.text.Substring(startPos, endPos - startPos);
        }

        private int FindNextWordBegin()
        {
            if (this.stringSelectPositionInternal + 1 >= this.text.Length)
                return this.text.Length;

            int spaceLoc = this.text.IndexOfAny(kSeparators, this.stringSelectPositionInternal + 1);

            if (spaceLoc == -1)
                spaceLoc = this.text.Length;
            else
                spaceLoc++;

            return spaceLoc;
        }

        private void MoveRight(bool shift, bool ctrl)
        {
            if (this.hasSelection && !shift)
            {
                // By convention, if we have a selection and move right without holding shift,
                // we just place the cursor at the end.
                this.stringPositionInternal = this.stringSelectPositionInternal =
                    Mathf.Max(this.stringPositionInternal, this.stringSelectPositionInternal);
                this.caretPositionInternal = this.caretSelectPositionInternal =
                    this.GetCaretPositionFromStringIndex(this.stringSelectPositionInternal);

#if TMP_DEBUG_MODE
                    Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
                return;
            }

            int position;
            if (ctrl)
                position = this.FindNextWordBegin();
            else
            {
                if (this.m_isRichTextEditingAllowed)
                {
                    // Special handling for Surrogate pairs and Diacritical marks.
                    if (this.stringSelectPositionInternal < this.text.Length &&
                        char.IsHighSurrogate(this.text[this.stringSelectPositionInternal]))
                        position = this.stringSelectPositionInternal + 2;
                    else
                        position = this.stringSelectPositionInternal + 1;
                }
                else
                {
                    position = this.m_TextComponent.textInfo.characterInfo[this.caretSelectPositionInternal].index +
                               this.m_TextComponent.textInfo.characterInfo[this.caretSelectPositionInternal].stringLength;
                }
            }

            if (shift)
            {
                this.stringSelectPositionInternal = position;
                this.caretSelectPositionInternal = this.GetCaretPositionFromStringIndex(this.stringSelectPositionInternal);
            }
            else
            {
                this.stringSelectPositionInternal = this.stringPositionInternal = position;

                // Only increase caret position as we cross character boundary.
                if (this.stringPositionInternal >= this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal].index +
                    this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal].stringLength)
                    this.caretSelectPositionInternal = this.caretPositionInternal =
                        this.GetCaretPositionFromStringIndex(this.stringSelectPositionInternal);
            }

#if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + "  Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + "  String Select Position: " + stringSelectPositionInternal);
#endif
        }

        private int FindPrevWordBegin()
        {
            if (this.stringSelectPositionInternal - 2 < 0)
                return 0;

            int spaceLoc = this.text.LastIndexOfAny(kSeparators, this.stringSelectPositionInternal - 2);

            if (spaceLoc == -1)
                spaceLoc = 0;
            else
                spaceLoc++;

            return spaceLoc;
        }

        private void MoveLeft(bool shift, bool ctrl)
        {
            if (this.hasSelection && !shift)
            {
                // By convention, if we have a selection and move left without holding shift,
                // we just place the cursor at the start.
                this.stringPositionInternal = this.stringSelectPositionInternal =
                    Mathf.Min(this.stringPositionInternal, this.stringSelectPositionInternal);
                this.caretPositionInternal = this.caretSelectPositionInternal =
                    this.GetCaretPositionFromStringIndex(this.stringSelectPositionInternal);

#if TMP_DEBUG_MODE
                    Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
                return;
            }

            int position;
            if (ctrl)
                position = this.FindPrevWordBegin();
            else
            {
                if (this.m_isRichTextEditingAllowed)
                {
                    // Special handling for Surrogate pairs and Diacritical marks.
                    if (this.stringSelectPositionInternal > 0 && char.IsLowSurrogate(this.text[this.stringSelectPositionInternal - 1]))
                        position = this.stringSelectPositionInternal - 2;
                    else
                        position = this.stringSelectPositionInternal - 1;
                }
                else
                {
                    position = this.caretSelectPositionInternal < 1
                        ? this.m_TextComponent.textInfo.characterInfo[0].index
                        : this.m_TextComponent.textInfo.characterInfo[this.caretSelectPositionInternal - 1].index;
                }
            }

            if (shift)
            {
                this.stringSelectPositionInternal = position;
                this.caretSelectPositionInternal = this.GetCaretPositionFromStringIndex(this.stringSelectPositionInternal);
            }
            else
            {
                this.stringSelectPositionInternal = this.stringPositionInternal = position;

                // Only decrease caret position as we cross character boundary.
                if (this.caretPositionInternal > 0 && this.stringPositionInternal <=
                    this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal - 1].index)
                    this.caretSelectPositionInternal = this.caretPositionInternal =
                        this.GetCaretPositionFromStringIndex(this.stringSelectPositionInternal);
            }

#if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + "  Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + "  String Select Position: " + stringSelectPositionInternal);
#endif
        }


        private int LineUpCharacterPosition(int originalPos, bool goToFirstChar)
        {
            if (originalPos >= this.m_TextComponent.textInfo.characterCount)
                originalPos -= 1;

            TMP_CharacterInfo originChar = this.m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            // We are on the first line return first character
            if (originLine - 1 < 0)
                return goToFirstChar ? 0 : originalPos;

            int endCharIdx = this.m_TextComponent.textInfo.lineInfo[originLine].firstCharacterIndex - 1;

            int closest = -1;
            float distance = TMP_Math.FLOAT_MAX;
            float range = 0;

            for (int i = this.m_TextComponent.textInfo.lineInfo[originLine - 1].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = this.m_TextComponent.textInfo.characterInfo[i];

                float d = originChar.origin - currentChar.origin;
                float r = d / (currentChar.xAdvance - currentChar.origin);

                if (r >= 0 && r <= 1)
                {
                    if (r < 0.5f)
                        return i;
                    else
                        return i + 1;
                }

                d = Mathf.Abs(d);

                if (d < distance)
                {
                    closest = i;
                    distance = d;
                    range = r;
                }
            }

            if (closest == -1) return endCharIdx;

            //Debug.Log("Returning nearest character with Range = " + range);

            if (range < 0.5f)
                return closest;
            else
                return closest + 1;
        }


        private int LineDownCharacterPosition(int originalPos, bool goToLastChar)
        {
            if (originalPos >= this.m_TextComponent.textInfo.characterCount)
                return this.m_TextComponent.textInfo.characterCount - 1; // text.Length;

            TMP_CharacterInfo originChar = this.m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            //// We are on the last line return last character
            if (originLine + 1 >= this.m_TextComponent.textInfo.lineCount)
                return goToLastChar ? this.m_TextComponent.textInfo.characterCount - 1 : originalPos;

            // Need to determine end line for next line.
            int endCharIdx = this.m_TextComponent.textInfo.lineInfo[originLine + 1].lastCharacterIndex;

            int closest = -1;
            float distance = TMP_Math.FLOAT_MAX;
            float range = 0;

            for (int i = this.m_TextComponent.textInfo.lineInfo[originLine + 1].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = this.m_TextComponent.textInfo.characterInfo[i];

                float d = originChar.origin - currentChar.origin;
                float r = d / (currentChar.xAdvance - currentChar.origin);

                if (r >= 0 && r <= 1)
                {
                    if (r < 0.5f)
                        return i;
                    else
                        return i + 1;
                }

                d = Mathf.Abs(d);

                if (d < distance)
                {
                    closest = i;
                    distance = d;
                    range = r;
                }
            }

            if (closest == -1) return endCharIdx;

            //Debug.Log("Returning nearest character with Range = " + range);

            if (range < 0.5f)
                return closest;
            else
                return closest + 1;
        }


        private int PageUpCharacterPosition(int originalPos, bool goToFirstChar)
        {
            if (originalPos >= this.m_TextComponent.textInfo.characterCount)
                originalPos -= 1;

            TMP_CharacterInfo originChar = this.m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            // We are on the first line return first character
            if (originLine - 1 < 0)
                return goToFirstChar ? 0 : originalPos;

            float viewportHeight = this.m_TextViewport.rect.height;

            int newLine = originLine - 1;
            // Iterate through each subsequent line to find the first baseline that is not visible in the viewport.
            for (; newLine > 0; newLine--)
            {
                if (this.m_TextComponent.textInfo.lineInfo[newLine].baseline >
                    this.m_TextComponent.textInfo.lineInfo[originLine].baseline + viewportHeight)
                    break;
            }

            int endCharIdx = this.m_TextComponent.textInfo.lineInfo[newLine].lastCharacterIndex;

            int closest = -1;
            float distance = TMP_Math.FLOAT_MAX;
            float range = 0;

            for (int i = this.m_TextComponent.textInfo.lineInfo[newLine].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = this.m_TextComponent.textInfo.characterInfo[i];

                float d = originChar.origin - currentChar.origin;
                float r = d / (currentChar.xAdvance - currentChar.origin);

                if (r >= 0 && r <= 1)
                {
                    if (r < 0.5f)
                        return i;
                    else
                        return i + 1;
                }

                d = Mathf.Abs(d);

                if (d < distance)
                {
                    closest = i;
                    distance = d;
                    range = r;
                }
            }

            if (closest == -1) return endCharIdx;

            //Debug.Log("Returning nearest character with Range = " + range);

            if (range < 0.5f)
                return closest;
            else
                return closest + 1;
        }


        private int PageDownCharacterPosition(int originalPos, bool goToLastChar)
        {
            if (originalPos >= this.m_TextComponent.textInfo.characterCount)
                return this.m_TextComponent.textInfo.characterCount - 1;

            TMP_CharacterInfo originChar = this.m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            // We are on the last line return last character
            if (originLine + 1 >= this.m_TextComponent.textInfo.lineCount)
                return goToLastChar ? this.m_TextComponent.textInfo.characterCount - 1 : originalPos;

            float viewportHeight = this.m_TextViewport.rect.height;

            int newLine = originLine + 1;
            // Iterate through each subsequent line to find the first baseline that is not visible in the viewport.
            for (; newLine < this.m_TextComponent.textInfo.lineCount - 1; newLine++)
            {
                if (this.m_TextComponent.textInfo.lineInfo[newLine].baseline <
                    this.m_TextComponent.textInfo.lineInfo[originLine].baseline - viewportHeight)
                    break;
            }

            // Need to determine end line for next line.
            int endCharIdx = this.m_TextComponent.textInfo.lineInfo[newLine].lastCharacterIndex;

            int closest = -1;
            float distance = TMP_Math.FLOAT_MAX;
            float range = 0;

            for (int i = this.m_TextComponent.textInfo.lineInfo[newLine].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = this.m_TextComponent.textInfo.characterInfo[i];

                float d = originChar.origin - currentChar.origin;
                float r = d / (currentChar.xAdvance - currentChar.origin);

                if (r >= 0 && r <= 1)
                {
                    if (r < 0.5f)
                        return i;
                    else
                        return i + 1;
                }

                d = Mathf.Abs(d);

                if (d < distance)
                {
                    closest = i;
                    distance = d;
                    range = r;
                }
            }

            if (closest == -1) return endCharIdx;

            if (range < 0.5f)
                return closest;
            else
                return closest + 1;
        }


        private void MoveDown(bool shift)
        {
            this.MoveDown(shift, true);
        }


        private void MoveDown(bool shift, bool goToLastChar)
        {
            if (this.hasSelection && !shift)
            {
                // If we have a selection and press down without shift,
                // set caret to end of selection before we move it down.
                this.caretPositionInternal = this.caretSelectPositionInternal =
                    Mathf.Max(this.caretPositionInternal, this.caretSelectPositionInternal);
            }

            int position = this.multiLine
                ? this.LineDownCharacterPosition(this.caretSelectPositionInternal, goToLastChar)
                : this.m_TextComponent.textInfo.characterCount - 1; // text.Length;

            if (shift)
            {
                this.caretSelectPositionInternal = position;
                this.stringSelectPositionInternal = this.GetStringIndexFromCaretPosition(this.caretSelectPositionInternal);
            }
            else
            {
                this.caretSelectPositionInternal = this.caretPositionInternal = position;
                this.stringSelectPositionInternal = this.stringPositionInternal =
                    this.GetStringIndexFromCaretPosition(this.caretSelectPositionInternal);
            }

#if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
        }

        private void MoveUp(bool shift)
        {
            this.MoveUp(shift, true);
        }


        private void MoveUp(bool shift, bool goToFirstChar)
        {
            if (this.hasSelection && !shift)
            {
                // If we have a selection and press up without shift,
                // set caret position to start of selection before we move it up.
                this.caretPositionInternal = this.caretSelectPositionInternal =
                    Mathf.Min(this.caretPositionInternal, this.caretSelectPositionInternal);
            }

            int position = this.multiLine ? this.LineUpCharacterPosition(this.caretSelectPositionInternal, goToFirstChar) : 0;

            if (shift)
            {
                this.caretSelectPositionInternal = position;
                this.stringSelectPositionInternal = this.GetStringIndexFromCaretPosition(this.caretSelectPositionInternal);
            }
            else
            {
                this.caretSelectPositionInternal = this.caretPositionInternal = position;
                this.stringSelectPositionInternal = this.stringPositionInternal =
                    this.GetStringIndexFromCaretPosition(this.caretSelectPositionInternal);
            }

#if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
        }


        private void MovePageUp(bool shift)
        {
            this.MovePageUp(shift, true);
        }

        private void MovePageUp(bool shift, bool goToFirstChar)
        {
            if (this.hasSelection && !shift)
            {
                // If we have a selection and press up without shift,
                // set caret position to start of selection before we move it up.
                this.caretPositionInternal = this.caretSelectPositionInternal =
                    Mathf.Min(this.caretPositionInternal, this.caretSelectPositionInternal);
            }

            int position = this.multiLine ? this.PageUpCharacterPosition(this.caretSelectPositionInternal, goToFirstChar) : 0;

            if (shift)
            {
                this.caretSelectPositionInternal = position;
                this.stringSelectPositionInternal = this.GetStringIndexFromCaretPosition(this.caretSelectPositionInternal);
            }
            else
            {
                this.caretSelectPositionInternal = this.caretPositionInternal = position;
                this.stringSelectPositionInternal = this.stringPositionInternal =
                    this.GetStringIndexFromCaretPosition(this.caretSelectPositionInternal);
            }


            // Scroll to top of viewport
            //int currentLine = m_TextComponent.textInfo.characterInfo[position].lineNumber;
            //float lineAscender = m_TextComponent.textInfo.lineInfo[currentLine].ascender;

            // Adjust text area up or down if not in single line mode.
            if (this.m_LineType != LineType.SingleLine)
            {
                float
                    offset = this.m_TextViewport.rect
                        .height; // m_TextViewport.rect.yMax - (m_TextComponent.rectTransform.anchoredPosition.y + lineAscender);

                float topTextBounds = this.m_TextComponent.rectTransform.position.y + this.m_TextComponent.textBounds.max.y;
                float topViewportBounds = this.m_TextViewport.position.y + this.m_TextViewport.rect.yMax;

                offset = topViewportBounds > topTextBounds + offset ? offset : topViewportBounds - topTextBounds;

                this.m_TextComponent.rectTransform.anchoredPosition += new Vector2(0, offset);
                this.AssignPositioningIfNeeded();
            }

#if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
        }


        private void MovePageDown(bool shift)
        {
            this.MovePageDown(shift, true);
        }

        private void MovePageDown(bool shift, bool goToLastChar)
        {
            if (this.hasSelection && !shift)
            {
                // If we have a selection and press down without shift,
                // set caret to end of selection before we move it down.
                this.caretPositionInternal = this.caretSelectPositionInternal =
                    Mathf.Max(this.caretPositionInternal, this.caretSelectPositionInternal);
            }

            int position = this.multiLine
                ? this.PageDownCharacterPosition(this.caretSelectPositionInternal, goToLastChar)
                : this.m_TextComponent.textInfo.characterCount - 1;

            if (shift)
            {
                this.caretSelectPositionInternal = position;
                this.stringSelectPositionInternal = this.GetStringIndexFromCaretPosition(this.caretSelectPositionInternal);
            }
            else
            {
                this.caretSelectPositionInternal = this.caretPositionInternal = position;
                this.stringSelectPositionInternal = this.stringPositionInternal =
                    this.GetStringIndexFromCaretPosition(this.caretSelectPositionInternal);
            }

            // Scroll to top of viewport
            //int currentLine = m_TextComponent.textInfo.characterInfo[position].lineNumber;
            //float lineAscender = m_TextComponent.textInfo.lineInfo[currentLine].ascender;

            // Adjust text area up or down if not in single line mode.
            if (this.m_LineType != LineType.SingleLine)
            {
                float
                    offset = this.m_TextViewport.rect
                        .height; // m_TextViewport.rect.yMax - (m_TextComponent.rectTransform.anchoredPosition.y + lineAscender);

                float bottomTextBounds = this.m_TextComponent.rectTransform.position.y + this.m_TextComponent.textBounds.min.y;
                float bottomViewportBounds = this.m_TextViewport.position.y + this.m_TextViewport.rect.yMin;

                offset = bottomViewportBounds > bottomTextBounds + offset
                    ? offset
                    : bottomViewportBounds - bottomTextBounds;

                this.m_TextComponent.rectTransform.anchoredPosition += new Vector2(0, offset);
                this.AssignPositioningIfNeeded();
            }

#if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
        }

        private void Delete()
        {
            if (this.m_ReadOnly)
                return;

            if (this.m_StringPosition == this.m_StringSelectPosition)
                return;

            if (this.m_isRichTextEditingAllowed || this.m_isSelectAll)
            {
                // Handling of Delete when Rich Text is allowed.
                if (this.m_StringPosition < this.m_StringSelectPosition)
                {
                    this.m_Text = this.text.Remove(this.m_StringPosition, this.m_StringSelectPosition - this.m_StringPosition);
                    this.m_StringSelectPosition = this.m_StringPosition;
                }
                else
                {
                    this.m_Text = this.text.Remove(this.m_StringSelectPosition, this.m_StringPosition - this.m_StringSelectPosition);
                    this.m_StringPosition = this.m_StringSelectPosition;
                }

                if (this.m_isSelectAll)
                {
                    this.m_CaretPosition = this.m_CaretSelectPosition = 0;
                    this.m_isSelectAll = false;
                }
            }
            else
            {
                if (this.m_CaretPosition < this.m_CaretSelectPosition)
                {
                    this.m_StringPosition = this.m_TextComponent.textInfo.characterInfo[this.m_CaretPosition].index;
                    this.m_StringSelectPosition = this.m_TextComponent.textInfo.characterInfo[this.m_CaretSelectPosition - 1].index +
                                                  this.m_TextComponent.textInfo.characterInfo[this.m_CaretSelectPosition - 1]
                                                      .stringLength;

                    this.m_Text = this.text.Remove(this.m_StringPosition, this.m_StringSelectPosition - this.m_StringPosition);

                    this.m_StringSelectPosition = this.m_StringPosition;
                    this.m_CaretSelectPosition = this.m_CaretPosition;
                }
                else
                {
                    this.m_StringPosition = this.m_TextComponent.textInfo.characterInfo[this.m_CaretPosition - 1].index +
                                            this.m_TextComponent.textInfo.characterInfo[this.m_CaretPosition - 1].stringLength;
                    this.m_StringSelectPosition = this.m_TextComponent.textInfo.characterInfo[this.m_CaretSelectPosition].index;

                    this.m_Text = this.text.Remove(this.m_StringSelectPosition, this.m_StringPosition - this.m_StringSelectPosition);

                    this.m_StringPosition = this.m_StringSelectPosition;
                    this.m_CaretPosition = this.m_CaretSelectPosition;
                }
            }

#if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
        }

        /// <summary>
        /// Handling of DEL key
        /// </summary>
        private void DeleteKey()
        {
            if (this.m_ReadOnly)
                return;

            if (this.hasSelection)
            {
                this.m_isLastKeyBackspace = true;

                this.Delete();
                this.UpdateTouchKeyboardFromEditChanges();
                this.SendOnValueChangedAndUpdateLabel();
            }
            else
            {
                if (this.m_isRichTextEditingAllowed)
                {
                    if (this.stringPositionInternal < this.text.Length)
                    {
                        // Special handling for Surrogate Pairs
                        if (char.IsHighSurrogate(this.text[this.stringPositionInternal]))
                            this.m_Text = this.text.Remove(this.stringPositionInternal, 2);
                        else
                            this.m_Text = this.text.Remove(this.stringPositionInternal, 1);

                        this.m_isLastKeyBackspace = true;

                        this.UpdateTouchKeyboardFromEditChanges();
                        this.SendOnValueChangedAndUpdateLabel();
                    }
                }
                else
                {
                    if (this.caretPositionInternal < this.m_TextComponent.textInfo.characterCount - 1)
                    {
                        int numberOfCharactersToRemove =
                            this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal].stringLength;

                        // Adjust string position to skip any potential rich text tags.
                        int nextCharacterStringPosition =
                            this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal].index;

                        this.m_Text = this.text.Remove(nextCharacterStringPosition, numberOfCharactersToRemove);

                        this.m_isLastKeyBackspace = true;

                        this.SendOnValueChangedAndUpdateLabel();
                    }
                }
            }

#if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
        }

        /// <summary>
        /// Handling of Backspace key
        /// </summary>
        private void Backspace()
        {
            if (this.m_ReadOnly)
                return;

            if (this.hasSelection)
            {
                this.m_isLastKeyBackspace = true;

                this.Delete();
                this.UpdateTouchKeyboardFromEditChanges();
                this.SendOnValueChangedAndUpdateLabel();
            }
            else
            {
                if (this.m_isRichTextEditingAllowed)
                {
                    if (this.stringPositionInternal > 0)
                    {
                        int numberOfCharactersToRemove = 1;

                        // Special handling for Surrogate pairs and Diacritical marks
                        if (char.IsLowSurrogate(this.text[this.stringPositionInternal - 1]))
                            numberOfCharactersToRemove = 2;

                        this.stringSelectPositionInternal = this.stringPositionInternal =
                            this.stringPositionInternal - numberOfCharactersToRemove;

                        this.m_Text = this.text.Remove(this.stringPositionInternal, numberOfCharactersToRemove);

                        this.caretSelectPositionInternal = this.caretPositionInternal = this.caretPositionInternal - 1;

                        this.m_isLastKeyBackspace = true;

                        this.UpdateTouchKeyboardFromEditChanges();
                        this.SendOnValueChangedAndUpdateLabel();
                    }
                }
                else
                {
                    if (this.caretPositionInternal > 0)
                    {
                        int numberOfCharactersToRemove = this.m_TextComponent.textInfo
                            .characterInfo[this.caretPositionInternal - 1].stringLength;

                        // Delete the previous character
                        this.m_Text = this.text.Remove(this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal - 1].index,
                            numberOfCharactersToRemove);

                        // Get new adjusted string position
                        this.stringSelectPositionInternal = this.stringPositionInternal = this.caretPositionInternal < 1
                            ? this.m_TextComponent.textInfo.characterInfo[0].index
                            : this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal - 1].index;

                        this.caretSelectPositionInternal = this.caretPositionInternal = this.caretPositionInternal - 1;
                    }

                    this.m_isLastKeyBackspace = true;

                    this.UpdateTouchKeyboardFromEditChanges();
                    this.SendOnValueChangedAndUpdateLabel();
                }
            }

#if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
        }


        /// <summary>
        /// Append the specified text to the end of the current.
        /// </summary>
        protected virtual void Append(string input)
        {
            if (this.m_ReadOnly)
                return;

            if (this.InPlaceEditing() == false)
                return;

            for (int i = 0, imax = input.Length; i < imax; ++i)
            {
                char c = input[i];

                if (c >= ' ' || c == '\t' || c == '\r' || c == 10 || c == '\n')
                {
                    this.Append(c);
                }
            }
        }

        protected virtual void Append(char input)
        {
            if (this.m_ReadOnly)
                return;

            if (this.InPlaceEditing() == false)
                return;

            // If we have an input validator, validate the input first
            int insertionPosition = Mathf.Min(this.stringPositionInternal, this.stringSelectPositionInternal);

            //Get the text based on selection for validation instead of whole text(case 1253193).
            var validateText = this.text;

            if (this.selectionFocusPosition != this.selectionAnchorPosition)
            {
                if (this.m_isRichTextEditingAllowed || this.m_isSelectAll)
                {
                    // Handling of Delete when Rich Text is allowed.
                    if (this.m_StringPosition < this.m_StringSelectPosition)
                        validateText = this.text.Remove(this.m_StringPosition, this.m_StringSelectPosition - this.m_StringPosition);
                    else
                        validateText = this.text.Remove(this.m_StringSelectPosition, this.m_StringPosition - this.m_StringSelectPosition);
                }
                else
                {
                    if (this.m_CaretPosition < this.m_CaretSelectPosition)
                    {
                        this.m_StringPosition = this.m_TextComponent.textInfo.characterInfo[this.m_CaretPosition].index;
                        this.m_StringSelectPosition =
                            this.m_TextComponent.textInfo.characterInfo[this.m_CaretSelectPosition - 1].index + this.m_TextComponent
                                .textInfo.characterInfo[this.m_CaretSelectPosition - 1].stringLength;

                        validateText = this.text.Remove(this.m_StringPosition, this.m_StringSelectPosition - this.m_StringPosition);
                    }
                    else
                    {
                        this.m_StringPosition = this.m_TextComponent.textInfo.characterInfo[this.m_CaretPosition - 1].index +
                                                this.m_TextComponent.textInfo.characterInfo[this.m_CaretPosition - 1].stringLength;
                        this.m_StringSelectPosition = this.m_TextComponent.textInfo.characterInfo[this.m_CaretSelectPosition].index;

                        validateText = this.text.Remove(this.m_StringSelectPosition, this.m_StringPosition - this.m_StringSelectPosition);
                    }
                }
            }

            if (this.onValidateInput != null)
            {
                input = this.onValidateInput(validateText, insertionPosition, input);
            }
            else if (this.characterValidation == CharacterValidation.CustomValidator)
            {
                input = this.Validate(validateText, insertionPosition, input);

                if (input == 0) return;

                this.SendOnValueChanged();
                this.UpdateLabel();

                return;
            }
            else if (this.characterValidation != CharacterValidation.None)
            {
                input = this.Validate(validateText, insertionPosition, input);
            }

            // If the input is invalid, skip it
            if (input == 0)
                return;

            // Append the character and update the label
            this.Insert(input);
        }


        // Insert the character and update the label.
        private void Insert(char c)
        {
            if (this.m_ReadOnly)
                return;

            //Debug.Log("Inserting character " + m_IsCompositionActive);

            string replaceString = c.ToString();
            this.Delete();

            // Can't go past the character limit
            if (this.characterLimit > 0 && this.text.Length >= this.characterLimit)
                return;

            this.m_Text = this.text.Insert(this.m_StringPosition, replaceString);

            if (!char.IsHighSurrogate(c))
                this.m_CaretSelectPosition = this.m_CaretPosition += 1;

            this.m_StringSelectPosition = this.m_StringPosition += 1;

            this.UpdateTouchKeyboardFromEditChanges();
            this.SendOnValueChanged();

#if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
#endif
        }

        private void UpdateTouchKeyboardFromEditChanges()
        {
            // Update the TouchKeyboard's text from edit changes
            // if in-place editing is allowed
            if (this.m_SoftKeyboard != null && this.InPlaceEditing())
            {
                this.m_SoftKeyboard.text = this.m_Text;
            }
        }

        private void SendOnValueChangedAndUpdateLabel()
        {
            this.UpdateLabel();
            this.SendOnValueChanged();
        }

        private void SendOnValueChanged()
        {
            if (this.onValueChanged != null)
                this.onValueChanged.Invoke(this.text);
        }

        /// <summary>
        /// Submit the input field's text.
        /// </summary>
        protected void SendOnEndEdit()
        {
            if (this.onEndEdit != null)
                this.onEndEdit.Invoke(this.m_Text);
        }

        protected void SendOnSubmit()
        {
            if (this.onSubmit != null)
                this.onSubmit.Invoke(this.m_Text);
        }

        protected void SendOnFocus()
        {
            if (this.onSelect != null)
                this.onSelect.Invoke(this.m_Text);
        }

        protected void SendOnFocusLost()
        {
            if (this.onDeselect != null)
                this.onDeselect.Invoke(this.m_Text);
        }

        protected void SendOnTextSelection()
        {
            this.m_isSelected = true;

            if (this.onTextSelection != null)
                this.onTextSelection.Invoke(this.m_Text, this.stringPositionInternal, this.stringSelectPositionInternal);
        }

        protected void SendOnEndTextSelection()
        {
            if (!this.m_isSelected) return;

            if (this.onEndTextSelection != null)
                this.onEndTextSelection.Invoke(this.m_Text, this.stringPositionInternal, this.stringSelectPositionInternal);

            this.m_isSelected = false;
        }

        protected void SendTouchScreenKeyboardStatusChanged()
        {
            if (this.onTouchScreenKeyboardStatusChanged != null)
                this.onTouchScreenKeyboardStatusChanged.Invoke(this.m_SoftKeyboard.status);
        }


        /// <summary>
        /// Update the visual text Text.
        /// </summary>
        protected void UpdateLabel()
        {
            if (this.m_TextComponent != null && this.m_TextComponent.font != null && this.m_PreventCallback == false)
            {
                // Prevent callback from the text component as we assign new text. This is to prevent a recursive call.
                this.m_PreventCallback = true;

                string fullText;
                if (this.compositionLength > 0 && this.m_ReadOnly == false)
                {
                    //Input.imeCompositionMode = IMECompositionMode.On;

                    // Handle selections
                    this.Delete();

                    if (this.m_RichText)
                        fullText = this.text.Substring(0, this.m_StringPosition) + "<u>" + this.compositionString + "</u>" +
                                   this.text.Substring(this.m_StringPosition);
                    else
                        fullText = this.text.Substring(0, this.m_StringPosition) + this.compositionString +
                                   this.text.Substring(this.m_StringPosition);

                    this.m_IsCompositionActive = true;

                    //Debug.Log("[" + Time.frameCount + "] Handling IME Input");
                }
                else
                {
                    fullText = this.text;
                    this.m_IsCompositionActive = false;
                    this.m_ShouldUpdateIMEWindowPosition = true;
                }

                //Debug.Log("Handling IME Input... [" + compositionString + "] of length [" + compositionLength + "] at StringPosition [" + m_StringPosition + "]  IsActive [" + m_IsCompositionActive + "]");

                string processed;
                if (this.inputType == InputType.Password)
                    processed = new string(this.asteriskChar, fullText.Length);
                else
                    processed = fullText;

                bool isEmpty = string.IsNullOrEmpty(fullText);

                if (this.m_Placeholder != null)
                    this.m_Placeholder.enabled = isEmpty;

                if (!isEmpty && this.m_ReadOnly == false)
                {
                    this.SetCaretVisible();
                }

                this.m_TextComponent.text = processed + "\u200B"; // Extra space is added for Caret tracking.

                // Rebuild layout if using Layout components.
                if (this.m_IsDrivenByLayoutComponents)
                    LayoutRebuilder.MarkLayoutForRebuild(this.m_RectTransform);

                // Special handling to limit the number of lines of text in the Input Field.
                if (this.m_LineLimit > 0)
                {
                    this.m_TextComponent.ForceMeshUpdate();

                    TMP_TextInfo textInfo = this.m_TextComponent.textInfo;

                    // Check if text exceeds maximum number of lines.
                    if (textInfo != null && textInfo.lineCount > this.m_LineLimit)
                    {
                        int lastValidCharacterIndex = textInfo.lineInfo[this.m_LineLimit - 1].lastCharacterIndex;
                        int characterStringIndex = textInfo.characterInfo[lastValidCharacterIndex].index +
                                                   textInfo.characterInfo[lastValidCharacterIndex].stringLength;
                        this.text = processed.Remove(characterStringIndex, processed.Length - characterStringIndex);
                        this.m_TextComponent.text = this.text + "\u200B";
                    }
                }

                if (this.m_IsTextComponentUpdateRequired || this.m_VerticalScrollbar)
                {
                    this.m_IsTextComponentUpdateRequired = false;
                    this.m_TextComponent.ForceMeshUpdate();
                }

                this.MarkGeometryAsDirty();

                this.m_PreventCallback = false;
            }
        }


        void UpdateScrollbar()
        {
            // Update Scrollbar
            if (this.m_VerticalScrollbar)
            {
                Rect viewportRect = this.m_TextViewport.rect;

                float size = viewportRect.height / this.m_TextComponent.preferredHeight;

                this.m_VerticalScrollbar.size = size;

                this.m_VerticalScrollbar.value = this.GetScrollPositionRelativeToViewport();

                //Debug.Log(GetInstanceID() + "- UpdateScrollbar() - Updating Scrollbar... Value: " + m_VerticalScrollbar.value);
            }
        }


        /// <summary>
        /// Function to update the vertical position of the text container when OnValueChanged event is received from the Scrollbar.
        /// </summary>
        /// <param name="value"></param>
        void OnScrollbarValueChange(float value)
        {
            //if (m_IsUpdatingScrollbarValues)
            //{
            //    m_IsUpdatingScrollbarValues = false;
            //    return;
            //}

            if (value < 0 || value > 1) return;

            this.AdjustTextPositionRelativeToViewport(value);

            this.m_ScrollPosition = value;

            this.scrollAction?.Invoke(0);

            Debug.Log(this.GetInstanceID() + "- OnScrollbarValueChange() - Scrollbar value is: " + value + "  Transform POS: " + this.m_TextComponent.rectTransform.anchoredPosition);
        }

        void UpdateMaskRegions()
        {
            // TODO: Figure out a better way to handle adding an offset to the masking region
            // This region is defined by the RectTransform of the GameObject that contains the RectMask2D component.
            /*
            // Update Masking Region
            if (m_TextViewportRectMask != null)
            {
                Rect viewportRect = m_TextViewportRectMask.canvasRect;

                if (viewportRect != m_CachedViewportRect)
                {
                    m_CachedViewportRect = viewportRect;

                    viewportRect.min -= m_TextViewport.offsetMin * 0.5f;
                    viewportRect.max -= m_TextViewport.offsetMax * 0.5f;

                    if (m_CachedInputRenderer != null)
                        m_CachedInputRenderer.EnableRectClipping(viewportRect);

                    if (m_TextComponent.canvasRenderer != null)
                        m_TextComponent.canvasRenderer.EnableRectClipping(viewportRect);

                    if (m_Placeholder != null && m_Placeholder.enabled)
                        m_Placeholder.canvasRenderer.EnableRectClipping(viewportRect);
                }
            }
            */
        }

        /// <summary>
        /// Adjusts the relative position of the body of the text relative to the viewport.
        /// </summary>
        /// <param name="relativePosition"></param>
        void AdjustTextPositionRelativeToViewport(float relativePosition)
        {
            if (this.m_TextViewport == null)
                return;

            TMP_TextInfo textInfo = this.m_TextComponent.textInfo;

            // Check to make sure we have valid data and lines to query.
            if (textInfo == null || textInfo.lineInfo == null || textInfo.lineCount == 0 ||
                textInfo.lineCount > textInfo.lineInfo.Length) return;

            float verticalAlignmentOffset = 0;
            float textHeight = this.m_TextComponent.preferredHeight;

            switch (this.m_TextComponent.verticalAlignment)
            {
                case VerticalAlignmentOptions.Top:
                    verticalAlignmentOffset = 0;
                    break;
                case VerticalAlignmentOptions.Middle:
                    verticalAlignmentOffset = 0.5f;
                    break;
                case VerticalAlignmentOptions.Bottom:
                    verticalAlignmentOffset = 1.0f;
                    break;
                case VerticalAlignmentOptions.Baseline:
                    break;
                case VerticalAlignmentOptions.Geometry:
                    verticalAlignmentOffset = 0.5f;
                    textHeight = this.m_TextComponent.bounds.size.y;
                    break;
                case VerticalAlignmentOptions.Capline:
                    verticalAlignmentOffset = 0.5f;
                    break;
            }

            this.m_TextComponent.rectTransform.anchoredPosition = new Vector2(
                this.m_TextComponent.rectTransform.anchoredPosition.x,
                (textHeight - this.m_TextViewport.rect.height) * (relativePosition - verticalAlignmentOffset));

            this.AssignPositioningIfNeeded();

            //Debug.Log("Text height: " + m_TextComponent.preferredHeight + "  Viewport height: " + m_TextViewport.rect.height + "  Adjusted RectTransform anchordedPosition:" + m_TextComponent.rectTransform.anchoredPosition + "  Text Bounds: " + m_TextComponent.bounds.ToString("f3"));
        }


        private int GetCaretPositionFromStringIndex(int stringIndex)
        {
            int count = this.m_TextComponent.textInfo.characterCount;

            for (int i = 0; i < count; i++)
            {
                if (this.m_TextComponent.textInfo.characterInfo[i].index >= stringIndex)
                    return i;
            }

            return count;
        }

        /// <summary>
        /// Returns / places the caret before the given character at the string index.
        /// </summary>
        /// <param name="stringIndex"></param>
        /// <returns></returns>
        private int GetMinCaretPositionFromStringIndex(int stringIndex)
        {
            int count = this.m_TextComponent.textInfo.characterCount;

            for (int i = 0; i < count; i++)
            {
                if (stringIndex < this.m_TextComponent.textInfo.characterInfo[i].index +
                    this.m_TextComponent.textInfo.characterInfo[i].stringLength)
                    return i;
            }

            return count;
        }

        /// <summary>
        /// Returns / places the caret after the given character at the string index.
        /// </summary>
        /// <param name="stringIndex"></param>
        /// <returns></returns>
        private int GetMaxCaretPositionFromStringIndex(int stringIndex)
        {
            int count = this.m_TextComponent.textInfo.characterCount;

            for (int i = 0; i < count; i++)
            {
                if (this.m_TextComponent.textInfo.characterInfo[i].index >= stringIndex)
                    return i;
            }

            return count;
        }

        private int GetStringIndexFromCaretPosition(int caretPosition)
        {
            // Clamp values between 0 and character count.
            this.ClampCaretPos(ref caretPosition);

            return this.m_TextComponent.textInfo.characterInfo[caretPosition].index;
        }


        public void ForceLabelUpdate()
        {
            this.UpdateLabel();
        }

        private void MarkGeometryAsDirty()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
                return;
#endif

            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        public virtual void Rebuild(CanvasUpdate update)
        {
            switch (update)
            {
                case CanvasUpdate.LatePreRender:
                    this.UpdateGeometry();
                    break;
            }
        }

        public virtual void LayoutComplete()
        {
        }

        public virtual void GraphicUpdateComplete()
        {
        }

        private void UpdateGeometry()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            // No need to draw a cursor on mobile as its handled by the devices keyboard.
            if (this.InPlaceEditing() == false)
                return;

            if (this.m_CachedInputRenderer == null)
                return;

            this.OnFillVBO(this.mesh);

            this.m_CachedInputRenderer.SetMesh(this.mesh);
        }


        /// <summary>
        /// Method to keep the Caret RectTransform properties in sync with the text object's RectTransform
        /// </summary>
        private void AssignPositioningIfNeeded()
        {
            if (this.m_TextComponent != null && this.caretRectTrans != null &&
                (this.caretRectTrans.localPosition != this.m_TextComponent.rectTransform.localPosition ||
                 this.caretRectTrans.localRotation != this.m_TextComponent.rectTransform.localRotation ||
                 this.caretRectTrans.localScale != this.m_TextComponent.rectTransform.localScale ||
                 this.caretRectTrans.anchorMin != this.m_TextComponent.rectTransform.anchorMin ||
                 this.caretRectTrans.anchorMax != this.m_TextComponent.rectTransform.anchorMax ||
                 this.caretRectTrans.anchoredPosition != this.m_TextComponent.rectTransform.anchoredPosition ||
                 this.caretRectTrans.sizeDelta != this.m_TextComponent.rectTransform.sizeDelta ||
                 this.caretRectTrans.pivot != this.m_TextComponent.rectTransform.pivot))
            {
                this.caretRectTrans.localPosition = this.m_TextComponent.rectTransform.localPosition;
                this.caretRectTrans.localRotation = this.m_TextComponent.rectTransform.localRotation;
                this.caretRectTrans.localScale = this.m_TextComponent.rectTransform.localScale;
                this.caretRectTrans.anchorMin = this.m_TextComponent.rectTransform.anchorMin;
                this.caretRectTrans.anchorMax = this.m_TextComponent.rectTransform.anchorMax;
                this.caretRectTrans.anchoredPosition = this.m_TextComponent.rectTransform.anchoredPosition;
                this.caretRectTrans.sizeDelta = this.m_TextComponent.rectTransform.sizeDelta;
                this.caretRectTrans.pivot = this.m_TextComponent.rectTransform.pivot;
            }
        }


        private void OnFillVBO(Mesh vbo)
        {
            using (var helper = new VertexHelper())
            {
                if (!this.isFocused && !this.m_SelectionStillActive)
                {
                    helper.FillMesh(vbo);
                    return;
                }

                if (this.m_IsStringPositionDirty)
                {
                    this.stringPositionInternal = this.GetStringIndexFromCaretPosition(this.m_CaretPosition);
                    this.stringSelectPositionInternal = this.GetStringIndexFromCaretPosition(this.m_CaretSelectPosition);
                    this.m_IsStringPositionDirty = false;
                }

                if (this.m_IsCaretPositionDirty)
                {
                    this.caretPositionInternal = this.GetCaretPositionFromStringIndex(this.stringPositionInternal);
                    this.caretSelectPositionInternal = this.GetCaretPositionFromStringIndex(this.stringSelectPositionInternal);
                    this.m_IsCaretPositionDirty = false;
                }

                if (!this.hasSelection)
                {
                    this.GenerateCaret(helper, Vector2.zero);
                    this.SendOnEndTextSelection();
                }
                else
                {
                    this.GenerateHightlight(helper, Vector2.zero);
                    this.SendOnTextSelection();
                }

                helper.FillMesh(vbo);
            }
        }


        private void GenerateCaret(VertexHelper vbo, Vector2 roundingOffset)
        {
            if (this.m_CaretVisible == false || this.m_TextComponent.canvas == null || this.m_ReadOnly)
                return;

            if (this.m_CursorVerts == null)
            {
                this.CreateCursorVerts();
            }

            float width = this.m_CaretWidth;

            // TODO: Optimize to only update the caret position when needed.

            Vector2 startPosition = Vector2.zero;
            float height = 0;
            TMP_CharacterInfo currentCharacter;

            // Make sure caret position does not exceed characterInfo array size.
            if (this.caretPositionInternal >= this.m_TextComponent.textInfo.characterInfo.Length)
                return;

            int currentLine = this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal].lineNumber;

            // Caret is positioned at the origin for the first character of each lines and at the advance for subsequent characters.
            if (this.caretPositionInternal == this.m_TextComponent.textInfo.lineInfo[currentLine].firstCharacterIndex)
            {
                currentCharacter = this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal];
                height = currentCharacter.ascender - currentCharacter.descender;

                if (this.m_TextComponent.verticalAlignment == VerticalAlignmentOptions.Geometry)
                    startPosition = new Vector2(currentCharacter.origin, 0 - height / 2);
                else
                    startPosition = new Vector2(currentCharacter.origin, currentCharacter.descender);
            }
            else
            {
                currentCharacter = this.m_TextComponent.textInfo.characterInfo[this.caretPositionInternal - 1];
                height = currentCharacter.ascender - currentCharacter.descender;

                if (this.m_TextComponent.verticalAlignment == VerticalAlignmentOptions.Geometry)
                    startPosition = new Vector2(currentCharacter.xAdvance, 0 - height / 2);
                else
                    startPosition = new Vector2(currentCharacter.xAdvance, currentCharacter.descender);
            }

            if (this.m_SoftKeyboard != null)
            {
                int selectionStart = this.m_StringPosition;
                int softKeyboardStringLength = this.m_SoftKeyboard.text == null ? 0 : this.m_SoftKeyboard.text.Length;

                if (selectionStart < 0)
                    selectionStart = 0;

                if (selectionStart > softKeyboardStringLength)
                    selectionStart = softKeyboardStringLength;

                this.m_SoftKeyboard.selection = new RangeInt(selectionStart, 0);
            }

            // Adjust the position of the RectTransform based on the caret position in the viewport (only if we have focus).
            if (this.isFocused && startPosition != this.m_LastPosition || this.m_forceRectTransformAdjustment || this.m_isLastKeyBackspace)
                this.AdjustRectTransformRelativeToViewport(startPosition, height, currentCharacter.isVisible);

            this.m_LastPosition = startPosition;

            // Clamp Caret height
            float top = startPosition.y + height;
            float bottom = top - height;

            // Minor tweak to address caret potentially being too thin based on canvas scaler values.
            float scale = this.m_TextComponent.canvas.scaleFactor;

            this.m_CursorVerts[0].position = new Vector3(startPosition.x, bottom, 0.0f);
            this.m_CursorVerts[1].position = new Vector3(startPosition.x, top, 0.0f);
            this.m_CursorVerts[2].position = new Vector3(startPosition.x + width, top, 0.0f);
            this.m_CursorVerts[3].position = new Vector3(startPosition.x + width, bottom, 0.0f);

            // Set Vertex Color for the caret color.
            this.m_CursorVerts[0].color = this.caretColor;
            this.m_CursorVerts[1].color = this.caretColor;
            this.m_CursorVerts[2].color = this.caretColor;
            this.m_CursorVerts[3].color = this.caretColor;

            vbo.AddUIVertexQuad(this.m_CursorVerts);

            // Update position of IME window when necessary.
            if (this.m_ShouldUpdateIMEWindowPosition || currentLine != this.m_PreviousIMEInsertionLine)
            {
                this.m_ShouldUpdateIMEWindowPosition = false;
                this.m_PreviousIMEInsertionLine = currentLine;

                // Calculate position of IME Window in screen space.
                Camera cameraRef;
                if (this.m_TextComponent.canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    cameraRef = null;
                else
                {
                    cameraRef = this.m_TextComponent.canvas.worldCamera;

                    if (cameraRef == null)
                        cameraRef = Camera.current;
                }

                Vector3 cursorPosition =
                    this.m_CachedInputRenderer.gameObject.transform.TransformPoint(this.m_CursorVerts[0].position);
                Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(cameraRef, cursorPosition);
                screenPosition.y = Screen.height - screenPosition.y;

                if (this.inputSystem != null)
                    this.inputSystem.compositionCursorPos = screenPosition;

                //Debug.Log("[" + Time.frameCount + "] Updating IME Window position  Cursor Pos: (" + cursorPosition + ")  Screen Pos: (" + screenPosition + ") with Composition Length: " + compositionLength);
            }

            //#if TMP_DEBUG_MODE
            //Debug.Log("Caret position updated at frame: " + Time.frameCount);
            //#endif
        }


        private void CreateCursorVerts()
        {
            this.m_CursorVerts = new UIVertex[4];

            for (int i = 0; i < this.m_CursorVerts.Length; i++)
            {
                this.m_CursorVerts[i] = UIVertex.simpleVert;
                this.m_CursorVerts[i].uv0 = Vector2.zero;
            }
        }


        private void GenerateHightlight(VertexHelper vbo, Vector2 roundingOffset)
        {
            // Update Masking Region
            this.UpdateMaskRegions();

            // Make sure caret position does not exceed characterInfo array size.
            //if (caretSelectPositionInternal >= m_TextComponent.textInfo.characterInfo.Length)
            //    return;

            TMP_TextInfo textInfo = this.m_TextComponent.textInfo;

            this.m_CaretPosition = this.GetCaretPositionFromStringIndex(this.stringPositionInternal);
            this.m_CaretSelectPosition = this.GetCaretPositionFromStringIndex(this.stringSelectPositionInternal);

            if (this.m_SoftKeyboard != null)
            {
                int stringPosition = this.m_CaretPosition < this.m_CaretSelectPosition
                    ? textInfo.characterInfo[this.m_CaretPosition].index
                    : textInfo.characterInfo[this.m_CaretSelectPosition].index;
                int length = this.m_CaretPosition < this.m_CaretSelectPosition
                    ? this.stringSelectPositionInternal - stringPosition
                    : this.stringPositionInternal - stringPosition;
                this.m_SoftKeyboard.selection = new RangeInt(stringPosition, length);
            }

            // Adjust text RectTranform position to make sure it is visible in viewport.
            Vector2 caretPosition;
            float height = 0;
            if (this.m_CaretSelectPosition < textInfo.characterCount)
            {
                caretPosition = new Vector2(textInfo.characterInfo[this.m_CaretSelectPosition].origin,
                    textInfo.characterInfo[this.m_CaretSelectPosition].descender);
                height = textInfo.characterInfo[this.m_CaretSelectPosition].ascender -
                         textInfo.characterInfo[this.m_CaretSelectPosition].descender;
            }
            else
            {
                caretPosition = new Vector2(textInfo.characterInfo[this.m_CaretSelectPosition - 1].xAdvance,
                    textInfo.characterInfo[this.m_CaretSelectPosition - 1].descender);
                height = textInfo.characterInfo[this.m_CaretSelectPosition - 1].ascender -
                         textInfo.characterInfo[this.m_CaretSelectPosition - 1].descender;
            }

            // TODO: Don't adjust the position of the RectTransform if Reset On Deactivation is disabled
            // and we just selected the Input Field again.
            this.AdjustRectTransformRelativeToViewport(caretPosition, height, true);

            int startChar = Mathf.Max(0, this.m_CaretPosition);
            int endChar = Mathf.Max(0, this.m_CaretSelectPosition);

            // Ensure pos is always less then selPos to make the code simpler
            if (startChar > endChar)
            {
                int temp = startChar;
                startChar = endChar;
                endChar = temp;
            }

            endChar -= 1;

            //Debug.Log("Updating Highlight... Caret Position: " + startChar + " Caret Select POS: " + endChar);


            int currentLineIndex = textInfo.characterInfo[startChar].lineNumber;
            int nextLineStartIdx = textInfo.lineInfo[currentLineIndex].lastCharacterIndex;

            UIVertex vert = UIVertex.simpleVert;
            vert.uv0 = Vector2.zero;
            vert.color = this.selectionColor;

            int currentChar = startChar;
            while (currentChar <= endChar && currentChar < textInfo.characterCount)
            {
                if (currentChar == nextLineStartIdx || currentChar == endChar)
                {
                    TMP_CharacterInfo startCharInfo = textInfo.characterInfo[startChar];
                    TMP_CharacterInfo endCharInfo = textInfo.characterInfo[currentChar];

                    // Extra check to handle Carriage Return
                    if (currentChar > 0 && endCharInfo.character == 10 &&
                        textInfo.characterInfo[currentChar - 1].character == 13)
                        endCharInfo = textInfo.characterInfo[currentChar - 1];

                    Vector2 startPosition =
                        new Vector2(startCharInfo.origin, textInfo.lineInfo[currentLineIndex].ascender);
                    Vector2 endPosition =
                        new Vector2(endCharInfo.xAdvance, textInfo.lineInfo[currentLineIndex].descender);

                    var startIndex = vbo.currentVertCount;
                    vert.position = new Vector3(startPosition.x, endPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vert.position = new Vector3(endPosition.x, endPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vert.position = new Vector3(endPosition.x, startPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vert.position = new Vector3(startPosition.x, startPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vbo.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                    vbo.AddTriangle(startIndex + 2, startIndex + 3, startIndex + 0);

                    startChar = currentChar + 1;
                    currentLineIndex++;

                    if (currentLineIndex < textInfo.lineCount)
                        nextLineStartIdx = textInfo.lineInfo[currentLineIndex].lastCharacterIndex;
                }

                currentChar++;
            }

            //#if TMP_DEBUG_MODE
            //    Debug.Log("Text selection updated at frame: " + Time.frameCount);
            //#endif
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="height"></param>
        /// <param name="isCharVisible"></param>
        private void AdjustRectTransformRelativeToViewport(Vector2 startPosition, float height, bool isCharVisible)
        {
            //Debug.Log("Adjusting transform position relative to viewport.");

            if (this.m_TextViewport == null)
                return;

            Vector3 localPosition = this.transform.localPosition;
            Vector3 textComponentLocalPosition = this.m_TextComponent.rectTransform.localPosition;
            Vector3 textViewportLocalPosition = this.m_TextViewport.localPosition;
            Rect textViewportRect = this.m_TextViewport.rect;

            Vector2 caretPosition =
                new Vector2(
                    startPosition.x + textComponentLocalPosition.x + textViewportLocalPosition.x + localPosition.x,
                    startPosition.y + textComponentLocalPosition.y + textViewportLocalPosition.y + localPosition.y);
            Rect viewportWSRect = new Rect(localPosition.x + textViewportLocalPosition.x + textViewportRect.x,
                localPosition.y + textViewportLocalPosition.y + textViewportRect.y, textViewportRect.width,
                textViewportRect.height);

            // Adjust the position of the RectTransform based on the caret position in the viewport.
            float rightOffset = viewportWSRect.xMax - (caretPosition.x + this.m_TextComponent.margin.z + this.m_CaretWidth);
            if (rightOffset < 0f)
            {
                if (!this.multiLine || (this.multiLine && isCharVisible))
                {
                    //Debug.Log("Shifting text to the LEFT by " + rightOffset.ToString("f3"));
                    this.m_TextComponent.rectTransform.anchoredPosition += new Vector2(rightOffset, 0);

                    this.AssignPositioningIfNeeded();
                }
            }

            float leftOffset = (caretPosition.x - this.m_TextComponent.margin.x) - viewportWSRect.xMin;
            if (leftOffset < 0f)
            {
                //Debug.Log("Shifting text to the RIGHT by " + leftOffset.ToString("f3"));
                this.m_TextComponent.rectTransform.anchoredPosition += new Vector2(-leftOffset, 0);
                this.AssignPositioningIfNeeded();
            }

            // Adjust text area up or down if not in single line mode.
            if (this.m_LineType != LineType.SingleLine)
            {
                float topOffset = viewportWSRect.yMax - (caretPosition.y + height);
                if (topOffset < -0.0001f)
                {
                    //Debug.Log("Shifting text to Up " + topOffset.ToString("f3"));
                    this.m_TextComponent.rectTransform.anchoredPosition += new Vector2(0, topOffset);
                    this.AssignPositioningIfNeeded();
                }

                float bottomOffset = caretPosition.y - viewportWSRect.yMin;
                if (bottomOffset < 0f)
                {
                    //Debug.Log("Shifting text to Down " + bottomOffset.ToString("f3"));
                    this.m_TextComponent.rectTransform.anchoredPosition -= new Vector2(0, bottomOffset);
                    this.AssignPositioningIfNeeded();
                }
            }

            // Special handling of backspace
            if (this.m_isLastKeyBackspace)
            {
                float anchoredPositionX = this.m_TextComponent.rectTransform.anchoredPosition.x;

                float firstCharPosition = localPosition.x + textViewportLocalPosition.x + textComponentLocalPosition.x +
                    this.m_TextComponent.textInfo.characterInfo[0].origin - this.m_TextComponent.margin.x;
                float lastCharPosition = localPosition.x + textViewportLocalPosition.x + textComponentLocalPosition.x +
                                         this.m_TextComponent.textInfo
                                             .characterInfo[this.m_TextComponent.textInfo.characterCount - 1].origin +
                                         this.m_TextComponent.margin.z + this.m_CaretWidth;

                if (anchoredPositionX > 0.0001f && firstCharPosition > viewportWSRect.xMin)
                {
                    float offset = viewportWSRect.xMin - firstCharPosition;

                    if (anchoredPositionX < -offset)
                        offset = -anchoredPositionX;

                    this.m_TextComponent.rectTransform.anchoredPosition += new Vector2(offset, 0);
                    this.AssignPositioningIfNeeded();
                }
                else if (anchoredPositionX < -0.0001f && lastCharPosition < viewportWSRect.xMax)
                {
                    float offset = viewportWSRect.xMax - lastCharPosition;

                    if (-anchoredPositionX < offset)
                        offset = -anchoredPositionX;

                    this.m_TextComponent.rectTransform.anchoredPosition += new Vector2(offset, 0);
                    this.AssignPositioningIfNeeded();
                }

                this.m_isLastKeyBackspace = false;
            }

            this.m_forceRectTransformAdjustment = false;
        }

        /// <summary>
        /// Validate the specified input.
        /// </summary>
        protected char Validate(string text, int pos, char ch)
        {
            // Validation is disabled
            if (this.characterValidation == CharacterValidation.None || !this.enabled)
                return ch;

            if (this.characterValidation == CharacterValidation.Integer ||
                this.characterValidation == CharacterValidation.Decimal)
            {
                // Integer and decimal
                bool cursorBeforeDash = (pos == 0 && text.Length > 0 && text[0] == '-');
                bool selectionAtStart = this.stringPositionInternal == 0 || this.stringSelectPositionInternal == 0;
                if (!cursorBeforeDash)
                {
                    if (ch >= '0' && ch <= '9') return ch;
                    if (ch == '-' && (pos == 0 || selectionAtStart)) return ch;

                    var separator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                    if (ch == Convert.ToChar(separator) && this.characterValidation == CharacterValidation.Decimal &&
                        !text.Contains(separator)) return ch;
                }
            }
            else if (this.characterValidation == CharacterValidation.Digit)
            {
                if (ch >= '0' && ch <= '9') return ch;
            }
            else if (this.characterValidation == CharacterValidation.Alphanumeric)
            {
                // All alphanumeric characters
                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
            }
            else if (this.characterValidation == CharacterValidation.Name)
            {
                char prevChar = (text.Length > 0) ? text[Mathf.Clamp(pos - 1, 0, text.Length - 1)] : ' ';
                char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';

                if (char.IsLetter(ch))
                {
                    // First letter is always capitalized
                    if (char.IsLower(ch) && pos == 0)
                        return char.ToUpper(ch);

                    // Letter following a space or hyphen is always capitalized
                    if (char.IsLower(ch) && (prevChar == ' ' || prevChar == '-'))
                        return char.ToUpper(ch);

                    // Uppercase letters are only allowed after spaces, apostrophes, hyphens or lowercase letter
                    if (char.IsUpper(ch) && pos > 0 && prevChar != ' ' && prevChar != '\'' && prevChar != '-' &&
                        !char.IsLower(prevChar))
                        return char.ToLower(ch);

                    // Do not allow uppercase characters to be inserted before another uppercase character
                    if (char.IsUpper(ch) && char.IsUpper(lastChar))
                        return (char)0;

                    // If character was already in correct case, return it as-is.
                    // Also, letters that are neither upper nor lower case are always allowed.
                    return ch;
                }
                else if (ch == '\'')
                {
                    // Don't allow more than one apostrophe
                    if (lastChar != ' ' && lastChar != '\'' && nextChar != '\'' && !text.Contains("'"))
                        return ch;
                }

                // Allow inserting a hyphen after a character
                if (char.IsLetter(prevChar) && ch == '-' && lastChar != '-')
                {
                    return ch;
                }

                if ((ch == ' ' || ch == '-') && pos != 0)
                {
                    // Don't allow more than one space in a row
                    if (prevChar != ' ' && prevChar != '\'' && prevChar != '-' &&
                        lastChar != ' ' && lastChar != '\'' && lastChar != '-' &&
                        nextChar != ' ' && nextChar != '\'' && nextChar != '-')
                        return ch;
                }
            }
            else if (this.characterValidation == CharacterValidation.EmailAddress)
            {
                // From StackOverflow about allowed characters in email addresses:
                // Uppercase and lowercase English letters (a-z, A-Z)
                // Digits 0 to 9
                // Characters ! # $ % & ' * + - / = ? ^ _ ` { | } ~
                // Character . (dot, period, full stop) provided that it is not the first or last character,
                // and provided also that it does not appear two or more times consecutively.

                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
                if (ch == '@' && text.IndexOf('@') == -1) return ch;
                if (kEmailSpecialCharacters.IndexOf(ch) != -1) return ch;
                if (ch == '.')
                {
                    char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                    char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';
                    if (lastChar != '.' && nextChar != '.')
                        return ch;
                }
            }
            else if (this.characterValidation == CharacterValidation.Regex)
            {
                // Regex expression
                if (Regex.IsMatch(ch.ToString(), this.m_RegexValue))
                {
                    return ch;
                }
            }
            else if (this.characterValidation == CharacterValidation.CustomValidator)
            {
                if (this.m_InputValidator != null)
                {
                    char c = this.m_InputValidator.Validate(ref text, ref pos, ch);
                    this.m_Text = text;
                    this.stringSelectPositionInternal = this.stringPositionInternal = pos;
                    return c;
                }
            }

            return (char)0;
        }

        public void ActivateInputField()
        {
            if (this.m_TextComponent == null || this.m_TextComponent.font == null || !this.IsActive() || !this.IsInteractable())
                return;

            if (this.isFocused)
            {
                if (this.m_SoftKeyboard != null && !this.m_SoftKeyboard.active)
                {
                    this.m_SoftKeyboard.active = true;
                    this.m_SoftKeyboard.text = this.m_Text;
                }
            }

            this.m_ShouldActivateNextUpdate = true;
        }

        private void ActivateInputFieldInternal()
        {
            if (EventSystem.current == null)
                return;

            if (EventSystem.current.currentSelectedGameObject != this.gameObject)
                EventSystem.current.SetSelectedGameObject(this.gameObject);

            if (TouchScreenKeyboard.isSupported && this.shouldHideSoftKeyboard == false)
            {
                if (this.inputSystem != null && this.inputSystem.touchSupported)
                {
                    TouchScreenKeyboard.hideInput = this.shouldHideMobileInput;
                }

                if (this.shouldHideSoftKeyboard == false && this.m_ReadOnly == false)
                {
                    this.m_SoftKeyboard = (this.inputType == InputType.Password)
                        ? TouchScreenKeyboard.Open(this.m_Text, this.keyboardType, false, this.multiLine, true, false, "",
                            this.characterLimit)
                        : TouchScreenKeyboard.Open(this.m_Text, this.keyboardType, this.inputType == InputType.AutoCorrect, this.multiLine,
                            false, false, "", this.characterLimit);

                    this.OnFocus();

                    // Opening the soft keyboard sets its selection to the end of the text.
                    // As such, we set the selection to match the Input Field's internal selection.
                    if (this.m_SoftKeyboard != null)
                    {
                        int length = this.stringPositionInternal < this.stringSelectPositionInternal
                            ? this.stringSelectPositionInternal - this.stringPositionInternal
                            : this.stringPositionInternal - this.stringSelectPositionInternal;
                        this.m_SoftKeyboard.selection =
                            new RangeInt(
                                this.stringPositionInternal < this.stringSelectPositionInternal
                                    ? this.stringPositionInternal
                                    : this.stringSelectPositionInternal, length);
                    }
                    //}
                }

                // Cache the value of isInPlaceEditingAllowed, because on UWP this involves calling into native code
                // The value only needs to be updated once when the TouchKeyboard is opened.
#if UNITY_2019_1_OR_NEWER
                this.m_TouchKeyboardAllowsInPlaceEditing = TouchScreenKeyboard.isInPlaceEditingAllowed;
#endif
            }
            else
            {
                if (!TouchScreenKeyboard.isSupported && this.m_ReadOnly == false && this.inputSystem != null)
                    this.inputSystem.imeCompositionMode = IMECompositionMode.On;

                this.OnFocus();
            }

            this.m_AllowInput = true;
            this.m_OriginalText = this.text;
            this.m_WasCanceled = false;
            this.SetCaretVisible();
            this.UpdateLabel();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            //Debug.Log("OnSelect()");

            base.OnSelect(eventData);
            this.SendOnFocus();

            this.ActivateInputField();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            //Debug.Log("Pointer Click Event...");

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            this.ActivateInputField();
        }

        public void OnControlClick()
        {
            //Debug.Log("Input Field control click...");
        }

        public void ReleaseSelection()
        {
            this.m_SelectionStillActive = false;
            this.m_ReleaseSelection = false;
            this.m_PreviouslySelectedObject = null;

            this.MarkGeometryAsDirty();

            this.SendOnEndEdit();
            this.SendOnEndTextSelection();
        }

        public void DeactivateInputField(bool clearSelection = false)
        {
            //Debug.Log("Deactivate Input Field...");

            // Not activated do nothing.
            if (!this.m_AllowInput)
                return;

            this.m_HasDoneFocusTransition = false;
            this.m_AllowInput = false;

            if (this.m_Placeholder != null)
                this.m_Placeholder.enabled = string.IsNullOrEmpty(this.m_Text);

            if (this.m_TextComponent != null && this.IsInteractable())
            {
                if (this.m_WasCanceled && this.m_RestoreOriginalTextOnEscape)
                    this.text = this.m_OriginalText;

                if (this.m_SoftKeyboard != null)
                {
                    this.m_SoftKeyboard.active = false;
                    this.m_SoftKeyboard = null;
                }

                this.m_SelectionStillActive = true;

                if (this.m_ResetOnDeActivation || this.m_ReleaseSelection)
                {
                    //m_StringPosition = m_StringSelectPosition = 0;
                    //m_CaretPosition = m_CaretSelectPosition = 0;
                    //m_TextComponent.rectTransform.localPosition = m_DefaultTransformPosition;

                    if (this.m_VerticalScrollbar == null)
                        this.ReleaseSelection();
                }

                if (this.inputSystem != null)
                    this.inputSystem.imeCompositionMode = IMECompositionMode.Auto;
            }

            this.MarkGeometryAsDirty();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            this.DeactivateInputField();

            base.OnDeselect(eventData);
            this.SendOnFocusLost();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            //Debug.Log("OnSubmit()");

            if (!this.IsActive() || !this.IsInteractable())
                return;

            if (!this.isFocused)
                this.m_ShouldActivateNextUpdate = true;

            this.SendOnSubmit();
        }

        //public virtual void OnLostFocus(BaseEventData eventData)
        //{
        //    if (!IsActive() || !IsInteractable())
        //        return;
        //}

        private void EnforceContentType()
        {
            switch (this.contentType)
            {
                case ContentType.Standard:
                {
                    // Don't enforce line type for this content type.
                    this.m_InputType = InputType.Standard;
                    this.m_KeyboardType = TouchScreenKeyboardType.Default;
                    this.m_CharacterValidation = CharacterValidation.None;
                    break;
                }
                case ContentType.Autocorrected:
                {
                    // Don't enforce line type for this content type.
                    this.m_InputType = InputType.AutoCorrect;
                    this.m_KeyboardType = TouchScreenKeyboardType.Default;
                    this.m_CharacterValidation = CharacterValidation.None;
                    break;
                }
                case ContentType.IntegerNumber:
                {
                    this.m_LineType = LineType.SingleLine;
                    this.m_InputType = InputType.Standard;
                    this.m_KeyboardType = TouchScreenKeyboardType.NumberPad;
                    this.m_CharacterValidation = CharacterValidation.Integer;
                    break;
                }
                case ContentType.DecimalNumber:
                {
                    this.m_LineType = LineType.SingleLine;
                    this.m_InputType = InputType.Standard;
                    this.m_KeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
                    this.m_CharacterValidation = CharacterValidation.Decimal;
                    break;
                }
                case ContentType.Alphanumeric:
                {
                    this.m_LineType = LineType.SingleLine;
                    this.m_InputType = InputType.Standard;
                    this.m_KeyboardType = TouchScreenKeyboardType.ASCIICapable;
                    this.m_CharacterValidation = CharacterValidation.Alphanumeric;
                    break;
                }
                case ContentType.Name:
                {
                    this.m_LineType = LineType.SingleLine;
                    this.m_InputType = InputType.Standard;
                    this.m_KeyboardType = TouchScreenKeyboardType.Default;
                    this.m_CharacterValidation = CharacterValidation.Name;
                    break;
                }
                case ContentType.EmailAddress:
                {
                    this.m_LineType = LineType.SingleLine;
                    this.m_InputType = InputType.Standard;
                    this.m_KeyboardType = TouchScreenKeyboardType.EmailAddress;
                    this.m_CharacterValidation = CharacterValidation.EmailAddress;
                    break;
                }
                case ContentType.Password:
                {
                    this.m_LineType = LineType.SingleLine;
                    this.m_InputType = InputType.Password;
                    this.m_KeyboardType = TouchScreenKeyboardType.Default;
                    this.m_CharacterValidation = CharacterValidation.None;
                    break;
                }
                case ContentType.Pin:
                {
                    this.m_LineType = LineType.SingleLine;
                    this.m_InputType = InputType.Password;
                    this.m_KeyboardType = TouchScreenKeyboardType.NumberPad;
                    this.m_CharacterValidation = CharacterValidation.Digit;
                    break;
                }
                default:
                {
                    // Includes Custom type. Nothing should be enforced.
                    break;
                }
            }

            this.SetTextComponentWrapMode();
        }

        void SetTextComponentWrapMode()
        {
            if (this.m_TextComponent == null)
                return;

            if (this.multiLine)
                this.m_TextComponent.enableWordWrapping = true;
            else
                this.m_TextComponent.enableWordWrapping = false;
        }

        // Control Rich Text option on the text component.
        void SetTextComponentRichTextMode()
        {
            if (this.m_TextComponent == null)
                return;

            this.m_TextComponent.richText = this.m_RichText;
        }

        void SetToCustomIfContentTypeIsNot(params ContentType[] allowedContentTypes)
        {
            if (this.contentType == ContentType.Custom)
                return;

            for (int i = 0; i < allowedContentTypes.Length; i++)
                if (this.contentType == allowedContentTypes[i])
                    return;

            this.contentType = ContentType.Custom;
        }

        void SetToCustom()
        {
            if (this.contentType == ContentType.Custom)
                return;

            this.contentType = ContentType.Custom;
        }

        void SetToCustom(CharacterValidation characterValidation)
        {
            if (this.contentType == ContentType.Custom)
            {
                characterValidation = CharacterValidation.CustomValidator;
                return;
            }

            this.contentType = ContentType.Custom;
            characterValidation = CharacterValidation.CustomValidator;
        }


        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (this.m_HasDoneFocusTransition)
                state = SelectionState.Selected;
            else if (state == SelectionState.Pressed)
                this.m_HasDoneFocusTransition = true;

            base.DoStateTransition(state, instant);
        }


        /// <summary>
        /// See ILayoutElement.CalculateLayoutInputHorizontal.
        /// </summary>
        public virtual void CalculateLayoutInputHorizontal()
        {
        }

        /// <summary>
        /// See ILayoutElement.CalculateLayoutInputVertical.
        /// </summary>
        public virtual void CalculateLayoutInputVertical()
        {
        }

        /// <summary>
        /// See ILayoutElement.minWidth.
        /// </summary>
        public virtual float minWidth
        {
            get { return 0; }
        }

        /// <summary>
        /// Get the displayed with of all input characters.
        /// </summary>
        public virtual float preferredWidth
        {
            get
            {
                if (this.textComponent == null)
                    return 0;

                float horizontalPadding = 0;

                if (this.m_LayoutGroup != null)
                    horizontalPadding = this.m_LayoutGroup.padding.horizontal;

                if (this.m_TextViewport != null)
                    horizontalPadding += this.m_TextViewport.offsetMin.x - this.m_TextViewport.offsetMax.x;

                return this.m_TextComponent.preferredWidth + horizontalPadding; // Should add some extra padding for caret
            }
        }

        /// <summary>
        /// See ILayoutElement.flexibleWidth.
        /// </summary>
        public virtual float flexibleWidth
        {
            get { return -1; }
        }

        /// <summary>
        /// See ILayoutElement.minHeight.
        /// </summary>
        public virtual float minHeight
        {
            get { return 0; }
        }

        /// <summary>
        /// Get the height of all the text if constrained to the height of the RectTransform.
        /// </summary>
        public virtual float preferredHeight
        {
            get
            {
                if (this.textComponent == null)
                    return 0;

                float verticalPadding = 0;

                if (this.m_LayoutGroup != null)
                    verticalPadding = this.m_LayoutGroup.padding.vertical;

                if (this.m_TextViewport != null)
                    verticalPadding += this.m_TextViewport.offsetMin.y - this.m_TextViewport.offsetMax.y;

                return this.m_TextComponent.preferredHeight + verticalPadding;
            }
        }

        /// <summary>
        /// See ILayoutElement.flexibleHeight.
        /// </summary>
        public virtual float flexibleHeight
        {
            get { return -1; }
        }

        /// <summary>
        /// See ILayoutElement.layoutPriority.
        /// </summary>
        public virtual int layoutPriority
        {
            get { return 1; }
        }


        /// <summary>
        /// Function to conveniently set the point size of both Placeholder and Input Field text object.
        /// </summary>
        /// <param name="pointSize"></param>
        public void SetGlobalPointSize(float pointSize)
        {
            TMP_Text placeholderTextComponent = this.m_Placeholder as TMP_Text;

            if (placeholderTextComponent != null) placeholderTextComponent.fontSize = pointSize;
            this.textComponent.fontSize = pointSize;
        }

        /// <summary>
        /// Function to conveniently set the Font Asset of both Placeholder and Input Field text object.
        /// </summary>
        /// <param name="fontAsset"></param>
        public void SetGlobalFontAsset(TMP_FontAsset fontAsset)
        {
            TMP_Text placeholderTextComponent = this.m_Placeholder as TMP_Text;

            if (placeholderTextComponent != null) placeholderTextComponent.font = fontAsset;
            this.textComponent.font = fontAsset;
        }
    }


    static class SetPropertyUtility
    {
        public static bool SetColor(ref Color currentValue, Color newValue)
        {
            if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b &&
                currentValue.a == newValue.a)
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetEquatableStruct<T>(ref T currentValue, T newValue) where T : IEquatable<T>
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }
    }
}