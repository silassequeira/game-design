using UnityEngine;

[System.Serializable]
public class GroundDetection
{
    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Head Check Settings")]
    [SerializeField] private float headCheckDistance = 0.6f;
    [SerializeField] private Vector2 headCheckOffset = new Vector2(0f, 0.5f);
    
    private bool isGrounded;
    private bool wasGrounded;
    private bool hasHeadCollision;
    
    public bool IsGrounded => isGrounded;
    public bool WasGrounded => wasGrounded;
    
    public void Initialize(Transform groundCheckTransform)
    {
        groundCheck = groundCheckTransform;
    }
    
    public void UpdateGroundState()
    {
        wasGrounded = isGrounded;
        
        // Check ground collision
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Check head collision
        Vector2 headCheckPosition = (Vector2)groundCheck.position + headCheckOffset;
        RaycastHit2D headHit = Physics2D.Raycast(
            headCheckPosition, 
            Vector2.up, 
            headCheckDistance, 
            groundLayer
        );
        
        hasHeadCollision = headHit.collider != null;
    }
    
    public bool JustLanded()
    {
        return isGrounded && !wasGrounded;
    }
    
    public bool HasHeadCollision()
    {
        return hasHeadCollision;
    }
    
    public void DrawGizmos()
    {
        if (groundCheck != null)
        {
            // Draw ground check circle
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            
            // Draw head check ray
            Vector2 headCheckPosition = (Vector2)groundCheck.position + headCheckOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(headCheckPosition, headCheckPosition + (Vector2.up * headCheckDistance));
        }
    }
}