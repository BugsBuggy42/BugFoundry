namespace Projects.Buggary.BuggaryGame.CharacterAbilities
{
    using UnityEngine;

    public class Projectile
    {
        public void Shoot(Vector3 start, Vector3 direction, GameObject prefab)
        {
            GameObject projectile = Object.Instantiate(prefab);
            prefab.transform.position = start;
            ProjectileBehaviour behaviour = projectile.AddComponent<ProjectileBehaviour>();
            behaviour.Initialize(direction, 20, x =>
            {
                Debug.Log($"HIT {x.name}");
            });
        }
    }
}