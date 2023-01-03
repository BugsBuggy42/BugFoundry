namespace BugFoundry.BugFoundryGame.FutureDevelopment.CharacterAbilities
{
    using System;
    using UnityEngine;

    public class ProjectileBehaviour : MonoBehaviour
    {
        private Vector3 direction;
        private Action<GameObject> hitEvent;
        private float duration;

        public void Initialize(Vector3 directionIn, float durationIn, Action<GameObject> hitEventIn)
        {
            this.direction = directionIn;
            this.hitEvent = hitEventIn;
            this.duration = durationIn;
        }

        private void Update()
        {
            this.transform.position += this.direction * Time.deltaTime;

            this.duration -= Time.deltaTime;
            if(this.duration < 0)
                this.Destroy();
        }

        void OnCollisionEnter(Collision collision)
        {
            this.hitEvent.Invoke(collision.gameObject);
            this.Destroy();
        }

        private void Destroy()
        {
            Destroy(this.gameObject);
        }
    }
}