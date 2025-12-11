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

    // direction is captured once and never changed
    [HideInInspector] public Vector2 moveDirection;

    // Fire trail toggle
    public bool enableFireTrail = false;
    public GameObject fireTrailPrefab;
    public float fireTrailInterval = 0.15f;
    private float fireTrailTimer = 0f;


    void Start()
    {
        // If spawner didn't set it, default to current right direction
        if (moveDirection == Vector2.zero)
            moveDirection = transform.right;
    }

    [Header("Spawner Settings")]
    public GameObject bullet;
    public GameObject spawnedBullet;
    public int spawnedBulletCount = 0;
    public bool spawnOnDeath = false;

    void Update()
    {

        if (timer > bulletLife)
        {
            KillBullet();
        }

        timer += Time.deltaTime;

        // Movement uses the stored direction
        if (!usePhysics)
        {
            transform.position += (Vector3)moveDirection * speed * Time.deltaTime;
        }

        // Visual spin – this can rotate freely now
        transform.Rotate(0f, 0f, rotation * Time.deltaTime);

        if (enableFireTrail && fireTrailPrefab != null)
        {
            fireTrailTimer += Time.deltaTime;
            if (fireTrailTimer >= fireTrailInterval)
            {
                fireTrailTimer = 0f;
                Instantiate(fireTrailPrefab, transform.position, Quaternion.identity);
            }
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
            KillBullet();
        }
    }

    private void KillBullet()
    {
        if (spawnOnDeath)
            SpawnSplitBullets();

        Destroy(gameObject);
    }



    private void SpawnSplitBullets()
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
