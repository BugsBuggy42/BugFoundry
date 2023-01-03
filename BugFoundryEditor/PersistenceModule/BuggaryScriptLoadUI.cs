namespace BugFoundry.BugFoundryEditor.PersistenceModule
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Commons.CommonHelpers;
    using DataClasses;
    using Management.Models;
    using SchwiftyUI.V3.Containers;
    using SchwiftyUI.V3.Containers.Sizers;
    using SchwiftyUI.V3.Elements;
    using UnityEngine;

    public class BuggaryScriptLoadUI
    {
        private List<SchwiftyButton> buttons = new();
        private VerticalGroup vg;
        private readonly Persistence<ScriptReference> persistence = new(PersistenceKeys.BuggaryScripts.ToString());

        private readonly SchwiftyElement parent;
        private readonly Action<SavedScriptData> action;

        public BuggaryScriptLoadUI(SchwiftyElement parentIn, Action<SavedScriptData> actionIn)
        {
            this.parent = parentIn;
            this.action = actionIn;
        }

        public void CreateSelection()
        {
            this.parent.SetActive(true);
            foreach (SchwiftyButton button in this.buttons)
                button.Destroy();
            this.vg?.Destroy();

            Color buttonColor = Color.gray;
            Color selectedButtonColor = Color.green;
            buttonColor.a = 0.5f;
            selectedButtonColor.a = 0.3f;

            this.buttons = this.persistence.Get().Paths.Select(x => new SchwiftyButton(this.parent, $"Button {x}",
                    out _, this.GetName(x),
                    act:
                    _ =>
                    {
                        string text = File.ReadAllText(x);
                        // delete entry
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            ScriptReference saves = this.persistence.Get();
                            saves.Paths.Remove(x);
                            if (saves.LastPath == x)
                                saves.LastPath = "";

                            this.persistence.Set(saves);
                            this.CreateSelection();
                        }
                        else
                        {
                            ScriptReference scriptPaths = this.persistence.Get();
                            scriptPaths.LastPath = x;
                            this.persistence.Set(scriptPaths);
                            this.CreateSelection();
                            this.action(new SavedScriptData()
                            {
                                Content = text,
                            });
                        }
                    })
                .SetBackgroundColor(x == this.persistence.Get().LastPath? selectedButtonColor : buttonColor)
                .ToButton6900()
            ).ToList();

            IEnumerable<string> p = this.persistence.Get().Paths.Concat(new []{this.persistence.Get().LastPath});
            // Debug.Log($"{string.Join("\n", p)}");

            this.vg = new();
            this.vg.Create(this.parent,
                5,
                5,
                new YSizer(SizerType.ElementFixedSideProportional, 0.1f), this.buttons.Cast<SchwiftyElement>().ToList(),
                contentPanelColor: buttonColor
            );
        }

        public void Destroy()
        {
            foreach (SchwiftyButton button in this.buttons)
                button.Destroy();

            this.vg?.Destroy();
            this.parent.SetActive(false);
        }

        private string GetName(string path)
        {
            string[] parts = path.Split(new[] { "\\", "/", "___" }, StringSplitOptions.RemoveEmptyEntries);
            return parts[parts.Length - 2];
        }
    }
}