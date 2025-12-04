using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
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
                StartCoroutine(damageTypeController.HandleLandCollision("Land"));
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
        if (tag == "Pirate" || tag == "Monster") // TODO: have seperate logic for the monsters
        {
            Debug.Log("hit enemy");
            TakeDamage();


            // Enable chase 
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy)
            {
                enemy.StartChasing(transform);
            }

            StartCoroutine(damageTypeController.HandleLandCollision(tag));

        }

        else if (tag == "WorldBorders")
        {
            StartCoroutine(damageTypeController.HandleLandCollision(tag));
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
}