
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    GameManager gameManager;
    private ShipController ship;
    private DamageTypeController damageTypeController;
    [Header("Ship sprites")]
    public Sprite fullHealthSprite;
    public Sprite damagedSprite;
    public Sprite heavilyDamagedSprite;
    private SpriteRenderer spriteRenderer;
    
    [Header("UI elements")]
    public PauseMenu pauseMenu;
    public Image[] heartImages;

    public TextMeshProUGUI healthInventoryText;
    public TextMeshProUGUI goldText;

    // Victory and defeat panels
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



    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("PlayerController: GameManager.Instance is null!");
        }

        ship = GetComponent<ShipController>();
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

        if (ship != null)
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

            ship.SetAccelerate(forward);
            ship.SetDecelerate(backward);
            ship.SetTurnPort(turnLeft);
            ship.SetTurnStarboard(turnRight);
        }

        if (gameManager.healthInventory > 0 && ship.health < ship.maxHealth && !autoHealPending)
        {
            StartCoroutine(AutoHealAfterDelay());
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameManager == null) return;

        string tag = other.tag;

        if (tag == "Land")
        {
            TakeDamage(1);
            StartCoroutine(damageTypeController.HandleLandCollision("Land"));
        }
        else if (tag == "Finish")
        {
            ShowVictoryScreen();
        }
        else if (tag == "HealthPickup")
        {
            gameManager.healthInventory++;
            UpdateHealthItemUI();

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
            gameManager.StartBattle(this, collision.gameObject.GetComponent<EnemyController>());
            return;
        }

        if (tag == "WorldBorders")
        {
            StartCoroutine(damageTypeController.HandleLandCollision(tag));
        }
    }

    public void PrepareForBattle()
    {
        if (ship != null)
        {
            ship.SetAccelerate(false);
            ship.SetDecelerate(false);
            ship.SetTurnPort(false);
            ship.SetTurnStarboard(false);
            ship.Stop();
            ship.DisableControl();
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    public void TakeDamage(int damage)
    {
        if (gameManager == null) return;

        if (ship.health > 0)
            ship.health -= damage;

        UpdateSprite();
        UpdateHeartsUI();

        if (ship.health <= 0)
        {
            gameManager.CancelChase();

            // Stop and disable ship controls
            if (ship != null)
            {
                ship.Stop();
                ship.DisableControl();
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
                ship.health = ship.maxHealth;
                UpdateSprite();
                UpdateHeartsUI();
            }
        }
    }

    public void Respawn()
    {
        ship.health = ship.maxHealth;
        UpdateSprite();
        UpdateHeartsUI();
        StartCoroutine(damageTypeController.HandleRespawn());
    }

    public void UseHealthItem()
    {
        if (gameManager == null) return;

        if (gameManager.healthInventory > 0 &&
            ship.health < ship.maxHealth)
        {
            gameManager.healthInventory--;
            GainHealth();
            UpdateHealthItemUI();
        }
    }

    public void GainHealth()
    {
        if (gameManager == null) return;

        if (ship.health < ship.maxHealth)
        {
            ship.health += 1;
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
            ship.health < ship.maxHealth)
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

        int currentHealth = gameManager != null ? ship.health : 0;

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
        if (ship == null) return;

        if (ship.health == 3) spriteRenderer.sprite = fullHealthSprite;
        else if (ship.health == 2) spriteRenderer.sprite = damagedSprite;
        else if (ship.health == 1) spriteRenderer.sprite = heavilyDamagedSprite;
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
        ship?.Stop();
        ship?.DisableControl();

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

    public bool IsDead()
    {
        return ship.health <= 0;
    }

    public int GetHealth()
    {
        return ship.health;
    }

    public void DisableControl()
    {
        ship.DisableControl();
    }
    public void EnableControl()
    {
        ship.EnableControl();
    }

    public void EnableSprite()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }
}

