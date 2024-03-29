﻿namespace BugFoundry.BugFoundryGame.FutureDevelopment.CharacterAbilities
{
    using UnityEngine;

    public class BugFoundryCharacterController : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private GameObject bulletPrefab;

        private Projectile projectiles = new();

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.projectiles.Shoot(
                    this.transform.position + Vector3.up * 1.8f,
                    this.playerCamera.transform.forward,
                    this.bulletPrefab);
            }
        }
    }
}