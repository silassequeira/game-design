using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }
    
    [Header("Cutscene References")]
    [SerializeField] private VideoCutscene videoCutscene;
    [SerializeField] private IntroductoryCutscene introCutscene;
    
    private bool cutsceneSequenceActive = false;
    
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
        
        // Find references if not assigned
        if (videoCutscene == null)
            videoCutscene = FindObjectOfType<VideoCutscene>();
            
        if (introCutscene == null)
            introCutscene = FindObjectOfType<IntroductoryCutscene>();
    }
    
    // Called from GameManager.StartGame() instead of directly handling title screen
    public void StartGameSequence()
    {
        if (cutsceneSequenceActive) return;
        
        cutsceneSequenceActive = true;
        
        // Game state is already set to Loading by GameManager
        
        // Start with the video cutscene
        if (videoCutscene != null)
        {
            // Subscribe to video cutscene completion event
            videoCutscene.OnCutsceneEnded += PlayIntroCutscene;
            
            // Start the video cutscene
            videoCutscene.StartVideoCutscene();
            //Debug.Log("CutsceneManager: Starting video cutscene");
        }
        else
        {
            // Skip to intro cutscene if video not available
            PlayIntroCutscene();
        }
    }
    
    private void PlayIntroCutscene()
    {
        // Unsubscribe from video event
        if (videoCutscene != null)
            videoCutscene.OnCutsceneEnded -= PlayIntroCutscene;
        
        //Debug.Log("CutsceneManager: Video cutscene complete, starting intro cutscene");
        
        // Play the introductory cutscene
        if (introCutscene != null)
        {
            introCutscene.StartIntroCutscene();
        }
        else
        {
            // If no intro cutscene, go straight to gameplay
            FinishCutsceneSequence();
        }
    }
    
    // Called when all cutscenes are complete
    private void FinishCutsceneSequence()
    {
        cutsceneSequenceActive = false;
        
        // Set game to playing state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(GameManager.GameState.Playing);
            //Debug.Log("CutsceneManager: All cutscenes complete, setting state to Playing");
        }
    }
}