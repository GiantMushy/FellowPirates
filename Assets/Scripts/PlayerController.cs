
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Ship sprites")]
    public Sprite fullHealthSprite;
    public Sprite damagedSprite;
    public Sprite heavilyDamagedSprite;

    [Header("UI elements")]
    public PauseMenu pauseMenu;
    public Image[] heartImages;

    public TextMeshProUGUI healthInventoryText;
    public TextMeshProUGUI goldText;

    // Victory and defeat panels
    GameManager gameManager;
    public VictoryPanelController victoryPanelController;
    public DeathPanelController deathPanelController;

    [Header("Auto Heal")]
    public float autoHealDelay = 1f; // seconds before using orange automatically
    private bool autoHealPending = false;

    [Header("Heal Effect")]
    public GameObject orangeHealEffectPrefab;
    public Vector3 orangeEffectOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Audio")]
    [SerializeField] private AudioClip healthPickupSound;
    [SerializeField] private AudioClip goldPickupSound;

    private ShipController shipController;
    private DamageTypeController damageTypeController;
    private SpriteRenderer spriteRenderer;


    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("PlayerController: GameManager.Instance is null!");
        }

        shipController = GetComponent<ShipController>();
        damageTypeController = GetComponent<DamageTypeController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (healthInventoryText == null)
        {
            var healthGO = GameObject.Find("HealthItem_UI");
            if (healthGO != null)
                healthInventoryText = healthGO.GetComponent<TextMeshProUGUI>();
        }

        if (goldText == null)
        {
            var goldGO = GameObject.Find("GoldCoin_UI");
            if (goldGO != null)
                goldText = goldGO.GetComponent<TextMeshProUGUI>();
        }


        UpdateSprite();
        UpdateHeartsUI();
        UpdateHealthItemUI();
        UpdateGoldUI();
    }

    void Update()
    {
        var keyboard = Keyboard.current;

        if (keyboard.escapeKey.wasPressedThisFrame)
            SceneManager.LoadScene("MainMenu");

        if (shipController != null)
        {
            bool forward =
            keyboard.upArrowKey.isPressed ||
            keyboard.wKey.isPressed;

            bool backward =
                keyboard.downArrowKey.isPressed ||
                keyboard.sKey.isPressed;

            bool turnLeft =
                keyboard.leftArrowKey.isPressed ||
                keyboard.aKey.isPressed;

            bool turnRight =
                keyboard.rightArrowKey.isPressed ||
                keyboard.dKey.isPressed;

            shipController.SetAccelerate(forward);
            shipController.SetDecelerate(backward);
            shipController.SetTurnPort(turnLeft);
            shipController.SetTurnStarboard(turnRight);
        }

        if (keyboard.spaceKey.wasPressedThisFrame && gameManager != null)
        {
            if (gameManager.health < gameManager.maxHealth &&
                gameManager.healthInventory > 0)
            {
                UseHealthItem();
                SpawnOrangeHealEffect();

                if (healthPickupSound != null)
                    SoundEffectManager.instance.PlaySoundClip(healthPickupSound, transform, 1f);

                StartCoroutine(PulseEffect.sprite_pulse(
                    spriteRenderer,
                    num_pulses: 3,
                    intensity: 1.2f,
                    speed: 5f
                ));
            }
        }

    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameManager == null) return;

        string tag = other.tag;

        if (tag == "Land")
        {
            TakeDamage();
            if (gameManager.health < gameManager.maxHealth)
            {
                // Calculate normal direction away from the collision point
                Vector2 collisionPoint = other.ClosestPoint(transform.position);
                Vector3 normal = (transform.position - (Vector3)collisionPoint).normalized;
                StartCoroutine(damageTypeController.HandleLandCollision("Land", normal));

                if (gameManager.healthInventory > 0 && gameManager.health < gameManager.maxHealth && !autoHealPending)
                {
                    StartCoroutine(AutoHealAfterDelay());
                }
            }
        }
        else if (tag == "Finish")
        {
            ShowVictoryScreen();
        }
        else if (tag == "HealthPickup")
        {
            if (gameManager.health == gameManager.maxHealth)
            {
                gameManager.healthInventory++;
                UpdateHealthItemUI();
                if (healthPickupSound != null)
                    SoundEffectManager.instance.PlaySoundClip(healthPickupSound, transform, 1f);
            }
            else
            {
                GainHealth();
                if (healthPickupSound != null)
                    SoundEffectManager.instance.PlaySoundClip(healthPickupSound, transform, 1f);

                StartCoroutine(PulseEffect.sprite_pulse(spriteRenderer, num_pulses: 3, intensity: 1.2f, speed: 5f));
                SpawnOrangeHealEffect();
            }

            other.gameObject.SetActive(false);
        }
        else if (tag == "GoldPickup")
        {
            gameManager.goldCoins++;
            UpdateGoldUI();
            if (goldPickupSound != null)
                SoundEffectManager.instance.PlaySoundClip(goldPickupSound, transform, 1f);

            other.gameObject.SetActive(false);
        }
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameManager == null)
        {
            return;
        }

        string tag = collision.gameObject.tag;

        if ((tag == "Pirate" || tag == "Monster") &&
            Time.time < gameManager.fleeCooldownUntil)
        {
            return;
        }

        if (tag == "Pirate" || tag == "Monster")
        {
            var enemy = collision.gameObject.GetComponent<EnemyController>();
            if (gameManager.chasingEnemy == enemy)
            {
                gameManager.playerCaughtWhileFleeing = true;

                if (!string.IsNullOrEmpty(enemy.enemyId))
                {
                    gameManager.fleeDisabledEnemies.Add(enemy.enemyId);
                }

                gameManager.CancelChase();
            }

            gameManager.StartBattle(this, enemy);
            return;
        }

        if (tag == "WorldBorders")
        {
            // Get the collision normal from the contact point
            Vector3 normal = collision.GetContact(0).normal;
            StartCoroutine(damageTypeController.HandleLandCollision(tag, normal));
        }
    }

    public void PrepareForBattle()
    {
        if (shipController != null)
        {
            shipController.EnableControl();
            shipController.Stop();
            shipController.SetAccelerate(false);
            shipController.SetDecelerate(false);
            shipController.SetTurnPort(false);
            shipController.SetTurnStarboard(false);
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    public void TakeDamage()
    {
        if (gameManager == null) return;

        if (gameManager.health > 0)
            gameManager.health--;

        UpdateSprite();
        UpdateHeartsUI();

        if (gameManager.health <= 0)
        {
            gameManager.CancelChase();

            // Stop and disable ship controls
            if (shipController != null)
            {
                shipController.Stop();
                shipController.DisableControl();
            }

            // Show the "You Died" overlay
            if (deathPanelController != null)
            {
                deathPanelController.Show();
            }
            else
            {
                //if no panel is assigned, keep old behaviour
                GetComponent<PlayerRespawn>().Respawn();
                gameManager.health = gameManager.maxHealth;
                UpdateSprite();
                UpdateHeartsUI();
            }
        }
    }

    private void UseHealthItem()
    {
        if (gameManager == null) return;

        if (gameManager.healthInventory > 0 &&
            gameManager.health < gameManager.maxHealth)
        {
            gameManager.healthInventory--;
            gameManager.health++;
            UpdateHealthItemUI();
            UpdateSprite();
            UpdateHeartsUI();
        }
    }

    public void GainHealth()
    {
        if (gameManager == null) return;

        if (gameManager.health < gameManager.maxHealth)
        {
            gameManager.health += 1;
            UpdateSprite();
            UpdateHeartsUI();
        }
    }

    private IEnumerator AutoHealAfterDelay()
    {
        autoHealPending = true;

        // Wait before healing
        yield return new WaitForSeconds(autoHealDelay);

        // Conditions might have changed during the delay, so re-check
        if (gameManager != null &&
            gameManager.healthInventory > 0 &&
            gameManager.health < gameManager.maxHealth)
        {
            UseHealthItem();

            // Same SFX + pulse
            if (healthPickupSound != null)
            {
                SoundEffectManager.instance.PlaySoundClip(healthPickupSound, transform, 1f);
            }
            if (spriteRenderer != null)
            {
                StartCoroutine(PulseEffect.sprite_pulse(spriteRenderer, num_pulses: 3, intensity: 1.2f, speed: 5f));
            }

            SpawnOrangeHealEffect();

        }

        autoHealPending = false;
    }

    private void SpawnOrangeHealEffect()
    {
        if (orangeHealEffectPrefab == null)
            return;

        Vector3 spawnPos = transform.position + orangeEffectOffset;
        Instantiate(orangeHealEffectPrefab, spawnPos, Quaternion.identity);
    }


    public void UpdateHeartsUI()
    {
        if (heartImages == null)
        {
            Debug.LogError("no heart images");
            return;
        }

        int currentHealth = gameManager != null ? gameManager.health : 0;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
            {
                Debug.LogError("no heart image at index " + i);
                continue;
            }

            bool fullHealth = i < currentHealth;
            heartImages[i].color = fullHealth ? Color.white : Color.black;
        }
    }

    public void UpdateSprite()
    {
        if (spriteRenderer == null) return;
        if (gameManager == null) return;

        int h = gameManager.health;

        if (h == 3) spriteRenderer.sprite = fullHealthSprite;
        else if (h == 2) spriteRenderer.sprite = damagedSprite;
        else if (h == 1) spriteRenderer.sprite = heavilyDamagedSprite;
    }



    void UpdateHealthItemUI()
    {
        if (healthInventoryText != null && gameManager != null)
        {
            healthInventoryText.text = gameManager.healthInventory.ToString();
        }
    }

    void UpdateGoldUI()
    {
        if (goldText != null && gameManager != null)
        {
            goldText.text = gameManager.goldCoins.ToString();
        }
    }

    public void ShowVictoryScreen()
    {
        shipController?.Stop();
        shipController?.DisableControl();

        if (victoryPanelController != null)
        {
            Debug.Log("Showing victory panel");
            victoryPanelController.Show();
        }
        else
        {
            Debug.LogWarning("VictoryPanelController is NOT assigned!");
        }
    }

}

