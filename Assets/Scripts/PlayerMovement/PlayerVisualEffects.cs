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
    
    [Header("Effect Settings")]
    [SerializeField] private Color doubleJumpAcquiredColor = Color.cyan;
    [SerializeField] private float minFallSpeedForLandParticles = 2.0f;
    
    // Store original colors for resetting later
    private Color jumpParticlesOriginalColor;
    private Color doubleJumpParticlesOriginalColor;
    private MonoBehaviour coroutineRunner;
    
    private bool isPlayingLandParticles = false;
    private bool isPlayingJumpParticles = false;
    private const float particleConflictPreventionTime = 0.1f;
    
    public void Initialize(MonoBehaviour runner)
    {
        coroutineRunner = runner;

        // Store original colors
        if (jumpParticles != null)
        {
            var main = jumpParticles.main;
            jumpParticlesOriginalColor = main.startColor.color;
            
            // Set simulation space to Local
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }

        if (doubleJumpParticles != null)
        {
            var main = doubleJumpParticles.main;
            doubleJumpParticlesOriginalColor = main.startColor.color;
            
            // Set simulation space to Local
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
    
    public void PlayJumpParticles(bool isDoubleJump = false)
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
            Debug.Log("Land particles skipped - jump particles already playing");
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
    
    public void StartSpeedParticles()
    {
        if (speedParticles != null && !speedParticles.isPlaying)
        {
            // Reset emission rate to default
            var emission = speedParticles.emission;
            var originalRate = 15f; // Default rate, adjust if needed
            emission.rateOverTime = originalRate;
            
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
    
    public void UpdateShadow(Transform playerTransform)
    {
        if (shadowObject == null) return;
        
        RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, Vector2.down, 50f, groundLayer);
        if (hit.collider != null)
        {
            shadowObject.transform.position = new Vector3(
                playerTransform.position.x, 
                hit.point.y + 0.05f, 
                shadowObject.transform.position.z
            );
            
            float distance = Mathf.Abs(playerTransform.position.y - hit.point.y);
            float scale = Mathf.Max(0.5f, 1f - (distance / 10f));
            shadowObject.transform.localScale = new Vector3(scale, scale, 1f);
            
            SpriteRenderer shadowRenderer = shadowObject.GetComponent<SpriteRenderer>();
            if (shadowRenderer != null)
            {
                Color color = shadowRenderer.color;
                color.a = Mathf.Max(0.1f, 0.5f - (distance / 20f));
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
                // Configure special effect for double jump acquisition
                var main = specialEffectsParticles.main;
                main.startColor = doubleJumpAcquiredColor;
                
                specialEffectsParticles.Play();
                return true;
            }
        }
        
        // Fallback: return false if we couldn't play the effect
        return false;
    }
    
    // Change particle color temporarily
    public void SetParticleColor(Color color, float duration)
    {
        // Apply to jump particles if available
        if (jumpParticles != null)
        {
            var main = jumpParticles.main;
            main.startColor = color;
        }
        
        // Apply to double jump particles if available
        if (doubleJumpParticles != null)
        {
            var main = doubleJumpParticles.main;
            main.startColor = color;
        }
        
        // Reset after duration
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(ResetParticleColorsAfterDelay(duration));
        }
    }
    
    // Coroutine to reset particle colors
    private IEnumerator ResetParticleColorsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Reset jump particles color
        if (jumpParticles != null)
        {
            var main = jumpParticles.main;
            main.startColor = jumpParticlesOriginalColor;
        }
        
        // Reset double jump particles color
        if (doubleJumpParticles != null)
        {
            var main = doubleJumpParticles.main;
            main.startColor = doubleJumpParticlesOriginalColor;
        }
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