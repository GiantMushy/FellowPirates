using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using UnityEngine.SceneManagement;

public class AttackButtonController : MonoBehaviour
{


    public void Attack()
    {
 
        Debug.Log("Attack button pressed");


        // EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(EventSystem.current.currentSelectedGameObject);
        SceneManager.LoadScene("AttackScene", LoadSceneMode.Additive);


    }

}
