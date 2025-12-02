using UnityEngine;

public class PlayerFightController : MonoBehaviour
{
    public float speed = 0.1f;
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position = new UnityEngine.Vector3(transform.position.x, transform.position.y + speed, transform.position.z);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position = new UnityEngine.Vector3(transform.position.x, transform.position.y - speed, transform.position.z);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position = new UnityEngine.Vector3(transform.position.x + speed, transform.position.y, transform.position.z);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position = new UnityEngine.Vector3(transform.position.x - speed, transform.position.y, transform.position.z);

        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
            Debug.Log("OnTriggerEnter with: " + other.name + " (tag: " + other.tag + ")");

        if (other.CompareTag("Bomb"))
        {
            Debug.Log("collided with boooomb!!");
        }
    }
}
