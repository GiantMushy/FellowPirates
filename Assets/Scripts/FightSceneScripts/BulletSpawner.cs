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
    public bool strafe;
    public float strafeDistance;
    public Vector2 slideVector;
    private Vector2 currSlideVector;
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

    private bool hasStartTransform = false;


    public void CaptureStartTransform()
    {
        if (hasStartTransform) return;
        startPos = transform.localPosition;
        startRot = transform.localRotation;
        hasStartTransform = true;
        currSlideVector = slideVector;
    }


    void Update()
    {
        if (minigameBackgroundSprite == null)
        {
            Debug.LogWarning("no minigame background set");
            return;
        }

        Bounds b = minigameBackgroundSprite.bounds;
        Vector3 pos = transform.position;

        if (!b.Contains(pos))
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = false;
        }
        else
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = true;
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
                    Vector2 toTarget = player.transform.position - transform.position;
                    float distance = toTarget.magnitude;
                    
                    // Normalize direction
                    Vector2 direction = toTarget.normalized;
                    float upwardBoost = distance * Mathf.Lerp(0.2f, 1.1f, Mathf.PingPong(Time.time, 1f));
                    Vector2 launchVelocity = direction * speed + Vector2.up * upwardBoost;
                    
                    rb.linearVelocity = launchVelocity;
                    
                    float angle = Mathf.Atan2(launchVelocity.y, launchVelocity.x) * Mathf.Rad2Deg;
                    spawnedBullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }
                else
                {
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

        if (strafe && t >= strafeDistance)
        {
            // if the spawner has reached strafe distance, reverse direction
            t = 0f;
            currSlideVector = -currSlideVector;
        }
        
        if (t < slideDuration)
        {
            transform.position = new UnityEngine.Vector3(
                transform.position.x + (currSlideVector.x * slideSpeed * Time.deltaTime),
                transform.position.y + (currSlideVector.y * slideSpeed * Time.deltaTime),
                transform.position.z);
        }

    }


    private void Duplicate()
    {
        // clone under same parent
        GameObject clone = Instantiate(gameObject, transform.parent);

        clone.transform.localPosition = startPos;
        clone.transform.localRotation = startRot;

        BulletSpawner cloneSpawner = clone.GetComponent<BulletSpawner>();
        if (cloneSpawner != null)
        {
            cloneSpawner.duplicate = false;
            cloneSpawner.delayTimer = 0f;
            cloneSpawner.minigameBackgroundSprite = minigameBackgroundSprite;
            cloneSpawner.hasStartTransform = true;
            cloneSpawner.startPos = startPos;
            cloneSpawner.startRot = startRot;
            cloneSpawner.currSlideVector = slideVector;
        }
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
        currSlideVector = slideVector;
        t = 0f;

        if (hasStartTransform)
        {
            transform.localPosition = startPos;
            transform.localRotation = startRot;
        }

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;
    }
}
