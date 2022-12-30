namespace Buggary.BuggaryGame.Levels.Level2Reflection
{
    using System;
    using System.Linq;
    using Contracts;
    using Shared;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class Level2Main : ILevel
    {
        private GameObject player;
        private GameObject target;
        private float longDistance = 1;

        private Transform playerTrans;
        private Transform targetTrans;

        private Vector3 playerInitPosition;
        private Vector3 targetInitPosition;

        private BuggaryGame game;

        private DateTime startTime;

        public void Initialize(BuggaryGame gameIn, GameObject prefabIn)
        {
            if (prefabIn == null)
                throw new Exception("Prefab should not be null!");

            this.game = gameIn;

            GameObject world = Object.Instantiate(prefabIn);

            this.player = world.transform.Find("Capsule").gameObject;
            this.target = world.transform.Find("Target").gameObject;

            this.playerInitPosition = this.player.transform.position;
            this.targetInitPosition = this.target.transform.position;

            this.playerTrans = this.player.transform;
            this.targetTrans = this.target.transform;

            gameIn.WinConditions.Add(new WinCondition(this.ReachTheTargetInTime,
                "<color=\"blue\"><link=\"https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/reflection\">Reflect</link></color> on how to reach the target in time with only modifying the allowed code.",
                gameIn.objectives[0]));

            Debug.Log($"Welcome to level 2");

            foreach (GameObject o in gameIn.objectives.Skip(1))
                o.SetActive(false);

            gameIn.SceneReset = () =>
            {
                this.player.transform.position = this.playerInitPosition;
                this.target.transform.position = this.targetInitPosition;
            };

            gameIn.ScenePlay = () => this.startTime = DateTime.Now;

            gameIn.LevelCleanUp = () => Object.Destroy(world);

            this.game.SetupLevelSpecificCode(string.Join("\n", DefaultCode.Level2));
        }

        private float maxTimeInSeconds = 4;

        private bool ReachTheTargetInTime()
        {
            if ((this.playerTrans.position - this.targetTrans.position).magnitude < this.longDistance)
            {
                double span = (DateTime.Now - this.startTime).TotalSeconds;
                if (span < this.maxTimeInSeconds)
                    return true;

                this.game.ResetLevel();
                Debug.Log($"Too Slow");
                return false;
            }

            return false;
        }
    }
}