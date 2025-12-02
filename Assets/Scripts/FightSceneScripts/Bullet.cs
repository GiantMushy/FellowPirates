using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    // based on : https://www.youtube.com/watch?v=YNJM7rWbbxY 
    public float bulletLife = 1f;
    public float rotation = 0f;
   public float speed = 10f;
    private float timer = 0f;

    private Vector2 spawnPoint;

    void Start()
    {
        spawnPoint = new Vector2(transform.position.x, transform.position.y);
    }

    void Update()
    {
        if (timer > bulletLife)
        {
            Destroy(this.gameObject);
        }

        timer += Time.deltaTime;
        // transform.position = Movement(timer);
         transform.position += transform.right * speed * Time.deltaTime;
    }

    private Vector2 Movement(float timer)
    {
        float x = timer * speed * transform.right.x;
        float y = timer * speed * transform.right.y;

        return new Vector2(spawnPoint.x + x, spawnPoint.y + y);

    }
}
