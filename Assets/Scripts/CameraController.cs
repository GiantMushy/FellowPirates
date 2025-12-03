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

        if (target != null && offset == Vector3.zero)
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

                if (offset == Vector3.zero)
                    offset = transform.position - target.position;
            }
            else
            {
                // no valid target
                return;
            }
        }
        Vector3 newPosition = target.position + offset;
        newPosition.x = -0.5f;
        transform.position = newPosition;
    }
}
