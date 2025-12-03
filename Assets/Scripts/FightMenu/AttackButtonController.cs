using UnityEngine;
using UnityEngine.EventSystems;


public class AttackButtonController : MonoBehaviour
{
    public AttackFlowController flow;
    void Start()
    {
        // if (EventSystem.current != null)
        // {
            // EventSystem.current.SetSelectedGameObject(gameObject);
        // }
    }
    public void Attack()
    {
        Debug.Log("Attack button pressed");
        flow.StartAttack();
    }
}
