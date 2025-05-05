using UnityEngine;
using UnityEngine.UI;

public class CollectableManager : MonoBehaviour
{
    // Singleton instance
    public static CollectableManager Instance;
    
    [Header("Collectable Settings")]
    public int totalCollectables = 0;
    public int collectedCount = 0;
    
    [Header("UI Elements")]
    public GameObject collectablesUI; // Parent UI object containing all collectable UI elements
    public Text collectableCountText;
    public bool showPercentage = false;
    
    [Header("Audio Settings")]
    public AudioClip collectSound;
    private AudioSource audioSource;

    private bool isActive = false;

    void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Set up audio
        if (collectSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = collectSound;
        }
    }

    void Start()
    {
        // Hide UI initially
        SetUIVisibility(false);
        
        // Count total collectables in the scene
        CountTotalCollectables();
    }

    void Update()
    {
        // Check game state
        if (GameManager.Instance != null)
        {
            bool shouldBeActive = GameManager.Instance.CurrentGameState == GameManager.GameState.Playing;
            
            // Only update if there's a state change
            if (shouldBeActive != isActive)
            {
                isActive = shouldBeActive;
                SetUIVisibility(isActive);
            }
        }
    }

    // Toggle UI visibility
    private void SetUIVisibility(bool visible)
    {
        if (collectablesUI != null)
        {
            collectablesUI.SetActive(visible);
        }
        
        // If becoming visible, update the UI
        if (visible)
        {
            UpdateUI();
        }
    }

    // Called by collectables when they're picked up
    public void CollectItem()
    {
        // Only process if the game is in playing state
        if (!isActive) return;
        
        collectedCount++;
        
        // Play sound effect
        if (audioSource != null && collectSound != null)
        {
            audioSource.Play();
        }
        
        UpdateUI();
    }
    
    private void CountTotalCollectables()
    {
        Collectable[] allCollectables = FindObjectsOfType<Collectable>();
        totalCollectables = allCollectables.Length;
    }
    
    private void UpdateUI()
    {
        if (collectableCountText != null && isActive)
        {
            if (showPercentage && totalCollectables > 0)
            {
                float percentage = (float)collectedCount / totalCollectables * 100f;
                collectableCountText.text = $"Collected: {percentage:0}%";
            }
            else
            {
                collectableCountText.text = $"Collected: {collectedCount} / {totalCollectables}";
            }
        }
    }
    
    // For saving game progress
    public void SaveProgress()
    {
        PlayerPrefs.SetInt("CollectedItems", collectedCount);
        PlayerPrefs.Save();
    }
    
    // For loading game progress
    public void LoadProgress()
    {
        collectedCount = PlayerPrefs.GetInt("CollectedItems", 0);
        UpdateUI();
    }
}