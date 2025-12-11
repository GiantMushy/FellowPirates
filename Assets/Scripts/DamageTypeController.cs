using System.Collections;
using UnityEngine;

public class DamageTypeController : MonoBehaviour
{
    public float knockbackForce = 2f;
    public float knockbackDuration = 0.3f;
    public float blinkDuration = 0.5f;
    public int blinkCount = 3;
    public bool takingDamage = false;
    private SpriteRenderer spriteRenderer;
    private ShipController shipController;

    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        shipController = GetComponent<ShipController>();
        rb = GetComponent<Rigidbody2D>();
    }

    public IEnumerator HandleLandCollision(string collisionTag, Vector3 normal)
    {
        float knockbackSpeed;
        float knockbackTimer = 0f;
        Vector3 knockbackDirection = normal;

        if (!takingDamage)
        {
            takingDamage = true;
            shipController.Stop();
            shipController.DisableControl();
        }

        while (knockbackTimer < knockbackDuration)
        {
            knockbackSpeed = Mathf.Lerp(knockbackForce, 0, knockbackTimer / knockbackDuration);
            Vector2 displacement = (Vector2)knockbackDirection * knockbackSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + displacement);
            // transform.Translate(knockbackDirection * knockbackSpeed * Time.deltaTime, Space.World);
            knockbackTimer += Time.deltaTime;
            yield return null;

        }

        bool blink = collisionTag != "WorldBorders";

        // Blink effect
        // Not blink when colliding with world
        if (blink && spriteRenderer != null)
            yield return StartCoroutine(BlinkEffect());

        shipController.EnableControl();
        takingDamage = false;
    }

    public IEnumerator HandleRespawn()
    {
        // Disable control and stop the ship
        shipController.Stop();
        shipController.DisableControl();

        // teleport to start position (-0.5, 0)
        transform.rotation = Quaternion.Euler(0, 0, 90);
        GetComponent<PlayerRespawn>().Respawn();

        // Blink effect
        if (spriteRenderer != null)
            yield return StartCoroutine(BlinkEffect());

        // Re-enable control
        shipController.EnableControl();
    }

    IEnumerator BlinkEffect()
    {
        float blinkInterval = blinkDuration / (blinkCount * 2);

        for (int i = 0; i < blinkCount; i++)
        {
            // Turn invisible
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(blinkInterval);

            // Turn visible
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
        }

        // Ensure sprite is visible at the end
        spriteRenderer.enabled = true;
    }
}
