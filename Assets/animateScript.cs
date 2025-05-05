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
            playerMovement.moveSpeed = 3.8f;
            playerMovement.maxSpeed = 12f;
            playerMovement.initialMoveSpeed = 3.2f;
            playerMovement.maxMoveSpeed = 5.2f;
            playerMovement.jumpForce = 7.8f;
            playerMovement.maxJumpDuration = 0.2f;
        }

        hasChanged = true;
    }
}
}