using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using System.Security.Cryptography;


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

    void Start()
    {
        // If spawner didn't set it, default to current right direction
        if (moveDirection == Vector2.zero)
            moveDirection = transform.right;
    }

    void Update()
    {

        if (timer > bulletLife)
        {
            Destroy(this.gameObject);
        }

        timer += Time.deltaTime;

        // Movement uses the stored direction
        if (!usePhysics)
        {
            transform.position += (Vector3)moveDirection * speed * Time.deltaTime;
        }

        // Visual spin – this can rotate freely now
        transform.Rotate(0f, 0f, rotation * Time.deltaTime);


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

}
