using UnityEngine;

public class SpriteDisabler : MonoBehaviour
{
    public Transform player;
    public float xThreshold = 5f;

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
        if (!hasDisappeared && player.position.x >= xThreshold)
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
