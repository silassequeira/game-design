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
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spr;
    private AudioSource audioSource;

    // Input
    private float horizontalInput;
    private bool jumpInput;
    private bool jumpInputHeld;
    private bool jumpInputReleasedSinceLastJump = true; // Track if jump button was released

    // State
    private bool isFacingRight = true;
    private bool spriteChanged = false;
    private CameraFollow cameraFollow;
    private float lastVerticalVelocity; // Track previous velocity for landing detection

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        audioSystem.Initialize(audioSource, this);

        // Initialize systems
        groundDetection.Initialize(transform.Find("GroundCheck"));
        visualEffects.Initialize(this);
        visualEffects.EnsureParticlesFollowPlayer(transform);

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

        // We'll still hook up the OnLand event for audio
        jumpSystem.OnLand += () =>
        {
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
        // Store previous velocity for landing detection
        lastVerticalVelocity = rb.linearVelocity.y;

        // Update ground detection
        groundDetection.UpdateGroundState();

        bool currentlyFalling = rb.linearVelocity.y < -0.1f && !groundDetection.IsGrounded;
        anim.SetBool("isFalling", currentlyFalling);

        // Check for landing using JustLanded()
        if (groundDetection.JustLanded())
        {
            // Calculate fall speed (how fast we were falling before landing)
            float fallSpeed = Mathf.Abs(lastVerticalVelocity);

            // Play landing particles with fall velocity info
            visualEffects.PlayLandParticles(fallSpeed);

            // Optional camera shake for heavy landings
            if (fallSpeed > 8f && cameraFollow != null)
            {
                cameraFollow.AddTrauma(fallSpeed * 0.03f);
            }
        }

        // Update jump system with ground state
        jumpSystem.SetGrounded(groundDetection.IsGrounded);
        anim.SetBool("isGrounded", groundDetection.IsGrounded); 

        // Sync particle positions
        visualEffects.SyncParticlesToPlayer(transform);

        HandleInput();
        HandleJumping();
        HandleSpeedMomentum();
        UpdateAnimations();
        UpdateVisuals();
        audioSystem.Update();
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

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Track if jump button was just pressed this frame
        jumpInput = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W);

        // Track if jump button is being held
        jumpInputHeld = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W);

        // Track when jump is released (important for double jump)
        if (!jumpInputHeld && !jumpInputReleasedSinceLastJump)
        {
            jumpInputReleasedSinceLastJump = true;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            anim.SetTrigger("Duck");
        }
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
            Debug.Log(jumpSystem.GetDebugStatus());

            // Also verify double jump is enabled
            //Debug.Log($"Double jump is {(jumpSystem.IsDoubleJumpEnabled ? "enabled" : "disabled")}");
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

        audioSystem.UpdateFootsteps(
            groundDetection.IsGrounded,
            rb.linearVelocity.x,
            movementSystem.MaxSpeed,
            transform.position
        );
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