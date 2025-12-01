using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using UnityEngine.SceneManagement;

public class AttackButtonController : MonoBehaviour
{
    private Button attackButton;

    // void Awake()
    // {
    //     attackButton = GetComponent<Button>();
    // }

    public void Attack()
    {
        // if (attackButton == null)
        // {
        //     Debug.LogWarning("attack button is null ");
        // }
        Debug.Log("Attack button pressed");


        // EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(EventSystem.current.currentSelectedGameObject);
        SceneManager.LoadScene("AttackScene", LoadSceneMode.Additive);


    }

}
