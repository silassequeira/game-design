using UnityEngine;

public class SpriteDisabler : MonoBehaviour
{
    [Header("Player Detection")]
    public Transform player;
    public float xThreshold = 2f;
    public float yThreshold = 2f;
    
    [Header("Components to Disable")]
    public AudioSource[] audioSourcesToDisable;
    public GameObject[] objectsToDisable;
    
    [Header("Optional Components")]
    public bool disableOwnAudioSource = true;

    private SpriteRenderer spriteRenderer;
    private AudioSource ownAudioSource;
    public Animator animator;
    public bool hasDisappeared = false;
    
    [Header("Debug")]
    public bool showDebugInfo = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // Get own audio source if present
        if (disableOwnAudioSource)
        {
            ownAudioSource = GetComponent<AudioSource>();
        }
        
        if (player == null)
        {
            // Try to find player if not assigned
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                if (showDebugInfo) Debug.Log($"Found player automatically: {player.name}");
            }
            else
            {
                Debug.LogWarning("No player assigned and none found with 'Player' tag!");
            }
        }
    }

    void Update()
    {
        if (player == null || hasDisappeared)
            return;
            
        // Check if the player is within the horizontal and vertical thresholds
        if (Mathf.Abs(player.position.x - transform.position.x) <= xThreshold &&
            Mathf.Abs(player.position.y - transform.position.y) <= yThreshold)
        {
            DisableEverything();
        }
    }
    
    void DisableEverything()
    {
        // Disable sprite and animation
        if (animator != null)
        {
            animator.enabled = false; // Stop the animation from running
            if (showDebugInfo) Debug.Log($"Disabled animator on {gameObject.name}");
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null; // Set sprite to none
            if (showDebugInfo) Debug.Log($"Cleared sprite on {gameObject.name}");
        }
        
        // Disable own audio source if present
        if (ownAudioSource != null && disableOwnAudioSource)
        {
            ownAudioSource.Stop();
            ownAudioSource.enabled = false;
            if (showDebugInfo) Debug.Log($"Disabled audio source on {gameObject.name}");
        }
        
        // Disable all specified audio sources
        if (audioSourcesToDisable != null)
        {
            foreach (AudioSource audioSource in audioSourcesToDisable)
            {
                if (audioSource != null)
                {
                    audioSource.Stop();
                    audioSource.enabled = false;
                    if (showDebugInfo) Debug.Log($"Disabled external audio source: {audioSource.gameObject.name}");
                }
            }
        }
        
        // Disable any other specified GameObjects
        if (objectsToDisable != null)
        {
            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    if (showDebugInfo) Debug.Log($"Disabled game object: {obj.name}");
                }
            }
        }
        
        hasDisappeared = true;
    }
    
    // Public method to manually trigger the disabling
    public void TriggerDisable()
    {
        if (!hasDisappeared)
        {
            DisableEverything();
        }
    }
    
    // Optional: Visualize the detection area in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(xThreshold * 2, yThreshold * 2, 0.1f));
    }
}