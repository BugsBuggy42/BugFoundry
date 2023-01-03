namespace BugFoundry.BugFoundryEditor.PersistenceModule
{
    using Management;
    using UnityEngine;

    public class PersistenceInputListener : MonoBehaviour
    {
        [SerializeField] private BugFoundry bugFoundry;

        public void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
                this.bugFoundry.PersistenceBugFoundryModule.SaveCurrent();
        }
    }
}