using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    
    [Header("Title Screen")]
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private Animator logoAnimator;
    
    private bool isLoading = false;
    private static readonly string PRELOAD_SCENE = "PreloadScene";
    private static readonly string LOADING_SCENE = "LoadingScene";
    private static readonly string TITLE_SCENE = "TitleScene";
    private static readonly string MAIN_MENU_SCENE = "MainMenuScene";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLoader();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeLoader()
    {
        // Ensure loading screen starts hidden
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
            
        // Start with preload scene if not already there
        if (SceneManager.GetActiveScene().name != PRELOAD_SCENE)
        {
            StartCoroutine(LoadPreloadScene());
        }
    }
    
    private IEnumerator LoadPreloadScene()
    {
        // Load preload scene to initialize core systems
        yield return StartCoroutine(LoadSceneAsync(PRELOAD_SCENE));
        
        // Initialize core systems
        InitializeCoreSystems();
        
        // Proceed to title screen
        StartCoroutine(LoadTitleScreen());
    }
    
    private void InitializeCoreSystems()
    {
        // Create and initialize GameManager if it doesn't exist
        if (GameManager.Instance == null)
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManagerObj.AddComponent<GameManager>();
        }
        
        // Create and initialize SaveManager if it doesn't exist
        if (SaveManager.Instance == null)
        {
            GameObject saveManagerObj = new GameObject("SaveManager");
            saveManagerObj.AddComponent<SaveManager>();
        }
        
        // Create and initialize AudioManager if it doesn't exist
        if (AudioManager.Instance == null)
        {
            GameObject audioManagerObj = new GameObject("AudioManager");
            audioManagerObj.AddComponent<AudioManager>();
        }
    }
    
    public IEnumerator LoadTitleScreen()
    {
        yield return StartCoroutine(LoadSceneAsync(TITLE_SCENE));
        
        if (titleScreen != null)
        {
            titleScreen.SetActive(true);
            if (logoAnimator != null)
                logoAnimator.Play("LogoAnimation");
        }
        
        StartCoroutine(WaitForAnyKeyPress());
    }
    
    private IEnumerator WaitForAnyKeyPress()
    {
        yield return new WaitUntil(() => Input.anyKeyDown);
        StartCoroutine(LoadMainMenu());
    }
    
    public IEnumerator LoadMainMenu()
    {
        yield return StartCoroutine(LoadSceneAsync(MAIN_MENU_SCENE));
        GameManager.Instance.CurrentGameState = GameManager.GameState.MainMenu;
    }
    
    public IEnumerator LoadGameLevel(int levelNumber)
    {
        yield return StartCoroutine(LoadSceneAsync($"Level_{levelNumber}"));
        GameManager.Instance.CurrentGameState = GameManager.GameState.Playing;
    }
    
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (isLoading) yield break;
        isLoading = true;
        
        // Show loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(true);
            
        // Start async loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        float progress = 0f;
        
        // Update progress bar until 90% (Unity keeps the last 10% for activation)
        while (progress < 0.9f)
        {
            progress = Mathf.MoveTowards(progress, asyncLoad.progress, Time.deltaTime);
            if (progressBar != null)
                progressBar.fillAmount = progress;
            if (progressText != null)
                progressText.text = $"Loading... {(progress * 100):0}%";
                
            yield return null;
        }
        
        // Complete loading
        asyncLoad.allowSceneActivation = true;
        
        // Wait for scene to fully load
        while (!asyncLoad.isDone)
            yield return null;
            
        // Hide loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
            
        isLoading = false;
    }
    
    // Public methods for other scripts to initiate scene loading
    public void LoadLevel(int levelNumber)
    {
        StartCoroutine(LoadGameLevel(levelNumber));
    }
    
    public void LoadMainMenuScene()
    {
        StartCoroutine(LoadMainMenu());
    }
    
    public void RestartCurrentScene()
    {
        StartCoroutine(LoadSceneAsync(SceneManager.GetActiveScene().name));
    }
}


// Scene Flow
// Preload Scene: Initializes core systems (GameManager, SaveManager) using DontDestroyOnLoad
// This scene is responsible for setting up essential game systems that persist across scenes.


// Loading Screen: Uses SceneManager.LoadSceneAsync with loading progress bar
// This scene displays a loading screen with a progress bar while the next scene is being loaded asynchronously.


// Title Screen: Simple "Press Any Key" with animated logo
// This scene shows a title screen with an animated logo and waits for the player to press any key to proceed.


// Main Menu: Implement using Unity UI with event-based navigation
// This scene contains the main menu, which is implemented using Unity's UI system and allows navigation through events.

