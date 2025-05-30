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
     public bool isPlaying => cutsceneActive;

    private void Awake()
    {
    if (Instance == null)
    {
        Instance = this;
        
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject);
        return;
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
        if (cutsceneActive && skipCutsceneWithInput && 
            (Input.GetKeyDown(KeyCode.Escape)))
        {
            EndCutscene();
        }
    }

public void StartIntroCutscene()
{
    if (GameManager.Instance != null)
    {
        GameManager.Instance.SetGameState(GameManager.GameState.Loading);
        
        CameraFollow cameraFollow = Camera.main?.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.SetFollowingEnabled(false);
        }
        
        StartCoroutine(PlayIntroCutscene());
    }
}

    private IEnumerator PlayIntroCutscene()
{
    cutsceneActive = true;
    
    DisablePlayerControls();
    
    Rigidbody2D playerRb = playerMovement?.GetComponent<Rigidbody2D>();
    
   
    if (playerAnimator != null)
    {
        playerAnimator.SetBool("isWalking", true);
        playerAnimator.SetFloat("moveX", 1.0f); 
    }
    
    // Flip sprite to face right if needed
    SpriteRenderer playerSprite = playerMovement?.GetComponent<SpriteRenderer>();
    if (playerSprite != null)
    {
        playerSprite.flipX = false; 
    }
    
    // Keep track of elapsed time
    float elapsedTime = 0f;
    
    while (elapsedTime < cutsceneDuration)
    {
        if (!cutsceneActive) break; 
        
        if (playerMovement != null)
        {
            Vector3 movement = new Vector3(playerWalkSpeed * Time.deltaTime, 0, 0);
            playerMovement.transform.Translate(movement);
            
            if (playerRb != null)
            {

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

public void EndCutscene()
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

        public bool IsPlaying() 
    {
        return cutsceneActive;
    }
}