using UnityEngine;
using System.Collections.Generic;

public class WaterEffect : MonoBehaviour
{
    public Transform target;
    public float pointSpacing = 0.1f;
    private LineRenderer lr;
    private List<Vector3> points = new();

    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (points.Count == 0 || Vector3.Distance(points[^1], target.position) > pointSpacing)
        {
            points.Add(target.position);
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
        }
    }
}