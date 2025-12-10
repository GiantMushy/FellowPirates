using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UIElements;


public class BulletSpawner : MonoBehaviour
{
    // some basic parts based on : https://www.youtube.com/watch?v=YNJM7rWbbxY 
    enum SpawnType { Straight, Spin, Targeted }

    public GameObject bullet;
    public List<GameObject> bulletList;
    public GameObject player;
    private int bulletIndex = 0;
    public float startDelay = 0f;
    private float delayTimer = 0f;

    public float firingRate = 1f;
    public float speed = 1f;
    public float bulletLife = 1f;

    [SerializeField] private SpawnType spawnerType;
    public bool useGravity = false; // Enable physics-based projectile motion with gravity
    private GameObject spawnedBullet;
    private float timer = 0f;

    public SpriteRenderer minigameBackgroundSprite;

    public bool slide;
    public Vector2 slideVector;
    public float slideDuration;
    public float slideSpeed;
    private float t;


    public bool duplicate;
    public float duplicate_frequence = 3f;
    private float duplicateTimer = 0f;
    private Vector3 startPos;
    private Quaternion startRot;


    public bool rotate;
    public float rotateSpeed = 90f;


    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void Update()
    {
        if (minigameBackgroundSprite == null)
        {
            Debug.LogWarning("no minigame background set");
            return;
        }

        Bounds b = minigameBackgroundSprite.bounds;
        UnityEngine.Vector3 pos = transform.position;

        if (!b.Contains(pos))
        {
            Destroy(gameObject);
        }

        delayTimer += Time.deltaTime;

        if (delayTimer < startDelay)
        {
            return;
        }

        timer += Time.deltaTime;
        if (spawnerType == SpawnType.Spin)
        {
            transform.eulerAngles = new Vector3(0f, 0f, transform.eulerAngles.z + 1f);
        }
        else if (spawnerType == SpawnType.Targeted)
        {
            if (player != null)
            {
                Vector3 direction = player.transform.position - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
            }
        }

        if (timer >= firingRate)
        {
            Fire();
            timer = 0;
        }

        if (slide)
        {
            Slide();
        }

        if (duplicate)
        {
            duplicateTimer += Time.deltaTime;
            if (duplicateTimer >= duplicate_frequence)
            {
                duplicateTimer = 0f;
                Duplicate();
            }
        }

        if (rotate)
        {
            Rotate();
        }
    }

    private void Fire()
    {
        if (bulletList != null && bulletList.Count > 0)
        {
            if (bulletIndex <= 6) bullet = bulletList[0];
            else if (bulletIndex < 9) bullet = bulletList[1];
            else bullet = bulletList[2];
            bulletIndex = (bulletIndex + 1) % 10;
        }
        if (bullet)
        {
            spawnedBullet = Instantiate(bullet, transform.position, Quaternion.identity);
            spawnedBullet.layer = LayerMask.NameToLayer("OverlayLayer");
            
            Bullet bulletScript = spawnedBullet.GetComponent<Bullet>();
            bulletScript.speed = speed;
            bulletScript.bulletLife = bulletLife;
            bulletScript.minigameBackgroundSprite = minigameBackgroundSprite;
            bulletScript.usePhysics = useGravity;
            
            // If using gravity-based physics
            if (useGravity)
            {
                Rigidbody2D rb = spawnedBullet.GetComponent<Rigidbody2D>();
                if (rb != null && spawnerType == SpawnType.Targeted && player != null)
                {
                    // Simple trajectory calculation: aim upward based on distance
                    Vector2 toTarget = player.transform.position - transform.position;
                    float distance = toTarget.magnitude;
                    
                    // Normalize direction
                    Vector2 direction = toTarget.normalized;
                    float upwardBoost = distance * Random.Range(0.4f, 0.9f);
                    Vector2 launchVelocity = direction * speed + Vector2.up * upwardBoost;
                    
                    rb.linearVelocity = launchVelocity;
                    
                    // Rotate bullet to initial launch direction
                    float angle = Mathf.Atan2(launchVelocity.y, launchVelocity.x) * Mathf.Rad2Deg;
                    spawnedBullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }
                else
                {
                    // Non-targeted gravity bullets
                    rb.linearVelocity = spawnedBullet.transform.right * speed;
                    spawnedBullet.transform.rotation = transform.rotation;
                }
            }
            else
            {
                // Original non-physics behavior
                spawnedBullet.transform.rotation = transform.rotation;
            }
        }
    }

    private void Slide()
    {
        t += Time.deltaTime;

        if (t < slideDuration)
        {
            transform.position = new UnityEngine.Vector3(transform.position.x + (slideVector.x * slideSpeed * Time.deltaTime), transform.position.y + (slideVector.y * slideSpeed * Time.deltaTime), transform.position.z);
        }
    }


    private void Duplicate()
    {
        GameObject clone = Instantiate(gameObject, startPos, startRot);
        BulletSpawner cloneSpawner = clone.GetComponent<BulletSpawner>();
        cloneSpawner.duplicate = false;
        cloneSpawner.delayTimer = 0f;
    }


    private void Rotate()
    {
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }


    public void ResetSpawner()
    {
        delayTimer = 0f;
        timer = 0f;
        duplicateTimer = 0f;
        t = 0f;

        transform.position = startPos;
        transform.rotation = startRot;
    }
}
