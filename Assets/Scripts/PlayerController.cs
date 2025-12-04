using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;


    public int maxHealth = 3;
    public int health = 3;
    public Sprite fullHealthSprite;
    public Sprite damagedSprite;
    public Sprite heavilyDamagedSprite;
    public Image[] heartImages;

    // Health invenentory
    public int healthInventory = 0;
    public TextMeshProUGUI healthInventoryText;

    // Gold amount
    public int goldCoins = 0;
    public TextMeshProUGUI goldText;
    [SerializeField] private AudioClip goldPickupSound;

    // Victory Panel
    public GameObject victoryPanel;

    // Player start position
    private Vector3 startPosition;
    private Quaternion startRotation;


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

    // for bribe
    public int enemyBribeCost; // taken from colliding enemy

    public GameObject levelObjects;


    void Awake()

    {
        Debug.Log($"PlayerController Awake, Instance={Instance}, health={health}, savedHealth=");

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


        // Save current position/rotation as spawn
        startPosition = transform.position;
        startRotation = transform.rotation;

        ResetPlayerState();
    }

    private void ResetPlayerState()
    {
        // Position + rotation
        transform.position = startPosition;
        transform.rotation = startRotation;

        // Core stats
        health = maxHealth;
        healthInventory = 0;
        goldCoins = 0;

        // Update visuals/UI
        UpdateSprite();
        UpdateHeartsUI();
        UpdateHealthItemUI();
        UpdateGoldUI();

        shipController.EnableControl();
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
                StartCoroutine(damageTypeController.HandleLandCollision("Land"));
            else
                StartCoroutine(damageTypeController.HandleRespawn());
        }
        else if (tag == "Finish")
        {
            ShowVictoryScreen();
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
                enemyBribeCost = currentEnemy.bribeCost;
                if (enemyRenderer != null)
                {
                    enemyRenderer.enabled = false;
                }
            }
            spriteRenderer.enabled = false;
            SceneManager.LoadScene("FightDemo");

            return;

        }

        else if (tag == "WorldBorders")
        {
            StartCoroutine(damageTypeController.HandleLandCollision(tag));
        }

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
        if (health > 0)
        {
            health -= 1;
        }
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
        if (heartImages == null)
        {
            Debug.LogError("no heart images");
            return;
        }

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
            {
                Debug.LogError("no heart images");
                continue;
            }

            bool fullHealth = i < health;

            heartImages[i].color = fullHealth ? Color.white : Color.black;

            Debug.Log("fullHealth " + fullHealth);

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

    public void ShowVictoryScreen()
    {
        // stop moving, disable controls
        shipController.Stop();
        shipController.DisableControl();
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        // Pause world
        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {   
        Debug.Log("RestartLevel BUTTON pressed");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Alpha_Test_Level");

    }

    public void GoToMainMenu()
    {   
        Debug.Log("GoToMainMenu BUTTON pressed");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void UpdateSprite()
    {

        Debug.Log("updating sprite with health: " + health);

        if (spriteRenderer == null)
        {
            Debug.Log("not gonna upsate health");
            return;
        }
        if (health == 3) { spriteRenderer.sprite = fullHealthSprite; }
        else if (health == 2) { spriteRenderer.sprite = damagedSprite; }
        else if (health == 1) { spriteRenderer.sprite = heavilyDamagedSprite; }
    }


    public void OnBattleWon()
    {
        Debug.Log("Battle won, returning to overworld");

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

    public void OnBribeAccepted()
    {
        Debug.Log("Bribe accepted, returning to overworld");

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

        fleeCooldownUntil = Time.time + 2f;

        // StartCoroutine(DestroyEnemyAfterReturn());
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

    void OnEnable()
    {
        Debug.Log($"PlayerController OnEnable, Instance={Instance}, health={health}, savedHealth=");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        Debug.Log($"[OnSceneLoaded] {scene.name}: health={health}, gold={goldCoins}, inv={healthInventory}");

        var itemsCanvas = GameObject.Find("ItemsCanvas");
        if (itemsCanvas != null)
        {
            heartImages = new Image[]
            {
                itemsCanvas.transform.Find("Heart_1")?.GetComponent<Image>(),
                itemsCanvas.transform.Find("Heart_2")?.GetComponent<Image>(),
                itemsCanvas.transform.Find("Heart_3")?.GetComponent<Image>()
            };

            Debug.Log("[HUD] Hearts rebound from ItemsCanvas");
        }
        else
        {
            Debug.LogError("[HUD] ItemsCanvas NOT FOUND");
        }



        var healthTextGO = GameObject.Find("HealthItem_UI");
        if (healthTextGO != null)
        {
            healthInventoryText = healthTextGO.GetComponent<TextMeshProUGUI>();
        }

        var goldTextGO = GameObject.Find("GoldCoin_UI");
        if (goldTextGO != null)
        {
            goldText = goldTextGO.GetComponent<TextMeshProUGUI>();
        }

        //if (scene.name == "Alpha_Test_Level")
        //{
          //  ResetPlayerState();
        //}
        //else

        UpdateHealthItemUI();
        UpdateGoldUI();
        UpdateSprite();
        UpdateHeartsUI();
    }


}
