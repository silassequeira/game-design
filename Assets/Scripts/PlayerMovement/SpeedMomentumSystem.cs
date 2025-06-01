using UnityEngine;

[System.Serializable]
public class SpeedMomentumSystem
{
    [Header("Speed Momentum")]
    [SerializeField] private bool useSpeedMomentum = true;
    [SerializeField] private float initialMoveSpeed = 5f;
    [SerializeField] private float maxMoveSpeed = 9f;
    [SerializeField] private float speedBuildupRate = 1f;
    [SerializeField] private float speedBuildupDelay = 0.5f;
    [SerializeField] private float speedLossRate = 2f;
    [SerializeField] private bool resetSpeedOnJump = false;
    
    [Header("Double Tap Sprint")]
    [SerializeField] private bool enableDoubleTapSprint = true;
    [SerializeField] private float doubleTapTimeWindow = 0.3f; // Time window for double tap detection
    [SerializeField] private float inputLossThreshold = 0.15f; // Time to wait before considering input lost
    
    private float currentMoveSpeed;
    private float movementDirection = 0f;
    private float directionChangeTime;
    private float speedBuildupTimer = 0f;
    private bool isAccelerating = false;
    private bool hasReachedMaxSpeed = false;
    
    // Double tap sprint variables
    private float lastLeftTapTime = -10f;
    private float lastRightTapTime = -10f;
    private bool isSprinting = false;
    private int sprintDirection = 0; // -1 for left, 1 for right, 0 for not sprinting
    private float lastMovementInputTime = 0f;
    
    public float CurrentMoveSpeed => currentMoveSpeed;
    public bool HasReachedMaxSpeed => hasReachedMaxSpeed;
    public bool UseSpeedMomentum => useSpeedMomentum;
    public float InitialMoveSpeed => initialMoveSpeed;
    public float MaxMoveSpeed => maxMoveSpeed;
    public bool ResetSpeedOnJump => resetSpeedOnJump;
    public bool IsSprinting => isSprinting;
    
    public System.Action OnSpeedBoost;
    public System.Action OnMaxSpeedReached;
    
    public void Initialize()
    {
        currentMoveSpeed = initialMoveSpeed;
        directionChangeTime = Time.time;
        isSprinting = false;
        sprintDirection = 0;
    }
    
    public void SetSpeedSettings(float initial, float max)
    {
        initialMoveSpeed = initial;
        maxMoveSpeed = max;
        
        // IMPORTANT: Don't reset current speed if sprinting
        if (!isSprinting)
        {
            currentMoveSpeed = initial;
        }
        else
        {
            // Maintain sprint at new max speed
            currentMoveSpeed = max;
        }
    }
    
    public void SetResetSpeedOnJump(bool reset)
    {
        resetSpeedOnJump = reset;
    }
    
    public void BoostSpeed(float amount)
    {
        if (!useSpeedMomentum || isSprinting) return; // Don't boost during sprint
        
        currentMoveSpeed = Mathf.Min(maxMoveSpeed, currentMoveSpeed + amount);
        
        // Calculate new buildup timer based on speed
        speedBuildupTimer = speedBuildupRate * (currentMoveSpeed - initialMoveSpeed) / (maxMoveSpeed - initialMoveSpeed);
        
        // Check if reached max speed
        if (currentMoveSpeed >= maxMoveSpeed * 0.98f && !hasReachedMaxSpeed)
        {
            hasReachedMaxSpeed = true;
            OnMaxSpeedReached?.Invoke();
        }
    }
    
    public void UpdateMomentum(float horizontalInput, bool isGrounded, Rigidbody2D rb)
    {
        if (!useSpeedMomentum) return;
        
        // Update last movement input time when there is significant input
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            lastMovementInputTime = Time.time;
        }
        
        // For double-tap sprint detection - only allow starting sprint when grounded
        if (enableDoubleTapSprint && isGrounded && !isSprinting)
        {
            // Check for key down events (detect when key is first pressed)
            bool leftKeyDown = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
            bool rightKeyDown = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
            
            // Left double tap
            if (leftKeyDown)
            {
                if (Time.time - lastLeftTapTime < doubleTapTimeWindow)
                {
                    StartSprint(-1);
                    Debug.Log("Sprint Started (Left)");
                }
                lastLeftTapTime = Time.time;
            }
            
            // Right double tap
            if (rightKeyDown)
            {
                if (Time.time - lastRightTapTime < doubleTapTimeWindow)
                {
                    StartSprint(1);
                    Debug.Log("Sprint Started (Right)");
                }
                lastRightTapTime = Time.time;
            }
        }
        
        // CRITICAL FIX: ONLY check input for sprint, ignore grounded state
        if (isSprinting)
        {
            // Check if player is still providing input in the sprint direction
            bool isStillHoldingDirection = false;
            
            if (sprintDirection < 0 && horizontalInput < -0.1f)
            {
                isStillHoldingDirection = true; // Still pressing left
            }
            else if (sprintDirection > 0 && horizontalInput > 0.1f)
            {
                isStillHoldingDirection = true; // Still pressing right
            }
            
            // Add input loss threshold before ending sprint
            bool inputLost = Time.time - lastMovementInputTime > inputLossThreshold;
            
            // ONLY end sprint if input is lost, regardless of grounded state
            if (inputLost || !isStillHoldingDirection)
            {
                EndSprint();
                Debug.Log("Sprint Ended - Input stopped or changed direction");
            }
            else
            {
                // We're still actively sprinting - maintain max speed
                currentMoveSpeed = maxMoveSpeed;
                
                // If we're not on ground, just maintain sprint but don't return
                // This lets the air movement code still work properly
                if (isGrounded)
                {
                    return; // Skip normal momentum when grounded and sprinting
                }
            }
        }
        
        // Normal momentum behavior (executes if not sprinting or if sprinting but in air)
        
        // Skip momentum updates when in the air (except sprint momentum which we keep)
        if (!isGrounded && !isSprinting) return;
        
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            // Track direction changes
            if (movementDirection != 0 && Mathf.Sign(horizontalInput) != Mathf.Sign(movementDirection))
            {
                directionChangeTime = Time.time;
            }
            movementDirection = horizontalInput;
            
            // Build up speed if moving consistently (only applies when not sprinting)
            if (!isSprinting && (Mathf.Sign(horizontalInput) == Mathf.Sign(rb.linearVelocity.x) || Mathf.Abs(rb.linearVelocity.x) < 0.1f))
            {
                if (Time.time - directionChangeTime > speedBuildupDelay)
                {
                    if (!isAccelerating)
                    {
                        isAccelerating = true;
                    }
                    
                    speedBuildupTimer += Time.deltaTime;
                    float newSpeed = Mathf.Lerp(initialMoveSpeed, maxMoveSpeed, speedBuildupTimer / speedBuildupRate);
                    
                    // If significant speed boost
                    if (newSpeed > currentMoveSpeed + 0.5f && !hasReachedMaxSpeed)
                    {
                        OnSpeedBoost?.Invoke();
                    }
                    
                    // If reached max speed
                    if (newSpeed >= maxMoveSpeed * 0.98f && !hasReachedMaxSpeed)
                    {
                        hasReachedMaxSpeed = true;
                        OnMaxSpeedReached?.Invoke();
                    }
                    
                    currentMoveSpeed = newSpeed;
                }
            }
        }
        else if (!isSprinting) // Only reduce speed if not sprinting
        {
            // Lose speed when not moving
            if (currentMoveSpeed > initialMoveSpeed)
            {
                currentMoveSpeed = Mathf.Max(initialMoveSpeed, currentMoveSpeed - speedLossRate * Time.deltaTime);
                
                if (currentMoveSpeed <= initialMoveSpeed + 0.1f)
                {
                    ResetMomentum();
                }
            }
        }
    }
    
    // Force sprint for state changers
    public void ForceSprint(int direction)
    {
        if (direction != 0)
        {
            StartSprint(direction);
        }
    }
    
    private void StartSprint(int direction)
    {
        isSprinting = true;
        sprintDirection = direction;
        currentMoveSpeed = maxMoveSpeed;
        hasReachedMaxSpeed = true;
        lastMovementInputTime = Time.time;
        OnMaxSpeedReached?.Invoke();
    }
    
    private void EndSprint()
    {
        isSprinting = false;
        sprintDirection = 0;
        ResetMomentum();
    }
    
    public void ResetMomentum()
    {
        // Don't reset if actively sprinting
        if (isSprinting) return;
        
        currentMoveSpeed = initialMoveSpeed;
        speedBuildupTimer = 0f;
        isAccelerating = false;
        hasReachedMaxSpeed = false;
    }
    
    public void OnDirectionChange()
    {
        directionChangeTime = Time.time;
        
        // End sprint if direction changed significantly
        if (isSprinting)
        {
            EndSprint();
        }
    }
    
    public float GetSpeedPercentage()
    {
        return Mathf.InverseLerp(initialMoveSpeed, maxMoveSpeed, currentMoveSpeed);
    }
}