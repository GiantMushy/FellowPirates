using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    public Sprite[] slides;
    public float displayTime = 3f;
    public float fadeDuration = 1f;
    public string menuSceneName = "MainMenu";

    private Image img;
    private CanvasGroup cg;
    private bool skipping = false;
    private AudioSource source;

    public AudioClip[] slideSounds;
    public float audioVolume = 1f;


    void Start()
    {
        img = GetComponent<Image>();
        cg = GetComponent<CanvasGroup>();
        source = GetComponent<AudioSource>();
        StartCoroutine(PlaySlideshow());
        if (slideSounds.Length != slides.Length)
            Debug.LogWarning("slideSounds count doesn't match slides! Missing audio?");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            skipping = true;
        }
    }

    IEnumerator PlaySlideshow()
    {
        for (int i = 0; i < slides.Length; i++)
        {
            if (skipping) break;

            img.sprite = slides[i];

            // Change slide audio
            if (i < slideSounds.Length && slideSounds[i] != null)
            {
                source.clip = slideSounds[i];
                source.volume = 0f;   // Start silent
                source.Play();
            }

            // Fade In (image + audio)
            yield return StartCoroutine(Fade(0f, 1f));

            // Hold
            float timer = 0f;
            while (timer < displayTime)
            {
                if (skipping) break;
                timer += Time.deltaTime;
                yield return null;
            }

            if (skipping) break;

            // Fade Out (image + audio)
            yield return StartCoroutine(Fade(1f, 0f));

            source.Stop();
        }

        // After all images, load main menu
        SceneManager.LoadScene(menuSceneName);
    }

    IEnumerator Fade(float start, float end)
    {
        float t = 0;
        while (t < fadeDuration)
        {
            if (skipping) yield break;

            t += Time.deltaTime;
            float a = Mathf.Lerp(start, end, t / fadeDuration);

            cg.alpha = a;                    // Image fade
            source.volume = a * audioVolume; // Audio fade

            yield return null;
        }
    }
}
