using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UIElements;


public class BulletSpawner : MonoBehaviour
{
    // some basic parts based on : https://www.youtube.com/watch?v=YNJM7rWbbxY 
    enum SpawnType { Straight, Spin }

    public GameObject bullet;
    public float startDelay = 0f;
    private float delayTimer = 0f;

    public float firingRate = 1f;
    public float speed = 1f;
    public float bulletLife = 1f;

    [SerializeField] private SpawnType spawnerType;
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
        if (bullet)
        {
            spawnedBullet = Instantiate(bullet, transform.position, Quaternion.identity);
            spawnedBullet.layer = LayerMask.NameToLayer("OverlayLayer");
            spawnedBullet.GetComponent<Bullet>().speed = speed;
            spawnedBullet.GetComponent<Bullet>().bulletLife = bulletLife;
            spawnedBullet.GetComponent<Bullet>().minigameBackgroundSprite = minigameBackgroundSprite;
            spawnedBullet.transform.rotation = transform.rotation;
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
