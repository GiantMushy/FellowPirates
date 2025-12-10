using UnityEngine;

public class SeaMonster : MonoBehaviour
{
    public Transform[] pathPoints;
    public float speed = 2f;

    int currentIndex = 0;

    void Start()
    {
        if (pathPoints != null && pathPoints.Length > 0)
            transform.position = pathPoints[0].position;
    }

    void Update()
    {
        if (pathPoints == null || pathPoints.Length == 0) return;

        Transform target = pathPoints[currentIndex];

        Vector3 toTarget = target.position - transform.position;
        float step = speed * Time.deltaTime;

        if (toTarget.magnitude <= step)
        {
            transform.position = target.position;
            currentIndex = (currentIndex + 1) % pathPoints.Length;
        }
        else
        {
            transform.position += toTarget.normalized * step;
        }
    }
}
