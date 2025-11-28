using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private ShipController shipController;
    void Start()
    {
        shipController = GetComponent<ShipController>();
        if (shipController == null)
            Debug.LogError("EnemyController requires a ShipController component!");

        shipController.SetSpeed(0.8f);
        shipController.SetTurnStarboard(true);
    }
}
