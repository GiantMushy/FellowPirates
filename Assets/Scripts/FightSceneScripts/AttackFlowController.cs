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
    private int defend_index = 0;

    private GameObject[] defendList;

    private bool isDefending = false;
    private bool isAttacking = false;
    private bool isStartingDefend = false;

    GameManager gameManager;
    public Image[] enemyHeartImages;

    // Items UI
    // public Sprite fullHealthSprite;
    // public Sprite damagedSprite;
    // public Sprite heavilyDamagedSprite;
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
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("AttackFlowController: GameManager.Instance is null!");
            return;
        }

        RefreshItemsUI();
    }

    public void StartAttack()
    {
        Debug.Log("StartAttack");
        if (isAttacking || isDefending)
        {
            return;
        }

        SetButtonsEnabled(false);

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

    public void OnAttackFinished(int damageToEnemy = 0)
    {
        if (damageToEnemy > 0)
        {
            DamageEnemy(damageToEnemy);
        }

        isAttacking = false;
        StartDefend();
    }

    public void StartDefend()
    {
        Debug.Log("StartDefend");
        if (isAttacking || isDefending || isStartingDefend)
        {
            return;
        }

        isStartingDefend = true;
        isDefending = true;

        var playerFight = FindObjectOfType<PlayerFightController>();
        if (playerFight != null)
        {
            playerFight.ResetForNewDefend();
        }


        timeBar.StartTimer();
        StartCoroutine(StartDefendDelayed());
    }


    private IEnumerator StartDefendDelayed()
    {
        Debug.Log("StartDefendDelayed");

        yield return new WaitForSeconds(0.3f);

        TimingBarCanvas.SetActive(false);

        defendList[defend_index].SetActive(true);
    }


    public void OnDefendFinished(bool tookDamage = false)
    {
        timeBar.StopTimer();

        ClearDefenseObjects();

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
            DamagePlayer();
        }

        if (defend_index >= defendList.Length || gameManager.health == 0)
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
        if (gameManager.health <= 0)
        {
            GameManager.Instance.EndBattlePlayerDied();
        }
        else
        {
            GameManager.Instance.EndBattleWon();
        }
    }


    public void RefreshItemsUI()
    {
        if (gameManager == null)
        {
            return;
        }
        UpdateHeartsUI();
        UpdatHealthItemUI();
        UpdateGoldUI();
        UpdateEnemyHeartsUI();
    }

    private void UpdateHeartsUI()
    {
        if (heartImages == null) return;

        int health = gameManager.health;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;
            bool full = i < health;
            heartImages[i].color = full ? Color.white : Color.black;
        }
    }

    private void UpdateEnemyHeartsUI()
    {
        if (enemyHeartImages == null || gameManager == null)
        {
            return;
        }

        int hpUnits = gameManager.enemyHealth;

        for (int i = 0; i < enemyHeartImages.Length; i++)
        {
            if (enemyHeartImages[i] == null) continue;

            int unitsForThisHeart = Mathf.Clamp(hpUnits - i * 2, 0, 2);

            Image img = enemyHeartImages[i];

            if (unitsForThisHeart == 2)
            {
                img.color = Color.red;
            }
            else if (unitsForThisHeart == 1)
            {
                img.color = new Color(0.5f, 0f, 0f); // darker red
            }
            else
            {
                img.color = Color.black;
            }
        }
    }



    private void UpdatHealthItemUI()
    {
        if (healthInventoryText != null)
        {
            healthInventoryText.text = gameManager.healthInventory.ToString();
        }
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = gameManager.goldCoins.ToString();
        }
    }

    private void DamagePlayer()
    {
        gameManager.health--;
        RefreshItemsUI();
    }



    private void DamageEnemy(int units)
    {
        if (gameManager == null) return;

        gameManager.enemyHealth -= units;
        if (gameManager.enemyHealth < 0)
            gameManager.enemyHealth = 0;

        int damageDone = gameManager.enemyMaxHealth - gameManager.enemyHealth;
        defend_index = damageDone / 2;

        if (defend_index >= defendList.Length)
        {
            defend_index = defendList.Length - 1;
        }

        UpdateEnemyHeartsUI();

        if (gameManager.enemyHealth <= 0)
        {
            BattleOver();
            return;
        }
    }

    private void ClearDefenseObjects()
    {
        for (int i = 0; i < defendList.Length; i++)
        {
            if (defendList[i] != null)
                defendList[i].SetActive(false);
        }

        BulletSpawner[] spawners = FindObjectsOfType<BulletSpawner>(true);
        foreach (var sp in spawners)
        {
            sp.ResetSpawner();
        }

        GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");
        foreach (GameObject bomb in bombs)
        {
            bomb.SetActive(false);
        }
    }




}
