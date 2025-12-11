using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Checkpoint : MonoBehaviour
{
    public GameObject bridge;
    public TextMeshProUGUI popupText;
    public float popupDuration = 2f;
    public AudioSource audioSource;

    public string checkpointId;

    void Start()
    {
        if (bridge != null) bridge.SetActive(false);
        if (popupText != null) popupText.gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var gm = GameManager.Instance;
        if (gm != null && !string.IsNullOrEmpty(checkpointId))
        {
            if (gm.activatedCheckpoints.Contains(checkpointId))
            {
                return;
            }

            gm.activatedCheckpoints.Add(checkpointId);
        }

        if (bridge != null) bridge.SetActive(true);

        PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();
        if (respawn != null)
            respawn.SetCheckpoint(transform.position);

        ShowPopup();
        PlaySound();
    }

    private void ShowPopup()
    {
        if (popupText == null) return;

        popupText.gameObject.SetActive(true);

        StartCoroutine(HidePopup());
    }


    private IEnumerator HidePopup()
    {
        yield return new WaitForSeconds(popupDuration);
        popupText.gameObject.SetActive(false);
    }


    private void PlaySound()
    {
        if (audioSource != null)
            audioSource.Play();
    }
}
