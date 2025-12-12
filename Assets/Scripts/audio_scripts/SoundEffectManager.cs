using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    // based on: https://www.youtube.com/watch?v=DU7cgVsU2rM

    public static SoundEffectManager instance;
    [SerializeField] private AudioSource soundEffectObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundClip(AudioClip audioClip, Transform spawnTransform, float volume, float startTime = 0f)
    {
        // spawn in gameObject
        AudioSource audioSource = Instantiate(soundEffectObject, spawnTransform.position, Quaternion.identity);

        // assign the audio clip
        audioSource.clip = audioClip;

        // assign volume
        audioSource.volume = volume;

        // play sound
        audioSource.Play();

        // get length of clip
        float clipLength = audioSource.clip.length;

        // destroy when done playing
        Destroy(audioSource.gameObject, clipLength);
    }
}
