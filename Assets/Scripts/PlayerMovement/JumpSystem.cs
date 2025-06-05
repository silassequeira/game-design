using UnityEngine;

[System.Serializable]
public class JumpSystem
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float maxJumpDuration = 0.3f;
    [SerializeField] private float jumpCutVelocityThreshold = 3f;
    [SerializeField] private float jumpControlForce = 5f;
    [SerializeField] private float jumpInputCooldown = 0.02f; 
    
    [Header("Double Jump Settings")]
    [SerializeField] private bool enableDoubleJump = false;
    [SerializeField] private float doubleJumpForce = 10f;
    [SerializeField] private bool requireButtonRelease = false;
    
    [Header("Advanced Settings")]
    [SerializeField] private bool resetJumpOnHeadbutt = true;
    [SerializeField] private float headbuttVelocityThreshold = -2f;
    [SerializeField] private bool allowJumpCancelOnFall = true;
    
    private bool isJumping = false;
    private bool jumpInputReleased = true;
    private float jumpHoldTimer = 0f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpInputTimer = 0f;
    private int jumpCount = 0;
    private bool wasHeadbutting = false;
    private bool wasGrounded = false; // Track previous ground state
    
    // Events
    public System.Action OnJump;
    public System.Action OnDoubleJump;
    public System.Action<Vector2> OnLand; // Modified to include landing position
    public System.Action OnHeadbutt;
    
    // Debug tracking
    public string LastJumpRejectionReason { get; private set; } = "";
    
    // Public properties
    public bool IsJumping => isJumping;
    public int JumpCount => jumpCount;
    public bool CanDoubleJump => enableDoubleJump && jumpCount == 1;
    public bool IsDoubleJumpEnabled => enableDoubleJump;
    public float JumpForce => jumpForce;
    public float DoubleJumpForce => doubleJumpForce;
    public float MaxJumpDuration => maxJumpDuration;
    public int MaxJumps => enableDoubleJump ? 2 : 1;
    public int RemainingJumps => MaxJumps - jumpCount;
    public bool JustLanded { get; private set; }
    
    // Setters for external configuration
    public void SetJumpForce(float force) => jumpForce = force;
    public void SetDoubleJumpForce(float force) => doubleJumpForce = force;
    
    public void SetDoubleJumpEnabled(bool enabled) 
    {
        enableDoubleJump = enabled;
    }
    
    public void SetMaxJumpDuration(float duration) => maxJumpDuration = duration;
    
    public void UpdateTimers()
    {
        // Reset just landed flag
        JustLanded = false;
        
        // Only decrease coyote time when not grounded
        if (coyoteTimeCounter > 0)
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
        
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        
        // Decrease jump input timer
        if (jumpInputTimer > 0)
        {
            jumpInputTimer -= Time.deltaTime;
        }
    }
    
    public void SetGrounded(bool grounded, Vector2 position)
    {
        // Check for landing
        JustLanded = grounded && !wasGrounded;
        
        if (grounded)
        {
            // Only trigger land event if we were actually in the air and jumping
            if (JustLanded && coyoteTimeCounter <= 0 && jumpCount > 0)
            {
                // Provide position data to OnLand event for precise surface detection
                OnLand?.Invoke(position);
            }
            
            coyoteTimeCounter = coyoteTime;
            
            // Reset jumping state when grounded
            isJumping = false;
            jumpHoldTimer = 0f;
            jumpCount = 0;
        }
        
        // Update previous ground state
        wasGrounded = grounded;
    }
    
    // Overload for backwards compatibility
    public void SetGrounded(bool grounded)
    {
        SetGrounded(grounded, Vector2.zero);
    }
    
    public void CheckHeadbutt(Rigidbody2D rb, bool hasHeadCollision)
    {
        // If player hits their head while jumping
        if (resetJumpOnHeadbutt && hasHeadCollision && rb.linearVelocity.y < headbuttVelocityThreshold && !wasHeadbutting)
        {
            wasHeadbutting = true;
            OnHeadbutt?.Invoke();
            
            // Trigger the jump cut logic to stop upward momentum
            if (allowJumpCancelOnFall)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }
        }
        else if (rb.linearVelocity.y >= 0 || !hasHeadCollision)
        {
            wasHeadbutting = false;
        }
    }
    
    public bool TryJump(bool jumpInput, bool jumpHeld, Rigidbody2D rb)
    {
        // Process jump release for input tracking
        if (!jumpHeld && !jumpInputReleased)
        {
            jumpInputReleased = true;
        }
        
        // Handle jump input
        if (jumpInput)
        {
            jumpBufferCounter = jumpBufferTime;
            
            // NORMAL JUMP - Check if we can do a first jump (on ground)
            if (jumpCount == 0 && coyoteTimeCounter > 0 && jumpInputTimer <= 0)
            {
                PerformJump(rb, false);
                return true;
            }
            
            // DOUBLE JUMP - Simplified, more permissive conditions
            if (enableDoubleJump && jumpCount == 1 && jumpInputTimer <= 0)
            {
                // Check if we need button release between jumps
                if (requireButtonRelease && !jumpInputReleased)
                {
                    LastJumpRejectionReason = "Button not released between jumps";
                    return false;
                }
                
                PerformJump(rb, true);
                return true;
            }
            
            // Track that button was pressed (for jumps that didn't happen)
            jumpInputReleased = false;
            
            // Debug info for why we couldn't jump
            if (jumpCount >= MaxJumps) 
                LastJumpRejectionReason = "Maximum jumps reached";
            else if (jumpInputTimer > 0)
                LastJumpRejectionReason = "Input cooldown active";
            else if (coyoteTimeCounter <= 0 && jumpCount == 0)
                LastJumpRejectionReason = "Not grounded and no double jump";
        }
        
        return false;
    }
    
    private void PerformJump(Rigidbody2D rb, bool isDoubleJump)
    {
        jumpCount++;
        float jumpPower = isDoubleJump ? doubleJumpForce : jumpForce;
        
        // Set y-velocity directly for consistent jump height
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
        
        isJumping = true;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        jumpHoldTimer = 0f;
        jumpInputTimer = jumpInputCooldown;
        
        // Trigger appropriate event
        if (isDoubleJump)
        {
            OnDoubleJump?.Invoke();
        }
        else
        {
            OnJump?.Invoke();
        }
    }
    
    public void HandleVariableJumpHeight(bool jumpHeld, Rigidbody2D rb)
    {
        if (!isJumping) return;
        
        // Add extra force while holding jump (rising only)
        if (rb.linearVelocity.y > 0 && jumpHeld && jumpHoldTimer < maxJumpDuration)
        {
            rb.AddForce(Vector2.up * jumpControlForce, ForceMode2D.Force);
            jumpHoldTimer += Time.deltaTime;
        }
        
        // Cut jump short if button released early (but only once)
        if (rb.linearVelocity.y > jumpCutVelocityThreshold && !jumpHeld && !jumpInputReleased)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            jumpInputReleased = true;
        }
        
        // Apply extra gravity when falling for better feel
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        // Apply lower gravity when doing a low jump
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }
    
    public void SetJumpInputCooldown(float cooldown)
    {
        jumpInputCooldown = Mathf.Max(0, cooldown);  // Ensure it's not negative
    }
    
    // Force a reset of the jump system (for testing)
    public void ResetJumpSystem()
    {
        jumpCount = 0;
        isJumping = false;
        jumpInputTimer = 0;
        jumpHoldTimer = 0;
        coyoteTimeCounter = 0;
        jumpBufferCounter = 0;
        jumpInputReleased = true;
        JustLanded = false;
    }
    
    // Debugging helper
    public string GetDebugStatus()
    {
        return $"Jumps: {jumpCount}/{MaxJumps}, DoubleJumpEnabled: {enableDoubleJump}, " +
               $"CanDoubleJump: {CanDoubleJump}, JumpInputTimer: {jumpInputTimer:F2}, " +
               $"InputReleased: {jumpInputReleased}, LastRejection: {LastJumpRejectionReason}";
    }
}