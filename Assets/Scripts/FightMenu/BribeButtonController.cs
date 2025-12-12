using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class BribeButtonController : MonoBehaviour
{
    [SerializeField] private Button bribeButton;
    public AttackFlowController flow;
    GameManager gameManager;
    private int bribeCost;
    private bool isSelected = false;
    public Button defaultButton;
    public AudioClip clickSound;

    void Start()
    {
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("BribeButtonController: GameManager.Instance is null!");
            return;
        }

        bribeCost = gameManager.enemyBribeCost;
    }

    void Update()
    {
        if (EventSystem.current == null || bribeButton == null || flow == null)
        {
            return;
        }

        if (!flow.buttonPanell.interactable)
        {
            if (isSelected)
            {
                isSelected = false;
            }
            return;
        }

        bool nowSelected = EventSystem.current.currentSelectedGameObject == bribeButton.gameObject;

        if (nowSelected && !isSelected)
        {
            isSelected = true;
            flow.ShowBribeCost();
        }
        else if (!nowSelected && isSelected)
        {
            isSelected = false;
        }
    }

    public void Bribe()
    {
        flow.HideMiddleScreenMessage();
        Debug.Log("Bribe button pressed");
        if (clickSound != null && SoundEffectManager.instance != null)
            SoundEffectManager.instance.PlaySoundClip(clickSound, transform, 0.5f);

        EventSystem.current.SetSelectedGameObject(bribeButton.gameObject);

        if (gameManager.goldCoins < bribeCost)
        {
            isSelected = false;
            flow.StartAngryAndDefend();

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
            }
        }
        else
        {
            Debug.Log("you have lost money in a bribe");
            gameManager.goldCoins -= bribeCost;
            flow.RefreshItemsUI();
            gameManager.EndBattleBribed();
        }
    }
}
