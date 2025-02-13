using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public bool loop;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    
    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    public Sound[] music;
    public Sound[] sfx;
    
    public AudioMixerGroup musicMixer;
    public AudioMixerGroup sfxMixer;
    
    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeAudio()
    {
        // Initialize music
        foreach (Sound s in music)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            s.source = source;
            source.clip = s.clip;
            source.loop = s.loop;
            source.volume = s.volume;
            source.pitch = s.pitch;
            source.outputAudioMixerGroup = musicMixer;
            
            soundDictionary[s.name] = s;
        }
        
        // Initialize SFX
        foreach (Sound s in sfx)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            s.source = source;
            source.clip = s.clip;
            source.loop = s.loop;
            source.volume = s.volume;
            source.pitch = s.pitch;
            source.outputAudioMixerGroup = sfxMixer;
            
            soundDictionary[s.name] = s;
        }
    }
    
    public void PlayMusic(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound s))
        {
            s.source.Play();
        }
        else
        {
            Debug.LogWarning($"Sound {name} not found!");
        }
    }
    
    public void PlaySFX(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound s))
        {
            s.source.PlayOneShot(s.clip);
        }
        else
        {
            Debug.LogWarning($"Sound {name} not found!");
        }
    }
    
    public void StopMusic(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound s))
        {
            s.source.Stop();
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        musicMixer.audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxMixer.audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }
}