using UnityEngine;
using System.Collections;

public class PlayerStateChanger : MonoBehaviour
{
    [Header("Transformation Settings")]
    public Sprite newSprite;
    public RuntimeAnimatorController newAnimatorController;
    public Transform targetObject; // Reference to the target object
    public float proximityThreshold = 2f; // Distance threshold for triggering the effect
    
    [Header("Movement Upgrades")]
    public float newMoveSpeed = 5f;
    public float newMaxSpeed = 8f;
    public float newInitialMoveSpeed = 3.2f;
    public float newMaxMoveSpeed = 8.2f;
    public float newJumpForce = 6.8f;
    public float newMaxJumpDuration = 0.18f;
    public float newDoubleJumpForce = 8f;
    public float newJumpInputCooldown = 0.01f;
    
    [Header("Visual Effects")]
    public float effectDuration = 2.0f;
    
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool hasChanged = false;
    
    // Reference to the PlayerMovement script
    public PlayerMovement playerMovement;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        if (spriteRenderer == null) Debug.LogError("No SpriteRenderer on player!");
        if (animator == null) Debug.LogError("No Animator on player!");
        if (targetObject == null) Debug.LogError("No target object assigned!");
        if (playerMovement == null) 
        {
            // Try to get the component if not assigned
            playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement == null) 
                Debug.LogError("No PlayerMovement script assigned!");
        }
    }
    
    void Update()
    {
        // Check if the player is within the horizontal and vertical thresholds
        if (!hasChanged && targetObject != null &&
            Mathf.Abs(transform.position.x - targetObject.position.x) <= proximityThreshold &&
            Mathf.Abs(transform.position.y - targetObject.position.y) <= proximityThreshold)
        {
            ApplyTransformation();
            hasChanged = true;
        }
        
        // Debug check - use this to verify double jump is enabled
        if (hasChanged && Input.GetKeyDown(KeyCode.F1))
        {
            JumpSystem jumpSystem = playerMovement.GetJumpSystem();
            Debug.Log($"Double Jump Enabled: {jumpSystem.IsDoubleJumpEnabled}");
        }
    }
    
private void ApplyTransformation()
{
    // Change the sprite
    if (newSprite != null)
        spriteRenderer.sprite = newSprite;
    
    // Switch to a new Animator Controller
    if (newAnimatorController != null)
        animator.runtimeAnimatorController = newAnimatorController;
    
    // Update PlayerMovement variables using the new modular system
    if (playerMovement != null)
    {
        // Debug state before changes
        Debug.Log("Before transformation - Double jump enabled: " + 
                  playerMovement.GetJumpSystem().IsDoubleJumpEnabled);
        
        // Get sprint state before changes
        SpeedMomentumSystem speedSystem = playerMovement.GetSpeedMomentumSystem();
        bool wasSprinting = speedSystem != null ? speedSystem.IsSprinting : false;
        int sprintDirection = 0;
        
        if (wasSprinting)
        {
            // Store sprint direction (approximate from input)
            sprintDirection = Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1;
        }
        
        // Update movement settings
        playerMovement.SetMovementSpeed(newMoveSpeed);
        playerMovement.SetMaxSpeed(newMaxSpeed);
        
        // Update speed momentum settings
        playerMovement.SetSpeedMomentumSettings(newInitialMoveSpeed, newMaxMoveSpeed);
        
        // Restore sprint if needed
        if (wasSprinting && speedSystem != null)
        {
            speedSystem.ForceSprint(sprintDirection);
        }
        
        // Update jump settings
        playerMovement.SetJumpForce(newJumpForce);
        playerMovement.SetMaxJumpDuration(newMaxJumpDuration);
        playerMovement.SetJumpInputCooldown(newJumpInputCooldown);
        
        // Enable double jump ability - set this FIRST before changing force
        playerMovement.SetDoubleJumpEnabled(true);
        playerMovement.SetDoubleJumpForce(newDoubleJumpForce);
        
        // Debug state after changes
        Debug.Log("After transformation - Double jump enabled: " + 
                  playerMovement.GetJumpSystem().IsDoubleJumpEnabled);
        
        // Visual feedback for double jump ability gained
        ShowDoubleJumpEffects();
    }
}
    
    // Optional: Add visual/audio feedback when double jump is enabled
    private void ShowDoubleJumpEffects()
    {
        // Get the visual effects module
        PlayerVisualEffects visualEffects = playerMovement.GetVisualEffects();
        
        if (visualEffects != null)
        {
            // Only try to play the special effect without changing particle colors
            visualEffects.TryPlaySpecialEffect("DoubleJumpAcquired");
        }
        
        // Get the audio system and play sound
        PlayerAudioSystem audioSystem = playerMovement.GetAudioSystem();
        if (audioSystem != null)
        {
            AudioClip doubleJumpSound = audioSystem.GetDoubleJumpSound();
            if (doubleJumpSound != null)
            {
                // Provide appropriate parameters for PlayCustomSound, e.g., volume and pitch
                audioSystem.PlayCustomSound(audioSystem.GetDoubleJumpSound(), 1.0f, 1.0f);     
            }
        }
        
        // Add screen shake effect
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.AddTrauma(0.3f);  // Add camera shake for feedback
            }
        }
        
        Debug.Log("Double Jump Ability Gained!");
    }
    
    // Public methods for external use
    public bool HasTransformed()
    {
        return hasChanged;
    }
    
    public void ForceTransformation()
    {
        if (!hasChanged)
        {
            ApplyTransformation();
            hasChanged = true;
        }
    }
    
    public void ResetTransformation()
    {
        hasChanged = false;
        // You could add logic here to revert changes if needed
    }
}