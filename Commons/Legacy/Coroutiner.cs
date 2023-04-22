namespace BugFoundry.Commons.Legacy
{
    using UnityEngine;

    public class Coroutiner: MonoBehaviour
    {
        public static Coroutiner Instance { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
    }
}