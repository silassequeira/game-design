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
    [SerializeField] private Image LogoImage;
    [SerializeField] private TextMeshProUGUI ContinueText;

    [Header("PauseMenu Controls")]
    [SerializeField] private Button ResumeButton;
    [SerializeField] private Button LoadButton;
    [SerializeField] private Button SettingsButton;
    [SerializeField] private Button QuitButton;

    [Header("Settings Controls")]
    [SerializeField] private Slider VolumeSlider;
    [SerializeField] private GameObject VolumeSliderContainer;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private List<Button> pauseMenuButtons;
    private int selectedButtonIndex = 0;
    private Color defaultColor = Color.white;
    private Color selectedColor = Color.yellow;
    private bool isVolumeSliderActive = false;
    private float savedVolumeValue = 0.5f;
    private bool isTransitioning = false;

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
        ShowTitleScreenWithAnimation();
        
        // If VolumeSliderContainer is not assigned, try to find parent of VolumeSlider
        if (VolumeSliderContainer == null && VolumeSlider != null)
        {
            VolumeSliderContainer = VolumeSlider.transform.parent.gameObject;
            if (VolumeSliderContainer == null)
            {
                VolumeSliderContainer = VolumeSlider.gameObject;
            }
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
            if (VolumeSlider != null)
            {
                VolumeSlider.value = savedVolumeValue;
            }
        }
        
        // Initialize buttons after UI is set up
        InitializePauseMenuButtons();
    }

    private void Update()
    {
        // Skip input handling during transitions
        if (isTransitioning)
            return;
            
        // Handle pause menu toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        // Handle pause menu navigation when volume slider is not active
        if (PauseMenu != null && PauseMenu.activeSelf && !isVolumeSliderActive)
        {
            HandlePauseMenuNavigation();
        }

        // Handle volume slider when it's active
        if (isVolumeSliderActive && VolumeSlider != null)
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
        pauseMenuButtons = new List<Button>();
        
        // Check if buttons are assigned before adding to list
        if (ResumeButton != null) pauseMenuButtons.Add(ResumeButton);
        if (LoadButton != null) pauseMenuButtons.Add(LoadButton);
        if (SettingsButton != null) pauseMenuButtons.Add(SettingsButton);
        if (QuitButton != null) pauseMenuButtons.Add(QuitButton);
        
        Debug.Log($"Pause menu buttons initialized. Count: {pauseMenuButtons.Count}");
        
        // Make sure we have buttons before trying to update colors
        if (pauseMenuButtons.Count > 0)
        {
            UpdateButtonColors();
        }
        else
        {
            Debug.LogError("No pause menu buttons were assigned in the Inspector!");
        }
    }

    private void HandlePauseMenuNavigation()
    {
        // Check if we have any buttons to navigate
        if (pauseMenuButtons == null || pauseMenuButtons.Count == 0)
        {
            Debug.LogWarning("No pause menu buttons available for navigation");
            return;
        }
        
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
            // Make sure we have a valid button index
            if (selectedButtonIndex >= 0 && selectedButtonIndex < pauseMenuButtons.Count)
            {
                // If settings button is selected
                if (pauseMenuButtons[selectedButtonIndex] == SettingsButton)
                {
                    ToggleVolumeSlider();
                }
                else
                {
                    pauseMenuButtons[selectedButtonIndex].onClick.Invoke();
                }
            }
        }
    }

    private void ToggleVolumeSlider()
    {
        Debug.Log("Toggling volume slider");
        isVolumeSliderActive = !isVolumeSliderActive;
        
        if (VolumeSliderContainer != null)
        {
            // Simple show/hide without transitions
            VolumeSliderContainer.SetActive(isVolumeSliderActive);
            
            if (isVolumeSliderActive)
            {
                // Load saved volume value
                if (VolumeSlider != null)
                {
                    VolumeSlider.value = savedVolumeValue;
                    Debug.Log($"Volume slider activated. Value: {savedVolumeValue}");
                }
            }
            else
            {
                // Save volume value when hiding
                SaveVolumeValue();
                Debug.Log("Volume slider deactivated");
            }
        }
        else
        {
            Debug.LogError("VolumeSliderContainer is not assigned!");
        }
    }

    private void HandleVolumeSliderNavigation()
    {
        if (VolumeSlider == null) return;
        
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
        if (VolumeSlider != null)
        {
            savedVolumeValue = VolumeSlider.value;
            PlayerPrefs.SetFloat("VolumeValue", savedVolumeValue);
            PlayerPrefs.Save();
            Debug.Log($"Volume saved: {savedVolumeValue}");
        }
    }

    private void UpdateButtonColors()
    {
        if (PauseMenu == null || !PauseMenu.activeSelf || pauseMenuButtons == null) return;
        
        for (int i = 0; i < pauseMenuButtons.Count; i++)
        {
            if (pauseMenuButtons[i] != null)
            {
                ColorBlock colors = pauseMenuButtons[i].colors;
                colors.normalColor = (i == selectedButtonIndex) ? selectedColor : defaultColor;
                pauseMenuButtons[i].colors = colors;
            }
        }
    }

    public void ShowTitleScreenWithAnimation()
    {
        HideAllMenus();
        if (TitleScreen != null)
        {
            TitleScreen.SetActive(true);
            
            // Animate title screen
            CanvasGroup titleCanvasGroup = GetOrAddCanvasGroup(TitleScreen);
            titleCanvasGroup.alpha = 0;
            
            isTransitioning = true;
            StartCoroutine(FadeCanvasGroup(titleCanvasGroup, 0, 1, fadeInDuration, () => {
                isTransitioning = false;
                // Start pulsating continue text if available
                if (ContinueText != null)
                {
                    StartCoroutine(PulseContinueText());
                }
            }));
        }
    }
    
    private IEnumerator PulseContinueText()
    {
        if (ContinueText == null) yield break;
        
        while (TitleScreen.activeSelf && ContinueText != null)
        {
            // Pulse the continue text's alpha
            float time = 0;
            float duration = 1.5f;
            float startAlpha = 0.2f;
            float endAlpha = 1.0f;
            
            while (time < duration && TitleScreen.activeSelf)
            {
                time += Time.deltaTime;
                float normalizedTime = time / duration;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.PingPong(normalizedTime * 2, 1));
                
                ContinueText.alpha = alpha;
                yield return null;
            }
            
            yield return null;
        }
    }

    public void ShowLoadingScreen()
    {
        HideAllMenus();
        if (LoadingScreen != null)
        {
            LoadingScreen.SetActive(true);
            
            // Animate loading screen
            CanvasGroup loadingCanvasGroup = GetOrAddCanvasGroup(LoadingScreen);
            loadingCanvasGroup.alpha = 0;
            
            isTransitioning = true;
            StartCoroutine(FadeCanvasGroup(loadingCanvasGroup, 0, 1, fadeInDuration, () => {
                isTransitioning = false;
            }));
        }
    }

    public void UpdateLoadingProgress(float progress)
    {
        if (ProgressText != null)
        {
            ProgressText.text = $"Loading... {(progress * 100):0}%";
        }
    }

    public void HideLoadingScreen()
    {
        if (LoadingScreen != null)
        {
            // Fade out loading screen
            CanvasGroup loadingCanvasGroup = GetOrAddCanvasGroup(LoadingScreen);
            
            isTransitioning = true;
            StartCoroutine(FadeCanvasGroup(loadingCanvasGroup, loadingCanvasGroup.alpha, 0, fadeOutDuration, () => {
                LoadingScreen.SetActive(false);
                isTransitioning = false;
            }));
        }
    }

    public void ShowTitleScreen()
    {
        HideAllMenus();
        if (TitleScreen != null)
        {
            TitleScreen.SetActive(true);
        }
    }

    public void TogglePauseMenu()
    {
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
        {
            if (PauseMenu != null)
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
                
                Debug.Log("Pause menu opened");
            }
        }
        else if (GameManager.Instance.CurrentGameState == GameManager.GameState.Paused)
        {
            // Save volume if the slider is active before closing
            if (isVolumeSliderActive)
            {
                SaveVolumeValue();
                isVolumeSliderActive = false;
            }
            
            if (PauseMenu != null)
            {
                PauseMenu.SetActive(false);
                GameManager.Instance.ResumeGame();
                
                Debug.Log("Pause menu closed");
            }
        }
    }

    private void HideAllMenus()
    {
        if (TitleScreen != null) TitleScreen.SetActive(false);
        if (LoadingScreen != null) LoadingScreen.SetActive(false);
        if (PauseMenu != null) PauseMenu.SetActive(false);
        
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
        
        // Don't allow multiple clicks during transition
        if (isTransitioning) return;
        
        if (TitleScreen != null)
        {
            // Fade out title screen before starting game
            CanvasGroup titleCanvasGroup = GetOrAddCanvasGroup(TitleScreen);
            
            isTransitioning = true;
            StartCoroutine(FadeCanvasGroup(titleCanvasGroup, titleCanvasGroup.alpha, 0, fadeOutDuration, () => {
                HideAllMenus();
                GameManager.Instance.StartGame();
                isTransitioning = false;
            }));
        }
        else
        {
            HideAllMenus();
            GameManager.Instance.StartGame();
        }
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
        Debug.Log("UserInterfaceSystem: Quit button clicked");
        GameManager.Instance.QuitGame();
    }

    public void ShowPauseMenu()
    {
        HideAllMenus();
        if (PauseMenu != null)
        {
            PauseMenu.SetActive(true);
            selectedButtonIndex = 0; // Reset to the first button
            UpdateButtonColors();
            Debug.Log("Pause menu shown via ShowPauseMenu method");
        }
        else
        {
            Debug.LogError("PauseMenu reference is null in ShowPauseMenu method");
        }
    }
    
    #region Animation Helpers
    
    private CanvasGroup GetOrAddCanvasGroup(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        return canvasGroup;
    }
    
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float targetAlpha, 
                                       float duration, System.Action onComplete = null)
    {
        float elapsedTime = 0;
        canvasGroup.alpha = startAlpha;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            float evaluatedTime = transitionCurve.Evaluate(normalizedTime);
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, evaluatedTime);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        
        if (onComplete != null)
        {
            onComplete();
        }
    }
    
    #endregion
}