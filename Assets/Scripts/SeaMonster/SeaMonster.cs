using UnityEngine;

public class SeaMonster : MonoBehaviour
{
    public Transform[] pathPoints;
    public float speed = 2f;

    int currentIndex = 0;
    private bool forwards = true;

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

            if (forwards)
                currentIndex++;
            else
                currentIndex--;

            if (currentIndex >= pathPoints.Length)
                currentIndex = 0;
            else if (currentIndex < 0)
                currentIndex = pathPoints.Length - 1;
        }
        else
        {
            transform.position += toTarget.normalized * step;
        }
    }


    public void ReverseDirection()
    {
        forwards = !forwards;
    }
}
