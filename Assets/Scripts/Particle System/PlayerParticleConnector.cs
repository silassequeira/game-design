using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerParticleConnector : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem jumpParticleSystem;
    [SerializeField] private ParticleSystem doubleJumpParticleSystem;
    [SerializeField] private ParticleSystem landParticleSystem;
    [SerializeField] private ParticleSystem speedParticleSystem;
    [SerializeField] private ParticleSystem specialEffectParticleSystem;
    
    [Header("Options")]
    [SerializeField] private bool createMissingSystems = true;
    
    private PlayerMovement playerMovement;
    private PlayerVisualEffects visualEffects;
    
    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component not found!");
            return;
        }
        
        visualEffects = playerMovement.GetVisualEffects();
        
        if (visualEffects == null)
        {
            Debug.LogError("PlayerVisualEffects not found in PlayerMovement!");
            return;
        }
        
        // Create missing particle systems if enabled
        if (createMissingSystems)
        {
            CreateMissingParticleSystems();
        }
        
        // Assign particle systems to visual effects
        AssignParticleSystems();
    }
    
    private void CreateMissingParticleSystems()
    {
        ParticleSystemCreator creator = gameObject.AddComponent<ParticleSystemCreator>();
        creator.playerTransform = transform;
        creator.attachToPlayer = true;
        
        if (jumpParticleSystem == null)
        {
            jumpParticleSystem = creator.CreateJumpParticleSystem();
            jumpParticleSystem.transform.SetParent(transform);
            jumpParticleSystem.transform.localPosition = Vector3.zero;
        }
        
        if (doubleJumpParticleSystem == null)
        {
            doubleJumpParticleSystem = creator.CreateDoubleJumpParticleSystem();
            doubleJumpParticleSystem.transform.SetParent(transform);
            doubleJumpParticleSystem.transform.localPosition = Vector3.zero;
        }
        
        if (landParticleSystem == null)
        {
            landParticleSystem = creator.CreateLandParticleSystem();
            landParticleSystem.transform.SetParent(transform);
            landParticleSystem.transform.localPosition = new Vector3(0, -0.5f, 0);
        }
        
        if (speedParticleSystem == null)
        {
            speedParticleSystem = creator.CreateSpeedParticleSystem();
            speedParticleSystem.transform.SetParent(transform);
            speedParticleSystem.transform.localPosition = new Vector3(0, -0.3f, 0);
        }
        
        if (specialEffectParticleSystem == null)
        {
            specialEffectParticleSystem = creator.CreateSpecialEffectParticleSystem();
            specialEffectParticleSystem.transform.SetParent(transform);
            specialEffectParticleSystem.transform.localPosition = Vector3.zero;
        }
        
        // Remove the temporary creator component
        Destroy(creator);
    }
    
    private void AssignParticleSystems()
    {
        // We need to use reflection to set the private fields in PlayerVisualEffects
        // This is a workaround since the fields are private serialized fields
        
        // For jumpParticles field
        if (jumpParticleSystem != null)
        {
            SetPrivateField(visualEffects, "jumpParticles", jumpParticleSystem);
        }
        
        // For doubleJumpParticles field
        if (doubleJumpParticleSystem != null)
        {
            SetPrivateField(visualEffects, "doubleJumpParticles", doubleJumpParticleSystem);
        }
        
        // For landParticles field
        if (landParticleSystem != null)
        {
            SetPrivateField(visualEffects, "landParticles", landParticleSystem);
        }
        
        // For speedParticles field
        if (speedParticleSystem != null)
        {
            SetPrivateField(visualEffects, "speedParticles", speedParticleSystem);
        }
        
        // For specialEffectsParticles field
        if (specialEffectParticleSystem != null)
        {
            SetPrivateField(visualEffects, "specialEffectsParticles", specialEffectParticleSystem);
        }
    }
    
    private void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.NonPublic);
        
        if (field != null)
        {
            field.SetValue(instance, value);
        }
        else
        {
            Debug.LogError($"Field '{fieldName}' not found in {instance.GetType().Name}");
        }
    }
}