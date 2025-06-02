using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GroundDetection groundDetection = new GroundDetection();
    [SerializeField] private JumpSystem jumpSystem = new JumpSystem();
    [SerializeField] private MovementSystem movementSystem = new MovementSystem();
    [SerializeField] private SpeedMomentumSystem speedMomentum = new SpeedMomentumSystem();
    [SerializeField] private PlayerAudioSystem audioSystem = new PlayerAudioSystem();
    [SerializeField] private PlayerVisualEffects visualEffects = new PlayerVisualEffects();

    [Header("References")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Sprite newSprite;

    // Core components
    private IPlayerInput inputSystem;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spr;
    private AudioSource audioSource;
    private CameraFollow cameraFollow;

    // Input
    private float horizontalInput;
    private bool jumpInput;
    private bool jumpInputHeld;
    private bool jumpInputReleasedSinceLastJump = true; // Track if jump button was released

    // State
    private bool isFacingRight = true;
    private bool spriteChanged = false;
    private float lastVerticalVelocity; // Track previous velocity for landing detection

private void Awake()
{
    // Make sure this is properly set
    inputSystem = new DefaultPlayerInput();
    
    // Debug verification
    //Debug.Log($"PlayerMovement: Initialized with {inputSystem.GetType().Name}");
}

private void Start()
{
    rb = GetComponent<Rigidbody2D>();
    anim = GetComponent<Animator>();
    spr = GetComponent<SpriteRenderer>();
    audioSource = GetComponent<AudioSource>();

    audioSystem.Initialize(audioSource, this);

    // Initialize systems
    if (groundDetection == null)
    {
        groundDetection = new GroundDetection();
        groundDetection.Initialize(transform.Find("GroundCheck"));
    }
    visualEffects.Initialize(this);
    visualEffects.EnsureParticlesFollowPlayer(transform);
    speedMomentum.Initialize();

    jumpSystem.OnJump += () =>
    {
        anim.SetTrigger("Jump");
        audioSystem.PlayJumpSound(false);
        visualEffects.PlayJumpParticles(false, isFacingRight);
    };

    jumpSystem.OnDoubleJump += () =>
    {
        anim.SetTrigger("DoubleJump");
        audioSystem.PlayJumpSound(true);
        visualEffects.PlayJumpParticles(true, isFacingRight);
    };

    // FIXED: Added the position parameter to match the delegate signature
    jumpSystem.OnLand += (landingPosition) =>
    {
        // Make sure we have the most current surface before playing the sound
        groundDetection.UpdateGroundState();
        audioSystem.UpdateCurrentSurface(groundDetection);
        audioSystem.PlayLandSound();
    };

    speedMomentum.OnSpeedBoost += () =>
    {
        audioSystem.PlaySpeedBoostSound();
        if (cameraFollow != null) cameraFollow.AddTrauma(0.1f);
    };

    speedMomentum.OnMaxSpeedReached += () =>
    {
        visualEffects.EnhanceSpeedParticles();
    };

    // Get camera follow
    Camera mainCamera = Camera.main;
    if (mainCamera != null)
    {
        cameraFollow = mainCamera.GetComponent<CameraFollow>();
    }
}
private void Update()
{
    // Store previous states
    bool wasGrounded = groundDetection.IsGrounded;
    float previousVerticalVelocity = rb.linearVelocity.y;
    
    // Update ground detection FIRST to get fresh surface data
    groundDetection.UpdateGroundState();
    
    // Handle landing with the updated surface info
    bool justLanded = !wasGrounded && groundDetection.IsGrounded;
    
    // Make sure audio system has the latest surface info before playing sounds
    if (audioSystem != null)
    {
        audioSystem.UpdateCurrentSurface(groundDetection);
    }
    
    if (justLanded || groundDetection.SurfaceJustChanged)
    {
        // Force audio system to use the correct surface
        audioSystem.UpdateCurrentSurface(groundDetection);
        
        #if UNITY_EDITOR
        // Debug info
        if (justLanded)
        {
            Debug.Log($"Landing on {groundDetection.CurrentSurface}");
        }
        else if (groundDetection.SurfaceJustChanged)
        {
            Debug.Log($"Surface transitioned to {groundDetection.CurrentSurface}");
        }
        #endif
        
        // Play landing sound AFTER surface has been updated
        audioSystem.PlayLandSound();
        
        // Play landing particles
        float fallSpeed = Mathf.Abs(previousVerticalVelocity);
        visualEffects.PlayLandParticles(fallSpeed);
        
        // Optional camera shake for heavy landings
        if (fallSpeed > 8f && cameraFollow != null)
        {
            cameraFollow.AddTrauma(fallSpeed * 0.03f);
        }
    }

    // Update jump system with ground state and position
    jumpSystem.SetGrounded(groundDetection.IsGrounded, transform.position);
    
    // Clear the jumpSystem.OnLand event to prevent double sound playing
    jumpSystem.OnLand = null;
    
    // Set animation parameters
    anim.SetBool("isGrounded", groundDetection.IsGrounded); 
    
    // Update falling animation
    bool isFalling = rb.linearVelocity.y < -0.1f && !groundDetection.IsGrounded;
    anim.SetBool("isFalling", isFalling);

    // Sync particle positions with player
    visualEffects.SyncParticlesToPlayer(transform);

    // Read player input
    ReadInput();
    
    // Handle player actions
    HandleJumping();
    HandleSpeedMomentum();
    HandleDuck(); // Make sure this method is called
    
    // Update visuals
    UpdateAnimations();
    UpdateVisuals();
    
    // Update audio system timers
    audioSystem.Update();
    
    // Store current velocity for next frame
    lastVerticalVelocity = rb.linearVelocity.y;
}

    private void FixedUpdate()
    {
        jumpSystem.UpdateTimers();

        HandleMovement();
        UpdateSpriteFlipping();

        // Check for headbutt
        jumpSystem.CheckHeadbutt(rb, groundDetection.HasHeadCollision());

        visualEffects.UpdateShadow(transform);

        visualEffects.UpdateRunningParticles(
            speedMomentum.HasReachedMaxSpeed,
            isFacingRight
        );
    }

    private void ReadInput()
    {

    //Debug.Log($"Reading input from {inputSystem?.GetType().Name}, value: {inputSystem?.GetHorizontalInput()}");

        horizontalInput = inputSystem.GetHorizontalInput();
        jumpInput = inputSystem.GetJumpInputDown();
        jumpInputHeld = inputSystem.GetJumpInputHeld();
   
    }


    private void HandleJumping()
    {
        // Track if we jumped successfully
        bool jumpedThisFrame = jumpSystem.TryJump(jumpInput, jumpInputHeld, rb);

        // Reset the released flag only if we actually performed a jump
        if (jumpedThisFrame)
        {
            jumpInputReleasedSinceLastJump = false;
        }

        // Always handle variable jump height
        jumpSystem.HandleVariableJumpHeight(jumpInputHeld, rb);

        // Debug: Show jump system status when F2 key is pressed
        if (Input.GetKeyDown(KeyCode.F2))
        {
            //Debug.Log(jumpSystem.GetDebugStatus());
        }

        // Debug: Force reset jump system when F3 key is pressed
        if (Input.GetKeyDown(KeyCode.F3))
        {
            jumpSystem.ResetJumpSystem();
        }
    }

    private void HandleMovement()
    {
        // Update movement speed from momentum system
        if (speedMomentum.UseSpeedMomentum)
        {
            movementSystem.SetMoveSpeed(speedMomentum.CurrentMoveSpeed);
        }

        movementSystem.ApplyMovement(horizontalInput, groundDetection.IsGrounded, rb);
    }

    private void HandleSpeedMomentum()
    {
        speedMomentum.UpdateMomentum(horizontalInput, groundDetection.IsGrounded, rb);

        // Handle visual effects
        if (speedMomentum.HasReachedMaxSpeed)
        {
            visualEffects.StartSpeedParticles(isFacingRight);
        }
        else
        {
            visualEffects.StopSpeedParticles();
        }
    }

    private void UpdateAnimations()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("isWalking", Mathf.Abs(rb.linearVelocity.x) > 0.1f);

        audioSystem.UpdateFootsteps(groundDetection, rb.linearVelocity.x, movementSystem.MaxSpeed);
    }

private void HandleDuck()
{
    if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
    {
        anim.SetTrigger("Duck");
        audioSystem.PlayCrouchSound(); // Play surface-specific crouch sound
    }
}

    private void UpdateSpriteFlipping()
    {
        if (rb.linearVelocity.x > 0.1f && !isFacingRight)
        {
            Flip();
        }
        else if (rb.linearVelocity.x < -0.1f && isFacingRight)
        {
            Flip();
        }
    }

    private void UpdateVisuals()
    {
        // Handle sprite change
        if (!spriteChanged && transform.position.x >= 64f)
        {
            spr.sprite = newSprite;
            spriteChanged = true;
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        // Update particle direction
        visualEffects.UpdateParticleDirection(isFacingRight);

        speedMomentum.OnDirectionChange();
    }

    private void OnDrawGizmosSelected()
    {
        groundDetection.DrawGizmos();
    }

public IPlayerInput GetInputSystem()
{
    if (inputSystem == null)
    {
        //Debug.LogWarning("Input system is null, creating new DefaultPlayerInput");
        inputSystem = new DefaultPlayerInput();
    }
    return inputSystem;
}

public void SetInputSystem(IPlayerInput newInputSystem)
{
    if (newInputSystem == null)
    {
        //Debug.LogError("Attempted to set null input system!");
        return;
    }
    
    inputSystem = newInputSystem;
    //Debug.Log($"PlayerMovement: Input system set to {newInputSystem.GetType().Name}");
}

    // Public API - Getters
    public bool GetIsGrounded() => groundDetection.IsGrounded;
    public float GetHorizontalVelocity() => rb.linearVelocity.x;
    public float GetVerticalVelocity() => rb.linearVelocity.y;
    public float GetCurrentMoveSpeed() => speedMomentum.CurrentMoveSpeed;
    public float GetSpeedPercentage() => speedMomentum.GetSpeedPercentage();
    public bool IsAtMaxSpeed() => speedMomentum.HasReachedMaxSpeed;
    public bool CanDoubleJump() => jumpSystem.CanDoubleJump;
    public bool IsFacingRight() => isFacingRight;

    // Public API - Actions
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
        speedMomentum.ResetMomentum();
    }

    public void BoostSpeed(float amount)
    {
        speedMomentum.BoostSpeed(amount);
    }

    public void ForceJump(float jumpHeight = 0f)
    {
        float force = jumpHeight > 0f ? jumpHeight : jumpSystem.JumpForce;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        anim.SetTrigger("Jump");
        audioSystem.PlayJumpSound(false);
        visualEffects.PlayJumpParticles(false, isFacingRight);
    }
    
    public MovementSystem GetMovementSystem() => movementSystem;
    public SpeedMomentumSystem GetSpeedMomentumSystem() => speedMomentum;
    public JumpSystem GetJumpSystem() => jumpSystem;
    public PlayerAudioSystem GetAudioSystem() => audioSystem;
    public PlayerVisualEffects GetVisualEffects() => visualEffects;

    public void SetMovementSpeed(float speed) => movementSystem.SetMoveSpeed(speed);
    public void SetMaxSpeed(float maxSpeed) => movementSystem.SetMaxSpeed(maxSpeed);
    public void SetJumpForce(float force) => jumpSystem.SetJumpForce(force);
    public void SetDoubleJumpForce(float force) => jumpSystem.SetDoubleJumpForce(force);
    public void SetDoubleJumpEnabled(bool enabled) => jumpSystem.SetDoubleJumpEnabled(enabled);
    public void SetSpeedMomentumSettings(float initial, float max) => speedMomentum.SetSpeedSettings(initial, max);
    public void SetMaxJumpDuration(float duration) => jumpSystem.SetMaxJumpDuration(duration);
    public void SetJumpInputCooldown(float cooldown) => jumpSystem.SetJumpInputCooldown(cooldown);
}

// Default input implementation that uses Unity's Input system
public class DefaultPlayerInput : IPlayerInput
{
    public float GetHorizontalInput()
    {
        return Input.GetAxisRaw("Horizontal");
    }
    
    public bool GetJumpInputDown()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
    }
    
    public bool GetJumpInputHeld()
    {
        return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
    }
}