namespace BugFoundry.BugFoundryGame
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class WinCondition
    {
        private readonly Func<bool> winCondition;
        private GameObject objectivePanel;

        public WinCondition(Func<bool> winConditionIn, string description, GameObject objectivePanel)
        {
            this.winCondition = winConditionIn;
            this.Description = description;
            this.objectivePanel = objectivePanel;
        }

        public bool Won { get; private set; } = false;

        public string Description { get; }

        public bool CheckWinCondition()
        {
            if (this.Won)
                return false;

            bool didWin = this.winCondition();

            if (didWin == false)
                return false;

            this.Won = true;
            this.objectivePanel.GetComponent<Image>().color = Color.green;
            return true;
        }
    }
}