using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;



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

    public void StartAttack()
    {
        if (isAttacking || isDefending || defend_index > 2)
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
        if (isAttacking || isDefending || defend_index > 2)
        {
            BattleOver();
            return;
        }
        timeBar.StartTimer();
        StartCoroutine(StartDefendDelayed());
    }

    private IEnumerator StartDefendDelayed()
    {
        yield return new WaitForSeconds(0.3f);

        TimingBarCanvas.SetActive(false);

        isDefending = true;
        defendList[defend_index].SetActive(true);
        defend_index++;
    }


    public void OnDefendFinished()
    {
        timeBar.StopTimer();

        for (int i = 0; i < defendList.Length; i++)
        {
            defendList[i].SetActive(false);
        }

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

}
