using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuSystem : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject TitleScreen;
    [SerializeField] private GameObject LoadingScreen;
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject SettingsMenu;


    [Header("PauseMenu Controls")]

    [SerializeField] private Button ResumeButton;
    [SerializeField] private Button LoadButton;
    [SerializeField] private Button SettingsButton;
    [SerializeField] private Button QuitButton;

    
    [Header("Settings Controls")]
    [SerializeField] private Button BackButton;
    [SerializeField] private Slider VolumeSlider;
    
    private void Start()
    {
        // Initialize volume sliders
        if (VolumeSlider != null)
        {
            VolumeSlider.onValueChanged.AddListener(OnVolumeChanged);
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
        TitleScreen.SetActive(true);
        GameManager.Instance.LoadTitleScreen();
    }
    
    public void ShowLoadingScreen()
    {
        HideAllMenus();
        LoadingScreen.SetActive(true);
    }
    
    public void ShowSettingsMenu()
    {
        HideAllMenus();
        SettingsMenu.SetActive(true);
    }
    
    public void TogglePauseMenu()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
        {
            PauseMenu.SetActive(true);
            GameManager.Instance.PauseGame();
        }
        else if (GameManager.Instance.CurrentGameState == GameManager.GameState.Paused)
        {
            PauseMenu.SetActive(false);
            GameManager.Instance.ResumeGame();
        }
    }
    
    private void HideAllMenus()
    {
        TitleScreen.SetActive(false);
        LoadingScreen.SetActive(false);
        PauseMenu.SetActive(false);
        SettingsMenu.SetActive(false);
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
    private void OnVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
        AudioManager.Instance.SetSFXVolume(value);
    }
}