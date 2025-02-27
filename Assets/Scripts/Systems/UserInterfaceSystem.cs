using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UserInterfaceSystem : MonoBehaviour
{
    public static UserInterfaceSystem Instance { get; private set; }

    [Header("Menu Panels")]
    [SerializeField] private GameObject TitleScreen;
    [SerializeField] private GameObject LoadingScreen;
    [SerializeField] private GameObject PauseMenu;

    [Header("Loading Screen")]
    [SerializeField] private TextMeshProUGUI ProgressText;

    [Header("Title Screen")]
    [SerializeField] private TextMeshProUGUI LogoText;
    [SerializeField] private TextMeshProUGUI ContinueText;

    [Header("PauseMenu Controls")]
    [SerializeField] private Button ResumeButton;
    [SerializeField] private Button LoadButton;
    [SerializeField] private Button SettingsButton;
    [SerializeField] private Button QuitButton;

    [Header("Settings Controls")]
    [SerializeField] private Slider VolumeSlider;
    [SerializeField] private GameObject VolumeSliderContainer; // Add this in Inspector or assign in Start()

    private List<Button> pauseMenuButtons;
    private int selectedButtonIndex = 0;
    private Color defaultColor = Color.white;
    private Color selectedColor = Color.yellow;
    private bool isVolumeSliderActive = false;
    private float savedVolumeValue = 0.5f; // Default value

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
    }

    private void Start()
    {
        ShowTitleScreen();
        InitializePauseMenuButtons();
        
        // If VolumeSliderContainer is not assigned, try to find parent of VolumeSlider
        if (VolumeSliderContainer == null && VolumeSlider != null)
        {
            VolumeSliderContainer = VolumeSlider.gameObject;
        }
        
        // Initialize with hidden volume slider
        if (VolumeSliderContainer != null)
        {
            VolumeSliderContainer.SetActive(false);
        }
        
        // Load saved volume if available
        if (PlayerPrefs.HasKey("VolumeValue"))
        {
            savedVolumeValue = PlayerPrefs.GetFloat("VolumeValue");
            VolumeSlider.value = savedVolumeValue;
        }
    }

    private void Update()
    {
        // Handle pause menu toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        // Handle pause menu navigation when volume slider is not active
        if (PauseMenu.activeSelf && !isVolumeSliderActive)
        {
            HandlePauseMenuNavigation();
        }

        // Handle volume slider when it's active
        if (isVolumeSliderActive)
        {
            HandleVolumeSliderNavigation();
        }

        // Check for any key press to start the game
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.TitleScreen && Input.anyKeyDown)
        {
            Debug.Log("Any key pressed, starting game...");
            OnStartGameClicked();
        }
    }

    private void InitializePauseMenuButtons()
    {
        Debug.Log("Initializing pause menu buttons...");
        Debug.Log($"ResumeButton: {ResumeButton}");
        Debug.Log($"LoadButton: {LoadButton}");
        Debug.Log($"SettingsButton: {SettingsButton}");
        Debug.Log($"QuitButton: {QuitButton}");

        pauseMenuButtons = new List<Button> { ResumeButton, LoadButton, SettingsButton, QuitButton };
        UpdateButtonColors();
    }

    private void HandlePauseMenuNavigation()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedButtonIndex = (selectedButtonIndex - 1 + pauseMenuButtons.Count) % pauseMenuButtons.Count;
            UpdateButtonColors();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedButtonIndex = (selectedButtonIndex + 1) % pauseMenuButtons.Count;
            UpdateButtonColors();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            // If settings button is selected
            if (selectedButtonIndex == 2) // 2 is the index of SettingsButton
            {
                ToggleVolumeSlider();
            }
            else
            {
                pauseMenuButtons[selectedButtonIndex].onClick.Invoke();
            }
        }
    }

    private void ToggleVolumeSlider()
    {
        isVolumeSliderActive = !isVolumeSliderActive;
        
        if (VolumeSliderContainer != null)
        {
            VolumeSliderContainer.SetActive(isVolumeSliderActive);
        }
        
        if (isVolumeSliderActive)
        {
            // Load saved volume value
            VolumeSlider.value = savedVolumeValue;
        }
        else
        {
            // Save volume value when hiding
            SaveVolumeValue();
        }
    }

    private void HandleVolumeSliderNavigation()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            VolumeSlider.value = Mathf.Max(VolumeSlider.minValue, VolumeSlider.value - 0.01f);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            VolumeSlider.value = Mathf.Min(VolumeSlider.maxValue, VolumeSlider.value + 0.01f);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Save volume and hide slider
            SaveVolumeValue();
            ToggleVolumeSlider();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Just hide without saving when pressing Escape
            ToggleVolumeSlider();
        }
    }

    private void SaveVolumeValue()
    {
        savedVolumeValue = VolumeSlider.value;
        PlayerPrefs.SetFloat("VolumeValue", savedVolumeValue);
        PlayerPrefs.Save();
        Debug.Log($"Volume saved: {savedVolumeValue}");
    }

    private void UpdateButtonColors()
    {
        if (PauseMenu.activeSelf)
        {
            for (int i = 0; i < pauseMenuButtons.Count; i++)
            {
                ColorBlock colors = pauseMenuButtons[i].colors;
                colors.normalColor = (i == selectedButtonIndex) ? selectedColor : defaultColor;
                pauseMenuButtons[i].colors = colors;
            }
        }
    }

    public void ShowTitleScreen()
    {
        HideAllMenus();
        TitleScreen.SetActive(true);
    }

    public void ShowLoadingScreen()
    {
        HideAllMenus();
        LoadingScreen.SetActive(true);
    }

    public void UpdateLoadingProgress(float progress)
    {
        ProgressText.text = $"Loading... {(progress * 100):0}%";
    }

    public void HideLoadingScreen()
    {
        LoadingScreen.SetActive(false);
    }

    public void TogglePauseMenu()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
        {
            PauseMenu.SetActive(true);
            GameManager.Instance.PauseGame();
            selectedButtonIndex = 0; // Reset to the first button
            UpdateButtonColors();
            
            // Make sure volume slider is hidden when opening pause menu
            if (VolumeSliderContainer != null)
            {
                VolumeSliderContainer.SetActive(false);
            }
            isVolumeSliderActive = false;
        }
        else if (GameManager.Instance.CurrentGameState == GameManager.GameState.Paused)
        {
            // Save volume if the slider is active before closing
            if (isVolumeSliderActive)
            {
                SaveVolumeValue();
                isVolumeSliderActive = false;
            }
            
            PauseMenu.SetActive(false);
            GameManager.Instance.ResumeGame();
        }
    }

    private void HideAllMenus()
    {
        TitleScreen.SetActive(false);
        LoadingScreen.SetActive(false);
        PauseMenu.SetActive(false);
        
        // Hide volume slider
        if (VolumeSliderContainer != null)
        {
            VolumeSliderContainer.SetActive(false);
        }
        isVolumeSliderActive = false;
    }

    // Button handlers
    public void OnStartGameClicked()
    {
        Debug.Log("UserInterfaceSystem: Start Game button clicked");
        HideAllMenus();
        GameManager.Instance.StartGame();
    }

    public void OnResumeClicked()
    {
        Debug.Log("UserInterfaceSystem: Resume button clicked");
        TogglePauseMenu();
    }

    public void OnSettingsClicked()
    {
        Debug.Log("UserInterfaceSystem: Settings button clicked");
        ToggleVolumeSlider();
    }

    public void OnQuitClicked()
    {
        GameManager.Instance.QuitGame();
    }

    public void ShowPauseMenu()
    {
        HideAllMenus();
        PauseMenu.SetActive(true);
        selectedButtonIndex = 0; // Reset to the first button
        UpdateButtonColors();
    }
}