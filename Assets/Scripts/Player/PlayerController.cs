using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private PlayerAnimationController animController;
    private float horizontalInput;
    private bool isGrounded;
    private bool isJumping;
    private bool facingRight = true;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animController = GetComponent<PlayerAnimationController>();
    }
    
    private void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Playing)
            return;
            
        // Get input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Jump input
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            AudioManager.Instance.PlaySFX("jump");
        }
        
        // Update animations
        UpdateAnimationState();
        
        // Flip character if needed
        if (horizontalInput > 0 && !facingRight)
            Flip();
        else if (horizontalInput < 0 && facingRight)
            Flip();
    }
    
    private void FixedUpdate()
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Playing)
            return;
            
        // Handle movement
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        
        // Handle jump
        if (isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = false;
        }
        
        // Better jump physics
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
    
    private void UpdateAnimationState()
    {
        // Set animation parameters
        animController.SetGrounded(isGrounded);
        animController.SetVerticalVelocity(rb.velocity.y);
        animController.SetHorizontalVelocity(Mathf.Abs(horizontalInput));
    }
    
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}