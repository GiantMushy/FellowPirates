using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


public class PlayerFightController : MonoBehaviour
{
    public float speed = 0.1f;
    public SpriteRenderer damageSprite;
    public SpriteRenderer wonSprite;

    public float damageFadeSpeed = 2f;
    Color damageClearColor = new Color(1f, 0f, 0f, 0f);
    Color damageHitColor = new Color(1f, 0f, 0f, 0.2f);

    Color wonClearColor = new Color(0f, 1f, 0f, 0f);
    Color wonHitColor = new Color(0f, 1f, 0f, 0.2f);

    public AttackFlowController flow;

    public SpriteRenderer minigameBackgroundSprite;

    public Vector2 SpriteSizeMargin = new Vector2(1f, 1.3f);

    public BattleTimeBar timeBar;

    private bool gameOver = false;
    private bool defendResolved = false;
    
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private bool isMovingUp = false;
    private bool isMovingDown = false;
    private float maxRotationAngle = 50f;
    private float rotationSpeed = 180f;

    public AudioSource audioSource;
    public AudioClip hitSound;


    void OnEnable()
    {
        defendResolved = false;
        gameOver = false;
    }

    void Update()
    {
        if (gameOver)
        {
            return;
        }

        if (!defendResolved && timeBar != null && timeBar.IsTimeOver)
        {
            defendResolved = true;
            gameOver = true;
            isMovingDown = false;
            isMovingUp = false;
            isMovingRight = false;
            isMovingLeft = false;

            var spriteToUse = wonSprite != null ? wonSprite : damageSprite;
            StartCoroutine(FlashAnimation(spriteToUse, wonHitColor, wonClearColor, false));

            Debug.Log("YOU WOOON!!");
            return;
        }

        var key = Keyboard.current;

        // Reset movement flags
        isMovingLeft = false;
        isMovingRight = false;
        isMovingUp = false;
        isMovingDown = false;

        if (key.upArrowKey.isPressed    || key.wKey.isPressed)  { isMovingUp = true;    }
        if (key.downArrowKey.isPressed  || key.sKey.isPressed)  { isMovingDown = true;  }
        if (key.rightArrowKey.isPressed || key.dKey.isPressed)  { isMovingRight = true; }
        if (key.leftArrowKey.isPressed  || key.aKey.isPressed)  { isMovingLeft = true;  }
    }

    void FixedUpdate()
    {
        if (isMovingUp)     MoveUp();
        if (isMovingDown)   MoveDown();
        if (isMovingRight)  MoveRight();
        if (isMovingLeft)   MoveLeft();

        RotateSprite();
    }

    private void MoveUp()
    {
        Bounds b = minigameBackgroundSprite.bounds;
        float new_y = transform.position.y + speed * Time.deltaTime;
        if (new_y + SpriteSizeMargin.y < b.max.y)
        {
            transform.position = new UnityEngine.Vector3(transform.position.x, new_y, transform.position.z);
        }
    }

    private void MoveDown()
    {
        Bounds b = minigameBackgroundSprite.bounds;
        float new_y = transform.position.y - speed * Time.deltaTime;
        if (new_y - SpriteSizeMargin.y > b.min.y)
        {
            transform.position = new UnityEngine.Vector3(transform.position.x, new_y, transform.position.z);
        }
    }

    private void MoveLeft()
    {
        Bounds b = minigameBackgroundSprite.bounds;
        float new_x = transform.position.x - speed * Time.deltaTime;
        if (new_x - SpriteSizeMargin.x > b.min.x)
        {
            transform.position = new UnityEngine.Vector3(new_x, transform.position.y, transform.position.z);
        }
    }

    private void MoveRight()
    {
        Bounds b = minigameBackgroundSprite.bounds;
        float new_x = transform.position.x + speed * Time.deltaTime;
        if (new_x + SpriteSizeMargin.x < b.max.x)
        {
            transform.position = new UnityEngine.Vector3(new_x, transform.position.y, transform.position.z);
        }
    }

    private void RotateSprite()
    {
        float targetRotation = 0f;
        float currentMaxRotation = maxRotationAngle;

        if (isMovingUp)         currentMaxRotation = 20f;
        else if (isMovingDown)  currentMaxRotation = 90f;
        
        if (isMovingLeft)       targetRotation = currentMaxRotation;
        else if (isMovingRight) targetRotation = -currentMaxRotation;
        
        float currentZ = transform.eulerAngles.z;

        if (currentZ > 180f) currentZ -= 360f;
        
        float newZ = Mathf.MoveTowards(currentZ, targetRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newZ);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bomb"))
        {
            Debug.Log("collided with boooomb!!");
            audioSource.PlayOneShot(hitSound);
            TakeDamage();
        }
    }


    private void TakeDamage()
    {
        if (defendResolved)
        {
            return;
        }

        defendResolved = true;
        gameOver = true;
        StopAllCoroutines();

        if (flow != null)
        {
            flow.TriggerPlayerHitFeedback();   
        }

        StartCoroutine(FlashAnimation(damageSprite, damageHitColor, damageClearColor, true));
    }

    private IEnumerator FlashAnimation(SpriteRenderer sprite, Color hitColor, Color clearColor, bool tookDamage)
    {
        if (sprite == null)
        {
            flow.OnDefendFinished(tookDamage);
            yield break;
        }

        sprite.color = hitColor;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            sprite.color = Color.Lerp(hitColor, clearColor, t);
            yield return null;
        }

        sprite.color = clearColor;

        gameOver = false;

        flow.OnDefendFinished(tookDamage);
    }


    public void ResetForNewDefend()
    {
        defendResolved = false;
        gameOver = false;
    }
}
