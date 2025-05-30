using UnityEngine;

public class PlayerStateChanger : MonoBehaviour
{
    public Sprite newSprite;
    public RuntimeAnimatorController newAnimatorController;
    public Transform targetObject; // Reference to the target object
    public float proximityThreshold = 2f; // Distance threshold for triggering the effect

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
        if (playerMovement == null) Debug.LogError("No PlayerMovement script assigned!");
    }

void Update()
{
    // Check if the player is within the horizontal and vertical thresholds
    if (!hasChanged && targetObject != null &&
        Mathf.Abs(transform.position.x - targetObject.position.x) <= proximityThreshold &&
        Mathf.Abs(transform.position.y - targetObject.position.y) <= proximityThreshold)
    {
        // Change the sprite
        if (newSprite != null)
            spriteRenderer.sprite = newSprite;

        // Switch to a new Animator Controller
        if (newAnimatorController != null)
            animator.runtimeAnimatorController = newAnimatorController;

        // Update PlayerMovement variables
        if (playerMovement != null)
        {
            // Existing movement buffs
            playerMovement.moveSpeed = 5f;
            playerMovement.maxSpeed = 8f;
            playerMovement.initialMoveSpeed = 3.2f;
            playerMovement.maxMoveSpeed = 8.2f;
            playerMovement.jumpForce = 6.8f;
            playerMovement.maxJumpDuration = 0.18f;
            
            // Enable double jump ability
            playerMovement.enableDoubleJump = true;
            
            // Optionally customize double jump parameters
            playerMovement.doubleJumpForce = 8f;  // Slightly higher than normal jump force
            
            // Visual feedback for double jump ability gained
            ShowDoubleJumpEffects();
        }

        hasChanged = true;
    }
}

// Optional: Add visual/audio feedback when double jump is enabled
private void ShowDoubleJumpEffects()
{
    // Create a temporary visual effect to show double jump was gained
    if (playerMovement.doubleJumpParticles != null)
    {
        ParticleSystem.MainModule main = playerMovement.doubleJumpParticles.main;
        Color originalColor = main.startColor.color;
        
        // Make particles more vibrant for a moment
        main.startColor = Color.white;
        playerMovement.doubleJumpParticles.Play();
        
        // Reset to original color after effect
        StartCoroutine(ResetParticleColor(playerMovement.doubleJumpParticles, originalColor));
    }
    
    // Play a sound effect if available
    if (playerMovement.audioSource != null && playerMovement.doubleJumpSound != null)
    {
        playerMovement.audioSource.PlayOneShot(playerMovement.doubleJumpSound);
    }
    
    // Could also add a UI notification here
}

private System.Collections.IEnumerator ResetParticleColor(ParticleSystem particles, Color originalColor)
{
    yield return new WaitForSeconds(1.0f);
    if (particles != null)
    {
        ParticleSystem.MainModule main = particles.main;
        main.startColor = originalColor;
    }
}
}