using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Header("Settings")]
    public int pointValue = 1;
    public bool destroyOnCollect = true;
    
    [Header("References")]
    public SpriteDisabler spriteDisabler;
    
    private bool isCollected = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Find the SpriteDisabler component if not assigned
        if (spriteDisabler == null)
        {
            spriteDisabler = GetComponent<SpriteDisabler>();
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        // Only process if game is in Playing state
        if (GameManager.Instance != null && 
            GameManager.Instance.CurrentGameState != GameManager.GameState.Playing)
        {
            return;
        }
        
        // Check if sprite has disappeared via SpriteDisabler
        if (!isCollected && spriteDisabler != null && spriteDisabler.hasDisappeared)
        {
            CollectItem();
        }
    }
    
    public void CollectItem()
    {
        if (isCollected) return;
        
        // Only collect if game is in Playing state
        if (GameManager.Instance != null && 
            GameManager.Instance.CurrentGameState != GameManager.GameState.Playing)
        {
            return;
        }
        
        isCollected = true;
        
        // Notify the manager
        if (CollectableManager.Instance != null)
        {
            CollectableManager.Instance.CollectItem();
        }
        
        // Destroy the object if specified
        if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
    }
}