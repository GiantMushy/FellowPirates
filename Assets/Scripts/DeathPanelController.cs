using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeathPanelController : MonoBehaviour
{
    public GameObject panelRoot;        // DeathPanel GameObject
    public Button respawnButton;
    public Button mainMenuButton;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        // Hide on start
        panelRoot.SetActive(false);

        // Hook up buttons
        if (respawnButton != null)
            respawnButton.onClick.AddListener(OnRespawnClicked);
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

    private void OnRespawnClicked()
    {
        Hide();

        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.health = gm.maxHealth;
        }

        // Respawn the player using the existing respawn script, change it maybe
        var player = FindObjectOfType<PlayerRespawn>();
        if (player != null)
        {
            player.Respawn();
        }

        // Update UI hearts / sprite after health reset
        var pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            pc.UpdateHeartsUI();
            pc.UpdateSprite();
        }
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
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
