using UnityEngine;
using System.Collections;
using InputSystems;

public class ObjectCutscene : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform targetObject;
    
    [Header("Trigger Settings")]
    [SerializeField] private float triggerRadius = 3f;
    [SerializeField] private bool triggerOnlyWhenGrounded = true;
    [SerializeField] private bool playOnce = true;
    [SerializeField] private bool showTriggerVisual = true;
    
    [Header("Movement Settings")]
    [SerializeField] private float duckDuration = 2.0f;
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private float horizontalSpeed = 1.0f;
    [SerializeField] private float cutsceneTimeout = 10f; // Maximum duration to prevent getting stuck
    
    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool controlCamera = true;
    [SerializeField] private float originalOrthoSize = 5f;
    [SerializeField] private float zoomedOrthoSize = 3f;
    [SerializeField] private float zoomSpeed = 2.0f;
    [SerializeField] private float zoomDelay = 0.2f;
    [SerializeField] private float unzoomDelay = 0.5f;
    
    // Internal state
    private bool cutsceneActive = false;
    private bool hasPlayed = false;
    private GameObject playerObject;
    private PlayerMovement playerMovement;
    private IPlayerInput originalInput;
    private ImprovedVirtualInput virtualInput; // Changed from VirtualInput to ImprovedVirtualInput
    private float cutsceneTimer = 0f;
    private bool isDucking = false;
    private bool reachedTarget = false;
    
    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (controlCamera && mainCamera == null)
            {
                Debug.LogWarning("No main camera found. Camera control will be disabled.");
                controlCamera = false;
            }
        }
        
        if (targetObject == null)
        {
            // Default to this object as the target if none is specified
            targetObject = transform;
        }
        
        // Create virtual input system that we'll use during the cutscene
        virtualInput = new ImprovedVirtualInput(this); // Use ImprovedVirtualInput with this as host
    }
    
    private void Update()
    {
        // Safety check: If cutscene has been active for too long, force end it
        if (cutsceneActive)
        {
            cutsceneTimer += Time.deltaTime;
            if (cutsceneTimer >= cutsceneTimeout)
            {
                Debug.LogWarning("Cutscene timed out - forcing end to prevent player from getting stuck");
                StopCutscene();
                return;
            }
        }
        
        if (cutsceneActive || (playOnce && hasPlayed))
            return;
            
        // Find player if we don't have a reference yet
        if (playerObject == null || playerMovement == null)
        {
            FindPlayer();
            if (playerObject == null) return; // Skip if we can't find a player
        }
            
        // Check if player is within trigger radius
        float distance = Vector2.Distance(
            new Vector2(playerObject.transform.position.x, playerObject.transform.position.y), 
            new Vector2(transform.position.x, transform.position.y)
        );
        
        if (distance <= triggerRadius)
        {
            // Check grounded state if required
            if (triggerOnlyWhenGrounded && !playerMovement.GetIsGrounded())
            {
                return; // Skip if player needs to be grounded but isn't
            }
            
            StartCutscene();
        }
    }
    
    private void FindPlayer()
    {
        // First try to find by tag
        GameObject player = GameObject.FindWithTag("Player");
        
        // If that fails, look for PlayerMovement component
        if (player == null)
        {
            PlayerMovement[] movements = FindObjectsOfType<PlayerMovement>();
            if (movements.Length > 0)
            {
                player = movements[0].gameObject;
            }
        }
        
        if (player != null)
        {
            playerObject = player;
            playerMovement = player.GetComponent<PlayerMovement>();
            
            if (playerMovement == null)
            {
                Debug.LogError("Found player object but it doesn't have a PlayerMovement component!");
            }
        }
    }
    
    public void StartCutscene()
    {
        if (cutsceneActive || (playOnce && hasPlayed) || playerMovement == null)
            return;
        
        cutsceneActive = true;
        cutsceneTimer = 0f;
        reachedTarget = false;
        isDucking = false;
        
        // Debug the player's animator to see if Duck animation is available
        Animator playerAnimator = playerObject.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            AnimatorControllerParameter[] parameters = playerAnimator.parameters;
            bool hasDuckParameter = false;
            foreach (var param in parameters)
            {
                if (param.name == "isDucked" || param.name == "Duck")
                {
                    hasDuckParameter = true;
                }
            }
            
            if (!hasDuckParameter)
            {
                Debug.LogWarning("Player animator doesn't have isDucked/Duck parameters. Duck animation may not work.");
            }
        }
        
        // Take over player control by replacing its input system
        originalInput = playerMovement.GetInputSystem();
        playerMovement.SetInputSystem(virtualInput);
        
        // Reset player state for clean movement
        playerMovement.ResetSpeed();
        
        // Start cutscene sequence
        StartCoroutine(PlayCutscene());
    }
    
    private IEnumerator PlayCutscene()
    {
        // Reset all inputs to start fresh
        virtualInput.ResetAllInputs();
        
        // Phase 1: Wait a moment before zooming
        yield return new WaitForSeconds(zoomDelay);
        
        // Phase 2: Zoom in camera
        if (controlCamera && mainCamera != null)
        {
            yield return StartCoroutine(ZoomCamera(mainCamera.orthographicSize, zoomedOrthoSize));
        }
        
        // Phase 3: Move toward target
        yield return StartCoroutine(MovePlayerToTarget());
        
        // Phase 4: Ducking at the target location - use TriggerDuck method for reliability
        isDucking = true;
        
        // Make sure both SetDuckInputDown and SetDuckInputHeld are set
        virtualInput.SetDuckInputDown(true);
        virtualInput.SetDuckInputHeld(true);
        
        // Force the player movement to handle ducking directly
        if (playerMovement != null)
        {
            // Direct call to ensure duck happens
            playerMovement.HandleDuck(true);
        }
        
        yield return new WaitForSeconds(0.1f); // Short pause
        
        // Release the down input but keep the held input true
        virtualInput.SetDuckInputDown(false);
        
        yield return new WaitForSeconds(duckDuration);
        
        // Phase 5: Release duck
        isDucking = false;
        virtualInput.SetDuckInputDown(false);
        virtualInput.SetDuckInputHeld(false);
        
        // Direct call to ensure duck release happens
        if (playerMovement != null)
        {
            playerMovement.HandleDuck(false);
        }
        
        // Wait before unzooming
        yield return new WaitForSeconds(unzoomDelay);
        
        // Phase 6: Zoom out camera
        if (controlCamera && mainCamera != null)
        {
            yield return StartCoroutine(ZoomCamera(mainCamera.orthographicSize, originalOrthoSize));
        }
        
        // Phase 7: End cutscene
        EndCutscene();
    }
    
    private IEnumerator MovePlayerToTarget()
    {
        float startTime = Time.time;
        float timeout = 5f; // Maximum 5 seconds to reach target as a safety measure
        
        while (Mathf.Abs(playerObject.transform.position.x - targetObject.position.x) > movementThreshold)
        {
            // Check if we've spent too long trying to reach the target
            if (Time.time - startTime > timeout)
            {
                Debug.LogWarning("Movement to target timed out - continuing with cutscene");
                break;
            }
            
            float direction = targetObject.position.x > playerObject.transform.position.x ? 1f : -1f;
            float distanceToTarget = Mathf.Abs(playerObject.transform.position.x - targetObject.position.x);

            // Calculate speed based on distance, with a minimum value to keep movement smooth
            float speedFactor = Mathf.Clamp01(distanceToTarget / triggerRadius);
            speedFactor = Mathf.Max(speedFactor, 0.3f); // Ensure a minimum speed factor
            float currentMoveSpeed = speedFactor * horizontalSpeed;

            // Apply horizontal input
            virtualInput.SetHorizontalInput(direction * currentMoveSpeed);
            
            yield return null;
        }
        
        // Successfully reached target
        reachedTarget = true;
        virtualInput.SetHorizontalInput(0f); // Stop completely once reached
        
        // Give the player a moment to settle before continuing
        yield return new WaitForSeconds(0.2f);
    }
    
    private IEnumerator ZoomCamera(float startSize, float targetSize)
    {
        float elapsed = 0f;
        float duration = 1f / zoomSpeed; // Convert speed to duration
        
        while (elapsed < duration)
        {
            // Calculate smooth t with easing
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // Apply smoothed interpolation
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, smoothT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure exact target size
        mainCamera.orthographicSize = targetSize;
    }
    
    private void EndCutscene()
    {
        // Reset virtual input
        virtualInput.ResetAllInputs();
        
        // Ensure player is not left in a ducking state
        if (isDucking && playerMovement != null)
        {
            playerMovement.HandleDuck(false);
        }
        
        // Restore original player input
        if (playerMovement != null && originalInput != null)
        {
            playerMovement.SetInputSystem(originalInput);
        }
        
        // Mark as played if necessary
        hasPlayed = playOnce;
        cutsceneActive = false;
        
        Debug.Log("Cutscene completed: " + 
            (reachedTarget ? "Successfully reached target" : "Failed to reach target"));
    }
    
    // Public methods for external control
    public void SetTarget(Transform newTarget)
    {
        targetObject = newTarget;
    }
    
    public void Reset()
    {
        hasPlayed = false;
    }
    
    public void StopCutscene()
    {
        if (!cutsceneActive)
            return;
            
        StopAllCoroutines();
        
        // Reset camera immediately if needed
        if (controlCamera && mainCamera != null)
        {
            mainCamera.orthographicSize = originalOrthoSize;
        }
        
        EndCutscene();
    }
    
    private void OnDrawGizmos()
    {
        if (showTriggerVisual)
        {
            // Draw trigger radius
            Gizmos.color = cutsceneActive ? Color.red : (hasPlayed ? Color.gray : Color.green);
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
            
            // Draw line to target if different from this object
            if (targetObject != null && targetObject != transform)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, targetObject.position);
            }
        }
    }
}