using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class VideoCutscene : MonoBehaviour
{
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private bool waitForVideoToEnd = true;
    
    // Add these references
    [SerializeField] private Canvas videoCanvas;
    [SerializeField] private RawImage videoImage;
    
    [Header("Skip Settings")]
    [SerializeField] private bool allowSkipping = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    // Removed: [SerializeField] private GameObject skipPrompt; // Optional UI element showing "Press SPACE to skip"
    // Removed: [SerializeField] private float skipPromptDelay = 2f; // Show skip hint after delay
    
    private VideoPlayer videoPlayer;
    // Removed: private float cutsceneActiveTime = 0f;

    public System.Action OnCutsceneEnded;
    public bool isPlaying = false;
    private RenderTexture renderTexture;


    private void Awake()
    {
        // Create video canvas and raw image if not assigned
        if (videoCanvas == null)
        {
            GameObject canvasObj = new GameObject("VideoCanvas");
            videoCanvas = canvasObj.AddComponent<Canvas>();
            videoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            videoCanvas.sortingOrder = 10000; // Very high sort order to be in front
            
            // Add a canvas scaler for proper scaling
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            GameObject imageObj = new GameObject("VideoImage");
            imageObj.transform.SetParent(canvasObj.transform, false);
            videoImage = imageObj.AddComponent<RawImage>();
            
            // Make the image fill the entire screen
            RectTransform rect = videoImage.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // Hide initially
            videoCanvas.gameObject.SetActive(false);
        }
        
        // Set up video player
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.clip = videoClip;
        
        // Create render texture
        int width = videoClip != null ? (int)videoClip.width : 1920;
        int height = videoClip != null ? (int)videoClip.height : 1080;
        if (width <= 0) width = 1920;
        if (height <= 0) height = 1080;
        
        renderTexture = new RenderTexture(width, height, 24);
        
        // Set up render texture as target
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        
        if (videoImage != null)
        {
            videoImage.texture = renderTexture;
        }
        
        // Register event when video is done
        videoPlayer.loopPointReached += OnVideoFinished;
    }
      
    private void Update()
    {
        // Handle skipping of video cutscene
        if (isPlaying && allowSkipping)
        {
            // Removed: Show skip prompt after delay
            // if (skipPrompt != null)
            // {
            //     cutsceneActiveTime += Time.deltaTime;
            //     skipPrompt.SetActive(cutsceneActiveTime >= skipPromptDelay);
            // }
            
            if (Input.GetKeyDown(skipKey))
            {
                ForceEndCutscene();
            }
        }
    }
    
    public void StartVideoCutscene()
    {
        if (isPlaying || videoClip == null) return;
        
        // Removed: Reset timer for skip prompt
        // cutsceneActiveTime = 0f;
        
        // Removed: Hide skip prompt initially
        // if (skipPrompt != null)
        // {
        //     skipPrompt.SetActive(false);
        // }
        
        // Disable player
        DisablePlayerControls();
        
        // If using the GameManager
        if (GameManager.Instance != null)
        {
            // Assuming GameManager.GameState.Loading exists and is appropriate
            // If not, you might need to adjust or remove this line based on your GameManager setup
            GameManager.Instance.SetGameState(GameManager.GameState.Loading); 
        }
        
        // Show the video canvas
        if (videoCanvas != null)
        {
            videoCanvas.gameObject.SetActive(true);
        }
        
        // Start playback
        videoPlayer.Play();
        isPlaying = true;
        
        // If not waiting for the video to complete, start a timer
        if (!waitForVideoToEnd)
        {
            StartCoroutine(EndAfterDuration());
        }
    }
    
    private void OnVideoFinished(VideoPlayer vp)
    {
        if (waitForVideoToEnd)
        {
            EndCutscene();
        }
    }
    
    // Timer for non-waiting mode
    private IEnumerator EndAfterDuration()
    {
        // Wait for the video to complete
        yield return new WaitForSeconds((float)videoClip.length);
        EndCutscene();
    }
    
    // End the cutscene 
    private void EndCutscene()
    {
        isPlaying = false;
        videoPlayer.Stop();
        
        // Hide the video canvas
        if (videoCanvas != null)
        {
            videoCanvas.gameObject.SetActive(false);
        }
        
        // Removed: Hide skip prompt if it exists
        // if (skipPrompt != null)
        // {
        //     skipPrompt.SetActive(false);
        // }
        
        // Invoke the event
        OnCutsceneEnded?.Invoke();
        
        // Re-enable player only if not in sequence
        if (CutsceneManager.Instance == null) // Assuming CutsceneManager.Instance exists
        {
            EnablePlayerControls();
            
            // If using the GameManager
            if (GameManager.Instance != null)
            {
                // Assuming GameManager.GameState.Playing exists and is appropriate
                GameManager.Instance.SetGameState(GameManager.GameState.Playing);
            }
        }
    }
    
    private void DisablePlayerControls()
    {
        PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
    }
    
    private void EnablePlayerControls()
    {
        PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    public bool IsPlaying() => isPlaying;

    public void ForceEndCutscene()
    {
        if (isPlaying)
            EndCutscene();
    }
}
