namespace Buggary.BuggaryEditor.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class Clicky : MonoBehaviour, IPointerDownHandler
    {
        private Action act;

        public void Setup(Action actIn) => this.act = actIn;

        public void OnPointerDown(PointerEventData eventData) => this.act();
    }
}