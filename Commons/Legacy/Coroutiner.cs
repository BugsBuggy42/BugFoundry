namespace Buggary.Commons.Legacy
{
    using UnityEngine;

    public class Coroutiner: MonoBehaviour
    {
        public static Coroutiner Instance { get; set; }
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
    }
}