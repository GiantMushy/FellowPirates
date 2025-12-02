// using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;


public class FightWindowController : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "FightMiniGame") return;

        GameObject root = scene.GetRootGameObjects()[0];
        root.transform.SetParent(transform, false); // attach to purple panel
    }
}
