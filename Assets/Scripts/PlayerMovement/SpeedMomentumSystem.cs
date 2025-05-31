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
    
    private float currentMoveSpeed;
    private float movementDirection = 0f;
    private float directionChangeTime;
    private float speedBuildupTimer = 0f;
    private bool isAccelerating = false;
    private bool hasReachedMaxSpeed = false;
    
    public float CurrentMoveSpeed => currentMoveSpeed;
    public bool HasReachedMaxSpeed => hasReachedMaxSpeed;
    public bool UseSpeedMomentum => useSpeedMomentum;
    public float InitialMoveSpeed => initialMoveSpeed;
    public float MaxMoveSpeed => maxMoveSpeed;
    public bool ResetSpeedOnJump => resetSpeedOnJump;
    
    public System.Action OnSpeedBoost;
    public System.Action OnMaxSpeedReached;
    
    public void Initialize()
    {
        currentMoveSpeed = initialMoveSpeed;
        directionChangeTime = Time.time;
    }
    
    public void SetSpeedSettings(float initial, float max)
    {
        initialMoveSpeed = initial;
        maxMoveSpeed = max;
        currentMoveSpeed = initial;
    }
    
    public void SetResetSpeedOnJump(bool reset)
    {
        resetSpeedOnJump = reset;
    }
    
    public void BoostSpeed(float amount)
    {
        if (!useSpeedMomentum) return;
        
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
        if (!useSpeedMomentum || !isGrounded) return;
        
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            // Track direction changes
            if (movementDirection != 0 && Mathf.Sign(horizontalInput) != Mathf.Sign(movementDirection))
            {
                directionChangeTime = Time.time;
            }
            movementDirection = horizontalInput;
            
            // Build up speed if moving consistently
            if (Mathf.Sign(horizontalInput) == Mathf.Sign(rb.linearVelocity.x) || Mathf.Abs(rb.linearVelocity.x) < 0.1f)
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
        else
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
    
    public void ResetMomentum()
    {
        currentMoveSpeed = initialMoveSpeed;
        speedBuildupTimer = 0f;
        isAccelerating = false;
        hasReachedMaxSpeed = false;
    }
    
    public void OnDirectionChange()
    {
        directionChangeTime = Time.time;
    }
    
    public float GetSpeedPercentage()
    {
        return Mathf.InverseLerp(initialMoveSpeed, maxMoveSpeed, currentMoveSpeed);
    }
}