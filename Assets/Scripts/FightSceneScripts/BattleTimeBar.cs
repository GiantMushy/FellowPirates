using UnityEngine;

public class BattleTimeBar : MonoBehaviour
{
    public Transform totalTimeTransform;
    public Transform timeLeftTransform;

    public float maxTime = 10f;
    private float timeLeft;

    private bool isRunning = false;
    public bool IsTimeOver => timeLeft <= 0f;
    private UnityEngine.Vector3 startScale;
    void Start()
    {
        timeLeft = maxTime;
        startScale = timeLeftTransform.localScale;
    }

    void Update()
    {
        if (!isRunning)
        {
            return;
        }

        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;

            float ratio = timeLeft / maxTime;
            timeLeftTransform.localScale = new Vector3(
                startScale.x * ratio,
                startScale.y,
                startScale.z
            );
        }
        else
        {
            StopTimer();
        }
    }

    public void StartTimer()
    {
        timeLeft = maxTime;
        timeLeftTransform.localScale = startScale;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }
}

