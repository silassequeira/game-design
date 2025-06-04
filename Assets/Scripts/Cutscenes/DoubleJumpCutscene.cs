using System.Collections;
using UnityEngine;
using InputSystems;

public class DoubleJumpCutscene : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private float triggerRadius = 2f;
    [SerializeField] private bool triggerOnlyWhenGrounded = true;
    [SerializeField] private bool playOnce = true;
    [SerializeField] private bool drawGizmos = true;
    
    [Header("Cutscene Timing")]
    [SerializeField] private float initialDelayDuration = 3f;
    [SerializeField] private float walkRightDuration = 3f;
    [SerializeField] private float timeBetweenJumps = 0.8f;
    [SerializeField] private float endDelayDuration = 1f;
    
    [Header("Camera Settings")]
    [SerializeField] private bool useZoom = true;
    [SerializeField] private float zoomSize = 3f; // Lower values = more zoom
    [SerializeField] private float zoomSpeed = 2f; // How fast to zoom in/out
    [SerializeField] private float zoomDelay = 0.5f; // Delay before starting zoom
    [SerializeField] private float unzoomDelay = 0.5f; // Delay before unzooming
    
    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = true;

    private bool hasPlayedCutscene = false;
    private bool isPlayingCutscene = false;
    private PlayerMovement playerMovement;
    private VirtualInput virtualInput;
    private Camera mainCamera;
    private float originalCameraSize;
    
    private void Start()
    {
        // Try to find the player if not assigned
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                LogDebug("Found player automatically using 'Player' tag");
            }
            else
            {
                Debug.LogError("Player GameObject not assigned and couldn't find object with 'Player' tag!");
            }
        }
        
        // Cache the PlayerMovement component
        if (playerObject != null)
        {
            playerMovement = playerObject.GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("Player GameObject doesn't have a PlayerMovement component!");
            }
        }
        
        // Get reference to main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Could not find main camera for zoom effects");
            useZoom = false;
        }
        else
        {
            // Store the original camera size for later restoration
            originalCameraSize = mainCamera.orthographicSize;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (drawGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
            
            // Draw text to indicate this is a cutscene trigger
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, "Double Jump Cutscene Trigger");
            #endif
        }
    }
    
    private void Update()
    {
        // Skip if already played or currently playing
        if (hasPlayedCutscene && playOnce || isPlayingCutscene || playerMovement == null)
            return;
            
        // Check if player is within trigger radius
        float distance = Vector2.Distance(transform.position, playerObject.transform.position);
        if (distance <= triggerRadius)
        {
            // Check if player should be grounded to trigger
            if (triggerOnlyWhenGrounded && !playerMovement.GetIsGrounded())
            {
                return;
            }
            
            LogDebug($"Player entered trigger radius (distance: {distance:F2})");
            StartCoroutine(PlayCutscene());
        }
    }
    
    private IEnumerator PlayCutscene()
    {
        isPlayingCutscene = true;
        hasPlayedCutscene = true;
        
        // Log debug info
        LogDebug("Starting cutscene sequence");
        
        // Store original input system to restore later
        IPlayerInput originalInput = playerMovement.GetInputSystem();
        
        // Wait for one frame to ensure we're not catching any existing inputs
        yield return null;
        
        // Create our virtual input system - FIXED: Using VirtualInput instead of VirtualPlayerInput
        virtualInput = new VirtualInput();
        
        // Take control of the player
        playerMovement.SetInputSystem(virtualInput);
        
        // Reset any momentum the player might have
        playerMovement.ResetSpeed();
        
        // Zoom in camera if enabled
        if (useZoom && mainCamera != null)
        {
            yield return new WaitForSeconds(zoomDelay);
            yield return StartCoroutine(ZoomCamera(zoomSize));
        }
        
        // 1. Wait with idle animation for 3 seconds
        LogDebug("Idle state");
        virtualInput.SetHorizontalInput(0);
        virtualInput.SetJumpInputDown(false);
        virtualInput.SetJumpInputHeld(false);
        yield return new WaitForSeconds(initialDelayDuration);
        
        // 2. Walk right for 3 seconds
        LogDebug("Walking right");
        virtualInput.SetHorizontalInput(1);
        yield return new WaitForSeconds(walkRightDuration);
        
        // Stop walking briefly before jumping
        virtualInput.SetHorizontalInput(0);
        yield return new WaitForSeconds(0.2f);
        
        // 3. First jump
        LogDebug("First jump");
        virtualInput.SetJumpInputDown(true);
        yield return null; // Wait one frame
        yield return null; // Wait another frame to ensure input is recognized
        virtualInput.SetJumpInputDown(false);
        virtualInput.SetJumpInputHeld(true);
        
        // Keep jump held for a realistic time
        yield return new WaitForSeconds(0.05f);
        virtualInput.SetJumpInputHeld(false);
        
        // Wait a bit before second jump
        yield return new WaitForSeconds(timeBetweenJumps);
        
        // 4. Double jump in the air
        LogDebug("Double jump");
        virtualInput.SetJumpInputDown(true);
        yield return null; // Wait one frame
        yield return null; // Wait another frame to ensure input is recognized
        virtualInput.SetJumpInputDown(false);
        virtualInput.SetJumpInputHeld(true);
        
        // Keep jump held for a realistic time
        yield return new WaitForSeconds(0.05f);
        virtualInput.SetJumpInputHeld(false);
        
        // Wait for player to land
        LogDebug("Waiting for player to land");
        
        // Add a safety timeout in case player never lands
        float timeout = 5f;
        float timer = 0f;
        
        while (!playerMovement.GetIsGrounded() && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        yield return new WaitForSeconds(endDelayDuration);
        
        // Zoom out camera if it was zoomed in
        if (useZoom && mainCamera != null)
        {
            yield return new WaitForSeconds(unzoomDelay);
            yield return StartCoroutine(ZoomCamera(originalCameraSize));
        }
        
        // 5. Return control to the player
        LogDebug("Cutscene complete - returning control to player");
        playerMovement.SetInputSystem(originalInput);
        
        isPlayingCutscene = false;
    }
    
    // Smooth camera zoom coroutine
    private IEnumerator ZoomCamera(float targetSize)
    {
        if (mainCamera == null)
            yield break;
            
        float startSize = mainCamera.orthographicSize;
        float duration = Mathf.Abs(targetSize - startSize) / zoomSpeed;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            
            // Use smoothstep for easing
            float smoothT = t * t * (3f - 2f * t);
            
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, smoothT);
            yield return null;
        }
        
        // Ensure we end at exactly the target size
        mainCamera.orthographicSize = targetSize;
        LogDebug($"Camera zoomed to size: {targetSize}");
    }
    
    public void ResetCutscene()
    {
        hasPlayedCutscene = false;
        isPlayingCutscene = false;
        LogDebug("Cutscene reset and can be played again");
    }
    
    private void LogDebug(string message)
    {
        if (logDebugMessages)
        {
            Debug.Log($"[DoubleJumpCutscene] {message}");
        }
    }
}