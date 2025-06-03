using UnityEngine;
using System.Collections;

public class MoveEveOnTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject eve; // Assign Eve GameObject in the Inspector
    [SerializeField] private GameObject[] additionalObjects; // Additional objects to move
    
    [Header("Fog of War")]
    [SerializeField] private GameObject[] fogObjects; // Assign fog objects that hide unexplored areas
    [SerializeField] private FogRevealType revealType = FogRevealType.Fade; // How to reveal the area
    [SerializeField] private float fadeSpeed = 2f; // Used for fade effect
    
    [Header("Settings")]
    [SerializeField] private Vector3 targetPosition = new Vector3(41.71f, 1.57f, 0f);
    [SerializeField] private bool useLocalPosition = true; // Set to true if objects are inside a parent
    [SerializeField] private bool moveOnce = true;
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool keepRelativePositions = false; // Keep objects' positions relative to Eve

    private bool hasMovedObjects = false;
    private Vector3[] relativePositions; // Store relative positions if needed
    
    // Define how to reveal fog
    public enum FogRevealType
    {
        Destroy,   // Simply destroy fog objects
        Disable,   // Set fog objects inactive
        Fade       // Gradually fade them out
    }
    
    private void Start()
    {
        // If keeping relative positions, calculate and store them at start
        if (keepRelativePositions && eve != null && additionalObjects != null && additionalObjects.Length > 0)
        {
            relativePositions = new Vector3[additionalObjects.Length];
            
            for (int i = 0; i < additionalObjects.Length; i++)
            {
                if (additionalObjects[i] != null)
                {
                    if (useLocalPosition)
                    {
                        // Store relative local positions
                        relativePositions[i] = additionalObjects[i].transform.localPosition - eve.transform.localPosition;
                    }
                    else
                    {
                        // Store relative world positions
                        relativePositions[i] = additionalObjects[i].transform.position - eve.transform.position;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Make sure we're colliding with the player
        if (collision.CompareTag("Player"))
        {
            // Only move objects if we haven't moved them yet (if moveOnce is true)
            if (!hasMovedObjects || !moveOnce)
            {
                if (debugMode) Debug.Log("Player triggered the object mover!");
                
                MoveObjects();
                RevealFogOfWar();
            }
        }
    }
    
    private void MoveObjects()
    {
        // First move Eve
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
                Debug.Log($"Eve moved from {originalPosition} to {eve.transform.position} (using {(useLocalPosition ? "local" : "world")} position)");
            }
        }
        
        // Then move additional objects
        if (additionalObjects != null && additionalObjects.Length > 0)
        {
            for (int i = 0; i < additionalObjects.Length; i++)
            {
                if (additionalObjects[i] != null)
                {
                    Vector3 newPosition;
                    
                    // Calculate position based on whether we're keeping relative positions
                    if (keepRelativePositions && relativePositions != null && i < relativePositions.Length)
                    {
                        if (useLocalPosition)
                        {
                            newPosition = targetPosition + relativePositions[i];
                        }
                        else
                        {
                            newPosition = targetPosition + relativePositions[i];
                        }
                    }
                    else
                    {
                        // Move to exact same target position
                        newPosition = targetPosition;
                    }
                    
                    // Apply the position
                    if (useLocalPosition)
                    {
                        additionalObjects[i].transform.localPosition = newPosition;
                    }
                    else
                    {
                        additionalObjects[i].transform.position = newPosition;
                    }
                    
                    if (debugMode)
                    {
                        Debug.Log($"Additional object '{additionalObjects[i].name}' moved to position {newPosition}");
                    }
                }
            }
        }
        
        hasMovedObjects = true;
    }
    
    private void RevealFogOfWar()
    {
        if (fogObjects == null || fogObjects.Length == 0)
        {
            if (debugMode) Debug.Log("No fog objects assigned to reveal.");
            return;
        }
        
        switch (revealType)
        {
            case FogRevealType.Destroy:
                DestroyFogObjects();
                break;
                
            case FogRevealType.Disable:
                DisableFogObjects();
                break;
                
            case FogRevealType.Fade:
                StartCoroutine(FadeFogObjects());
                break;
        }
        
        if (debugMode) Debug.Log($"Revealed fog of war using {revealType} effect.");
    }
    
    private void DestroyFogObjects()
    {
        foreach (GameObject fogObject in fogObjects)
        {
            if (fogObject != null)
            {
                Destroy(fogObject);
            }
        }
    }
    
    private void DisableFogObjects()
    {
        foreach (GameObject fogObject in fogObjects)
        {
            if (fogObject != null)
            {
                fogObject.SetActive(false);
            }
        }
    }
    
    private IEnumerator FadeFogObjects()
    {
        // Create an array to track all renderers that need fading
        SpriteRenderer[] renderers = new SpriteRenderer[fogObjects.Length];
        Color[] originalColors = new Color[fogObjects.Length];
        
        // Get all renderers and store original colors
        for (int i = 0; i < fogObjects.Length; i++)
        {
            if (fogObjects[i] != null)
            {
                renderers[i] = fogObjects[i].GetComponent<SpriteRenderer>();
                if (renderers[i] != null)
                {
                    originalColors[i] = renderers[i].color;
                }
            }
        }
        
        float alpha = 1.0f;
        
        // Continue fading until fully transparent
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            
            // Apply alpha to all renderers
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    Color newColor = originalColors[i];
                    newColor.a = Mathf.Max(0, alpha);
                    renderers[i].color = newColor;
                }
            }
            
            yield return null;
        }
        
        // After fade, disable objects
        DisableFogObjects();
    }
}