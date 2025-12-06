using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;

        if (target == null)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                target = playerGO.transform;
        }

        if (gameManager != null && gameManager.hasSavedCameraOffset)
        {
            offset = gameManager.savedCameraOffset;
        }
        else if (target != null && offset == Vector3.zero)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                target = playerGO.transform;
            else
                return;
        }

        // Do not effect the rotation or the x position of the camera (only move up and down with the target)
        Vector3 newPosition = target.position + offset;
        newPosition.x = -0.5f;
        transform.position = newPosition;
    }
}
