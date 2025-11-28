using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    void LateUpdate()
    {
        // Do not effect the rotation or the x position of the camera (only move up and down with the target)
        Vector3 newPosition = target.position + offset;
        newPosition.x = -0.5f;
        transform.position = newPosition;
    }
}
