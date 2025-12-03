using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    void Start()
    {
        if (target == null && PlayerController.Instance != null)
        {
            target = PlayerController.Instance.transform;
        }

        if (PlayerController.Instance != null && PlayerController.Instance.hasSavedCameraOffset)
        {
            offset = PlayerController.Instance.savedCameraOffset;
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
            if (PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }
            else
            {
                return;
            }
        }

        Vector3 newPosition = target.position + offset;
        newPosition.x = -0.5f;
        transform.position = newPosition;
    }
}
