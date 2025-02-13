// CheckpointSystem.cs
using UnityEngine;
using System.Collections.Generic;

public class CheckpointSystem : MonoBehaviour
{
    public static CheckpointSystem Instance { get; private set; }
    
    [SerializeField] private Transform playerSpawnPoint;
    private List<Vector3> checkpoints = new List<Vector3>();
    private int currentCheckpointIndex = -1;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeCheckpoints();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeCheckpoints()
    {
        // Add initial spawn point
        if (playerSpawnPoint != null)
        {
            checkpoints.Add(playerSpawnPoint.position);
            currentCheckpointIndex = 0;
        }
        
        // Find all checkpoint objects in the level
        CheckpointTrigger[] checkpointTriggers = FindObjectsOfType<CheckpointTrigger>();
        foreach (CheckpointTrigger trigger in checkpointTriggers)
        {
            checkpoints.Add(trigger.transform.position);
        }
    }
    
    public void ActivateCheckpoint(Vector3 position)
    {
        int index = checkpoints.IndexOf(position);
        if (index > currentCheckpointIndex)
        {
            currentCheckpointIndex = index;
            GameManager.Instance.SetCheckpoint(position);
            // Optional: Play checkpoint activation effect
            AudioManager.Instance.PlaySFX("checkpoint");
        }
    }
    
    public Vector3 GetLastCheckpoint()
    {
        if (currentCheckpointIndex >= 0 && currentCheckpointIndex < checkpoints.Count)
        {
            return checkpoints[currentCheckpointIndex];
        }
        return playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
    }
}

// Companion class for CheckpointSystem
public class CheckpointTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointSystem.Instance.ActivateCheckpoint(transform.position);
        }
    }
}