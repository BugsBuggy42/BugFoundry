namespace Buggary.BuggaryGame.Levels.Level0TheIntro
{
    using System.Linq;
    using BuggaryEditor.TextEditors.Console;
    using Contracts;
    using Shared;
    using UnityEngine;

    public class Level0Main : ILevel
    {
        private BuggaryGame game;
        private Transform playerTrans;
        private Transform targetTrans;
        private Vector3 playerInitPosition;
        private Vector3 targetInitPosition;

        public void Initialize(BuggaryGame gameIn, GameObject prefabIn)
        {
            this.game = gameIn;
            this.game.WinConditions.Add(new WinCondition(this.HelloUnity, "Debug.Log(\"Hello Unity\")",
                this.game.objectives[0]));
            this.game.WinConditions.Add(new WinCondition(this.HelloUnity50, "Debug.Log(\"Hello Unity\") 50 times",
                this.game.objectives[1]));
            Debug.Log($"Welcome to level the Intro");
            foreach (GameObject o in this.game.objectives.Skip(2))
                o.SetActive(false);

            gameIn.SceneReset = () => { };

            gameIn.LevelCleanUp = () => { };

            gameIn.ScenePlay = () => { };

            this.game.SetupLevelSpecificCode(string.Join("\n", DefaultCode.Default));
        }

        private bool HelloUnity()
        {
            if (BuggaryConsole.Instance.GetLogs().Contains("Hello Unity"))
                return true;

            return false;
        }

        private bool HelloUnity50()
        {
            int count = BuggaryConsole.Instance.GetLogs().Count(x => x == "Hello Unity");
            if (count >= 50)
                return true;

            return false;
        }
    }
}