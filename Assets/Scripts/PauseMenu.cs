using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;           // assign PausePanel here in the Inspector
    public string mainMenuSceneName = "MainMenu";

    public bool IsPaused { get; private set; }

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        IsPaused = false;
    }

    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
        IsPaused = true;
    }

    public void Resume()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
