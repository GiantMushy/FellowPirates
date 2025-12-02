using UnityEngine;

public class PlayerFightController : MonoBehaviour
{
    public float speed = 0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Debug.Log("up");
            transform.position = new UnityEngine.Vector3(transform.position.x, transform.position.y + speed, transform.position.z);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Debug.Log("down");
            transform.position = new UnityEngine.Vector3(transform.position.x, transform.position.y - speed, transform.position.z);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Debug.Log("right");
            transform.position = new UnityEngine.Vector3(transform.position.x + speed, transform.position.y, transform.position.z);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Debug.Log("left");
            transform.position = new UnityEngine.Vector3(transform.position.x - speed, transform.position.y, transform.position.z);

        }
    }
}
