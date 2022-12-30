namespace Buggary.BuggaryGame.Levels.Contracts
{
    using UnityEngine;

    public interface ILevel
    {
        public void Initialize(BuggaryGame gameIn, GameObject prefabIn = null);

        // /// <returns>whether all the problems of the level have been completed</returns>
        // public bool Update();
    }
}