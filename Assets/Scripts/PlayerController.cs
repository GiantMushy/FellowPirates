
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections;


public class PlayerController : MonoBehaviour
{
    public PauseMenu pauseMenu;
    public Sprite fullHealthSprite;
    public Sprite damagedSprite;
    public Sprite heavilyDamagedSprite;
    public Image[] heartImages;

    public TextMeshProUGUI healthInventoryText;
    private ShipController shipController;
    private DamageTypeController damageTypeController;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private AudioClip healthPickupSound;
    [SerializeField] private AudioClip goldPickupSound;

    public TextMeshProUGUI goldText;

    GameManager gameManager;
    public VictoryPanelController victoryPanelController;


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
            if (pauseMenu != null)
            {
                pauseMenu.TogglePause();
            }

        if (shipController != null)
        {
            shipController.SetAccelerate(keyboard.upArrowKey.isPressed);
            shipController.SetDecelerate(keyboard.downArrowKey.isPressed);
            shipController.SetTurnPort(keyboard.leftArrowKey.isPressed);
            shipController.SetTurnStarboard(keyboard.rightArrowKey.isPressed);
        }

        if (keyboard.eKey.wasPressedThisFrame && gameManager != null)
        {
            if (gameManager.health < gameManager.maxHealth &&
                gameManager.healthInventory > 0)
            {
                UseHealthItem();

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
            gameManager.StartBattle(this, enemy);
            return;
        }

        if (tag == "WorldBorders")
        {
            StartCoroutine(damageTypeController.HandleLandCollision(tag));
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
            GetComponent<PlayerRespawn>().Respawn();
            gameManager.health = gameManager.maxHealth;
            UpdateSprite();
            UpdateHeartsUI();
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

    void UpdateSprite()
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

        victoryPanelController.Show();
    }

}

