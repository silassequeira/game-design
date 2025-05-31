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
    
    [Header("Visual Effects")]
    public Color doubleJumpParticleColor = Color.cyan;
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
            
            // Update movement settings
            playerMovement.SetMovementSpeed(newMoveSpeed);
            playerMovement.SetMaxSpeed(newMaxSpeed);
            
            // Update speed momentum settings
            playerMovement.SetSpeedMomentumSettings(newInitialMoveSpeed, newMaxMoveSpeed);
            
            // Update jump settings
            playerMovement.SetJumpForce(newJumpForce);
            playerMovement.SetMaxJumpDuration(newMaxJumpDuration);
            
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
        // First try to play a special effect if available
        if (!visualEffects.TryPlaySpecialEffect("DoubleJumpAcquired"))
        {
            // If no special effect, use regular particles with custom color
            visualEffects.PlayJumpParticles(true);  // Use double jump particles
            visualEffects.SetParticleColor(doubleJumpParticleColor, effectDuration);
        }
    }
    
    // Get the audio system and play sound
    PlayerAudioSystem audioSystem = playerMovement.GetAudioSystem();
    if (audioSystem != null)
    {
        AudioClip doubleJumpSound = audioSystem.GetDoubleJumpSound();
        if (doubleJumpSound != null)
        {
            audioSystem.PlaySound(doubleJumpSound, 1.5f);  // Play at higher volume for emphasis
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
    
    private System.Collections.IEnumerator ResetParticleColor(ParticleSystem particles, Color originalColor)
    {
        yield return new WaitForSeconds(effectDuration);
        if (particles != null)
        {
            ParticleSystem.MainModule main = particles.main;
            main.startColor = originalColor;
        }
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