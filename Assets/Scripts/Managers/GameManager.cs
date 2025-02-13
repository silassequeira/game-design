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
        MainMenu,
        Playing,
        Paused
    }
    
    private GameState currentGameState;
    public GameState CurrentGameState => currentGameState;
    
    // Add this method to change game state
    public void SetGameState(GameState newState)
    {
        currentGameState = newState;
    }

    public int CurrentLevel { get; private set; }
    public Vector3 LastCheckpoint { get; private set; }
    
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
        SceneManager.LoadScene("TitleScreen");
    }
    
    public void StartGame()
    {
        currentGameState = GameState.Playing;
        LoadLevel(CurrentLevel);
    }
    
    public void LoadLevel(int levelNumber)
    {
        if (levelNumber >= 1 && levelNumber <= 6)
        {
            CurrentLevel = levelNumber;
            SceneManager.LoadScene($"Level_{levelNumber}");
        }
    }
    
    public void SetCheckpoint(Vector3 position)
    {
        LastCheckpoint = position;
        SaveManager.Instance.SaveGame();
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
        if (currentGameState == GameState.Paused)
        {
            currentGameState = GameState.Playing;
            Time.timeScale = 1f;
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