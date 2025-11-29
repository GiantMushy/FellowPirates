using UnityEngine;
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
            GainHealth();
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

        }
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