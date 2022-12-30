namespace Buggary.BuggaryEditor.PersistenceModule
{
    using Management;
    using UnityEngine;

    public class PersistenceInputListener : MonoBehaviour
    {
        [SerializeField] private Buggary buggary;

        public void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
                this.buggary.persistenceBuggaryModule.SaveCurrent();
        }
    }
}