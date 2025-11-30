using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public int maxHealth = 3;
    public int health = 3;
    public Sprite fullHealthSprite;
    public Sprite damagedSprite;
    public Sprite heavilyDamagedSprite;

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
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Land"))
        {
            TakeDamage();
            if (health < maxHealth)
                StartCoroutine(damageTypeController.HandleLandCollision());
            else
                StartCoroutine(damageTypeController.HandleRespawn());
        }
        if (other.CompareTag("Finish"))
        {
            int currScene = SceneManager.GetActiveScene().buildIndex + 1;
            if (currScene >= SceneManager.sceneCountInBuildSettings)
                currScene = 0; // Loop back to main menu or first scene
            SceneManager.LoadScene(currScene);
        }
        if (other.CompareTag("HealthPickup"))
        {
            GainHealth();
            SoundEffectManager.instance.PlaySoundClip(healthPickupSound, transform, 1f);
            StartCoroutine(PulseEffect.sprite_pulse(spriteRenderer, num_pulses: 3, intensity: 1.2f, speed: 5f));
            other.gameObject.SetActive(false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pirate") || collision.gameObject.CompareTag("Monster"))
            TakeDamage();
        StartCoroutine(damageTypeController.HandleLandCollision()); 
    }

    public void TakeDamage()
    {
        health = (health > 1) ? health - 1 : 3;
        UpdateSprite();
    }
    public void GainHealth()
    {
        if (health < maxHealth)
        {
            health += 1;
            UpdateSprite();
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