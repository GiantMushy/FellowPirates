using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using System.Security.Cryptography;
using System;


public class Bullet : MonoBehaviour
{

    // based on : https://www.youtube.com/watch?v=YNJM7rWbbxY 
    public float bulletLife = 1f;
    public float rotation = 0f;
    public float speed = 10f;
    private float timer = 0f;

    public SpriteRenderer minigameBackgroundSprite;
    public bool usePhysics = false; // When true, bullet uses rigidbody physics (gravity), when false uses direct movement
    public bool destroyOutOfBounds = true;

    [Header("Spawner Settings")]
    public GameObject bullet;
    public GameObject spawnedBullet;
    public int spawnedBulletCount = 0;
    void Update()
    {

        if (timer > bulletLife)
        {
            Destroy(this.gameObject);
        }

        timer += Time.deltaTime;

        // Only move directly if NOT using physics
        if (!usePhysics)
        {
            transform.position += transform.right * speed * Time.deltaTime;
        }



        if (minigameBackgroundSprite == null)
        {
            Debug.LogWarning("no minigame background seet");
            return;
        }

        Bounds b = minigameBackgroundSprite.bounds;
        UnityEngine.Vector3 pos = transform.position;

        if (!b.Contains(pos) && destroyOutOfBounds)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (bullet != null && spawnedBulletCount > 0)
        {
            int bulletCount = spawnedBulletCount;
            float angleStep = 360f / bulletCount; // Divide circle into equal angles
            float angleOffset = UnityEngine.Random.Range(0f, angleStep); // Start at a random angle offset
            
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = (i * angleStep) + angleOffset; // Calculate angle for this bullet
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

                spawnedBullet = Instantiate(bullet, transform.position, rotation);
                spawnedBullet.layer = LayerMask.NameToLayer("OverlayLayer");
            }
        }
    }
}
