using UnityEngine;

public class SpriteDisabler : MonoBehaviour
{
    public Transform player;
    public float xThreshold = 2f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool hasDisappeared = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Get Animator if attached
    }

    void Update()
    {
        // Check if the player is within the threshold relative to this object's position
        if (!hasDisappeared && Mathf.Abs(player.position.x - transform.position.x) <= xThreshold)
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