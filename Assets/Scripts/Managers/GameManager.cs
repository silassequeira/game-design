// Singleton for game state
// Global game state and level management.

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public enum GameState
    {
        Loading,
        TitleScreen,
        Playing, 
        Paused
    }
    
    private GameState currentGameState;
    public GameState CurrentGameState => currentGameState;
    
    public void SetGameState(GameState newState)
    {
        currentGameState = newState;
    }

    public int CurrentLevel { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeGame()
    {
        currentGameState = GameState.Loading;
        CurrentLevel = 1;
        StartCoroutine(LoadingSequence());
    }
    
    private IEnumerator LoadingSequence()
    {
        // Simulate loading time
        yield return new WaitForSeconds(2f);
        LoadTitleScreen();
    }
    
    public void LoadTitleScreen()
    {
        currentGameState = GameState.TitleScreen;
        UserInterfaceSystem.Instance.ShowTitleScreen();
    }
    
    public void StartGame()
    {
        Debug.Log("GameManager: Starting game...");
        currentGameState = GameState.Playing;
        CurrentLevel = 1;
        Debug.Log("GameManager: Game state set to Playing");
    }
    
    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
        {
            currentGameState = GameState.Paused;
            Time.timeScale = 0f;
        }
    }
    
public void ResumeGame()
{
    Debug.Log("ResumeGame button clicked");
    if (currentGameState == GameState.Paused)
    {
        currentGameState = GameState.Playing;
        Time.timeScale = 1f;
        Debug.Log("Game state set to Playing");
    }
    else
    {
        Debug.Log("Game is not in Paused state");
    }
}
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}