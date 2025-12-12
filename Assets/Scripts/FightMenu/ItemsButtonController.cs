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

    private const int MaxHealth = 3;


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
        int healAmount = GetHealAmount();


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
                attackFlow.ShowItemsMessageDisabled();
            }
            else
            {
                attackFlow.ShowItemsMessage(healAmount);
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

        int healAmount = GetHealAmount();
        if (healAmount <= 0)
        {
            EventSystem.current.SetSelectedGameObject(attackButton.gameObject);
            return;
        }

        for (int i = 0; i < healAmount; i++)
        {
            gameManager.UseHealthItem();
        }

        audioSource.PlayOneShot(clickSound);
        attackFlow.RefreshItemsUI();

        if (GetHealAmount() <= 0)
        {
            EventSystem.current.SetSelectedGameObject(attackButton.gameObject);
        }

        attackFlow.StartHealVisualAndDefend();
    }

    private int GetHealAmount()
    {
        if (gameManager == null) return 0;
        int missing = MaxHealth - gameManager.health;
        if (missing <= 0) return 0;
        return Mathf.Min(missing, gameManager.healthInventory);
    }

}