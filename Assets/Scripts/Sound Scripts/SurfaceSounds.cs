using UnityEngine;

[System.Serializable]
public class SurfaceSounds
{
    public SurfaceType surfaceType;
    public AudioClip[] footstepSounds;
    public AudioClip[] jumpSounds;
    public AudioClip[] landSounds;
    public AudioClip[] crouchSounds;
    
    // Optional: Surface-specific settings
    public float footstepVolume = 1.0f;
    public float pitchVariation = 0.1f;
}