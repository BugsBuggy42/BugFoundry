namespace BugFoundry.BugFoundryGame.Levels.Level1ReachTarget
{
    using System;
    using System.Linq;
    using Contracts;
    using Shared;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class Level1Main: ILevel
    {
        private GameObject player;
        private GameObject target;
        private float longDistance = 1;

        private Transform playerTrans;
        private Transform targetTrans;

        private Vector3 playerInitPosition;
        private Vector3 targetInitPosition;

        private BuggaryGame game;

        public void Initialize(BuggaryGame gameIn, GameObject prefabIn)
        {
            if (prefabIn == null)
                throw new Exception("Prefab should not be null!");

            this.game = gameIn;

            GameObject world = Object.Instantiate(prefabIn);

            this.player = world.transform.Find("Capsule").gameObject;
            this.target =  world.transform.Find("Target").gameObject;

            this.playerInitPosition = this.player.transform.position;
            this.targetInitPosition = this.target.transform.position;

            this.lastPosition = this.playerInitPosition;

            this.playerTrans = this.player.transform;
            this.targetTrans = this.target.transform;

            gameIn.WinConditions.Add(new WinCondition(this.SimpleReachTheTarget, "Just reach the damn target!", gameIn.objectives[0]));
            gameIn.WinConditions.Add(new WinCondition(this.AccelerateToTarget, "Accelerate to target!", gameIn.objectives[1]));

            Debug.Log($"Welcome to level 1");

            foreach (GameObject o in gameIn.objectives.Skip(2))
                o.SetActive(false);

            gameIn.SceneReset = () =>
            {
                this.player.transform.position = this.playerInitPosition;
                this.target.transform.position = this.targetInitPosition;
                this.lastDistance = 0;
                this.successes = 0;
                this.fails = 0;
            };

            gameIn.LevelCleanUp = () => Object.Destroy(world);

            gameIn.ScenePlay = () => { };

            this.game.SetupLevelSpecificCode(string.Join("\n", DefaultCode.Level1));
        }

        private bool SimpleReachTheTarget()
        {
            if ((this.playerTrans.position - this.targetTrans.position).magnitude < this.longDistance)
                return true;

            return false;
        }

        private Vector3 lastPosition;
        private float lastDistance = 0;
        private int successes = 0;
        private int fails = 0;

        private bool AccelerateToTarget()
        {
            Vector3 position = this.playerTrans.position;
            float distance = (this.lastPosition - position).magnitude / Time.deltaTime;

            if (distance > this.lastDistance)
                this.successes++;
            else
                this.fails++;

            this.lastDistance = distance;
            this.lastPosition = position;

            if ((this.playerTrans.position - this.targetTrans.position).magnitude < this.longDistance)
            {
                if (this.successes == 0)
                    return false;

                float ratio = this.fails / (float)this.successes;

                if (ratio < 0.1f)
                    return true;

                // Debug.Log($"Ratio: {ratio}");

                return false;
            }

            return false;
        }
    }
}