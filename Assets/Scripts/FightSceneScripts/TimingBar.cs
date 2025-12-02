using UnityEngine;
using UnityEngine.UI;

public class TimingBar : MonoBehaviour
{
    public RectTransform bar;
    public RectTransform pointer;
    public RectTransform redZone;

    private AttackFlowController flow;

    public float speed = 1.5f;
    public int maxScore = 100;

    private float currPos;
    bool hasPressed = false;

    void Reset()
    {
        bar = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (hasPressed)
            return;

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
                Debug.LogError("TimingBar: flow is null, StartTiming was never called!");
                return;
            }

            flow.StartDefend();
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
                Debug.LogError("TimingBar: flow is null, StartTiming was never called!");
                return;
            }

            flow.StartDefend();
        }
    }

    private void UpdatePointerPosition(float t)
    {
        float width = bar.rect.width;
        float x = Mathf.Lerp(-width / 2f, width / 2f, t);

        Vector3 pos = pointer.localPosition;
        pos.x = x;
        pointer.localPosition = pos;
    }

    int CalculateScore()
    {
        float pointer_x = pointer.position.x;

        Vector3[] corners = new Vector3[4];
        redZone.GetWorldCorners(corners);

        float red_left_bounds = corners[0].x;
        float red_right_bounds = corners[3].x;

        float red_center = (red_left_bounds + red_right_bounds) * 0.5f;
        float half_width = (red_right_bounds - red_left_bounds) * 0.5f;

        float dist_to_red_center = Mathf.Abs(pointer_x - red_center);
        float t = dist_to_red_center / half_width;

        if (t >= 1f)
            return 0;

        float normalized = 1f - t;
        int score = Mathf.RoundToInt(normalized * maxScore);
        return score;
    }

    public void StartTiming(AttackFlowController f)
    {
        flow = f;
        currPos = 0f;
        hasPressed = false;
        speed = 1.5f;
    }
}
