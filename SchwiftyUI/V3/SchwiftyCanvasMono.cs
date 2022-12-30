namespace SchwiftyUI.V3
{
    using System;
    using UnityEngine;

    public class SchwiftyCanvasMono: MonoBehaviour
    {
        private Action action;

        public void Setup(Action actionIn)
        {
            this.action = actionIn;
        }

        private void Update()
        {
            this.action.Invoke();
        }
    }
}