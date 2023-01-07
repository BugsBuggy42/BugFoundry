namespace BugFoundry.BugFoundryEditor.Management
{
    using System;
    using System.IO;
    using System.Linq;
    using Commons.CommonHelpers;
    using PersistenceModule;
    using PersistenceModule.DataClasses;
    using SchwiftyUI.BugFoundry.SchwiftyUI.V3.Elements;
    using SchwiftyUI.BugFoundry.SchwiftyUI.V3.Inputs;
    using UnityEngine;

    public class PersistenceBugFoundryModule
    {
        private SchwiftyButton saveButton;
        private SchwiftyInput input;
        private SchwiftyPanel savePanel;
        private BugFoundry bugFoundry;
        private BugFoundryScriptLoadUI loadUI;
        private bool active = false;

        private readonly Persistence<ScriptReference> persistence = new(PersistenceKeys.BugFoundryScripts.ToString());

        public PersistenceBugFoundryModule(SchwiftyRoot root, Camera cam, BugFoundry bugFoundry)
        {
            this.bugFoundry = bugFoundry;

            Color gray = Color.gray;
            Color black = Color.black;
            Color transparent = Color.black;
            gray.a = 0.5f;
            black.a = 0.2f;
            transparent.a = 0f;

            this.savePanel = new SchwiftyPanel(root, "SaveLoadPanel")
                .SetBackgroundColor(gray)
                .UsePositioner15(new Positioner().SameAsParent())
                .ToPanel6900();

            SchwiftyElement loadUIPanel = new SchwiftyPanel(this.savePanel, "LoadUIPanel")
                .SetBackgroundColor(transparent)
                .UsePositioner15(new Positioner().SameAsParent())
                .SetAnchors10(new Vector2(0f, 0.05f), new Vector2(1, 1f))
                .ZeroOffsets();

            this.loadUI = new BugFoundryScriptLoadUI(loadUIPanel, x =>
            {
                this.bugFoundry.HandleNewFileLoaded(x.Content);
            });

            this.savePanel.SetActive(false);

            SchwiftyButton activateButton = new SchwiftyButton(root, "S/L", out _)
                .SetBackgroundColor(black)
                .SetTextColor(Color.white)
                .SetAnchors10(new Vector2(0.88f, 0f), new Vector2(1, 0.05f))
                .ZeroOffsets()
                .ToButton6900();

            activateButton.Button.onClick.AddListener(() =>
            {
                this.active = !this.active;
                this.savePanel.SetActive(this.active);
                if (this.active)
                    this.loadUI.CreateSelection();
                else
                    this.loadUI.Destroy();
            });

            this.saveButton = new SchwiftyButton(this.savePanel, "Save", out _)
                .SetBackgroundColor(Color.green)
                .SetAnchors10(new Vector2(0, 0), new Vector2(0.33f, 0.05f))
                .ZeroOffsets()
                .ToButton6900();
            this.saveButton.Button.onClick.AddListener(this.OnSave);

            this.input = new SchwiftyInput(this.savePanel, "NameInput", cam)
                .SetBackgroundColor(Color.white)
                .SetColor(Color.black)
                .SetCaretColor(Color.black)
                .SetAnchors10(new Vector2(0.33f, 0), new Vector2(0.66f, 0.05f))
                .ZeroOffsets()
                .ToInput6900();
        }

        private void OnSave()
        {
            ScriptReference current = this.persistence.Get();
            string fileName = this.input.InputField.text;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                if (string.IsNullOrWhiteSpace(current.LastPath) == false)
                {
                    Debug.Log($"Overriding current save");
                    File.WriteAllText(current.LastPath, this.bugFoundry.GetText());
                }

                return;
            }

            string directory = Path.Combine(Application.persistentDataPath, "BugFoundryScripts");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string unique = new string(Guid.NewGuid().ToString().Take(6).ToArray());

            string path = Path.Combine(directory, $"{fileName}___{unique}.txt");

            File.WriteAllText(path, this.bugFoundry.GetText());

            current.Paths.Add(path);
            current.LastPath = path;
            this.persistence.Set(current);
            this.loadUI.CreateSelection();
            this.input.InputField.text = "";
        }

        public void SaveCurrent()
        {
            this.input.InputField.text = "";
            this.OnSave();
        }

        public void RemoveLastPath()
        {
            ScriptReference current = this.persistence.Get();
            current.LastPath = "";
            this.persistence.Set(current);
        }
    }
}