using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ItemsButtonController : MonoBehaviour
{
    [SerializeField] private Button itemsButton;
    [SerializeField] private Button attackButton;
    GameManager gameManager;

    public AttackFlowController attackFlow;

    void Start()
    {
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("ItemsButtonController: GameManager.Instance is null!");
            return;
        }
    }

    void Update()
    {

        if (gameManager == null)
        {
            return;
        }


        if (gameManager.healthInventory <= 0 || gameManager.health > 2)
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

        gameManager.UseHealthItem();
        attackFlow.RefreshItemsUI();

        if (gameManager.healthInventory < 0 || gameManager.health > 2)
        {
            EventSystem.current.SetSelectedGameObject(attackButton.gameObject);
        }

        attackFlow.StartHealVisualAndDefend();
    }
}