using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFightController : MonoBehaviour
{
    public float speed = 0.1f;
    public SpriteRenderer damageSprite;

    public float damageFadeSpeed = 2f;
    Color clearColor = new Color(1f, 0f, 0f, 0f);
    Color hitColor = new Color(1f, 0f, 0f, 0.2f);

    public AttackFlowController flow;

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position = new UnityEngine.Vector3(transform.position.x, transform.position.y + speed, transform.position.z);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position = new UnityEngine.Vector3(transform.position.x, transform.position.y - speed, transform.position.z);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position = new UnityEngine.Vector3(transform.position.x + speed, transform.position.y, transform.position.z);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position = new UnityEngine.Vector3(transform.position.x - speed, transform.position.y, transform.position.z);

        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter with: " + other.name + " (tag: " + other.tag + ")");

        if (other.CompareTag("Bomb"))
        {
            Debug.Log("collided with boooomb!!");
            TakeDamage();
        }
    }


    private void TakeDamage()
    {

        StopAllCoroutines();

        StartCoroutine(DamageFlash());


    }

    private IEnumerator DamageFlash()
    {
        damageSprite.color = hitColor;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            damageSprite.color = Color.Lerp(hitColor, clearColor, t);
            yield return null;
        }

        damageSprite.color = clearColor;

        flow.OnDefendFinished();

    }

}
