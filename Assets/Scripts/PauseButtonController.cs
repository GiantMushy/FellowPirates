using UnityEngine;

public class PauseButtonController : MonoBehaviour
{

    [SerializeField] private GameObject soundMenuPanel;

    private bool isPaused = false;

    public void ToggleSoundMenu()
    {
        isPaused = !isPaused;

        if (soundMenuPanel != null)
        {
            soundMenuPanel.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
