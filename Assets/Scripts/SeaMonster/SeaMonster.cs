using UnityEngine;

public class SeaMonster : MonoBehaviour
{
    public Transform[] pathPoints;
    public float speed = 2f;
    int currentIndex = 0;
    private bool forwards = true;
    private bool stunned = false;
    private float originalSpeed;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (pathPoints != null && pathPoints.Length > 0)
            transform.position = pathPoints[0].position;

        originalSpeed = speed;
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (stunned) return;

        if (pathPoints == null || pathPoints.Length == 0) return;

        Transform target = pathPoints[currentIndex];

        Vector3 toTarget = target.position - transform.position;
        float step = speed * Time.deltaTime;

        if (toTarget.magnitude <= step)
        {
            transform.position = target.position;

            if (forwards)
                currentIndex++;
            else
                currentIndex--;

            if (currentIndex >= pathPoints.Length)
                currentIndex = 0;
            else if (currentIndex < 0)
                currentIndex = pathPoints.Length - 1;
        }
        else
        {
            transform.position += toTarget.normalized * step;
        }
    }


    public void ReverseDirection()
    {
        forwards = !forwards;

        if (forwards == false)
        {
            currentIndex--;
        }
        if (forwards == true)
        {
            currentIndex++;
        }
    }

    private System.Collections.IEnumerator StunRoutine()
    {
        stunned = true;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;


        speed = 0f;

        float elapsed = 0f;
        float scaleUp = 1.5f;
        float pulseSpeed = 6f;

        // yield return new WaitForSeconds(1f);

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;

            // ping-pong using sin
            float s = (Mathf.Sin(elapsed * pulseSpeed) + 1f) * 0.5f;
            float scaleFactor = Mathf.Lerp(1f, scaleUp, s);

            transform.localScale = originalScale * scaleFactor;

            yield return null;
        }
        transform.localScale = originalScale;
        speed = originalSpeed;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        stunned = false;
    }

    public void Stun()
    {
        if (!gameObject.activeInHierarchy) return;
        if (stunned) return;

        StartCoroutine(StunRoutine());
    }
}
