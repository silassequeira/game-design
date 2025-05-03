using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spr;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float maxSpeed = 7f;
    [SerializeField] private float airControlMultiplier = 0.5f;
    
    [Header("Speed Momentum")]
    [SerializeField] private bool useSpeedMomentum = true;
    [SerializeField] private float initialMoveSpeed = 5f;       // Starting movement speed
    [SerializeField] private float maxMoveSpeed = 9f;           // Maximum movement speed after acceleration
    [SerializeField] private float speedBuildupRate = 1f;       // How quickly speed builds up (units/second)
    [SerializeField] private float speedBuildupDelay = 0.5f;    // Time before speed starts increasing
   // [SerializeField] private float speedRetentionTime = 1.0f;   // How long speed is retained when changing direction
    [SerializeField] private float speedLossRate = 2f;          // How quickly speed is lost when not moving
    [SerializeField] private bool resetSpeedOnJump = false;     // Whether to reset speed when jumping
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float maxJumpDuration = 0.3f;      // Maximum time jump can be held
    [SerializeField] private float jumpCutVelocityThreshold = 3f; // Minimum velocity required before jump cut applies
    [SerializeField] private float jumpControlForce = 5f;       // Force applied while holding jump button
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip speedBoostSound;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem landParticles;
    [SerializeField] private ParticleSystem speedParticles;
    [SerializeField] private GameObject shadowObject;
    [SerializeField] private Sprite newSprite;
    
    // State variables
    private bool isGrounded;
    private bool wasGrounded;
    private bool isFacingRight = true;
    private bool isJumping = false;
    private bool jumpInputReleased = true; // Track if jump button was released since last jump
    private bool spriteChanged = false;
    private float jumpHoldTimer = 0f;      // Track how long jump is being held
    
    // Speed momentum variables
    private float currentMoveSpeed;        // Current movement speed
    private float movementDirection = 0f;  // Last non-zero horizontal input direction
    private float lastMovementTime;        // Time of last movement input
    private float directionChangeTime;     // Time when direction was last changed
    private float lastSpeedIncreaseTime;   // Time when speed was last increased
    private float speedBuildupTimer = 0f;  // Timer for building up speed
    private bool isAccelerating = false;   // Whether player is currently accelerating
    private bool hasReachedMaxSpeed = false; // Whether player has reached max speed
    
    // Input handling
    private float horizontalInput;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool jumpInput;
    private bool jumpInputHeld;

    // Constants
    //private const float groundedRememberTime = 0.1f;
    //private float groundedRemember = 0;
    
    // References to other components
    private CameraFollow cameraFollow;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spr = GetComponent<SpriteRenderer>();
        
        // Initialize move speed
        currentMoveSpeed = initialMoveSpeed;
        moveSpeed = currentMoveSpeed;
        
        // Initialize audio source if not assigned
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize shadow
        if (shadowObject != null)
        {
            shadowObject.SetActive(true);
        }
        
        // Try to get camera follow component
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraFollow = mainCamera.GetComponent<CameraFollow>();
        }
        
        // Disable speed particles if they exist
        if (speedParticles != null)
        {
            speedParticles.Stop();
        }
    }
    
    private void Update()
    {
        // Input handling
        horizontalInput = Input.GetAxisRaw("Horizontal");
        jumpInput = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W);
        jumpInputHeld = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W);
        
        // Track direction changes for speed momentum
        if (useSpeedMomentum && Mathf.Abs(horizontalInput) > 0.1f)
        {
            // If direction changed
            if (movementDirection != 0 && Mathf.Sign(horizontalInput) != Mathf.Sign(movementDirection))
            {
                directionChangeTime = Time.time;
            }
            
            // Update movement direction
            movementDirection = horizontalInput;
            lastMovementTime = Time.time;
        }
        
        // Handle jump input with buffer
        if (jumpInput)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpInputReleased = false;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
            
            // Mark jump as released when player releases button
            if (!jumpInputHeld)
            {
                jumpInputReleased = true;
            }
        }
        
        // Handle crouch input
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            anim.SetTrigger("Duck");
        }
        
        // Apply jump if conditions are met
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isJumping)
        {
            Jump();
        }
        
        // Handle variable jump height
        if (isJumping)
        {
            // If ascending and holding jump
            if (rb.linearVelocity.y > 0 && jumpInputHeld)
            {
                if (jumpHoldTimer < maxJumpDuration)
                {
                    // Apply sustained upward force while button is held (up to max duration)
                    rb.AddForce(Vector2.up * jumpControlForce, ForceMode2D.Force);
                    jumpHoldTimer += Time.deltaTime;
                }
            }
            
            // If still going up but released jump button (short hop)
            if (rb.linearVelocity.y > jumpCutVelocityThreshold && !jumpInputHeld && !jumpInputReleased)
            {
                // Apply higher gravity to cut the jump short
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
                jumpInputReleased = true; // Mark as processed
            }
            
            // When falling, apply increased gravity for snappier feel
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
        }
        
        // Handle speed momentum
        if (useSpeedMomentum && isGrounded)
        {
            UpdateSpeedMomentum();
        }
        
        // Update animator parameters
        anim.SetBool("isWalking", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        if (hasReachedMaxSpeed)
        {
            //anim.SetBool("isRunning", true);
        }
        else
        {
            //anim.SetBool("isRunning", false);
        }
        
        // Check for sprite change condition
        if (!spriteChanged && transform.position.x >= 64f)
        {
            spr.sprite = newSprite;
            spriteChanged = true;
        }
    }
    
    private void UpdateSpeedMomentum()
    {
        // If we're moving in a consistent direction
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            // Ensure we're moving in the same direction as input
            if (Mathf.Sign(horizontalInput) == Mathf.Sign(rb.linearVelocity.x) || Mathf.Abs(rb.linearVelocity.x) < 0.1f)
            {
                // If we've been moving for the delay period, start building speed
                if (Time.time - directionChangeTime > speedBuildupDelay)
                {
                    if (!isAccelerating)
                    {
                        isAccelerating = true;
                        if (speedParticles != null && !speedParticles.isPlaying)
                        {
                            speedParticles.Play();
                        }
                    }
                    
                    // Increase speed over time
                    speedBuildupTimer += Time.deltaTime;
                    float newSpeed = Mathf.Lerp(initialMoveSpeed, maxMoveSpeed, speedBuildupTimer / speedBuildupRate);
                    
                    // If we've reached a new threshold speed
                    if (newSpeed > currentMoveSpeed + 0.5f && !hasReachedMaxSpeed)
                    {
                        if (speedBoostSound != null && audioSource != null)
                        {
                            audioSource.PlayOneShot(speedBoostSound);
                        }
                        
                        // Add a small camera shake for feedback
                        if (cameraFollow != null)
                        {
                            cameraFollow.AddTrauma(0.1f);
                        }
                    }
                    
                    // Check if we reached max speed
                    if (newSpeed >= maxMoveSpeed * 0.98f && !hasReachedMaxSpeed)
                    {
                        hasReachedMaxSpeed = true;
                        
                        // Indicate max speed attained
                        if (speedParticles != null)
                        {
                            var emission = speedParticles.emission;
                            emission.rateOverTime = emission.rateOverTime.constant * 2f;
                        }
                    }
                    
                    // Update speed
                    currentMoveSpeed = newSpeed;
                    moveSpeed = currentMoveSpeed;
                }
            }
        }
        else
        {
            // If not moving, gradually lose speed
            if (currentMoveSpeed > initialMoveSpeed)
            {
                currentMoveSpeed = Mathf.Max(initialMoveSpeed, 
                                            currentMoveSpeed - speedLossRate * Time.deltaTime);
                moveSpeed = currentMoveSpeed;
                
                // Reset acceleration state if speed is lost
                if (currentMoveSpeed <= initialMoveSpeed + 0.1f)
                {
                    ResetSpeedMomentum();
                }
            }
        }
    }
    
    private void ResetSpeedMomentum()
    {
        currentMoveSpeed = initialMoveSpeed;
        moveSpeed = currentMoveSpeed;
        speedBuildupTimer = 0f;
        isAccelerating = false;
        hasReachedMaxSpeed = false;
        
        if (speedParticles != null && speedParticles.isPlaying)
        {
            speedParticles.Stop();
        }
    }
    
    private void FixedUpdate()
    {
        // Ground check
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Handle coyote time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            
            // Reset jumping state when grounded
            if (rb.linearVelocity.y <= 0)
            {
                isJumping = false;
                jumpHoldTimer = 0f; // Reset jump hold timer when grounded
            }
            
            // Play landing effect when first touching ground
            if (!wasGrounded && rb.linearVelocity.y <= 0.1f)
            {
                if (landParticles != null)
                {
                    landParticles.Play();
                }
                
                if (landSound != null)
                {
                    audioSource.PlayOneShot(landSound);
                }
                
                // Add camera shake on landing based on fall distance
                if (cameraFollow != null)
                {
                    float fallDistance = Mathf.Abs(rb.linearVelocity.y);
                    if (fallDistance > 10f)
                    {
                        cameraFollow.AddTrauma(Mathf.Min(0.3f, fallDistance / 40f));
                    }
                }
            }
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
        
        // Movement handling with acceleration and deceleration
        float targetVelocityX = horizontalInput * moveSpeed;
        float accelerationRate = isGrounded ? acceleration : acceleration * airControlMultiplier;
        
        // Apply acceleration or deceleration based on input
        if (Mathf.Abs(targetVelocityX) > 0.1f)
        {
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(rb.linearVelocity.x, targetVelocityX, accelerationRate * Time.fixedDeltaTime),
                rb.linearVelocity.y
            );
        }
        else
        {
            // Apply deceleration when no input
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(rb.linearVelocity.x, 0f, deceleration * Time.fixedDeltaTime),
                rb.linearVelocity.y
            );
        }
        
        // Limit max horizontal speed
        float currentMaxSpeed = useSpeedMomentum ? Mathf.Max(maxSpeed, maxMoveSpeed) : maxSpeed;
        rb.linearVelocity = new Vector2(
            Mathf.Clamp(rb.linearVelocity.x, -currentMaxSpeed, currentMaxSpeed),
            rb.linearVelocity.y
        );
        
        // Handle sprite flipping
        if (rb.linearVelocity.x > 0.1f && !isFacingRight)
        {
            Flip();
        }
        else if (rb.linearVelocity.x < -0.1f && isFacingRight)
        {
            Flip();
        }
        
        // Update shadow position if present
        UpdateShadow();
    }
    
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isJumping = true;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        jumpHoldTimer = 0f; // Reset jump hold timer on new jump
        
        // Reset speed momentum if configured
        if (useSpeedMomentum && resetSpeedOnJump)
        {
            ResetSpeedMomentum();
        }
        
        // Play jump animation
        anim.SetTrigger("Jump");
        
        // Play jump particles
        if (jumpParticles != null)
        {
            jumpParticles.Play();
        }
        
        // Play jump sound
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }
    
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        
        // Reset speed buildup when changing direction
        if (useSpeedMomentum)
        {
            directionChangeTime = Time.time;
        }
    }
    
    private void UpdateShadow()
    {
        if (shadowObject != null)
        {
            // Raycast down to find ground
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 50f, groundLayer);
            if (hit.collider != null)
            {
                // Position shadow on ground below player
                shadowObject.transform.position = new Vector3(transform.position.x, hit.point.y + 0.05f, shadowObject.transform.position.z);
                
                // Scale shadow based on distance (smaller when farther)
                float distance = Mathf.Abs(transform.position.y - hit.point.y);
                float scale = Mathf.Max(0.5f, 1f - (distance / 10f));
                shadowObject.transform.localScale = new Vector3(scale, scale, 1f);
                
                // Fade shadow based on distance
                SpriteRenderer shadowRenderer = shadowObject.GetComponent<SpriteRenderer>();
                if (shadowRenderer != null)
                {
                    Color color = shadowRenderer.color;
                    color.a = Mathf.Max(0.1f, 0.5f - (distance / 20f));
                    shadowRenderer.color = color;
                }
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    
    // Public methods for external components
    public bool GetIsGrounded()
    {
        return isGrounded;
    }
    
    public float GetHorizontalVelocity()
    {
        return rb.linearVelocity.x;
    }
    
    public float GetVerticalVelocity()
    {
        return rb.linearVelocity.y;
    }
    
    public float GetCurrentMoveSpeed()
    {
        return currentMoveSpeed;
    }
    
    public float GetSpeedPercentage()
    {
        return Mathf.InverseLerp(initialMoveSpeed, maxMoveSpeed, currentMoveSpeed);
    }
    
    public bool IsAtMaxSpeed()
    {
        return hasReachedMaxSpeed;
    }
    
    public void SetExternalForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
    {
        rb.AddForce(force, mode);
    }
    
    public void SetExternalVelocity(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
    }
    
    public void ResetSpeed()
    {
        if (useSpeedMomentum)
        {
            ResetSpeedMomentum();
        }
    }
    
    public void BoostSpeed(float amount)
    {
        if (useSpeedMomentum)
        {
            currentMoveSpeed = Mathf.Min(maxMoveSpeed, currentMoveSpeed + amount);
            moveSpeed = currentMoveSpeed;
            
            // Calculate new buildup timer based on speed
            speedBuildupTimer = speedBuildupRate * (currentMoveSpeed - initialMoveSpeed) / (maxMoveSpeed - initialMoveSpeed);
            
            // Check if we reached max speed
            if (currentMoveSpeed >= maxMoveSpeed * 0.98f && !hasReachedMaxSpeed)
            {
                hasReachedMaxSpeed = true;
                
                // Indicate max speed attained
                if (speedParticles != null)
                {
                    var emission = speedParticles.emission;
                    emission.rateOverTime = emission.rateOverTime.constant * 2f;
                }
            }
        }
    }
}