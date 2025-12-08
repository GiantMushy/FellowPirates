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

    void Update()
    {
        if (gameOver)
        {
            return;
        }

        if (!defendResolved && timeBar.IsTimeOver)
        {
            defendResolved = true;
            gameOver = true;
            StartCoroutine(FlashAnimation(damageSprite, wonHitColor, wonClearColor, false));

            Debug.Log("YOU WOOON!!");
            return;
        }

        Bounds b = minigameBackgroundSprite.bounds;

        var key = Keyboard.current;

        if (key.upArrowKey.isPressed || key.wKey.isPressed)
        {
            float new_y = transform.position.y + speed * Time.deltaTime;
            if (new_y + SpriteSizeMargin.y < b.max.y)
            {
                transform.position = new UnityEngine.Vector3(transform.position.x, new_y, transform.position.z);
            }
        }
        if (key.downArrowKey.isPressed || key.sKey.isPressed)
        {
            float new_y = transform.position.y - speed * Time.deltaTime;
            if (new_y - SpriteSizeMargin.y > b.min.y)
            {
                transform.position = new UnityEngine.Vector3(transform.position.x, new_y, transform.position.z);
            }
        }

        if (key.rightArrowKey.isPressed || key.dKey.isPressed)
        {
            float new_x = transform.position.x + speed * Time.deltaTime;
            if (new_x + SpriteSizeMargin.x < b.max.x)
            {
                transform.position = new UnityEngine.Vector3(new_x, transform.position.y, transform.position.z);
            }
        }
        if (key.leftArrowKey.isPressed || key.aKey.isPressed)
        {
            float new_x = transform.position.x - speed * Time.deltaTime;
            if (new_x - SpriteSizeMargin.x > b.min.x)
            {
                transform.position = new UnityEngine.Vector3(new_x, transform.position.y, transform.position.z);
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bomb"))
        {
            Debug.Log("collided with boooomb!!");
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

        StartCoroutine(FlashAnimation(damageSprite, damageHitColor, damageClearColor, true));
    }

    private IEnumerator FlashAnimation(SpriteRenderer sprite, Color hitColor, Color clearColor, bool tookDamage)
    {

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
        StopAllCoroutines();
    }
}
