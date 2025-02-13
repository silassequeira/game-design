// SaveManager.cs
using UnityEngine;
using System.IO;
using System;

[Serializable]
public class GameSaveData
{
    public int currentLevel;
    public Vector3 checkpointPosition;
    public float playerHealth;
    // Add more save data properties as needed
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private string SavePath => Path.Combine(Application.persistentDataPath, "gamesave.json");
    
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
    
    public void SaveGame()
    {
        GameSaveData saveData = new GameSaveData
        {
            currentLevel = GameManager.Instance.CurrentLevel,
            checkpointPosition = GameManager.Instance.LastCheckpoint,
            // Add more data to save
        };
        
        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(SavePath, json);
    }
    
    public bool LoadGame()
    {
        if (!File.Exists(SavePath))
            return false;
            
        try
        {
            string json = File.ReadAllText(SavePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            
            // Apply loaded data
            GameManager.Instance.LoadLevel(saveData.currentLevel);
            // Set player position to checkpoint
            if (FindObjectOfType<PlayerController>() is PlayerController player)
            {
                player.transform.position = saveData.checkpointPosition;
            }
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading save file: {e.Message}");
            return false;
        }
    }
    
    public void DeleteSaveData()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }
}



//Checkpoint System

//Use trigger colliders with Checkpoint tags
//Save game state when player enters trigger
//Visual indicator (flag/light) for active checkpoint