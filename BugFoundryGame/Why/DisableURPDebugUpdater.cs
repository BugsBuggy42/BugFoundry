namespace BugFoundry.BugFoundryGame.Why
{
    using UnityEngine;
    using UnityEngine.Rendering;

    public class DisableURPDebugUpdater : MonoBehaviour
    {
        private void Awake()
        {
            DebugManager.instance.enableRuntimeUI = false;
        }
    }
}