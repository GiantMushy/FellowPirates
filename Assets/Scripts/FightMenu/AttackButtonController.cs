using UnityEngine;

public class AttackButtonController : MonoBehaviour
{
    public AttackFlowController flow;

    public void Attack()
    {
        Debug.Log("Attack button pressed");
        flow.StartAttack();
    }
}
