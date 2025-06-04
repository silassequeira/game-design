using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlayerAudioSystem
{
    [Header("Surface-Based Audio")]
    [SerializeField] private SurfaceSounds[] surfaceSounds;
    [SerializeField] private SurfaceType defaultSurface = SurfaceType.Default;
    
    [Header("Generic Audio Clips")]
    [SerializeField] private AudioClip doubleJumpSound;
    [SerializeField] private AudioClip speedBoostSound;
    
    [Header("Footstep Settings")]
    [SerializeField] private float footstepInterval = 0.3f;
    [SerializeField] private float minPitchVariation = 0.9f;
    [SerializeField] private float maxPitchVariation = 1.1f;
    [SerializeField] private float minVolumeThreshold = 0.5f;
    [SerializeField] private bool speedAffectsFootsteps = true;
    
    [Header("Volume Settings")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1.0f;
    [SerializeField, Range(0f, 1f)] private float effectsVolume = 1.0f;
    [SerializeField, Range(0f, 1f)] private float footstepsVolume = 0.7f;

    [Header("Sound Overlap Prevention")]
    [SerializeField] private bool preventSoundOverlap = true;
    [SerializeField] private float soundCooldown = 0.2f;
    
    private AudioSource audioSource;
    private float footstepTimer = 0f;
    private int lastFootstepIndex = -1;
    private MonoBehaviour coroutineRunner;
    private Dictionary<SoundType, float> soundTimers = new Dictionary<SoundType, float>();
    
    // Current surface the player is on
    private SurfaceType currentSurface = SurfaceType.Default;

    private float lastCustomSoundTime = 0f;
    private const float CUSTOM_SOUND_COOLDOWN = 0.2f;
    
    // Sound type enum for cooldown tracking
    public enum SoundType
    {
        Jump,
        Land,
        Footstep,
        Crouch,
        DoubleJump,
        SpeedBoost
    }
    
    public void Initialize(AudioSource source, MonoBehaviour runner = null)
    {
        audioSource = source;
        coroutineRunner = runner;
        
        // Clear and initialize all sound timers
        soundTimers.Clear();
        foreach (SoundType type in System.Enum.GetValues(typeof(SoundType)))
        {
            soundTimers[type] = 0f;
        }
        
        // Configure audio source
        if (audioSource != null)
        {
            audioSource.volume = masterVolume;
            audioSource.spatialBlend = 0f; // 2D sound
        }
    }
    
    public void Update()
    {
        // Create a temporary list of keys to avoid modification issues
        List<SoundType> keys = new List<SoundType>(soundTimers.Keys);
        
        // Update all sound timers using the temporary list
        foreach (SoundType type in keys)
        {
            if (soundTimers[type] > 0)
                soundTimers[type] -= Time.deltaTime;
        }
    }
    
    // Get the appropriate sounds for current surface
    private SurfaceSounds GetCurrentSurfaceSounds()
    {
        foreach (var sounds in surfaceSounds)
        {
            if (sounds.surfaceType == currentSurface)
                return sounds;
        }
        
        // Return the first surface as fallback (should be default)
        return surfaceSounds.Length > 0 ? surfaceSounds[0] : null;
    }
    
    // Update current surface from GroundDetection
    public void UpdateCurrentSurface(GroundDetection groundDetection)
    {
        if (groundDetection != null)
        {
            currentSurface = groundDetection.CurrentSurface;
        
        }
    }
    
    // Play jump sound based on current surface
    public void PlayJumpSound(bool isDoubleJump = false)
    {
        if (isDoubleJump)
        {
            PlaySound(doubleJumpSound, SoundType.DoubleJump);
            return;
        }
        
        SurfaceSounds sounds = GetCurrentSurfaceSounds();
        if (sounds == null || sounds.jumpSounds == null || sounds.jumpSounds.Length == 0)
            return;
            
        // Get random jump sound for current surface
        AudioClip clip = GetRandomClip(sounds.jumpSounds);
        PlaySound(clip, SoundType.Jump, sounds.footstepVolume, sounds.pitchVariation);
    }
    
    // Play land sound based on current surface
public void PlayLandSound()
{
    // Re-get the current surface sounds to ensure freshness
    SurfaceSounds sounds = GetCurrentSurfaceSounds();
    //Debug.Log($"Playing landing sound for surface: {currentSurface}");
    
    if (sounds == null || sounds.landSounds == null || sounds.landSounds.Length == 0)
    {
        //Debug.LogWarning($"No landing sounds available for surface: {currentSurface}");
        return;
    }
        
    // Get random land sound for current surface
    AudioClip clip = GetRandomClip(sounds.landSounds);
    PlaySound(clip, SoundType.Land, sounds.footstepVolume, sounds.pitchVariation);
}
    
    // Play crouch sound based on current surface
    public void PlayCrouchSound()
    {
        SurfaceSounds sounds = GetCurrentSurfaceSounds();
        if (sounds == null || sounds.crouchSounds == null || sounds.crouchSounds.Length == 0)
            return;
            
        // Get random crouch sound for current surface
        AudioClip clip = GetRandomClip(sounds.crouchSounds);
        PlaySound(clip, SoundType.Crouch, sounds.footstepVolume, sounds.pitchVariation);
    }
    
    public void PlaySpeedBoostSound()
    {
        PlaySound(speedBoostSound, SoundType.SpeedBoost, 1.0f, 0.1f);
    }
    
    // Updated footstep system with GroundDetection integration
    public void UpdateFootsteps(GroundDetection groundDetection, float velocity, float maxSpeed)
    {
        if (groundDetection == null)
            return;
            
        // Update current surface from ground detection
        UpdateCurrentSurface(groundDetection);
        
        if (groundDetection.IsGrounded && Mathf.Abs(velocity) > minVolumeThreshold)
        {
            // Calculate interval based on speed if enabled
            float currentInterval = footstepInterval;
            if (speedAffectsFootsteps)
            {
                // Faster movement = faster footsteps
                float speedFactor = Mathf.InverseLerp(minVolumeThreshold, maxSpeed, Mathf.Abs(velocity));
                currentInterval = Mathf.Lerp(footstepInterval, footstepInterval * 0.6f, speedFactor);
            }
            
            footstepTimer += Time.deltaTime;
            
            if (footstepTimer >= currentInterval)
            {
                PlayFootstepSound(velocity, maxSpeed);
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }
    
    // Keeping the old method for backward compatibility
    public void UpdateFootsteps(bool isGrounded, float velocity, float maxSpeed, Vector2 playerPosition)
    {
        
        if (isGrounded && Mathf.Abs(velocity) > minVolumeThreshold)
        {
            // Calculate interval based on speed if enabled
            float currentInterval = footstepInterval;
            if (speedAffectsFootsteps)
            {
                // Faster movement = faster footsteps
                float speedFactor = Mathf.InverseLerp(minVolumeThreshold, maxSpeed, Mathf.Abs(velocity));
                currentInterval = Mathf.Lerp(footstepInterval, footstepInterval * 0.6f, speedFactor);
            }
            
            footstepTimer += Time.deltaTime;
            
            if (footstepTimer >= currentInterval)
            {
                PlayFootstepSound(velocity, maxSpeed);
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }
    
    private void PlayFootstepSound(float velocity, float maxSpeed)
    {
        SurfaceSounds sounds = GetCurrentSurfaceSounds();
        if (sounds == null || sounds.footstepSounds == null || sounds.footstepSounds.Length == 0)
            return;
        
        // Skip if we're in cooldown period
        if (preventSoundOverlap && soundTimers[SoundType.Footstep] > 0)
            return;
            
        // Select random sound avoiding repeats
        AudioClip clip = GetRandomClip(sounds.footstepSounds, lastFootstepIndex);
        lastFootstepIndex = System.Array.IndexOf(sounds.footstepSounds, clip);
        
        // Calculate volume based on speed
        float speedRatio = Mathf.InverseLerp(minVolumeThreshold, maxSpeed, Mathf.Abs(velocity));
        float volumeMultiplier = Mathf.Lerp(0.2f, 1.0f, speedRatio);
        float finalVolume = masterVolume * footstepsVolume * sounds.footstepVolume * volumeMultiplier;
        
        // Play with pitch variation
        float originalPitch = audioSource.pitch;
        audioSource.pitch = Random.Range(minPitchVariation, maxPitchVariation);
        
        audioSource.PlayOneShot(clip, finalVolume);
        soundTimers[SoundType.Footstep] = clip.length * 0.7f; // Set cooldown to portion of clip length
        
        audioSource.pitch = originalPitch;
    }
    
    // Helper to get a random clip, optionally avoiding a specific index
    private AudioClip GetRandomClip(AudioClip[] clips, int avoidIndex = -1)
    {
        if (clips == null || clips.Length == 0)
            return null;
            
        if (clips.Length == 1)
            return clips[0];
            
        int index = Random.Range(0, clips.Length);
        if (index == avoidIndex && clips.Length > 1)
        {
            index = (index + 1) % clips.Length;
        }
        
        return clips[index];
    }
    
    private void PlaySound(AudioClip clip, SoundType type, float volumeScale = 1.0f, float pitchVariation = 0.1f)
    {
        if (clip == null || audioSource == null)
            return;
            
        // Skip if we're in cooldown period and prevention is enabled
        if (preventSoundOverlap && soundTimers[type] > 0)
            return;
            
        float originalPitch = audioSource.pitch;
        if (pitchVariation > 0)
        {
            audioSource.pitch = Random.Range(1.0f - pitchVariation, 1.0f + pitchVariation);
        }
        
        float finalVolume = masterVolume * effectsVolume * volumeScale;
        audioSource.PlayOneShot(clip, finalVolume);
        
        // Set cooldown timer
        soundTimers[type] = preventSoundOverlap ? clip.length : soundCooldown;
        
        audioSource.pitch = originalPitch;
    }
    
    public void PlayCustomSound(AudioClip clip, float volumeScale = 1.0f, float pitchVariation = 0.1f)
    {
        if (clip == null || audioSource == null)
            return;
    
        // Simple cooldown system to prevent sound spam
        if (Time.time - lastCustomSoundTime < CUSTOM_SOUND_COOLDOWN)
            return;
            
        lastCustomSoundTime = Time.time;
        
        float originalPitch = audioSource.pitch;
        if (pitchVariation > 0)
        {
            audioSource.pitch = Random.Range(1.0f - pitchVariation, 1.0f + pitchVariation);
        }
        
        float finalVolume = masterVolume * effectsVolume * volumeScale;
        audioSource.PlayOneShot(clip, finalVolume);
        
        // Reset pitch to original value
        audioSource.pitch = originalPitch;
    }
 
    // Volume control methods
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
            audioSource.volume = masterVolume;
    }
    
    public void SetEffectsVolume(float volume)
    {
        effectsVolume = Mathf.Clamp01(volume);
    }
    
    public void SetFootstepsVolume(float volume)
    {
        footstepsVolume = Mathf.Clamp01(volume);
    }

    // Getters
    public AudioClip GetDoubleJumpSound() => doubleJumpSound;
    public AudioSource GetAudioSource() => audioSource;
    public SurfaceType GetCurrentSurface() => currentSurface;
    public bool IsSoundPlaying(SoundType type) => soundTimers.ContainsKey(type) && soundTimers[type] > 0;
    
    // Debug drawing method
    public void DrawGizmos(Transform ownerTransform)
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying) return;
        
        // Draw surface type info
        UnityEditor.Handles.BeginGUI();
        Vector3 textPos = ownerTransform.position;
        textPos.y += 0.5f;
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 12;
        
        UnityEditor.Handles.Label(textPos, $"Audio Surface: {currentSurface}", style);
        UnityEditor.Handles.EndGUI();
        #endif
    }
}