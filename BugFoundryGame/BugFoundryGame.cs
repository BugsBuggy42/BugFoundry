namespace BugFoundry.BugFoundryGame
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using BugFoundryEditor.Management;
    using BugFoundryEditor.TextEditors.Console;
    using Commons.CommonHelpers;
    using DataClasses;
    using Levels.Level0TheIntro;
    using Levels.Level1ReachTarget;
    using Levels.Level2Reflection;
    using SchwiftyUI.V3.Containers;
    using SchwiftyUI.V3.Elements;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class BugFoundryGame : MonoBehaviour
    {
        private Persistence<LevelPersistence> currentLevelPer = new("CurrentLevel");

        [SerializeField] public BugFoundry bugFoundry;
        [SerializeField] public List<GameObject> objectives;
        [SerializeField] public Canvas canvas;
        [SerializeField] public TMP_FontAsset fontAsset;

        [SerializeField] public GameObject levelOneWorld;

        private InfoPanelFloating floatingPanel;
        public List<WinCondition> WinConditions { get; set; } = new();
        private bool ScenePlaying { get; set; } = false;
        private List<LevelInfo> levels;
        private LevelInfo level;
        private int levelIndex = 0;
        private bool gameWon = false;
        public Action SceneReset;
        public Action ScenePlay;
        public Action LevelCleanUp;

        private void Start()
        {
            this.levels = new List<LevelInfo>()
            {
                new()
                {
                    level = new Level0Main(),
                },
                new()
                {
                    level = new Level1Main(),
                    worldPrefab = this.levelOneWorld,
                },
                new()
                {
                    level = new Level2Main(),
                    worldPrefab = this.levelOneWorld,
                },
            };

            SchwiftyRoot root = this.canvas.ToRoot();
            this.floatingPanel = new InfoPanelFloating(root, root.RectTransform, this.fontAsset);

            for (int index = 0; index < this.objectives.Count; index++)
            {
                int local = index;
                GameObject objPanel = this.objectives[index];
                MouseOverMono mono = objPanel.GetComponent<MouseOverMono>();
                mono.Initialize(
                    () => this.OnMouseOverObjective(local, objPanel.transform.position),
                    () => this.OnMouseExitObjective(local));
            }

            int? currentLevel = this.currentLevelPer.Get().CurrentLevel;

            if (currentLevel.HasValue)
            {
                this.levelIndex = currentLevel.Value;
            }

            this.level = this.levels[this.levelIndex];
            this.level.level.Initialize(this, this.level.worldPrefab);

            this.bugFoundry.ApplyButton.SetAction(_ => this.PlayScene());
            this.bugFoundry.ResetButton.SetAction(_ => this.ResetLevel());
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.PageDown))
            {
                if (this.levelIndex == 0)
                    Debug.Log($"Level index is 0, cannot go down");
                else
                    this.SetupLevel(this.levelIndex - 1);
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.PageUp))
            {
                if (this.levelIndex == this.levels.Count - 1)
                    Debug.Log($"Level index is {this.levelIndex}, cannot go up");
                else
                    this.SetupLevel(this.levelIndex + 1);
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
                // Debug.Log($"HERE");
                LevelPersistence levelPer = this.currentLevelPer.Get();
                levelPer.CurrentLevel = 0;
                this.currentLevelPer.Set(levelPer);
                this.levelIndex = -1;
                this.SetupNewLevel();
            }

            if (this.ScenePlaying == false) return;
            if (this.gameWon) return;

            // this.level.Update();

            this.CheckObjectivesForBeingMet();

            this.CheckAllObjectivesMet();
        }

        private void CheckObjectivesForBeingMet()
        {
            for (int i = 0; i < this.WinConditions.Count; i++)
            {
                WinCondition condition = this.WinConditions[i];

                if (condition.CheckWinCondition())
                    this.ResetLevel();
            }
        }

        private void CheckAllObjectivesMet()
        {
            if (this.WinConditions.All(x => x.Won))
            {
                if (this.levelIndex == this.levels.Count - 1) //
                {
                    this.gameWon = true;
                    Debug.Log($"You won the game!");
                }
                else
                {
                    this.SetupNewLevel();
                }
            }
        }

        private void OnMouseOverObjective(int index, Vector2 center)
        {
            try
            {
                WinCondition some = this.WinConditions[index];
                this.floatingPanel.DisplayTextMouseOver(some.Description, center, 10, 0, true);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private void OnMouseExitObjective(int index)
        {
            this.StartCoroutine(this.Test(index));
        }

        private IEnumerator Test(int index)
        {
            yield return null;

            if (this.floatingPanel.MouseOverTheElement)
                yield break;

            this.floatingPanel.Hide();
        }

        private void SetupNewLevel()
        {
            this.LevelCleanUp?.Invoke();
            foreach (GameObject objective in this.objectives)
            {
                objective.GetComponent<Image>().color = Color.red;
                objective.SetActive(true);
            }

            this.levelIndex++;

            LevelPersistence levelPer = this.currentLevelPer.Get();
            levelPer.CurrentLevel = this.levelIndex;
            this.currentLevelPer.Set(levelPer);

            this.level = this.levels[this.levelIndex];
            this.WinConditions.Clear();
            this.level.level.Initialize(this, this.level.worldPrefab);
        }

        private void SetupLevel(int levelIn)
        {
            this.LevelCleanUp?.Invoke();
            foreach (GameObject objective in this.objectives)
            {
                objective.GetComponent<Image>().color = Color.red;
                objective.SetActive(true);
            }

            this.levelIndex = levelIn;

            LevelPersistence levelPer = this.currentLevelPer.Get();
            levelPer.CurrentLevel = this.levelIndex;
            this.currentLevelPer.Set(levelPer);

            this.level = this.levels[this.levelIndex];
            this.WinConditions.Clear();
            this.level.level.Initialize(this, this.level.worldPrefab);
        }

        public void PlayScene()
        {
            this.ScenePlaying = true;
            this.bugFoundry.AttachMonoCurrentTextContent();
            this.ScenePlay?.Invoke();
        }

        public void ResetLevel()
        {
            this.ScenePlaying = false;
            this.bugFoundry.RemoveMono();
            this.SceneReset?.Invoke();
            BugFoundryConsole.Instance.Reset();
        }

        public void SetupLevelSpecificCode(string code)
        {
            this.bugFoundry.SetupCode(code);
            this.bugFoundry.PersistenceBugFoundryModule.RemoveLastPath();
        }
    }
}