namespace Projects.Buggary.BuggaryEditor.Management
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using Commons.CommonHelpers;
    using Contracts;
    using Infrastructure;
    using Microsoft.CodeAnalysis;
    using Models;
    using PersistenceModule;
    using PersistenceModule.DataClasses;
    using Roslyn;
    using Roslyn.Compilation;
    using SchwiftyUI.V3.Containers;
    using SchwiftyUI.V3.Elements;
    using SchwiftyUI.V3.Inputs;
    using TextEditors.Console;
    using TextEditors.OpenEditor;
    using TMPro;
    using UI;
    using UnityEngine;

    public class Buggary : MonoBehaviour
    {
        [SerializeField] public RectTransform editorRectTransformExternal;
        [SerializeField] public RectTransform consoleRectTransformExternal;
        [SerializeField] public RectTransform defaultEditorRoot;
        [SerializeField] public bool usePassedRTs;
        [SerializeField] public BuggaryColors colors;
        [SerializeField] public OpenEditor openEditor;
        [SerializeField] public ITextEditor fancyEditor;
        [SerializeField] public TMP_FontAsset fontAsset;
        [SerializeField] public bool useOpenEditor;
        [SerializeField] public RectTransform consoleButton;
        [SerializeField] public Texture2D resizeCursor;
        [SerializeField] private Camera cam;
        [SerializeField] private bool initText;

        private readonly BoxedBuggaryState state = new(BuggaryState.Edit);
        private readonly Persistence<ScriptReference> persistence = new(PersistenceKeys.BuggaryScripts.ToString());
        private ITextEditor editor;
        private List<Diagnostic> diagnostics = new();

        // Roslyn
        private readonly CompileRoslynModule compileRoslynModule = new();
        private readonly CompilationErrorRoslynModule compilationErrorRoslyn = new();
        private readonly DocumentRoslynModule documentRoslynModule = new();

        // Management
        private SuggestionBuggaryModule suggestionBuggaryModule;
        private HighlightBuggaryModule highlightBuggaryModule;
        private OverloadBuggaryModule overloadBuggaryModule;
        private ContextActionBuggaryModule contextActionBuggaryModule;
        private ErrorBuggaryModule errorBuggaryModule;
        public PersistenceBuggaryModule persistenceBuggaryModule;

        // UI/Mixed
        private SchwiftyPanel editorPanel;
        private SchwiftyTransform consoleTransform;

        // private SchwiftyPanel leftPanel;
        private SchwiftyPanel saveLoadPanel;
        private BuggaryConsole console;
        private BottomRightInfoPanel bottomInfoPanel;
        private BuggaryScriptLoadUI loadScriptUI;

        private bool initiated = false;
        private int caretIndex = 0;
        private string editorText = "";
        private bool newCharacterInserted = false;
        private Type previousAttachment; // this might need to move
        private bool showingScriptSelect = false;
        private bool isShowingConsole = false;

        private void Start()
        {
            this.SetupDefaultUIPanels();
            this.SetupSelectedEditor();
            this.SetupUI();
            this.SetupEditor();
            this.SetupBuggaryModules();
            this.SetupConsole();
            if (this.initText)
                this.StartCoroutine(this.AfterStart(1));
            else
                this.initiated = true;
        }

        private IEnumerator AfterStart(int inFrames)
        {
            for (int i = 0; i < inFrames; i++)
                yield return null;

            string text = string.Join('\n', Defaults.CurrentTest);

            ScriptReference localPersistence = this.persistence.Get();
            if (string.IsNullOrWhiteSpace(localPersistence.LastPath) == false)
                this.editor.SetText(File.ReadAllText(localPersistence.LastPath));
            else
                this.editor.SetText(text);

            this.highlightBuggaryModule.UpdateHighlighting(true);
            this.compilationErrorRoslyn.CheckCompilation(text);
            this.initiated = true;
        }

        /// <summary>
        /// This is used to setup default code for the game levels
        /// </summary>
        public void SetupCode(string code)
        {
            this.persistenceBuggaryModule.RemoveLastPath();
            this.editor.SetText(code);
            this.highlightBuggaryModule.UpdateHighlighting(true);
            this.compilationErrorRoslyn.CheckCompilation(code);
        }

        private void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
                Debug.Log(this.editor.GetText(true));

            if (!this.initiated) return;

            string text = this.editor.GetText(true);
            Vector3 caretPos = this.editor.GetCaretPosition(true, false);
            caretPos = this.CaretPositionScrollYDeltaAdjust(caretPos);
            // This needs to be called every frame to work
            this.newCharacterInserted = this.editor.GetCharacterChangedThisFrame();

            bool textChanged = this.editorText != text;
            bool caretIndexChanged = this.caretIndex != this.editor.GetCaretIndex();

            this.editorText = text;
            this.caretIndex = this.editor.GetCaretIndex();

            if (textChanged)
            {
                this.documentRoslynModule.UpdateText(this.editorText);
                // Debug.Log($"DOCUMENT UPDATE {Time.frameCount}");
                this.highlightBuggaryModule.UpdateHighlighting(true);
                this.diagnostics = this.compilationErrorRoslyn.CheckCompilation(this.editor.GetText(true));
                this.errorBuggaryModule.SetErrorRedUnderlines(this.diagnostics);
            }

            this.suggestionBuggaryModule.Update(caretIndexChanged, this.newCharacterInserted, text, this.caretIndex, caretPos);
            this.errorBuggaryModule.Update();
            this.overloadBuggaryModule.Update(this.caretIndex, caretIndexChanged, text, caretPos);
            this.contextActionBuggaryModule.Update(this.caretIndex, caretPos, caretIndexChanged);

            // this is here so that diagnostic lines scroll
            // it is throwing and error when using open editor, this explains the second check
            // TODO: figure out a working way for scroll updates
            // TODO: make this trigger on scroll only
            if (caretIndexChanged && this.useOpenEditor == false)
                this.errorBuggaryModule.DisplayDiagnosticData(this.caretIndex);

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L))
                this.ToggleLoadMenu();
            this.bottomInfoPanel.SetText($"Index: {this.caretIndex}");
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A))
                this.AttachMono(this.editorText);
        }

        private Vector3 CaretPositionScrollYDeltaAdjust(Vector3 caretPos)
        {
            float yDelta = this.editor.GetScrollYDelta();
            caretPos.y -= 20; // margin
            caretPos.y -= yDelta; // scroll correction
            return caretPos;
        }

        public void ToggleConsole()
        {
            if (this.isShowingConsole)
            {
                this.isShowingConsole = false;
                this.console.Hide();
            }
            else
            {
                this.isShowingConsole = true;
                this.console.Show();
            }
        }

        public void ToggleLoadMenu()
        {
            if (this.showingScriptSelect == false)
            {
                this.showingScriptSelect = true;
                this.loadScriptUI.CreateSelection();
            }
            else
            {
                this.loadScriptUI.Destroy();
                this.showingScriptSelect = false;
            }
        }

        public string GetText() => this.editor.GetText(true);

        private void AttachMono(string code)
        {
            if (this.previousAttachment != null)
                Destroy(this.gameObject.GetComponent(this.previousAttachment));

            Type type = this.compileRoslynModule.Parse(code);
            this.gameObject.AddComponent(type);
            this.previousAttachment = type;
        }

        public void AttachMonoCurrentTextContent()
        {
            if (this.previousAttachment != null)
                Destroy(this.gameObject.GetComponent(this.previousAttachment));

            Type type = this.compileRoslynModule.Parse(this.editor.GetText(true));
            this.gameObject.AddComponent(type);
            this.previousAttachment = type;
        }

        public void RemoveMono()
        {
            if (this.previousAttachment != null)
                Destroy(this.gameObject.GetComponent(this.previousAttachment));
        }

        public void HideInterface()
        {
            // this.leftPanel.SetActive(false);
            this.editorPanel.SetActive(false);
        }

        public void ShowInterface()
        {
            // this.leftPanel.SetActive(true);
            this.editorPanel.SetActive(true);
        }

        private void SetupDefaultUIPanels()
        {
            SchwiftyElement editorLocal;
            SchwiftyElement consoleLocal;

            if (this.usePassedRTs)
            {
                editorLocal = new SchwiftyRoot(this.editorRectTransformExternal);
                consoleLocal = new SchwiftyRoot(this.consoleRectTransformExternal);
            }
            else
            {
                // SchwiftyRoot root = new(this.canvas.GetComponent<RectTransform>());
                SchwiftyRoot root = new(this.defaultEditorRoot);
                XSplit split = new();
                split.SplitPanel(out SchwiftyPanel left, out SchwiftyPanel right, 0.5f, root, this.resizeCursor, 0.2f);
                Destroy(left.GetImage());
                editorLocal = right;
                consoleLocal = left;
            }

            Positioner positioner = new Positioner().SameAsParent();
            this.editorPanel = new SchwiftyPanel(editorLocal, "EditorPanel")
                .UsePositioner15(positioner)
                .ToPanel6900();
            this.saveLoadPanel = new SchwiftyPanel(editorLocal, "SaveLoadPanel")
                .SetBackgroundColor(Color.gray)
                .UsePositioner15(positioner)
                .ToPanel6900();

            this.consoleTransform = new SchwiftyTransform(consoleLocal, "ConsolePanel")
                .UsePositioner15(positioner)
                .ToTransform6900();

            this.saveLoadPanel.SetActive(false);
        }

        private void SetupEditor()
        {
            if (this.useOpenEditor)
            {
                this.openEditor.Initialize(this.editorPanel,
                    () => this.suggestionBuggaryModule.DisableDotInsert(true),
                    _ =>
                    {
                        this.errorBuggaryModule.RepositionDiagnosticData(this.caretIndex);
                        this.errorBuggaryModule.SetErrorRedUnderlines(this.diagnostics);
                    },
                    x => this.errorBuggaryModule.DisplayDiagnosticData(x)
                );
            }
            else
            {
                // we are not working on the fancy editor in the public release since it is licenced
                // this.fancyEditor.Initialize(this.editorPanel, () => this.suggestionBuggaryModule.DisableDotInsert(true),
                //     _ =>
                //     {
                //         this.errorBuggaryModule.RepositionDiagnosticData(this.caretIndex);
                //         this.errorBuggaryModule.SetErrorRedUnderlines(this.diagnostics);
                //     });
            }
        }

        private void SetupSelectedEditor()
        {
            if (this.useOpenEditor)
                this.editor = this.openEditor;
            else
                this.editor = this.fancyEditor;
        }

        public void HandleNewFileLoaded(string content)
        {
            this.editor.SetCaretIndex(0);
            this.editor.SetText(content);
            this.highlightBuggaryModule.UpdateHighlighting(true);
        }

        private void SetupConsole()
        {
            this.console = new BuggaryConsole(this.consoleTransform, this.fontAsset, this.cam, this.colors);
            this.console.Hide();
        }

        private void SetupBuggaryModules()
        {
            this.overloadBuggaryModule =
                new OverloadBuggaryModule(this.defaultEditorRoot, this.editorPanel, this.fontAsset);
            this.errorBuggaryModule =
                new ErrorBuggaryModule(this.defaultEditorRoot, this.editorPanel, this.editor,
                    this.colors.backgroundColor, this.fontAsset);
            this.suggestionBuggaryModule =
                new SuggestionBuggaryModule(this.state, this.editor, this.useOpenEditor,
                    this.documentRoslynModule,this.defaultEditorRoot, this.editorPanel);
            this.highlightBuggaryModule = new HighlightBuggaryModule(this.editor, this.colors);
            this.contextActionBuggaryModule = new ContextActionBuggaryModule(this.defaultEditorRoot,
                this.editorPanel, this.documentRoslynModule, this.editor);
            SchwiftyRoot saveRoot = new(this.editorPanel.RectTransform, "SaveRoot");
            this.persistenceBuggaryModule = new PersistenceBuggaryModule(saveRoot, this.cam, this);
        }

        private void SetupUI()
        {
            this.bottomInfoPanel = new BottomRightInfoPanel(this.defaultEditorRoot);
        }
    }
}