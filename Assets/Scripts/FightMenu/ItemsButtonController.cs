using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ItemsButtonController : MonoBehaviour
{
    [SerializeField] private Button itemsButton;
    [SerializeField] private Button attackButton;
    GameManager gameManager;
    private bool isSelected = false;
    public AudioSource audioSource;
    public AudioClip clickSound; 


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

        if (EventSystem.current == null || itemsButton == null || attackFlow == null)
        {
            return;
        }

        if (!attackFlow.buttonPanell.interactable)
        {
            if (isSelected)
            {
                isSelected = false;
            }
            return;
        }

        bool nowSelected = EventSystem.current.currentSelectedGameObject == itemsButton.gameObject;

        if (nowSelected && !isSelected)
        {
            isSelected = true;

            if (gameManager.healthInventory <= 0 || gameManager.health > 2)
            {
                attackFlow.ShowItemsMessageFullHealth();
            }
            else
            {
                attackFlow.ShowItemsMessage();
            }
        }
        else if (!nowSelected && isSelected)
        {
            isSelected = false;
        }
    }


    public void Items()
    {
        attackFlow.HideMiddleScreenMessage();

        EventSystem.current.SetSelectedGameObject(itemsButton.gameObject);

        gameManager.UseHealthItem();
        audioSource.PlayOneShot(clickSound);
        attackFlow.RefreshItemsUI();

        if (gameManager.healthInventory < 0 || gameManager.health > 2)
        {
            EventSystem.current.SetSelectedGameObject(attackButton.gameObject);
        }

        attackFlow.StartHealVisualAndDefend();
    }
}