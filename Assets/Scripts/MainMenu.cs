using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Reset game state before starting the level
        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.healthInventory = 0;
            gm.goldCoins = 0;
            gm.fleeCooldownUntil = 0f;
            gm.CancelChase();
        }

        SceneManager.LoadScene("Alpha_Test_Level");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public void GoToSettings()
    {
        // TODO: Implement settings menu
        //SceneManager.LoadScene("SettingsMenu");
    }
}
