using UnityEngine;

public class PlayerStateChanger : MonoBehaviour
{
    public Sprite newSprite;
    public RuntimeAnimatorController newAnimatorController;
    public float xThreshold = 5f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool hasChanged = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer == null) Debug.LogError("No SpriteRenderer on player!");
        if (animator == null) Debug.LogError("No Animator on player!");
    }

    void Update()
    {
        if (!hasChanged && transform.position.x >= xThreshold)
        {
            // Change the sprite
            if (newSprite != null)
                spriteRenderer.sprite = newSprite;

            // Switch to a new Animator Controller
            if (newAnimatorController != null)
                animator.runtimeAnimatorController = newAnimatorController;

            hasChanged = true;
        }
    }
}
