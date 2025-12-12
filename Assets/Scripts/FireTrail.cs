using UnityEngine;

public class FireTrail : MonoBehaviour
{
    [Tooltip("How long this fire patch stays alive (seconds)")]
    public float lifeTime = 1.5f;

    public bool fadeOut = true;

    public float fadeDuration = 0.5f;

    private float timer = 0f;

    private SpriteRenderer sr;
    private Color originalColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    private void OnEnable()
    {
        timer = 0f;

        // reset color in case prefab is reused
        if (sr != null)
        {
            sr.color = originalColor;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (fadeOut && sr != null && fadeDuration > 0f)
        {
            float timeLeft = lifeTime - timer;
            if (timeLeft <= fadeDuration)
            {
                
                float t = Mathf.Clamp01(timeLeft / fadeDuration);

                Color c = originalColor;
                c.a = t;
                sr.color = c;
            }
        }

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player touched fire trail");
            
        }
    }
}
