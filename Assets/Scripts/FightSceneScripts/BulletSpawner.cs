using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    // based on : https://www.youtube.com/watch?v=YNJM7rWbbxY 
    enum SpawnType { Straight, Spin }

    public GameObject bullet;
    public float firingRate = 1f;
    public float speed = 1f;
    public float bulletLife = 1f;

    [SerializeField] private SpawnType spawnerType;
    private GameObject spawnedBullet;
    private float timer = 0f;

    void Start()
    {

    }

    void Update()
    {
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
    }

    private void Fire()
    {
        if (bullet)
        {
            spawnedBullet = Instantiate(bullet, transform.position, Quaternion.identity);
            spawnedBullet.GetComponent<Bullet>().speed = speed;
            spawnedBullet.GetComponent<Bullet>().bulletLife = bulletLife;
            spawnedBullet.transform.rotation = transform.rotation;
        }
    }
}
