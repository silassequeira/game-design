using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerVisualEffects
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem doubleJumpParticles; 
    [SerializeField] private ParticleSystem landParticles;
    [SerializeField] private ParticleSystem speedParticles;
    [SerializeField] private ParticleSystem specialEffectsParticles;

    
[Header("Shadow")]
[SerializeField] private GameObject shadowObject;
[SerializeField] private LayerMask groundLayer;
[SerializeField] private float shadowBaseScale = 1f;      // Base size multiplier
[SerializeField] private float shadowMinScale = 0.5f;     // Minimum scale when far from ground
[SerializeField] private float shadowScaleRate = 10f;     // How quickly shadow shrinks with height
[SerializeField] private float shadowFadeRate = 20f;      // How quickly shadow fades with height
    
    [Header("Effect Settings")]
    [SerializeField] private float minFallSpeedForLandParticles = 2.0f;
    
    private MonoBehaviour coroutineRunner;
    
    private bool isPlayingLandParticles = false;
    private bool isPlayingJumpParticles = false;
    private const float particleConflictPreventionTime = 0.1f;
    
    public void Initialize(MonoBehaviour runner)
    {
        coroutineRunner = runner;

        // Configure jump particles
        if (jumpParticles != null)
        {
            var main = jumpParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }

        // Configure double jump particles
        if (doubleJumpParticles != null)
        {
            var main = doubleJumpParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }
        
        // Configure land particles
        if (landParticles != null)
        {
            var main = landParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }
        
        // Configure speed particles
        if (speedParticles != null)
        {
            var main = speedParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }
        
        // Configure special effects particles
        if (specialEffectsParticles != null)
        {
            var main = specialEffectsParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }
    }

    // Ensure particles follow the player properly
    public void EnsureParticlesFollowPlayer(Transform playerTransform)
    {
        ConfigureParticleSystem(jumpParticles, playerTransform);
        ConfigureParticleSystem(doubleJumpParticles, playerTransform);
        ConfigureParticleSystem(landParticles, playerTransform, new Vector3(0f, -0.5f, 0f));
        ConfigureParticleSystem(speedParticles, playerTransform, new Vector3(0f, -0.3f, 0f));
        ConfigureParticleSystem(specialEffectsParticles, playerTransform);
    }
    
    public void UpdateParticleDirection(bool isFacingRight)
    {
        // Update jump particles direction
        UpdateParticleSystemDirection(jumpParticles, isFacingRight);
        
        // Update double jump particles direction
        UpdateParticleSystemDirection(doubleJumpParticles, isFacingRight);
        
        // Update land particles direction
        UpdateParticleSystemDirection(landParticles, isFacingRight);
        
        // Add this line to update speed/running particles direction
        UpdateParticleSystemDirection(speedParticles, isFacingRight);
    }

    private void UpdateParticleSystemDirection(ParticleSystem particleSystem, bool isFacingRight)
    {
        if (particleSystem == null) return;
        
        // Get the shape component which controls emission direction
        var shape = particleSystem.shape;
        
        // For cone emitters (Edge type doesn't exist in Unity's enum)
        if (shape.shapeType == ParticleSystemShapeType.Cone)
        {
            // Rotate emission direction based on facing
            float rotationAngle = isFacingRight ? 0f : 180f;
            shape.rotation = new Vector3(0, rotationAngle, 0);
        }
        
        // For velocity-based particles, we modify the initial velocity
        var velocity = particleSystem.velocityOverLifetime;
        if (velocity.enabled)
        {
            // Flip X velocity multiplier based on direction
            float xMultiplier = isFacingRight ? 1f : -1f;
            
            // Check if using constant or curve
            if (velocity.x.mode == ParticleSystemCurveMode.Constant)
            {
                velocity.x = velocity.x.constant * xMultiplier;
            }
        }
        
        // For particles using a local velocity (relative to transform)
        var main = particleSystem.main;
        if (main.simulationSpace == ParticleSystemSimulationSpace.Local)
        {
            // Simply flip the particle transform's scale
            Vector3 scale = particleSystem.transform.localScale;
            scale.x = isFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            particleSystem.transform.localScale = scale;
        }
    }
    
    private void ConfigureParticleSystem(ParticleSystem ps, Transform parentTransform, Vector3 localOffset = default)
    {
        if (ps == null || parentTransform == null) return;

        // Ensure proper parenting
        if (ps.transform.parent != parentTransform)
        {
            ps.transform.SetParent(parentTransform);
        }

        // Set correct local position
        ps.transform.localPosition = localOffset;

        // Ensure simulation space is local
        var main = ps.main;
        if (main.simulationSpace != ParticleSystemSimulationSpace.Local)
        {
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }
    }

    public void SyncParticlesToPlayer(Transform playerTransform)
    {
        // For world-space particles, this helps ensure they stay with the player
        // when first emitted
        if (jumpParticles != null && jumpParticles.isEmitting)
        {
            jumpParticles.transform.position = playerTransform.position;
        }
        
        if (doubleJumpParticles != null && doubleJumpParticles.isEmitting)
        {
            doubleJumpParticles.transform.position = playerTransform.position;
        }
        
        if (landParticles != null && landParticles.isEmitting)
        {
            Vector3 landPos = playerTransform.position;
            landPos.y -= 0.5f; // Offset downward
            landParticles.transform.position = landPos;
        }
        
        // Speed particles should continuously follow
        if (speedParticles != null && speedParticles.isPlaying)
        {
            Vector3 speedPos = playerTransform.position;
            speedPos.y -= 0.3f; // Offset downward
            speedParticles.transform.position = speedPos;
        }
        
        if (specialEffectsParticles != null && specialEffectsParticles.isEmitting)
        {
            specialEffectsParticles.transform.position = playerTransform.position;
        }
    }
    
    public void PlayJumpParticles(bool isDoubleJump = false, bool facingRight = true)
    {
        // Skip land particles that might be playing
        if (isPlayingLandParticles && landParticles != null)
        {
            landParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            isPlayingLandParticles = false;
        }

        ParticleSystem particles = isDoubleJump ? doubleJumpParticles : jumpParticles;
        if (particles != null)
        {
            // Update direction before playing
            UpdateParticleSystemDirection(particles, facingRight);
            
            // Make sure the particle system is properly positioned before playing
            if (particles.transform.parent != null)
            {
                particles.transform.localPosition = Vector3.zero;
            }
            particles.Play();
            isPlayingJumpParticles = true;
            
            // Reset flag after a short delay
            if (coroutineRunner != null)
            {
                coroutineRunner.StartCoroutine(ResetJumpParticleFlag(particleConflictPreventionTime));
            }
        }
    }

    public void PlayLandParticles(float fallVelocity = 0f)
    {
        // Don't play land particles if velocity is too small
        if (fallVelocity > 0 && fallVelocity < minFallSpeedForLandParticles)
        {
            return;
        }

        // Don't play land particles if we're in the process of jumping
        if (isPlayingJumpParticles)
        {
            //Debug.Log("Land particles skipped - jump particles already playing");
            return;
        }
        
        if (landParticles != null)
        {
            // Ensure position is at the bottom of the player
            if (landParticles.transform.parent != null)
            {
                landParticles.transform.localPosition = new Vector3(0, -0.5f, 0);
            }
            
            // Scale emission based on fall velocity for more dramatic landings
            if (fallVelocity > 0)
            {
                var emission = landParticles.emission;
                var burst = emission.GetBurst(0);
                burst.count = Mathf.Lerp(5, 20, Mathf.Min(fallVelocity / 15f, 1f));
                emission.SetBurst(0, burst);
            }
            
            // Stop any previous emission and play fresh
            landParticles.Stop(true);
            landParticles.Play();
            isPlayingLandParticles = true;
            
            // Reset flag after a short delay
            if (coroutineRunner != null)
            {
                coroutineRunner.StartCoroutine(ResetLandParticleFlag(particleConflictPreventionTime));
            }
        }
    }
    
    public void StartSpeedParticles(bool isFacingRight = true)
    {
        if (speedParticles != null && !speedParticles.isPlaying)
        {
            // Reset emission rate to default
            var emission = speedParticles.emission;
            var originalRate = 15f; // Default rate, adjust if needed
            emission.rateOverTime = originalRate;
            
            // Update direction before playing
            UpdateParticleSystemDirection(speedParticles, isFacingRight);
            
            speedParticles.Play();
        }
    }
    
    public void StopSpeedParticles()
    {
        if (speedParticles != null && speedParticles.isPlaying)
        {
            speedParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
    
    public void EnhanceSpeedParticles()
    {
        if (speedParticles != null)
        {
            var emission = speedParticles.emission;
            emission.rateOverTime = emission.rateOverTime.constant * 2f;
        }
    }
    
    public void UpdateRunningParticles(bool isRunning, bool isFacingRight)
    {
        if (speedParticles == null) return;
        
        if (isRunning)
        {
            // If not already playing, start it
            if (!speedParticles.isPlaying)
            {
                StartSpeedParticles(isFacingRight);
            }
            else
            {
                // Just update the direction for already playing particles
                UpdateParticleSystemDirection(speedParticles, isFacingRight);
            }
        }
        else if (speedParticles.isPlaying)
        {
            StopSpeedParticles();
        }
    }
    
 public void UpdateShadow(Transform playerTransform)
{
    if (shadowObject == null) return;

    RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, Vector2.down, 50f, groundLayer);
    if (hit.collider != null)
    {
        // Position shadow slightly above ground
        shadowObject.transform.position = new Vector3(
            playerTransform.position.x,
            hit.point.y + 0.05f,
            shadowObject.transform.position.z
        );

        // Calculate distance from player to ground
        float distance = Mathf.Abs(playerTransform.position.y - hit.point.y);
        
        // Scale shadow based on distance (shrinks as height increases)
        float scale = Mathf.Max(shadowMinScale, shadowBaseScale * (1f - (distance / shadowScaleRate)));
        shadowObject.transform.localScale = new Vector3(scale, scale * 0.5f, 1f); // Make shadow oval-shaped
        
        // Fade shadow based on distance
        SpriteRenderer shadowRenderer = shadowObject.GetComponent<SpriteRenderer>();
        if (shadowRenderer != null)
        {
            Color color = shadowRenderer.color;
            color.a = Mathf.Max(0.1f, 0.5f - (distance / shadowFadeRate));
            shadowRenderer.color = color;
        }
    }
}
    
    // Try to play a named special effect
    public bool TryPlaySpecialEffect(string effectName)
    {
        if (specialEffectsParticles != null)
        {
            if (effectName == "DoubleJumpAcquired")
            {
                specialEffectsParticles.Play();
                return true;
            }
        }
        
        // Fallback: return false if we couldn't play the effect
        return false;
    }

    private IEnumerator ResetJumpParticleFlag(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPlayingJumpParticles = false;
    }

    private IEnumerator ResetLandParticleFlag(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPlayingLandParticles = false;
    }
    
    // Access to particle systems for external effects
    public ParticleSystem GetJumpParticles() => jumpParticles;
    public ParticleSystem GetDoubleJumpParticles() => doubleJumpParticles;
}