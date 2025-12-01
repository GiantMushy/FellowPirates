
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class TimingBar : MonoBehaviour
{
    public RectTransform bar;
    public RectTransform pointer;
    public RectTransform redZone;


    public float speed = 1.5f;

    public int maxScore = 100;

    private float t;

    private float currPos;


    void Reset()
    {
        bar = GetComponent<RectTransform>();
    }

    void Update()
    {
        currPos += Time.deltaTime * speed;
        if (currPos >= 1f)
        {
            currPos = 1f;
            UpdatePointerPosition(currPos);

            Debug.Log("Score: 0 --> miss");
            LoadDefenceScene();
            return;
        }

        UpdatePointerPosition(currPos);

        if (Input.GetKeyDown(KeyCode.Space))
        {

            int score = CalculateScore();
            Debug.Log($"Score: {score}");
            LoadDefenceScene();
        }
    }

    private void UpdatePointerPosition(float t)
    {
        float width = bar.rect.width;

        float x = Mathf.Lerp(-width / 2f, width / 2f, t);

        UnityEngine.Vector3 pos = pointer.localPosition;
        pos.x = x;

        pointer.localPosition = pos;
    }

    int CalculateScore()
    {
        float pointer_x = pointer.position.x;

        UnityEngine.Vector3[] corners = new UnityEngine.Vector3[4];
        redZone.GetWorldCorners(corners);

        float red_left_bounds = corners[0].x;
        float red_right_bounds = corners[3].x;

        float red_center = (red_left_bounds + red_right_bounds) * 0.5f;
        float half_width = (red_right_bounds - red_left_bounds) * 0.5f;

        float dist_to_red_center = Mathf.Abs(pointer_x - red_center);

        float t = dist_to_red_center / half_width;

        if (t >= 1f)
        {
            return 0;
        }

        float normalized = 1f - t;

        int score = Mathf.RoundToInt(normalized * maxScore);
        return score;

    }


    void LoadDefenceScene()
    {
        SceneManager.LoadScene("DefendScene", LoadSceneMode.Single);
    }

}
