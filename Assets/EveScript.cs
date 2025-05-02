using UnityEngine;

public class SpriteDisabler : MonoBehaviour
{
    public Transform player;
    public float xThreshold = 63.17f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool hasDisappeared = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); 
    }

    void Update()
    {
        if (!hasDisappeared && player.position.x >= xThreshold)
        {
            if (animator != null)
            {
                animator.enabled = false; 
            }

            spriteRenderer.sprite = null; 
            hasDisappeared = true;
        }
    }
}
