using UnityEngine;
using TMPro;
using System.Collections;

public class TimingBar : MonoBehaviour
{
    public Transform pointer;
    public Transform redPart;
    public Transform greenPart;
    public Transform yellowPart;

    private AttackFlowController flow;

    public float speed = 1f;

    private float currPos;
    private SpriteRenderer redSR;
    private SpriteRenderer yellowSR;
    private SpriteRenderer greenSR;

    public BattleTimeBar timeBar;

    private bool hasFinished = false; // so they cant double press for more damage (that was a bug lolz)

    [Header("Speed Ramp")]
    public float speedIncrease = 0.3f;
    public float maxSpeed = 4f;

    [Header("Green Hit Effects")]
    public float slowMoTime = 0.15f;
    public float greenSlowScale = 0.15f;
    private Vector3 originalPointerPos;



    void Awake()
    {
        if (!(redPart != null || yellowPart != null || greenPart != null))
        {
            Debug.LogWarning("missing a part in attack in fight");
            return;
        }

        redSR = redPart.GetComponent<SpriteRenderer>();
        yellowSR = yellowPart.GetComponent<SpriteRenderer>();
        greenSR = greenPart.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (redPart == null || hasFinished)
        {
            return;
        }

        speed += speedIncrease * Time.deltaTime;
        speed = Mathf.Min(speed, maxSpeed);

        currPos += Time.deltaTime * speed;

        float ping = Mathf.PingPong(currPos, 1f);
        float t = 0.5f - 0.5f * Mathf.Cos(ping * Mathf.PI);

        UpdatePointerPosition(t);

        if (
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetMouseButtonDown(0)
        )
        {
            speed = 0;

            if (flow == null)
            {
                Debug.LogError("TimingBar: flow is null!");
                return;
            }

            int damage = CalculateDamage();

            hasFinished = true;

            flow.OnAttackFinished(damage);

        }

    }

    private void UpdatePointerPosition(float t)
    {
        if (redSR == null || pointer == null) return;

        float left = redSR.bounds.min.x;
        float right = redSR.bounds.max.x;

        float x = Mathf.Lerp(left, right, t);

        Vector3 pos = pointer.position;
        pos.x = x;
        pointer.position = pos;
    }



    private int CalculateDamage()
    {
        if (redPart == null || pointer == null)
        {
            return 0;
        }

        float pointerX = pointer.position.x;
        Debug.Log($"PointerX: {pointerX}");

        if (greenSR != null)
        {
            float gLeft = greenSR.bounds.min.x;
            float gRight = greenSR.bounds.max.x;
            Debug.Log($"Green: {gLeft}  {gRight}");

            if (pointerX >= gLeft && pointerX <= gRight)
            {
                Debug.Log("GREEN HIT");
                return 2;
            }
        }

        if (yellowSR != null)
        {
            float yLeft = yellowSR.bounds.min.x;
            float yRight = yellowSR.bounds.max.x;
            Debug.Log($"Yellow: {yLeft}  {yRight}");

            if (pointerX >= yLeft && pointerX <= yRight)
            {
                Debug.Log("YELLOW HIT");
                return 1;
            }
        }
        return 0;
    }



    public void StartTiming(AttackFlowController f)
    {
        Debug.Log("started timing bar");
        flow = f;
        currPos = 0f;
        speed = 1.5f;
        hasFinished = false;
    }


}
