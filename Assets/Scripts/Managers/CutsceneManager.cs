using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }
    
    [Header("Cutscene References")]
    [SerializeField] private EndLevelCutscene videoCutscene;
    [SerializeField] private IntroductoryCutscene introCutscene;
    
    [Header("Settings")]
    [SerializeField] private bool allowSkippingAllCutscenes = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Escape;
    
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
            videoCutscene = FindObjectOfType<EndLevelCutscene>();
            
        if (introCutscene == null)
            introCutscene = FindObjectOfType<IntroductoryCutscene>();
    }
    
    private void Update()
    {
        // Skip all cutscenes if enabled
        if (cutsceneSequenceActive && allowSkippingAllCutscenes && Input.GetKeyDown(skipKey))
        {
            SkipAllCutscenes();
        }
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
            videoCutscene.StartEndLevelCutscene();
            Debug.Log("CutsceneManager: Starting video cutscene");
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
        
        Debug.Log("CutsceneManager: Video cutscene complete, starting intro cutscene");
        
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
            Debug.Log("CutsceneManager: All cutscenes complete, setting state to Playing");
        }
    }
    
    public void SkipAllCutscenes()
    {
        Debug.Log("CutsceneManager: Skipping all cutscenes");
        
        // Unsubscribe from events
        if (videoCutscene != null)
            videoCutscene.OnCutsceneEnded -= PlayIntroCutscene;
            
        // Stop any active cutscenes
        if (videoCutscene != null && videoCutscene.IsPlaying())
            videoCutscene.ForceEndCutscene();
            
        if (introCutscene != null)
            introCutscene.EndCutscene();
            
        // Finish the sequence
        FinishCutsceneSequence();
    }
}