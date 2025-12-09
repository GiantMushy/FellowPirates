using UnityEngine;
using TMPro;
using System.Collections;


public class ChaseTime : MonoBehaviour
{
    public TextMeshProUGUI chaseTimerText;
    public TextMeshProUGUI chasingText;
    public TextMeshProUGUI chaseTimeLeftText;

    private bool chaseWait = false;
    public float timeWait = 3f;

    private bool isChasing = false;
    public float chaseDuration = 15f;
    private float chaseTimeLeft;

    public RectTransform TimeRemaining;
    public RectTransform FullTime;

    private Vector3 timeRemainingInitialScale;


    // text blink stuff
    public float blinkSpeed = 3f;
    private Coroutine blinkCoroutine;


    void Start()
    {
        chaseTimerText.gameObject.SetActive(false);
        chasingText.gameObject.SetActive(false);
        chaseTimeLeftText.gameObject.SetActive(false);
        TimeRemaining.gameObject.SetActive(false);
        FullTime.gameObject.SetActive(false);


        timeRemainingInitialScale = TimeRemaining.localScale;
    }

    void Update()
    {

        if (chaseWait)
        {
            if (timeWait > 0f)
            {
                timeWait -= Time.deltaTime;
                DisplayTime(timeWait);
            }
            else
            {
                chaseTimerText.gameObject.SetActive(false);
                chasingText.gameObject.SetActive(false);
                chaseWait = false;
                // StartChase();
            }
        }


        else if (isChasing)
        {
            if (chaseTimeLeft > 0f)
            {
                chaseTimeLeft -= Time.deltaTime;
                DisplayTime(chaseTimeLeft);
                ChaseTimeBar();
            }
            else
            {
                chaseTimeLeft = 0f;
                ChaseTimeBar();
                EndChase();
            }
        }
    }


    public void startChaseCountodwn()
    {
        gameObject.SetActive(true);
        chaseTimerText.gameObject.SetActive(true);
        chasingText.gameObject.SetActive(true);

        chaseWait = true;
        isChasing = false;


        DisplayTime(timeWait);
        TimeRemaining.localScale = timeRemainingInitialScale;
    }

    public void DisplayTime(float timeToDisplay)
    {

        int seconds = Mathf.FloorToInt(timeToDisplay);
        chaseTimerText.text = seconds.ToString();
    }


    public void StartChase(bool fromBattle = true)
    {
        isChasing = true;
        chaseTimeLeft = chaseDuration;
        chaseTimeLeftText.gameObject.SetActive(true);
        FullTime.gameObject.SetActive(true);
        TimeRemaining.gameObject.SetActive(true);
        DisplayTime(chaseTimeLeft);

        ChaseTimeBar();

        blinkCoroutine = StartCoroutine(BlinkChaseTimeLeft());
    }

    void EndChase()
    {
        isChasing = false;

        chaseTimeLeftText.gameObject.SetActive(false);
        TimeRemaining.gameObject.SetActive(false);
        FullTime.gameObject.SetActive(false);

        StopCoroutine(blinkCoroutine);
        blinkCoroutine = null;
    }

    public void ChaseTimeBar()
    {
        if (TimeRemaining == null || chaseDuration <= 0f)
            return;

        float t = Mathf.Clamp01(chaseTimeLeft / chaseDuration);

        TimeRemaining.localScale = new Vector3(
            timeRemainingInitialScale.x * t,
            timeRemainingInitialScale.y,
            timeRemainingInitialScale.z
        );
    }


    private IEnumerator BlinkChaseTimeLeft()
    {
        while (isChasing)
        {
            Color c = chaseTimeLeftText.color;
            c.a = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            chaseTimeLeftText.color = c;

            yield return null;
        }
    }


    public void ForceStopChaseUI()
    {
        chaseWait = false;
        isChasing = false;

        chaseTimerText.gameObject.SetActive(false);
        chasingText.gameObject.SetActive(false);
        chaseTimeLeftText.gameObject.SetActive(false);
        TimeRemaining.gameObject.SetActive(false);
        FullTime.gameObject.SetActive(false);

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        gameObject.SetActive(false);
    }

}
