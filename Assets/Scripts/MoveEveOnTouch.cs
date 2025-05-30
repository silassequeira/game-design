using UnityEngine;

public class MoveEveOnTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject eve; // Assign Eve GameObject in the Inspector
    
[Header("Settings")]
[SerializeField] private Vector3 targetPosition = new Vector3(41.71f, 1.57f, 0f);
[SerializeField] private bool useLocalPosition = true; // Set to true if Eve is inside a parent
[SerializeField] private bool moveOnce = true;
[SerializeField] private bool debugMode = true;

    private bool hasMovedEve = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Make sure we're colliding with the player
        if (collision.CompareTag("Player"))
        {
            // Only move Eve if we haven't moved her yet (if moveOnce is true)
            if (!hasMovedEve || !moveOnce)
            {
                if (debugMode) Debug.Log("Player triggered the Eve mover!");
                
                MoveEve();
            }
        }
    }
    
private void MoveEve()
{
    if (eve != null)
    {
        Vector3 originalPosition = eve.transform.position;
        
        // Set either local or world position based on the flag
        if (useLocalPosition)
        {
            eve.transform.localPosition = targetPosition;
        }
        else
        {
            eve.transform.position = targetPosition;
        }
        
        if (debugMode)
        {
            Debug.Log($"Eve moved from {originalPosition} to {eve.transform.position} " + 
                      $"(using {(useLocalPosition ? "local" : "world")} position)");
        }
        
        hasMovedEve = true;
    }
    else
    {
        Debug.LogError("Eve GameObject is not assigned in the Inspector on " + gameObject.name);
    }
}
}