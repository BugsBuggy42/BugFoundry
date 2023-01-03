namespace BugFoundry.BugFoundryEditor.TextEditors.Console
{
    using System.Collections.Generic;
    using Management;
    using SchwiftyUI.V3.Elements;
    using SchwiftyUI.V3.Inputs;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class BugFoundryConsole
    {
        public static BugFoundryConsole Instance;
        private readonly SchwiftyInput input;
        private List<string> logs = new();
        private BugFoundryColors colors;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            Instance = null;
        }

        public BugFoundryConsole(SchwiftyElement parent, TMP_FontAsset font, Camera cam, BugFoundryColors colors)
        {
            if (Instance != null)
                Debug.LogWarning($"Instance != null");

            this.colors = colors;

            Instance = this;

            this.input = new SchwiftyInput(parent, "BugFoundryConsoleInput", cam)
                .SetFontAsset(font)
                .SetTextSize(12)
                .SetVerticalAlignment(VerticalAlignmentOptions.Top)
                .SetHorizontalAlignment(HorizontalAlignmentOptions.Left)
                .SetBackgroundColor(new Color(0, 0, 0, 0.8f))
                .UsePositioner15(new Positioner().SameAsParent())
                .SetGoNameAndEleName("BugFoundry Console")
                .ToInput6900();

            SchwiftyScrollbar sb =
                new SchwiftyScrollbar(parent, "OpenEditorScrollbar", direction: Scrollbar.Direction.TopToBottom)
                    .SetBodyColor(this.colors.scrollbarBody)
                    .SetHandleColor(this.colors.scrollbarHandle)
                    .SetRightBarConstSize(0.3f)
                    .ToScrollBar6900();

            this.input.InputField.verticalScrollbar = sb.ScrollBar;

            // this might be a problem with Domain Reloading disabled but does not seem to be the case
            Application.logMessageReceived += this.LogListener;
        }

        public List<string> GetLogs() => this.logs;

        private void LogListener(string s1, string s2, LogType type)
        {
            this.logs.Add(s1);
            this.AddLine(s1);
        }

        private void AddLine(string line) => this.input.InputField.text += $"\n{line}";

        public void Reset()
        {
            this.logs = new List<string>();
            this.input.InputField.text = "";
        }

        public void Show() => this.input.SetActive(true);

        public void Hide() => this.input.SetActive(false);
    }
}