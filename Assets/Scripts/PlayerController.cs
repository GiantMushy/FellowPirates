using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    public int maxHealth = 3;
    public int health = 3;
    public Sprite fullHealthSprite;
    public Sprite damagedSprite;
    public Sprite heavilyDamagedSprite;
    public Image[] heartImages;

    // Health invenentory
    public int healthInventory;
    public TextMeshProUGUI healthInventoryText;

    // Gold amount
    public int goldCoins;
    public TextMeshProUGUI goldText;
    [SerializeField] private AudioClip goldPickupSound;

    private ShipController shipController;
    private DamageTypeController damageTypeController;
    private SpriteRenderer spriteRenderer;


    [SerializeField] private AudioClip healthPickupSound;


    // Scene switch logic
    private EnemyController currentEnemy;
    private string returnSceneName;
    private float fleeCooldownUntil = 0f;
    private Vector3 lastEnemyPosition;
    public Vector3 savedCameraOffset;
    public bool hasSavedCameraOffset;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {
        shipController = GetComponent<ShipController>();
        if (shipController == null)
            Debug.LogError("PlayerController requires a ShipController component!");

        damageTypeController = GetComponent<DamageTypeController>();
        if (damageTypeController == null)
            Debug.LogError("PlayerController requires a DamageTypeController component!");

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError("PlayerController requires a SpriteRenderer component!");

        UpdateSprite();
        UpdateGoldUI();
        UpdateHeartsUI();
        UpdateHealthItemUI();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard.escapeKey.wasPressedThisFrame)
            SceneManager.LoadScene("MainMenu");

        if (shipController != null)
        {
            // Pass inputs to the ShipController
            shipController.SetAccelerate(keyboard.upArrowKey.isPressed);
            shipController.SetDecelerate(keyboard.downArrowKey.isPressed);
            shipController.SetTurnPort(keyboard.leftArrowKey.isPressed);
            shipController.SetTurnStarboard(keyboard.rightArrowKey.isPressed);
        }

        if (keyboard.eKey.wasPressedThisFrame)
        {
            if (health < maxHealth && healthInventory > 0)
            {
                UseHealthItem();
                SoundEffectManager.instance.PlaySoundClip(healthPickupSound, transform, 1f);
                StartCoroutine(PulseEffect.sprite_pulse(spriteRenderer, num_pulses: 3, intensity: 1.2f, speed: 5f));
            }
            else return;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        string tag = other.tag;

        if (tag == "Land")
        {
            TakeDamage();
            if (health < maxHealth)
                StartCoroutine(damageTypeController.HandleLandCollision());
            else
                StartCoroutine(damageTypeController.HandleRespawn());
        }
        else if (tag == "Finish")
        {
            int currScene = SceneManager.GetActiveScene().buildIndex + 1;
            if (currScene >= SceneManager.sceneCountInBuildSettings)
                currScene = 0; // Loop back to main menu or first scene
            SceneManager.LoadScene(currScene);
        }
        else if (tag == "HealthPickup")
        {   

            if (health == maxHealth)
            {
                GainHealthItem();
                SoundEffectManager.instance.PlaySoundClip(healthPickupSound, transform, 1f);
            }
            else
            {
                GainHealth();
                SoundEffectManager.instance.PlaySoundClip(healthPickupSound, transform, 1f);
                StartCoroutine(PulseEffect.sprite_pulse(spriteRenderer, num_pulses: 3, intensity: 1.2f, speed: 5f));
            }
            other.gameObject.SetActive(false);
        }
        else if (tag == "GoldPickup")
        {
            GainGold();
            SoundEffectManager.instance.PlaySoundClip(goldPickupSound, transform, 1f);
            other.gameObject.SetActive(false);
        }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        if ((tag == "Pirate" || tag == "Monster") && Time.time < fleeCooldownUntil)
        {
            Debug.Log("Ignoring enemy collision during flee cooldown");
            return;
        }

        if (tag == "Pirate" || tag == "Monster") // TODO: seperate logic for the monsters
        {
            if (Camera.main != null)
            {
                savedCameraOffset = Camera.main.transform.position - transform.position;
                hasSavedCameraOffset = true;
            }

            currentEnemy = collision.gameObject.GetComponent<EnemyController>();

            lastEnemyPosition = currentEnemy.transform.position;

            returnSceneName = SceneManager.GetActiveScene().name;
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (shipController != null)
            {
                shipController.EnableControl();
                shipController.Stop();
                shipController.SetAccelerate(false);
                shipController.SetDecelerate(false);
                shipController.SetTurnPort(false);
                shipController.SetTurnStarboard(false);
            }

            if (currentEnemy != null)
            {
                var enemyRenderer = currentEnemy.GetComponent<SpriteRenderer>();
                if (enemyRenderer != null) enemyRenderer.enabled = false;
            }
            spriteRenderer.enabled = false;
            SceneManager.LoadScene("FightDemo");

            return;

        }
        StartCoroutine(damageTypeController.HandleLandCollision());
    }

    public void StartChase()
    {
        Debug.Log("StartChase called from Flee");

        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (shipController != null) shipController.enabled = true;



        SceneManager.LoadScene(returnSceneName);

        shipController.EnableControl();
        shipController.Stop();
        shipController.SetAccelerate(false);
        shipController.SetDecelerate(false);
        shipController.SetTurnPort(false);
        shipController.SetTurnStarboard(false);


        fleeCooldownUntil = Time.time + 2f;
        StartCoroutine(StartChaseAfterReturn());
    }

    private System.Collections.IEnumerator StartChaseAfterReturn()
    {
        // wait one frame so the overworld scene has actually spawned its enemies
        yield return null;

        var enemies = FindObjectsOfType<EnemyController>();
        if (enemies.Length > 0)
        {
            EnemyController best = null;
            float bestDist = float.MaxValue;

            foreach (var e in enemies)
            {
                float d = (e.transform.position - lastEnemyPosition).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = e;
                }
            }

            if (best != null)
            {
                best.StartChasing(transform);
            }
        }
    }




    public void TakeDamage()
    {
        health = (health > 1) ? health - 1 : 3;
        UpdateSprite();
        UpdateHeartsUI();
    }
    public void GainHealth()
    {
        if (health < maxHealth)
        {
            health += 1;
            UpdateSprite();
            UpdateHeartsUI();
        }
    }

    public void UpdateHeartsUI()
    {
        if (heartImages == null) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;

            bool fullHealth = i < health;

            heartImages[i].color = fullHealth ? Color.white : Color.black;

        }

    }

    public void GainHealthItem()
        {
            healthInventory += 1;
            UpdateHealthItemUI();
            
        }

    public void UpdateHealthItemUI()
    {
        if (healthInventoryText != null)
        {
            healthInventoryText.text = healthInventory.ToString();
        }
        
    }

    public void UseHealthItem()
    {
        if (healthInventory > 0 && health < maxHealth)
        {
            healthInventory -= 1;
            GainHealth();
            UpdateHealthItemUI();
        }
    }

    public void GainGold()
    {
        goldCoins += 1;
        UpdateGoldUI();
    }

    public void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = goldCoins.ToString();
        }
    }

    void UpdateSprite()
    {
        if (spriteRenderer == null) return;
        if (health == 3) spriteRenderer.sprite = fullHealthSprite;
        else if (health == 2) spriteRenderer.sprite = damagedSprite;
        else if (health == 1) spriteRenderer.sprite = heavilyDamagedSprite;
    }


    public void OnBattleWon()
    {
        Debug.Log("Battle won – returning to overworld and destroying enemy");

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        if (shipController != null)
        {
            shipController.EnableControl();
            shipController.Stop();
            shipController.SetAccelerate(false);
            shipController.SetDecelerate(false);
            shipController.SetTurnPort(false);
            shipController.SetTurnStarboard(false);
        }

        fleeCooldownUntil = Time.time + 2f;

        SceneManager.LoadScene(returnSceneName);

        StartCoroutine(DestroyEnemyAfterReturn());
    }

    private System.Collections.IEnumerator DestroyEnemyAfterReturn()
    {
        // wait one frame so the scene has actually spawned its enemies
        yield return null;

        var enemies = FindObjectsOfType<EnemyController>();
        if (enemies.Length > 0)
        {
            EnemyController best = null;
            float bestDist = float.MaxValue;

            foreach (var e in enemies)
            {
                float d = (e.transform.position - lastEnemyPosition).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = e;
                }
            }

            if (best != null)
            {
                Debug.Log("Destroying defeated enemy: " + best.name);
                Destroy(best.gameObject);
            }
        }
    }

}