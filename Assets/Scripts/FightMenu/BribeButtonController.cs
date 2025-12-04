using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class BribeButtonController : MonoBehaviour
{
    [SerializeField] private Button bribeButton;
    public AttackFlowController flow;
    PlayerController player;
    private int bribeCost;

    void Start()
    {
        player = PlayerController.Instance;

        if (player == null)
        {
            Debug.LogError("BribeButtonController: PlayerController.Instance is null!");
            return;
        }

        bribeCost = player.enemyBribeCost;
    }


    public void Bribe()
    {
        Debug.Log("Bribe button pressed");
        EventSystem.current.SetSelectedGameObject(bribeButton.gameObject);

        Debug.Log("Enemy bribe cost: " + bribeCost);

        if (player.goldCoins < bribeCost)
        {
            Debug.Log("you cant afford that buddy");
        }
        else
        {
            Debug.Log("you have lost money in a bribe");
            player.goldCoins -= bribeCost;
            flow.RefreshItemsUI();
            player.OnBribeAccepted();
        }
    }
}
