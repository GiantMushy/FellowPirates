using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour
{
    public float maxSpeed;
    public float turnSpeed;
    public float acceleration;
    public float deceleration;
    public int health;
    
    private float speed;
    private bool disabled = false;
    private bool wantsToAccelerate;
    private bool wantsToDecelerate;
    private bool wantsToTurnPort;
    private bool wantsToTurnStarboard;

    public void SetAccelerate(bool value) { wantsToAccelerate = value; }
    public void SetDecelerate(bool value) { wantsToDecelerate = value; }
    public void SetTurnPort(bool value) { wantsToTurnPort = value; }
    public void SetTurnStarboard(bool value) { wantsToTurnStarboard = value; }
    public void SetSpeed(float value) { speed = value; }
    void FixedUpdate()
    {
        if (!disabled)
        {
            if (wantsToAccelerate) Accelerate();
            if (wantsToDecelerate) Decelerate();
            if (wantsToTurnPort) TurnPort();
            if (wantsToTurnStarboard) TurnStarboard();
        }
        Move();
    }
    void Move()
    {
        transform.Translate(Vector3.up * speed * Time.fixedDeltaTime);
    }
    void Accelerate()
    {
        speed = Mathf.Min(maxSpeed, speed + acceleration * Time.fixedDeltaTime);
    }
    void Decelerate()   { speed = Mathf.Max(0, speed - deceleration * Time.fixedDeltaTime); }
    void TurnPort()     { transform.Rotate(0, 0, turnSpeed * Time.fixedDeltaTime); }
    void TurnStarboard() { transform.Rotate(0, 0, -turnSpeed * Time.fixedDeltaTime); }
    
    public void DisableControl() { disabled = true; }
    public void EnableControl() { disabled = false; }
    public void Stop() { speed = 0f; }
}
