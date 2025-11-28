using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Level_1");
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
