using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryPanelController : MonoBehaviour
{
    public GameObject panelRoot;
    public Button mainMenuButton;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI healthText;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        panelRoot.SetActive(false);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void Show()
    {
        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.SetItemsCanvasActive(false);

            if (healthText != null) healthText.text = gm.healthInventory.ToString();
            if (coinText != null) coinText.text = gm.goldCoins.ToString();
        }

        panelRoot.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        panelRoot.SetActive(false);

        var gm = GameManager.Instance;
        if (gm != null) gm.SetItemsCanvasActive(true);

        Time.timeScale = 1f;
    }

    private void OnMainMenuClicked()
    {
        Hide();

        var gm = GameManager.Instance;
        if (gm != null) gm.GoToMainMenu();
        else SceneManager.LoadScene("MainMenu");
    }
}
