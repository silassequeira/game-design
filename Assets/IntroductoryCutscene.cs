using System.Collections;
using UnityEngine;

public class IntroductoryCutscene : MonoBehaviour
{
    public static IntroductoryCutscene Instance { get; private set; }

    [Header("Cutscene Settings")]
    [SerializeField] private float cutsceneDuration = 3.0f;
    [SerializeField] private float playerWalkSpeed = 3.0f;
    [SerializeField] private bool skipCutsceneWithInput = true;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Animator playerAnimator;

    private bool cutsceneActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (playerMovement == null)
        {
            // Try to find player movement script if not assigned
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        if (playerAnimator == null && playerMovement != null)
        {
            // Try to get animator from player
            playerAnimator = playerMovement.GetComponent<Animator>();
        }
    }

    private void Update()
    {
        // Check for skip input during cutscene
        if (cutsceneActive && skipCutsceneWithInput && 
            (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape)))
        {
            EndCutscene();
        }
    }

    // Call this after the title screen
public void StartIntroCutscene()
{
    if (GameManager.Instance != null)
    {
        // Create a transitional state between title and playing
        GameManager.Instance.SetGameState(GameManager.GameState.Loading);
        
        // Disable camera following during cutscene
        CameraFollow cameraFollow = Camera.main?.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.SetFollowingEnabled(false);
        }
        
        // Start the cutscene
        StartCoroutine(PlayIntroCutscene());
    }
}

    private IEnumerator PlayIntroCutscene()
{
    cutsceneActive = true;
    
    // Disable player input
    DisablePlayerControls();
    
    // Get Rigidbody2D for physics-based movement
    Rigidbody2D playerRb = playerMovement?.GetComponent<Rigidbody2D>();
    
    // Set walking animation
    if (playerAnimator != null)
    {
        // These parameter names should match your Animator parameters
        playerAnimator.SetBool("isWalking", true);
        playerAnimator.SetFloat("moveX", 1.0f); // Set horizontal movement parameter
    }
    
    // Flip sprite to face right if needed
    SpriteRenderer playerSprite = playerMovement?.GetComponent<SpriteRenderer>();
    if (playerSprite != null)
    {
        playerSprite.flipX = false; // Ensure player faces right (adjust based on your sprite setup)
    }
    
    // Keep track of elapsed time
    float elapsedTime = 0f;
    
    while (elapsedTime < cutsceneDuration)
    {
        if (!cutsceneActive) break; // In case we skip
        
        // Move player to the right
        if (playerMovement != null)
        {
            // Option 1: Move transform directly
            Vector3 movement = new Vector3(playerWalkSpeed * Time.deltaTime, 0, 0);
            playerMovement.transform.Translate(movement);
            
            // Option 2: Use physics (may be more consistent with your movement system)
            if (playerRb != null)
            {
                // Apply velocity directly
                playerRb.linearVelocity = new Vector2(playerWalkSpeed, playerRb.linearVelocity.y);
                
                // Alternative: Add force
                // playerRb.AddForce(new Vector2(playerWalkSpeed * 10, 0));
            }
            
            // Force animation parameters in case they're being reset elsewhere
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("isWalking", true);
                playerAnimator.SetFloat("moveX", 1.0f);
            }
        }
        
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    
    EndCutscene();
}

private void EndCutscene()
{
    cutsceneActive = false;
    
    // Re-enable player controls
    EnablePlayerControls();
    
    // Reset player animations to idle
    if (playerAnimator != null)
    {
        playerAnimator.SetBool("isWalking", false);
        playerAnimator.SetFloat("moveX", 0f);
    }
    
    // Enable camera following
    CameraFollow cameraFollow = Camera.main?.GetComponent<CameraFollow>();
    if (cameraFollow != null)
    {
        cameraFollow.SetFollowingEnabled(true);
    }
    
    // Set game state to playing
    if (GameManager.Instance != null)
    {
        GameManager.Instance.SetGameState(GameManager.GameState.Playing);
    }
}

    private void DisablePlayerControls()
    {
        if (playerMovement != null)
        {
            // Store current state and disable player controls
            playerMovement.enabled = false;
        }
    }

    private void EnablePlayerControls()
    {
        if (playerMovement != null)
        {
            // Restore player controls
            playerMovement.enabled = true;
        }
    }
}