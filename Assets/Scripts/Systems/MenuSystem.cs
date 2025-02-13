using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuSystem : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;
    
    [Header("Settings Controls")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    private void Start()
    {
        // Initialize volume sliders
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        
        ShowTitleScreen();
    }
    
    private void Update()
    {
        // Handle pause menu toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }
    
    public void ShowTitleScreen()
    {
        HideAllMenus();
        titleScreen.SetActive(true);
        GameManager.Instance.LoadTitleScreen();
    }
    
    public void ShowMainMenu()
    {
        HideAllMenus();
        mainMenu.SetActive(true);
    }
    
    public void ShowSettingsMenu()
    {
        HideAllMenus();
        settingsMenu.SetActive(true);
    }
    
    public void TogglePauseMenu()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
        {
            pauseMenu.SetActive(true);
            GameManager.Instance.PauseGame();
        }
        else if (GameManager.Instance.CurrentGameState == GameManager.GameState.Paused)
        {
            pauseMenu.SetActive(false);
            GameManager.Instance.ResumeGame();
        }
    }
    
    private void HideAllMenus()
    {
        titleScreen.SetActive(false);
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }
    
    // Button handlers
    public void OnStartGameClicked()
    {
        HideAllMenus();
        GameManager.Instance.StartGame();
    }
    
    public void OnResumeClicked()
    {
        TogglePauseMenu();
    }
    
    public void OnLoadGameClicked()
    {
        if (SaveManager.Instance.LoadGame())
        {
            HideAllMenus();
        }
    }
    
    public void OnSettingsClicked()
    {
        ShowSettingsMenu();
    }
    
    public void OnQuitClicked()
    {
        GameManager.Instance.QuitGame();
    }
    
    // Settings handlers
    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }
}