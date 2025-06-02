using UnityEngine;

[System.Serializable]
public class GroundDetection
{
    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Head Check Settings")]
    [SerializeField] private float headCheckDistance = 0.6f;
    [SerializeField] private Vector2 headCheckOffset = new Vector2(0f, 0.5f);
    
    // Add surface detection
    [Header("Surface Detection")]
    [SerializeField] private SurfaceType defaultSurface = SurfaceType.Default;
    
    private bool isGrounded;
    private bool wasGrounded;
    private bool hasHeadCollision;
    private SurfaceType currentSurface;
    private Collider2D groundCollider;  // Store the collider we're standing on
    
    public bool IsGrounded => isGrounded;
    public bool WasGrounded => wasGrounded;
    public SurfaceType CurrentSurface => currentSurface;
    public Collider2D GroundCollider => groundCollider;

    private SurfaceType previousSurface;
private bool surfaceJustChanged = false;

// Add this property
public bool SurfaceJustChanged => surfaceJustChanged;
    
    public void Initialize(Transform groundCheckTransform)
    {
        groundCheck = groundCheckTransform;
        currentSurface = defaultSurface;
    }
    
public void UpdateGroundState()
{
    wasGrounded = isGrounded;
    
    // Store previous surface before updating
    previousSurface = currentSurface;
    
    // Check ground collision and get the collider
    Collider2D hitCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    bool isNowGrounded = hitCollider != null;
    
    // If we just landed, immediately update the surface
    bool justLanded = !wasGrounded && isNowGrounded;
    
    // Store the ground collider for surface detection
    groundCollider = hitCollider;
    
    // Update the current surface when necessary
    if (isNowGrounded)
    {
        // Always detect surface when landing to ensure fresh sound
        DetectSurface();
        
        // Check if surface changed while remaining grounded
        surfaceJustChanged = (previousSurface != currentSurface);
        
        // Debug landing and surface changes
        if (justLanded)
        {
            Debug.Log($"Just landed on {currentSurface}");
        }
        else if (surfaceJustChanged)
        {
            Debug.Log($"Surface changed from {previousSurface} to {currentSurface}");
        }
    }
    else
    {
        surfaceJustChanged = false;
    }
    
    // Set isGrounded after detection to ensure proper sequencing
    isGrounded = isNowGrounded;
}
    
    private void DetectSurface()
    {
        // Default to the default surface type
        currentSurface = defaultSurface;
        
        if (groundCollider != null)
        {
            // Method 1: Try to get surface from collision tag
            string surfaceTag = groundCollider.tag;
            if (System.Enum.TryParse(surfaceTag, true, out SurfaceType detectedSurface))
            {
                currentSurface = detectedSurface;
                return;
            }
            
            // Method 2: Check for a SurfaceIdentifier component
            SurfaceIdentifier identifier = groundCollider.GetComponent<SurfaceIdentifier>();
            if (identifier != null)
            {
                currentSurface = identifier.SurfaceType;
                return;
            }
            
            // Method 3: Try to determine from physics material name
            PhysicsMaterial2D physicsMaterial = groundCollider.sharedMaterial;
            if (physicsMaterial != null)
            {
                string materialName = physicsMaterial.name.ToLower();
                if (materialName.Contains("grass")) currentSurface = SurfaceType.Grass;
                else if (materialName.Contains("wood")) currentSurface = SurfaceType.Wood;
                else if (materialName.Contains("stone")) currentSurface = SurfaceType.Stone;
                else if (materialName.Contains("water")) currentSurface = SurfaceType.Water;
                else if (materialName.Contains("snow")) currentSurface = SurfaceType.Snow;
            }
        }
    }
    
    public bool JustLanded()
    {
        return isGrounded && !wasGrounded;
    }
    
    public bool HasHeadCollision()
    {
        return hasHeadCollision;
    }
    
    public void DrawGizmos()
    {
        if (groundCheck != null)
        {
            // Draw ground check circle
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            
            // Draw head check ray
            Vector2 headCheckPosition = (Vector2)groundCheck.position + headCheckOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(headCheckPosition, headCheckPosition + (Vector2.up * headCheckDistance));
            
            // Draw surface type text in editor
            #if UNITY_EDITOR
            if (Application.isPlaying && isGrounded)
            {
                UnityEditor.Handles.BeginGUI();
                Vector3 textPos = groundCheck.position;
                textPos.y += 0.3f;
                
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 12;
                
                UnityEditor.Handles.Label(textPos, $"Surface: {currentSurface}", style);
                UnityEditor.Handles.EndGUI();
            }
            #endif
        }
    }
}