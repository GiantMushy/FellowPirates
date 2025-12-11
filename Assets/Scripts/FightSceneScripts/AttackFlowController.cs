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
    public Image[] heartImages;
    public TextMeshProUGUI healthInventoryText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI enemyGoldText;
    public TextMeshProUGUI actionText;

    // healing
    public GameObject healEffect;

    //bribe stuff
    public TextMeshProUGUI buttonMiddleScreenText;
    public TextMeshProUGUI bribeCostButtonText;

    // failed bribe
    public Image enemyImage;
    public TextMeshProUGUI redMiddleScreenMessage;
    private float angryDuration = 2f;
    private float shakeStrength = 3f;
    public SpriteRenderer fightingWindowBackground;

    public TextMeshProUGUI DamageText;
    public Button fleeButton;
    public float playerShakeStrength = 3f;

    // for player too
    public Image playerImage;

    // to shake camera
    public Transform cameraTransform;
    private float cameraShakeDuration = 2f;
    private float cameraShakeStrength = 0.2f;

    // audio 
    public AudioSource audioSource;
    public AudioClip twoDamageSound;
    public AudioClip oneDamageSound;
    public AudioClip zeroDamageSound;

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

        enemyGoldText.text = gameManager.enemyRewardAmount.ToString();


        if (gameManager == null)
        {
            Debug.LogError("AttackFlowController: GameManager.Instance is null!");
            return;
        }
        SetChooseActionText();
        RefreshItemsUI();
        bribeCostButtonText.text = $"{gameManager.enemyBribeCost} gold coins";

        UpdateFleeButtonState();
        if (gameManager.playerCaughtWhileFleeing)
        {
            gameManager.playerCaughtWhileFleeing = false;
            StartCoroutine(CaughtAfterFleeRoutine());
        }
    }

    public void ShowBribeCost()
    {
        Debug.Log("ShowBribeCost");
        if (buttonMiddleScreenText == null || gameManager == null) return;

        buttonMiddleScreenText.text = $"BRIBE COST: {gameManager.enemyBribeCost} GOLD. \nWill allow you to go from the battle unharmed.";
        buttonMiddleScreenText.gameObject.SetActive(true);
    }

    public void ShowAttackMessage()
    {
        Debug.Log("ShowAttackMessage");
        if (buttonMiddleScreenText == null || gameManager == null) return;

        buttonMiddleScreenText.text =
               "<color=green>GREEN</color>:   2x Damage\n" +
               "<color=yellow>YELLOW</color>  1x Damage\n" +
               "<color=red>RED</color>        0x Damage\n" +
               "\n<color=#00FFFF><b>[SPACE]</b></color>  to  Attack";

        buttonMiddleScreenText.gameObject.SetActive(true);
    }

    public void ShowFleeMessage()
    {
        Debug.Log("ShowFleeMessage");
        if (buttonMiddleScreenText == null || gameManager == null) return;

        buttonMiddleScreenText.text = "<color=red><b>Attempt to Flee?</b></color>\n" +
                "You have <b>3 seconds</b>\n" +
                "<size=90%>(Only one chance)</size>";


        buttonMiddleScreenText.gameObject.SetActive(true);
    }

    public void ShowItemsMessageDisabled()
    {
        Debug.Log("ShowFleeMessage");
        if (buttonMiddleScreenText == null || gameManager == null) return;

        if (gameManager.healthInventory <= 0)
        {
            buttonMiddleScreenText.text = "No oranges!";
        }
        else
        {
            buttonMiddleScreenText.text = "Full health";
        }

        buttonMiddleScreenText.gameObject.SetActive(true);
    }

    public void ShowItemsMessage()
    {
        Debug.Log("ShowFleeMessage");
        if (buttonMiddleScreenText == null || gameManager == null) return;

        buttonMiddleScreenText.text = "[SPACE] to heal before next battle";

        buttonMiddleScreenText.gameObject.SetActive(true);
    }




    public void HideMiddleScreenMessage()
    {
        if (buttonMiddleScreenText == null) return;
        buttonMiddleScreenText.gameObject.SetActive(false);
    }


    private void SetChooseActionText()
    {
        if (actionText != null)
        {
            actionText.text = "CHOOSE ACTION";
        }
    }


    private void SetAttackText()
    {
        if (actionText != null)
        {
            actionText.text = "ATTACK";
        }
    }

    private void SetHealText()
    {
        if (actionText != null)
        {
            actionText.text = "HEALING";
        }
    }

    private void SetDefendText()
    {
        if (actionText != null)
        {
            actionText.text = "DEFEND";
        }
    }


    public void StartAttack()
    {
        Debug.Log("StartAttack");
        if (isAttacking || isDefending)
        {
            return;
        }

        SetAttackText();
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

        ShowDamageAmount(damageToEnemy);
        if (damageToEnemy > 0)
        {
            DamageEnemy(damageToEnemy);
        }
    }

    public void StartDefendAfterPlayerAction()
    {
        SetButtonsEnabled(false);
        StartDefend();
    }


    public void StartDefend()
    {
        Debug.Log("StartDefend");
        if (isAttacking || isDefending || isStartingDefend)
        {
            return;
        }

        SetDefendText();

        isStartingDefend = true;
        isDefending = true;

        timeBar.StartTimer();
        StartCoroutine(StartDefendDelayed());
    }


    private IEnumerator StartDefendDelayed()
    {
        yield return new WaitForSeconds(0.3f);

        TimingBarCanvas.SetActive(false);

        GameObject pattern = defendList[defend_index];
        pattern.SetActive(true);

        var playerFight = pattern.GetComponentInChildren<PlayerFightController>();
        if (playerFight != null)
        {
            Debug.Log("ResetForNewDefend on " + playerFight.name);
            playerFight.ResetForNewDefend();
        }


        BulletSpawner[] spawners = pattern.GetComponentsInChildren<BulletSpawner>(true);
        foreach (var sp in spawners)
        {
            sp.gameObject.SetActive(true);

            sp.CaptureStartTransform();

            sp.ResetSpawner();
        }
    }


    public void OnDefendFinished(bool tookDamage)
    {
        timeBar.StopTimer();

        ClearDefenseObjects();

        for (int i = 0; i < defendList.Length; i++)
        {
            defendList[i].SetActive(false);
        }

        isStartingDefend = false;
        isDefending = false;


        GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");
        foreach (GameObject bomb in bombs)
        {
            bomb.SetActive(false);
        }


        if (tookDamage)
        {
            DamagePlayer();
        }

        // if (defend_index >= defendList.Length || gameManager.health == 0)
        if (defend_index >= defendList.Length)

        {
            BattleOver();
            return;
        }

        SetChooseActionText();
        SetButtonsEnabled(true);
    }

    private void SetButtonsEnabled(bool enabled)
    {
        buttonPanell.interactable = enabled;
        buttonPanell.blocksRaycasts = enabled;
        buttonPanell.alpha = enabled ? 1f : 0.5f;
        buttonPanelPointer.SetActive(enabled);

        UpdateFleeButtonState();
    }

    private void UpdateFleeButtonState()
    {
        if (fleeButton == null || gameManager == null)
            return;

        bool fleeDisabledForThisEnemy =
            !string.IsNullOrEmpty(gameManager.currentEnemyId) &&
            gameManager.fleeDisabledEnemies.Contains(gameManager.currentEnemyId);

        fleeButton.interactable = !fleeDisabledForThisEnemy;
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

        if (gameManager.health == 0)
        {
            BattleOver();
            return;
        }
        // StartCoroutine(PlayerHitFeedback());
    }

    private void DamageEnemy(int units)
    {
        if (gameManager == null) return;

        gameManager.enemyHealth -= units;
        if (gameManager.enemyHealth < 0)
            gameManager.enemyHealth = 0;

        if (!string.IsNullOrEmpty(gameManager.currentEnemyId))
        {
            gameManager.enemyHealthById[gameManager.currentEnemyId] = gameManager.enemyHealth;
        }


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


        GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");
        foreach (GameObject bomb in bombs)
        {
            bomb.SetActive(false);
        }

        // Destroy fire trails
        FireTrail[] fireTrails = FindObjectsOfType<FireTrail>(true);
        foreach (var fire in fireTrails)
        {
            Destroy(fire.gameObject);
        }


    }
    public void StartHealVisualAndDefend()
    {
        SetButtonsEnabled(false);
        StartCoroutine(HealAndDefendRoutine());
    }

    private IEnumerator HealAndDefendRoutine()
    {
        SetHealText();

        SpriteRenderer healSprite = null;
        Coroutine pulseRoutine = null;

        if (healEffect != null)
        {
            healEffect.SetActive(true);

            healSprite = healEffect.GetComponent<SpriteRenderer>();
            if (healSprite != null)
            {
                pulseRoutine = StartCoroutine(
                    PulseEffect.sprite_pulse(healSprite)
                );
            }
        }

        yield return new WaitForSeconds(2f);

        if (pulseRoutine != null && healSprite != null)
        {
            StopCoroutine(pulseRoutine);
            healSprite.color = Color.white;
        }

        if (healEffect != null)
        {
            healEffect.SetActive(false);
        }

        StartDefend();
    }



    public void StartAngryAndDefend()
    {
        SetButtonsEnabled(false);

        if (cameraTransform != null)
        {
            StartCoroutine(CameraShakeRoutine(cameraShakeDuration, cameraShakeStrength));
        }


        StartCoroutine(AngryAndDefendRoutine());
    }

    private IEnumerator AngryAndDefendRoutine()
    {
        if (redMiddleScreenMessage != null)
        {
            redMiddleScreenMessage.gameObject.SetActive(true);
        }

        Color originalEnemyColor = Color.white;
        RectTransform rect = null;

        if (enemyImage != null)
        {
            originalEnemyColor = enemyImage.color;
            enemyImage.color = Color.purple;
            rect = enemyImage.rectTransform;
        }

        Color originalPanelColor = Color.white;
        if (fightingWindowBackground != null)
        {
            originalPanelColor = fightingWindowBackground.color;
            fightingWindowBackground.color = Color.black;
        }

        // shake
        Vector2 originalPos = rect.anchoredPosition;
        float t = 0f;

        while (t < angryDuration)
        {
            t += Time.deltaTime;

            if (rect != null)
            {
                float x = Random.Range(-shakeStrength, shakeStrength);
                float y = Random.Range(-shakeStrength, shakeStrength);
                rect.anchoredPosition = originalPos + new Vector2(x, y);
            }

            yield return null;
        }

        // reset
        if (rect != null)
        {
            rect.anchoredPosition = originalPos;
        }

        if (enemyImage != null)
        {
            enemyImage.color = originalEnemyColor;
        }

        if (fightingWindowBackground != null)
        {
            fightingWindowBackground.color = originalPanelColor;
        }

        if (redMiddleScreenMessage != null)
        {
            redMiddleScreenMessage.gameObject.SetActive(false);
        }

        StartDefend();
    }

    private void UpdateDamageText(int damage)
    {
        if (damage == 2)
        {
            DamageText.text = "2X DAMAGE";
            audioSource.PlayOneShot(twoDamageSound);
        }
        else if (damage == 1)
        {
            DamageText.text = "1X DAMAGE";
            audioSource.PlayOneShot(oneDamageSound);
        }
        else if (damage == 0)
        {
            DamageText.text = "0X DAMAGE";
            audioSource.PlayOneShot(zeroDamageSound);
        }
        else
        {
            // should not happen
            Debug.LogError("UpdateDamageText called with wrong damage type");
        }
    }

    private IEnumerator ShowDamageAmountRoutine(int damage)
    {
        DamageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        isAttacking = false;
        DamageText.gameObject.SetActive(false);
        StartDefend();
    }


    public void ShowDamageAmount(int damage)
    {
        UpdateDamageText(damage);

        if (damage > 0)
        {
            StartCoroutine(EnemyHitFeedback(damage));
        }

        StartCoroutine(ShowDamageAmountRoutine(damage));
    }

    private IEnumerator CaughtAfterFleeRoutine()
    {
        Color originalPanelColor = Color.white;
        if (fightingWindowBackground != null)
        {
            originalPanelColor = fightingWindowBackground.color;
            fightingWindowBackground.color = Color.black;
        }

        SetButtonsEnabled(false);


        if (cameraTransform != null)
        {
            StartCoroutine(CameraShakeRoutine(cameraShakeDuration, cameraShakeStrength));
        }


        if (redMiddleScreenMessage != null)
        {
            redMiddleScreenMessage.gameObject.SetActive(true);
            redMiddleScreenMessage.text = "YOU WERE CAUGHT!";
        }

        yield return new WaitForSeconds(1.5f);

        if (redMiddleScreenMessage != null)
        {
            redMiddleScreenMessage.gameObject.SetActive(false);
        }

        UpdateFleeButtonState();

        if (fightingWindowBackground != null)
        {
            fightingWindowBackground.color = originalPanelColor;
        }

        redMiddleScreenMessage.text = "YOU CANNOT AFFORD THAT!";

        StartDefend();
    }


    private IEnumerator EnemyHitFeedback(int damage)
    {
        if (enemyImage == null) yield break;

        RectTransform rect = enemyImage.rectTransform;
        if (rect == null) yield break;

        Vector2 originalPos = rect.anchoredPosition;
        Color originalColor = enemyImage.color;

        Color hitColor = originalColor;
        if (damage == 2)
        {
            hitColor = Color.red;
        }
        else if (damage == 1)
        {
            hitColor = Color.yellow;
        }

        enemyImage.color = hitColor;

        float duration = 1f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float x = Random.Range(-shakeStrength, shakeStrength);
            float y = Random.Range(-shakeStrength, shakeStrength);
            rect.anchoredPosition = originalPos + new Vector2(x, y);

            yield return null;
        }

        rect.anchoredPosition = originalPos;
        enemyImage.color = originalColor;
    }

    public void TriggerPlayerHitFeedback()
    {
        StartCoroutine(PlayerHitFeedback());
    }


    private IEnumerator PlayerHitFeedback()
    {
        StartCoroutine(CameraShakeRoutine(cameraShakeDuration, cameraShakeStrength));

        if (playerImage == null) yield break;

        RectTransform rect = playerImage.rectTransform;
        if (rect == null) yield break;

        Vector2 originalPos = rect.anchoredPosition;
        Color originalColor = playerImage.color;

        Color hitColor = Color.red;
        playerImage.color = hitColor;

        float t = 0f;
        float duration = 1f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float x = Random.Range(-playerShakeStrength, playerShakeStrength);
            float y = Random.Range(-playerShakeStrength, playerShakeStrength);
            rect.anchoredPosition = originalPos + new Vector2(x, y);

            yield return null;
        }

        rect.anchoredPosition = originalPos;
        playerImage.color = originalColor;
    }


    private IEnumerator CameraShakeRoutine(float duration, float strength)
    {
        if (cameraTransform == null) yield break;

        Vector3 originalPos = cameraTransform.position;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float x = Random.Range(-strength, strength);
            float y = Random.Range(-strength, strength);

            cameraTransform.position = originalPos + new Vector3(x, y, 0f);

            yield return null;
        }

        cameraTransform.position = originalPos;
    }


}
