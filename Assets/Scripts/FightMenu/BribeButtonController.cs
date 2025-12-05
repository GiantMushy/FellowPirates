using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class BribeButtonController : MonoBehaviour
{
    [SerializeField] private Button bribeButton;
    public AttackFlowController flow;
    GameManager gameManager;
    private int bribeCost;

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


    public void Bribe()
    {
        Debug.Log("Bribe button pressed");
        EventSystem.current.SetSelectedGameObject(bribeButton.gameObject);

        if (gameManager.goldCoins < bribeCost)
        {
            Debug.Log("you cant afford that buddy");
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
