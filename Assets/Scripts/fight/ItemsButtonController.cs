using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ItemsButtonController : MonoBehaviour
{
    [SerializeField] private Button itemsButton;

    public void Items()
    {
        Debug.Log("Items button pressed");
        EventSystem.current.SetSelectedGameObject(itemsButton.gameObject);

    }
}
