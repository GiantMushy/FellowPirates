using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    public float lifetime = 1f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
