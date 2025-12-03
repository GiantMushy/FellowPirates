using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    // private bool timeHandled = false;

    void Update()
    {
        if (gameOver)
        {
            return;
        }

        if (timeBar.IsTimeOver)
        {
            StartCoroutine(FlashAnimation(damageSprite, wonHitColor, wonClearColor));

            Debug.Log("YOU WOOON!!");
            // flow.OnDefendFinished();
            // StopAllCoroutines();
            return;
        }

        Bounds b = minigameBackgroundSprite.bounds;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            float new_y = transform.position.y + speed * Time.deltaTime;
            if (new_y + SpriteSizeMargin.y < b.max.y)
            {
                transform.position = new UnityEngine.Vector3(transform.position.x, new_y, transform.position.z);
            }
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            float new_y = transform.position.y - speed * Time.deltaTime;
            if (new_y - SpriteSizeMargin.y > b.min.y)
            {
                transform.position = new UnityEngine.Vector3(transform.position.x, new_y, transform.position.z);
            }
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            float new_x = transform.position.x + speed * Time.deltaTime;
            if (new_x + SpriteSizeMargin.x < b.max.x)
            {
                transform.position = new UnityEngine.Vector3(new_x, transform.position.y, transform.position.z);
            }
        }
        if (Input.GetKey(KeyCode.LeftArrow))
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
        gameOver = true;
        StopAllCoroutines();

        StartCoroutine(FlashAnimation(damageSprite, damageHitColor, damageClearColor));
    }

    private IEnumerator FlashAnimation(SpriteRenderer sprite, Color hitColor, Color clearColor)
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

        flow.OnDefendFinished();

    }

}
