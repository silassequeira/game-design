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
    private IPlayerInput originalInput;
    private SimulatedInput simulatedInput;

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
        
        // Initialize simulated input
        simulatedInput = new SimulatedInput();
        //Debug.Log("IntroductoryCutscene: SimulatedInput created in Awake");
    }

    private void Start()
    {
        if (playerMovement == null)
        {
            // Try to find player movement script if not assigned
            playerMovement = FindObjectOfType<PlayerMovement>();
            //Debug.Log($"IntroductoryCutscene: Found player movement: {(playerMovement != null ? "Yes" : "No")}");
        }

        if (playerAnimator == null && playerMovement != null)
        {
            // Try to get animator from player
            playerAnimator = playerMovement.GetComponent<Animator>();
        }
        
        // Verify the player movement has an input system
        if (playerMovement != null)
        {
            var currentInput = playerMovement.GetInputSystem();
            //Debug.Log($"IntroductoryCutscene: Player has input system: {(currentInput != null ? "Yes" : "No")}");
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
        
        // Wait one frame to ensure everything is initialized
        yield return null;
        
        if (playerMovement != null)
        {
            // Ensure the component is enabled
            playerMovement.enabled = true;
            
            //Debug.Log("Cutscene: Starting cutscene with simulated input");
            
            // Store original input system
            originalInput = playerMovement.GetInputSystem();
            if (originalInput == null)
            {
                //Debug.LogError("Original input is null! Player controls will fail when cutscene ends.");
            }
            
            // Reset simulated input values
            simulatedInput.HorizontalInput = 0;
            simulatedInput.JumpInput = false;
            simulatedInput.JumpInputHeld = false;
            
            // Set our simulated input
            playerMovement.SetInputSystem(simulatedInput);
            
            // Verify it was set correctly
            var currentInput = playerMovement.GetInputSystem();
            bool correctlySet = currentInput == simulatedInput;
            //Debug.Log($"Cutscene: Simulated input set correctly: {correctlySet}");
            
            // Wait another frame for the change to take effect
            yield return null;
            
            // Configure the simulated input for moving right
            simulatedInput.HorizontalInput = 1.0f; // Right direction
            //Debug.Log($"Cutscene: Set horizontal input to {simulatedInput.HorizontalInput}");
        }
        else
        {
            //Debug.LogError("Cutscene: PlayerMovement is null! Cannot simulate input.");
        }
        
        // Keep track of elapsed time
        float elapsedTime = 0f;
        
        while (elapsedTime < cutsceneDuration)
        {
            if (!cutsceneActive) break; 
            
            // IMPORTANT: Ensure the input stays set correctly
            if (simulatedInput != null)
            {
                // Make sure horizontal input is continuously set to right
                simulatedInput.HorizontalInput = 1.0f;
            }
            
            // Print debug info every half second
            if (Mathf.FloorToInt(elapsedTime * 2) != Mathf.FloorToInt((elapsedTime + Time.deltaTime) * 2))
            {
                //Debug.Log($"Cutscene: Time {elapsedTime:F1}s, HorizInput={simulatedInput.HorizontalInput}");
                
                // Verify player velocity
                if (playerMovement != null)
                {
                    var rb = playerMovement.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        //Debug.Log($"Player velocity: {rb.linearVelocity}");
                        
                        // If player isn't moving, check player movement component status
                        if (rb.linearVelocity.magnitude < 0.01f)
                        {
                            //Debug.Log($"Player not moving - PlayerMovement enabled: {playerMovement.enabled}");
                        }
                    }
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
        
        // Always make sure PlayerMovement is enabled when ending the cutscene
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        //Debug.Log("Cutscene: Ending cutscene");
        
        // Restore the original input system
        if (playerMovement != null && originalInput != null)
        {
            playerMovement.SetInputSystem(originalInput);
            //Debug.Log("Cutscene: Restored original input system");
            
            // Verify it was restored correctly
            var currentInput = playerMovement.GetInputSystem();
            bool correctlyRestored = currentInput == originalInput;
            //Debug.Log($"Cutscene: Original input restored correctly: {correctlyRestored}");
        }
        else
        {
            //Debug.LogError($"Cannot restore original input. PlayerMovement: {(playerMovement != null ? "Valid" : "Null")}, OriginalInput: {(originalInput != null ? "Valid" : "Null")}");
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

    public bool IsPlaying() 
    {
        return cutsceneActive;
    }
}