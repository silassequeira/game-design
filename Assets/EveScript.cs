using UnityEngine;

public class SpriteDisabler : MonoBehaviour
{
    public Transform player;
    public float xThreshold = 2f;
    public float yThreshold = 2f; // Add a vertical threshold

    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public bool hasDisappeared = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Get Animator if attached
    }

    void Update()
    {
        // Check if the player is within the horizontal and vertical thresholds
        if (!hasDisappeared &&
            Mathf.Abs(player.position.x - transform.position.x) <= xThreshold &&
            Mathf.Abs(player.position.y - transform.position.y) <= yThreshold)
        {
            if (animator != null)
            {
                animator.enabled = false; // Stop the animation from running
            }

            spriteRenderer.sprite = null; // Set sprite to none
            hasDisappeared = true;
        }
    }
}