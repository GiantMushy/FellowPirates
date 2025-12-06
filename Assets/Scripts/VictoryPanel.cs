using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryPanelController : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;

    void Awake()
    {
        if (victoryPanel == null)
            victoryPanel = gameObject;

        victoryPanel.SetActive(false);
    }

    public void Show()
    {
        victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        victoryPanel.SetActive(false);
    }

    public void OnRestartLevel()
    {
        Hide();
        GameManager.Instance.RestartCurrentLevel();
    }

    public void OnGoToMainMenu()
    {
        Hide();
        GameManager.Instance.GoToMainMenu();
    }

}
