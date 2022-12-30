namespace Buggary.SchwiftyUI.V3.Containers.Behaviours
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class SplitBehaviour : MonoBehaviour
        , IPointerClickHandler
        , IDragHandler
        , IPointerEnterHandler
        , IPointerExitHandler
    {
        SpriteRenderer sprite;
        Color target = Color.red;
        private Action dragAction;
        private Texture2D texture;

        public void Initilize(Action dragActionIn, Texture2D textureIn)
        {
            this.dragAction = dragActionIn;
            this.texture = textureIn;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData) => this.dragAction.Invoke();

        public void OnPointerEnter(PointerEventData eventData)
        {
            Cursor.SetCursor(this.texture, Vector2.zero, CursorMode.ForceSoftware);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}