using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class AttackButtonController : MonoBehaviour
{
    public AttackFlowController flow;
    private bool isSelected = false;
    public Button attackButton;


    void Update()
    {
        if (EventSystem.current == null || attackButton == null || flow == null)
        {
            return;
        }

        if (!flow.buttonPanell.interactable)
        {
            if (isSelected)
            {
                isSelected = false;
            }
            return;
        }


        bool nowSelected = EventSystem.current.currentSelectedGameObject == attackButton.gameObject;

        if (nowSelected && !isSelected)
        {
            isSelected = true;
            flow.ShowAttackMessage();
        }
        else if (!nowSelected && isSelected)
        {
            isSelected = false;
        }
    }

    public void Attack()
    {
        flow.HideMiddleScreenMessage();
        Debug.Log("Attack button pressed");
        flow.StartAttack();
    }
}
