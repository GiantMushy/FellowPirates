using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FleeButtonController : MonoBehaviour
{
    [SerializeField] private Button fleeButton;
    public AttackFlowController flow;
    private bool isSelected = false;
    public AudioClip clickSound;

    void Update()
    {
        if (EventSystem.current == null || fleeButton == null || flow == null)
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


        bool nowSelected = EventSystem.current.currentSelectedGameObject == fleeButton.gameObject;

        if (nowSelected && !isSelected)
        {
            isSelected = true;
            flow.ShowFleeMessage();
        }
        else if (!nowSelected && isSelected)
        {
            isSelected = false;
        }
    }

    public void Flee()
    {
        Debug.Log("Flee button pressed");
        if (clickSound != null && SoundEffectManager.instance != null)
            SoundEffectManager.instance.PlaySoundClip(clickSound, transform, 0.5f);
        EventSystem.current.SetSelectedGameObject(fleeButton.gameObject);

        var gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            flow.HideMiddleScreenMessage();
            gameManager.StartChase();
        }
        else
        {
            Debug.LogError("FleeButtonController: PlayerController.Instance is null!");
        }
    }
}
