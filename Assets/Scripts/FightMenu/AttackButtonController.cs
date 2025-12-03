using UnityEngine;
using UnityEngine.EventSystems;


public class AttackButtonController : MonoBehaviour
{
    public AttackFlowController flow;
    void Start()
    { 
    }
    public void Attack()
    {
        Debug.Log("Attack button pressed");
        flow.StartAttack();
    }
}
