using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ItemsButtonController : MonoBehaviour
{
    [SerializeField] private Button itemsButton;
    [SerializeField] private Button attackButton;
    PlayerController player;

    void Start()
    {
        player = PlayerController.Instance;

        if (player == null)
        {
            Debug.LogError("ItemsButtonController: PlayerController.Instance is null!");
            return;
        }
    }

    void Update()
    {

        if (player == null)
        {
            return;
        }


        if (player.healthInventory < 0 || player.health > 2)
        {
            itemsButton.interactable = false;
        }
        else
        {
            itemsButton.interactable = true;
        }
    }


    public void Items()
    {
        Debug.Log("Items button pressed");
        EventSystem.current.SetSelectedGameObject(itemsButton.gameObject);

        player.UseHealthItem();


        if (player.healthInventory < 0 || player.health > 2)
        {
            EventSystem.current.SetSelectedGameObject(attackButton.gameObject);
        }
    }
}