namespace Buggary.Commons.CommonHelpers
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class MouseOverMono : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Action enter;
        private Action exit;
        private int index;

        public void Initialize(Action enterIn, Action exitIn)
        {
            this.enter = enterIn;
            this.exit = exitIn;
        }

        public void OnPointerEnter(PointerEventData eventData)
            =>
                this.enter?.Invoke();


        public void OnPointerExit(PointerEventData eventData)
            =>
                this.exit?.Invoke();
    }
}