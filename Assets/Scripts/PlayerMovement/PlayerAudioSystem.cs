using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerAudioSystem
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip doubleJumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip speedBoostSound;
    [SerializeField] private AudioClip[] footstepSounds;
    
    [Header("Footstep Settings")]
    [SerializeField] private float footstepInterval = 0.3f;
    [SerializeField] private float minPitchVariation = 0.9f;
    [SerializeField] private float maxPitchVariation = 1.1f;
    [SerializeField] private float minVolumeThreshold = 0.5f;
    [SerializeField] private bool speedAffectsFootsteps = true;
    [SerializeField] private LayerMask footstepSurfaceLayers;
    
    [Header("Volume Settings")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1.0f;
    [SerializeField, Range(0f, 1f)] private float effectsVolume = 1.0f;
    [SerializeField, Range(0f, 1f)] private float footstepsVolume = 0.7f;
    
    [Header("Sound Overlap Prevention")]
    [SerializeField] private bool preventJumpSoundOverlap = true;
    [SerializeField] private bool preventLandSoundOverlap = true;
    [SerializeField] private float jumpSoundCooldown = 0.5f; // Minimum time between jump sounds
    [SerializeField] private float landSoundCooldown = 0.5f; // Minimum time between land sounds
    
    private AudioSource audioSource;
    private float footstepTimer = 0f;
    private int lastFootstepIndex = -1;
    private MonoBehaviour coroutineRunner;
    
    // Sound overlap prevention timers
    private float jumpSoundTimer = 0f;
    private float landSoundTimer = 0f;
    
    // For tracking one-shot sounds currently playing
    private AudioSource jumpAudioSource;
    private AudioSource landAudioSource;
    
    public void Initialize(AudioSource source, MonoBehaviour runner = null)
    {
        audioSource = source;
        coroutineRunner = runner;
        
        // Configure audio source
        if (audioSource != null)
        {
            audioSource.volume = masterVolume;
            audioSource.spatialBlend = 0f; // 2D sound
        }
    }
    
    public void Update()
    {
        // Update sound timers
        if (jumpSoundTimer > 0)
            jumpSoundTimer -= Time.deltaTime;
            
        if (landSoundTimer > 0)
            landSoundTimer -= Time.deltaTime;
    }
    
    public void PlayJumpSound(bool isDoubleJump = false)
    {
        AudioClip soundToPlay = isDoubleJump ? doubleJumpSound : jumpSound;
        
        // Skip if we're in cooldown period and prevention is enabled
        if (preventJumpSoundOverlap && jumpSoundTimer > 0)
            return;
            
        if (soundToPlay != null && audioSource != null)
        {
            // Calculate clip duration and set timer
            float clipDuration = soundToPlay.length;
            jumpSoundTimer = preventJumpSoundOverlap ? clipDuration : jumpSoundCooldown;
            
            // Play at appropriate volume
            float finalVolume = masterVolume * effectsVolume;
            audioSource.PlayOneShot(soundToPlay, finalVolume);
        }
    }
    
    public void PlayLandSound()
    {
        // Skip if we're in cooldown period and prevention is enabled
        if (preventLandSoundOverlap && landSoundTimer > 0)
            return;
            
        if (landSound != null && audioSource != null)
        {
            // Calculate clip duration and set timer
            float clipDuration = landSound.length;
            landSoundTimer = preventLandSoundOverlap ? clipDuration : landSoundCooldown;
            
            // Play at appropriate volume
            float finalVolume = masterVolume * effectsVolume;
            audioSource.PlayOneShot(landSound, finalVolume);
        }
    }
    
    public void PlaySpeedBoostSound()
    {
        if (speedBoostSound != null && audioSource != null)
        {
            // Play sound with variation
            PlaySoundWithVariation(speedBoostSound, effectsVolume, 0.0f);
        }
    }
    
    // New method to play any sound with common settings
    public void PlaySound(AudioClip clip, float volumeScale = 1.0f)
    {
        if (clip != null && audioSource != null)
        {
            float finalVolume = masterVolume * effectsVolume * volumeScale;
            audioSource.PlayOneShot(clip, finalVolume);
        }
    }
    
    // Play a sound with random pitch variation
    private void PlaySoundWithVariation(AudioClip clip, float volumeScale = 1.0f, float pitchVariation = 0.1f)
    {
        if (clip != null && audioSource != null)
        {
            float originalPitch = audioSource.pitch;
            audioSource.pitch = Random.Range(1.0f - pitchVariation, 1.0f + pitchVariation);
            
            float finalVolume = masterVolume * volumeScale;
            audioSource.PlayOneShot(clip, finalVolume);
            
            audioSource.pitch = originalPitch;
        }
    }
    
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
        if (audioSource == null || footstepSounds == null || footstepSounds.Length == 0)
            return;
        
        // Select random sound
        int index = Random.Range(0, footstepSounds.Length);
        if (footstepSounds.Length > 1 && index == lastFootstepIndex)
        {
            index = (index + 1) % footstepSounds.Length;
        }
        lastFootstepIndex = index;
        
        // Calculate volume based on speed
        float speedRatio = Mathf.InverseLerp(minVolumeThreshold, maxSpeed, Mathf.Abs(velocity));
        float volume = masterVolume * footstepsVolume * Mathf.Lerp(0.2f, 1.0f, speedRatio);
        
        audioSource.pitch = Random.Range(minPitchVariation, maxPitchVariation);
        audioSource.PlayOneShot(footstepSounds[index], volume);
        audioSource.pitch = 1.0f;
    }
    
    // Volume control methods
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }
    
    public void SetEffectsVolume(float volume)
    {
        effectsVolume = Mathf.Clamp01(volume);
    }
    
    // Sound overlap prevention methods
    public void SetPreventJumpSoundOverlap(bool prevent)
    {
        preventJumpSoundOverlap = prevent;
    }
    
    public void SetPreventLandSoundOverlap(bool prevent)
    {
        preventLandSoundOverlap = prevent;
    }
    
    // Getters
    public AudioSource GetAudioSource() => audioSource;
    public AudioClip GetDoubleJumpSound() => doubleJumpSound;
    public bool IsJumpSoundPlaying => jumpSoundTimer > 0;
    public bool IsLandSoundPlaying => landSoundTimer > 0;
}