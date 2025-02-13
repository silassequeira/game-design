public class ParallaxLayer : MonoBehaviour {
    [SerializeField] [Range(0,1)] float parallaxWeight;
    Transform cameraTransform;
    
    void Update() {
        transform.position = new Vector3(
            cameraTransform.position.x * parallaxWeight,
            cameraTransform.position.y * parallaxWeight,
            transform.position.z
        );
    }
}

// Use different Z positions and Sorting Layers:

// Foreground (z=5, Sorting Layer: FG)
// Gameplay (z=0, Sorting Layer: Midground)
// Background (z=-5, Sorting Layer: BG)
