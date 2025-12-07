using System.Collections;
using UnityEngine;

public class FloatingHealIcon : MonoBehaviour
{
    public float moveUpDistance = 0.5f;   
    public float duration = 1f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("FloatingHealIcon needs a SpriteRenderer on the same GameObject.");
        }
    }

    void Start()
    {
        StartCoroutine(PlayAndDestroy());
    }

    private IEnumerator PlayAndDestroy()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * moveUpDistance;

        Color startColor = spriteRenderer.color;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);

            // Move up
            transform.position = Vector3.Lerp(startPos, endPos, normalized);

            // Fade out
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, normalized);
            spriteRenderer.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}

