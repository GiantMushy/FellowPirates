using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using System.Security.Cryptography;


public class Bullet : MonoBehaviour
{

    // const int WORLD_WIDTH = 700;
    // const int WORLD_HEIGHT = 300;

    // based on : https://www.youtube.com/watch?v=YNJM7rWbbxY 
    public float bulletLife = 1f;
    public float rotation = 0f;
    public float speed = 10f;
    private float timer = 0f;

    public Camera overlayCamera;


    void Start()
    {
    }

    void Update()
    {

        if (timer > bulletLife)
        {
            Destroy(this.gameObject);
        }

        timer += Time.deltaTime;

        transform.position += transform.right * speed * Time.deltaTime;



        if (overlayCamera == null)
        {
            Debug.LogWarning("no overlay camera");
        }
        float margin = 0.0f;

        Vector3 vp = overlayCamera.WorldToViewportPoint(transform.position);

        if (vp.x < margin || vp.x > 1f - margin ||
            vp.y < margin || vp.y > 1f - margin ||
            vp.z < 0f)
        {
            Destroy(gameObject);
        }
    }

}
