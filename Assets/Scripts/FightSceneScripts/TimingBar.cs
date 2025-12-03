using UnityEngine;

public class TimingBar : MonoBehaviour
{
    public Transform bar;
    public Transform pointer;
    public Transform redZone;

    private AttackFlowController flow;

    public float speed = 1.5f;
    public int maxScore = 100;

    private float currPos;
    bool hasPressed = false;

    private SpriteRenderer barSR;
    private SpriteRenderer redSR;

    public BattleTimeBar timeBar;

    void Awake()
    {
        barSR = bar.GetComponent<SpriteRenderer>();
        redSR = redZone.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // if (hasPressed)
        // {
        //     flow.OnAttackFinished();
        //     return;
        // }

        currPos += Time.deltaTime * speed;

        if (currPos >= 1f)
        {
            hasPressed = true;
            speed = 0;
            currPos = 1f;

            UpdatePointerPosition(currPos);
            Debug.Log("Score: 0 --> miss");

            if (flow == null)
            {
                Debug.LogError("TimingBar: flow is null!");
                return;
            }

            flow.OnAttackFinished();
            return;
        }

        UpdatePointerPosition(currPos);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasPressed = true;
            speed = 0;

            int score = CalculateScore();
            Debug.Log($"Score: {score}");

            if (flow == null)
            {
                Debug.LogError("TimingBar: flow is null!");
                return;
            }

            flow.OnAttackFinished();
        }
    }

    private void UpdatePointerPosition(float t)
    {
        if (barSR == null || pointer == null) return;

        float left = barSR.bounds.min.x;
        float right = barSR.bounds.max.x;

        float x = Mathf.Lerp(left, right, t);

        Vector3 pos = pointer.position;
        pos.x = x;
        pointer.position = pos;
    }

    int CalculateScore()
    {
        if (redSR == null || pointer == null) return 0;

        float pointer_x = pointer.position.x;

        float redLeft = redSR.bounds.min.x;
        float redRight = redSR.bounds.max.x;

        float redCenter = (redLeft + redRight) * 0.5f;
        float halfWidth = (redRight - redLeft) * 0.5f;

        float dist = Mathf.Abs(pointer_x - redCenter);
        float t = dist / halfWidth;

        if (t >= 1f)
            return 0;

        float normalized = 1f - t;
        return Mathf.RoundToInt(normalized * maxScore);
    }

    public void StartTiming(AttackFlowController f)
    {
        Debug.Log("started timing bar");
        flow = f;
        currPos = 0f;
        hasPressed = false;
        speed = 1.5f;
    }

}
