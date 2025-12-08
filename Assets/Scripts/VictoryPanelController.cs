using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryPanelController : MonoBehaviour
{
    public GameObject panelRoot;
    public Button mainMenuButton;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        // Hide on start
        panelRoot.SetActive(false);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void Show()
    {
        panelRoot.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnMainMenuClicked()
    {
        Hide();

        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.GoToMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
