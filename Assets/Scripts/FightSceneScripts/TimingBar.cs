using UnityEngine;
using TMPro;
using System.Collections;

public class TimingBar : MonoBehaviour
{
    public Transform bar;
    public Transform pointer;
    public Transform redZone;

    private AttackFlowController flow;

    public float speed = 1.5f;
    public int maxScore = 100;

    private float currPos;
    private SpriteRenderer barSR;
    private SpriteRenderer redSR;

    public BattleTimeBar timeBar;

    private bool hasFinished = false; // so they cant double press for more damage (that was a bug lolz)


    void Awake()
    {
        barSR = bar.GetComponent<SpriteRenderer>();
        redSR = redZone.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        currPos += Time.deltaTime * speed;

        if (currPos >= 1f)
        {
            speed = 0;
            currPos = 1f;

            UpdatePointerPosition(currPos);
            Debug.Log("Score: 0 --> miss");


            if (flow == null)
            {
                Debug.LogError("TimingBar: flow is null!");
                return;
            }
            hasFinished = true;
            flow.OnAttackFinished(0);
            return;
        }

        UpdatePointerPosition(currPos);

        if (hasFinished)
        {
            return;
        }


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
        if (barSR == null || pointer == null) return;

        float left = barSR.bounds.min.x;
        float right = barSR.bounds.max.x;

        float x = Mathf.Lerp(left, right, t);

        Vector3 pos = pointer.position;
        pos.x = x;
        pointer.position = pos;
    }



    private int CalculateDamage()
    {
        if (barSR == null || redSR == null || pointer == null) return 0;

        float pointerX = pointer.position.x;

        float redLeft = redSR.bounds.min.x;
        float redRight = redSR.bounds.max.x;

        // full damage: inside red zone
        if (pointerX >= redLeft && pointerX <= redRight)
        {
            return 2;   // full life
        }

        float barLeft = barSR.bounds.min.x;
        float barRight = barSR.bounds.max.x;

        if (pointerX >= barLeft && pointerX <= barRight)
        {
            return 1;   // half life
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
