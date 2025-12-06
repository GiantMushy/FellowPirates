using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FleeButtonController : MonoBehaviour
{
    [SerializeField] private Button fleeButton;

    public void Flee()
    {
        Debug.Log("Flee button pressed");
        EventSystem.current.SetSelectedGameObject(fleeButton.gameObject);

        var gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.StartChase();
        }
        else
        {
            Debug.LogError("FleeButtonController: PlayerController.Instance is null!");
        }
    }
}
