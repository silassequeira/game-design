using UnityEngine;

[System.Serializable]
public class MovementSystem
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float maxSpeed = 7f;
    [SerializeField] private float airControlMultiplier = 0.5f;
    
    public float MoveSpeed => moveSpeed;
    public float MaxSpeed => maxSpeed;
    
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    public void SetMaxSpeed(float speed)
    {
        maxSpeed = speed;
    }
    
    public void ApplyMovement(float horizontalInput, bool isGrounded, Rigidbody2D rb)
    {
        float targetVelocityX = horizontalInput * moveSpeed;
        float accelerationRate = isGrounded ? acceleration : acceleration * airControlMultiplier;
        
        if (Mathf.Abs(targetVelocityX) > 0.1f)
        {
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(rb.linearVelocity.x, targetVelocityX, accelerationRate * Time.fixedDeltaTime),
                rb.linearVelocity.y
            );
        }
        else
        {
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(rb.linearVelocity.x, 0f, deceleration * Time.fixedDeltaTime),
                rb.linearVelocity.y
            );
        }
        
        // Limit max speed
        rb.linearVelocity = new Vector2(
            Mathf.Clamp(rb.linearVelocity.x, -maxSpeed, maxSpeed),
            rb.linearVelocity.y
        );
    }
}