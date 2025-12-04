using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class AttackFlowController : MonoBehaviour
{
    public GameObject TimingBarCanvas;
    public GameObject DefendPattern1;
    public GameObject DefendPattern2;
    public GameObject DefendPattern3;

    public CanvasGroup buttonPanell;
    public GameObject buttonPanelPointer;

    private TimingBar Attack;
    public BattleTimeBar timeBar;

    public int enemyHealth = 150;

    private int defend_index = 0;

    private GameObject[] defendList;

    private bool isDefending = false;
    private bool isAttacking = false;
    private bool isStartingDefend = false;

    PlayerController player;

    // Items UI
    public Sprite fullHealthSprite;
    public Sprite damagedSprite;
    public Sprite heavilyDamagedSprite;
    public Image[] heartImages;
    public TextMeshProUGUI healthInventoryText;
    public TextMeshProUGUI goldText;


    private void Awake()
    {
        Attack = TimingBarCanvas.GetComponentInChildren<TimingBar>(true);
        TimingBarCanvas.SetActive(false);

        defendList = new GameObject[3];
        defendList[0] = DefendPattern1;
        defendList[1] = DefendPattern2;
        defendList[2] = DefendPattern3;

        for (int i = 0; i < defendList.Length; i++)
        {
            defendList[i].SetActive(false);
        }
    }

    void Start()
    {
        player = PlayerController.Instance;

        if (player == null)
        {
            Debug.LogError("AttackFlowController: PlayerController.Instance is null!");
            return;
        }

        RefreshItemsUI();
    }

    public void StartAttack()
    {
        if (isAttacking || isDefending)
        {
            BattleOver();
            return;
        }

        SetButtonsEnabled(false);

        Debug.Log("starting attack");
        if (Attack == null)
        {
            return;
        }

        for (int i = 0; i < defendList.Length; i++)
        {
            defendList[i].SetActive(false);
        }

        TimingBarCanvas.SetActive(true);

        isAttacking = true;
        Attack.StartTiming(this);
    }

    public void OnAttackFinished()
    {
        isAttacking = false;
        StartDefend();
    }

    public void StartDefend()
    {
        if (isAttacking || isDefending || isStartingDefend)
        {
            BattleOver();
            return;
        }

        isStartingDefend = true;
        isDefending = true;


        timeBar.StartTimer();
        StartCoroutine(StartDefendDelayed());
    }


    private IEnumerator StartDefendDelayed()
    {
        yield return new WaitForSeconds(0.3f);

        TimingBarCanvas.SetActive(false);

        // isDefending = true;
        defendList[defend_index].SetActive(true);
        defend_index++;
    }


    public void OnDefendFinished(bool tookDamage = false)
    {
        timeBar.StopTimer();

        for (int i = 0; i < defendList.Length; i++)
        {
            defendList[i].SetActive(false);
        }

        isStartingDefend = false;
        isDefending = false;

        GameObject[] BulletSpawner = GameObject.FindGameObjectsWithTag("BulletSpawner");
        foreach (GameObject bullet in BulletSpawner)
        {
            bullet.SetActive(false);
        }


        GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");
        foreach (GameObject bomb in bombs)
        {
            bomb.SetActive(false);
        }


        if (tookDamage)
        {
            DoDamageInFight();
        }

        if (defend_index >= defendList.Length)
        {
            BattleOver();
            return;
        }

        SetButtonsEnabled(true);
    }

    private void SetButtonsEnabled(bool enabled)
    {
        buttonPanell.interactable = enabled;
        buttonPanell.blocksRaycasts = enabled;
        buttonPanell.alpha = enabled ? 1f : 0.5f;
        buttonPanelPointer.SetActive(enabled);
    }

    void BattleOver()
    {
        PlayerController.Instance.OnBattleWon();
    }


    //  UPDATING THE UI ELEMENTS 
    public void RefreshItemsUI()
    {
        if (player == null)
        {
            return;
        }
        UpdateHeartsUI();
        UpdatHealthItemUI();
        UpdateGoldUI();
    }

    private void UpdateHeartsUI()
    {
        if (heartImages == null)
        {
            return;
        }

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
            {
                continue;
            }
            bool full = i < player.health;
            heartImages[i].color = full ? Color.white : Color.black;
        }
    }

    private void UpdatHealthItemUI()
    {
        if (healthInventoryText != null)
        {
            healthInventoryText.text = player.healthInventory.ToString();
        }
    }
    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = player.goldCoins.ToString();
        }
    }

    private void DoDamageInFight()
    {
        player.TakeDamage();
        RefreshItemsUI();
    }


    // bribe logic
    void bribeAccepted()
    {

    }

}
