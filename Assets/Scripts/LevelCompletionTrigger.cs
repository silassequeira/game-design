using UnityEngine;
using System.Collections;

public class LevelCompleteTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private float proximityThreshold = 2f;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private float delayBeforeCutscene = 0.5f;

    [Header("References")]
    [SerializeField] private EndLevelCutscene cutsceneController;

    private bool hasTriggered = false;
    private Transform playerTransform;
private void Start()
{
    if (playerTransform == null)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    // Find cutscene controller if not assigned
    if (cutsceneController == null)
    {
        cutsceneController = Object.FindFirstObjectByType<EndLevelCutscene>();
        if (cutsceneController == null)
        {
            Debug.LogError("No EndLevelCutscene found in scene!");
        }
    }
}

    private void Update()
    {
        // Skip if already triggered or player not found
        if (hasTriggered || playerTransform == null) return;

        // Check if player is within proximity threshold
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        if (distance <= proximityThreshold)
        {
            hasTriggered = triggerOnce;
            StartCoroutine(TriggerCutsceneWithDelay());
        }
    }

    private IEnumerator TriggerCutsceneWithDelay()
    {
        // Optional: Do some visual effect to indicate trigger activation
        
        // Wait for delay
        yield return new WaitForSeconds(delayBeforeCutscene);
        
        // Start cutscene
        if (cutsceneController != null)
        {
            cutsceneController.StartEndLevelCutscene();
        }
    }

    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, proximityThreshold);
    }
}