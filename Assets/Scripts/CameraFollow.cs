using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;  // Reference to the player's transform
    [SerializeField] private bool enableFollowing = false;
    [SerializeField] private bool useTargetVelocity = true;  // Whether to look ahead based on velocity
    [SerializeField] private float lookAheadFactor = 3.0f;   // How far to look ahead based on player speed
    [SerializeField] private float lookAheadReturnSpeed = 2.0f;  // How fast to return to normal position
    [SerializeField] private float lookAheadMoveThreshold = 0.1f;  // Minimum player speed to trigger look ahead

    [Header("Follow Settings")]
    [SerializeField] private Vector2 followOffset = new Vector2(0f, 1f);  // Offset from target position
    [SerializeField] private float smoothTimeX = 0.15f;  // Horizontal smoothing time
    [SerializeField] private float smoothTimeY = 0.15f;  // Vertical smoothing time
    [SerializeField] private bool followY = true;        // Whether to follow on the Y axis
    [SerializeField] private float verticalDeadzone = 2f;  // Vertical deadzone before camera starts moving

    [Header("Bounds Settings")]
    [SerializeField] private bool useBounds = false;     // Whether to use camera bounds
    [SerializeField] private float leftBound = -10f;     // Left boundary of camera movement
    [SerializeField] private float rightBound = 10f;     // Right boundary of camera movement
    [SerializeField] private float bottomBound = -10f;   // Bottom boundary of camera movement
    [SerializeField] private float topBound = 10f;       // Top boundary of camera movement

    [Header("Effects")]
    [SerializeField] private bool useScreenShake = true;  // Whether to use screen shake
    [SerializeField] private float traumaDecay = 1.5f;    // How quickly trauma decays
    [SerializeField] private float maxShakeAmount = 1.0f; // Maximum shake amplitude

    // Runtime variables
    private Vector3 lastTargetPosition;
    private Vector3 currentVelocity;
    private float lookAheadPos;
    private float currentLookAheadX;
    private float lastLookAheadXPos;
    private float targetLookAheadX;
    private float trauma = 0f;  // Current trauma level for screen shake (0-1)

    // Component references
    private Rigidbody2D targetRigidbody;
    private PlayerMovement playerMovement;

    void Awake()
    {
        if (target == null)
        {
            // Try to find player if not assigned
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("No target assigned to CameraFollow and no Player tag found!");
            }
        }

        if (target != null)
        {
            // Cache components
            targetRigidbody = target.GetComponent<Rigidbody2D>();
            playerMovement = target.GetComponent<PlayerMovement>();
            lastTargetPosition = target.position;
        }
    }

    void LateUpdate()
    {
        
        if (target == null || !ShouldFollowTarget())
            return;

        float deltaX = 0;
        float deltaY = 0;
        
        // Get current camera position
        Vector3 aheadTargetPos = target.position + Vector3.right * followOffset.x + Vector3.up * followOffset.y;

        // Calculate look-ahead based on player velocity
        if (useTargetVelocity && targetRigidbody != null)
        {
            // Get the facing direction from player
            bool isFacingRight = true;
            if (playerMovement != null)
            {
                // Determine facing direction from player velocity or stored value
                float velocityX = playerMovement.GetHorizontalVelocity();
                isFacingRight = velocityX > 0 || (velocityX == 0 && transform.position.x < target.position.x);
            }
            else
            {
                // Fallback if no player movement script
                isFacingRight = targetRigidbody.linearVelocity.x > 0;
            }

            // Calculate look ahead distance based on player speed
            float speedX = Mathf.Abs(targetRigidbody.linearVelocity.x);
            if (speedX > lookAheadMoveThreshold)
            {
                targetLookAheadX = lookAheadFactor * Mathf.Sign(targetRigidbody.linearVelocity.x) * 
                                   Mathf.Min(speedX * 0.5f, 1f);
            }
            else
            {
                targetLookAheadX = Mathf.MoveTowards(targetLookAheadX, 0, lookAheadReturnSpeed * Time.deltaTime);
            }

            currentLookAheadX = Mathf.MoveTowards(currentLookAheadX, targetLookAheadX, 
                                                 lookAheadReturnSpeed * Time.deltaTime);
            
            deltaX = currentLookAheadX;
        }

        // Follow target horizontally with smoothing
        float targetX = aheadTargetPos.x + deltaX;
        
        // Follow target vertically with smoothing and deadzone
        float targetY = transform.position.y;
        if (followY)
        {
            // Apply deadzone to vertical following
            float verticalDistance = Mathf.Abs(transform.position.y - (aheadTargetPos.y + deltaY));
            if (verticalDistance > verticalDeadzone)
            {
                targetY = Mathf.Lerp(
                    transform.position.y,
                    aheadTargetPos.y + deltaY,
                    0.5f
                );
            }
        }

        // Apply smooth damping to position
        Vector3 newPos = transform.position;
        newPos.x = Mathf.SmoothDamp(transform.position.x, targetX, ref currentVelocity.x, smoothTimeX);
        if (followY)
        {
            newPos.y = Mathf.SmoothDamp(transform.position.y, targetY, ref currentVelocity.y, smoothTimeY);
        }

        // Clamp to bounds if enabled
        if (useBounds)
        {
            newPos.x = Mathf.Clamp(newPos.x, leftBound, rightBound);
            newPos.y = Mathf.Clamp(newPos.y, bottomBound, topBound);
        }

        // Apply screen shake if enabled
        if (useScreenShake && trauma > 0)
        {
            // Calculate shake with decreasing intensity
            float shake = trauma * trauma;
            newPos.x += maxShakeAmount * shake * (Mathf.PerlinNoise(Time.time * 10f, 0) * 2 - 1);
            newPos.y += maxShakeAmount * shake * (Mathf.PerlinNoise(0, Time.time * 10f) * 2 - 1);
            
            // Decay trauma over time
            trauma = Mathf.Max(0, trauma - traumaDecay * Time.deltaTime);
        }

        // Set final position
        transform.position = newPos;
        
        // Save current position for next frame
        lastTargetPosition = target.position;
    }

        private bool ShouldFollowTarget()
    {
        // If following is manually disabled, return false
        if (!enableFollowing)
            return false;
            
        // Check game state - only follow if in Playing state
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.CurrentGameState == GameManager.GameState.Playing;
        }
        
        // If no GameManager, use the manual enableFollowing setting
        return enableFollowing;
    }

        // New public method to enable/disable following
    public void SetFollowingEnabled(bool enabled)
    {
        enableFollowing = enabled;
    }

    // Exposed methods for gameplay events

    /// <summary>
    /// Add trauma for screen shake (0-1 range)
    /// </summary>
    public void AddTrauma(float amount)
    {
        if (useScreenShake)
        {
            trauma = Mathf.Clamp01(trauma + amount);
        }
    }

    /// <summary>
    /// Move camera to target instantly (for level transitions, etc.)
    /// </summary>
    public void SetPositionInstant()
    {
        if (target != null)
        {
            Vector3 targetPos = target.position + Vector3.right * followOffset.x + Vector3.up * followOffset.y;
            transform.position = new Vector3(targetPos.x, followY ? targetPos.y : transform.position.y, transform.position.z);
            currentVelocity = Vector3.zero;
            currentLookAheadX = 0f;
            targetLookAheadX = 0f;
        }
    }

    /// <summary>
    /// Set camera bounds for level containment
    /// </summary>
    public void SetBounds(float left, float right, float bottom, float top)
    {
        leftBound = left;
        rightBound = right;
        bottomBound = bottom;
        topBound = top;
        useBounds = true;
    }

    /// <summary>
    /// Disable camera bounds
    /// </summary>
    public void ClearBounds()
    {
        useBounds = false;
    }

    /// <summary>
    /// Change camera offset
    /// </summary>
    public void SetOffset(Vector2 newOffset)
    {
        followOffset = newOffset;
    }

    /// <summary>
    /// Set new target for camera to follow
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetRigidbody = target.GetComponent<Rigidbody2D>();
            playerMovement = target.GetComponent<PlayerMovement>();
            lastTargetPosition = target.position;
        }
    }

    // Visual debug
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying && useBounds)
        {
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
            Gizmos.DrawCube(new Vector3((rightBound + leftBound) / 2, (topBound + bottomBound) / 2, 0), 
                            new Vector3(rightBound - leftBound, topBound - bottomBound, 1));
        }
    }
}