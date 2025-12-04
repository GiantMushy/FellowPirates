using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class BribeButtonController : MonoBehaviour
{
    [SerializeField] private Button bribeButton;
    public void Bribe()
    {
        Debug.Log("Bribe button pressed");
        EventSystem.current.SetSelectedGameObject(bribeButton.gameObject);
    }
}
