using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    public Transform transform;
    [Range(0f, 1f)]
    public float parallaxFactor;
}

public class ParallaxSystem : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private ParallaxLayer[] parallaxLayers;
    
    private Vector3 previousCameraPosition;
    
    private void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        previousCameraPosition = cameraTransform.position;
    }
    
    private void LateUpdate()
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Playing)
            return;
            
        Vector3 deltaMovement = cameraTransform.position - previousCameraPosition;
        
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            if (layer.transform != null)
            {
                float parallaxX = deltaMovement.x * layer.parallaxFactor;
                float parallaxY = deltaMovement.y * layer.parallaxFactor;
                
                Vector3 newPosition = layer.transform.position;
                newPosition.x += parallaxX;
                newPosition.y += parallaxY;
                layer.transform.position = newPosition;
            }
        }
        
        previousCameraPosition = cameraTransform.position;
    }
}

// Use different Z positions and Sorting Layers:

// Foreground (z=5, Sorting Layer: FG)
// Gameplay (z=0, Sorting Layer: Midground)
// Background (z=-5, Sorting Layer: BG)
