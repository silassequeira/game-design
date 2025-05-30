using UnityEngine;

public class MusicProximityController : MonoBehaviour
{
    public AudioSource ambientMusic;
    public Transform player;
    public Transform evaObject;
    public float maxDistance = 10f;
    public float minVolume = 0.2f;
    public float maxVolume = 1f;
    public float fadeSpeed = 1f;

    private float targetVolume;

    void Start()
    {
        if (ambientMusic == null)
        {
            Debug.LogError("AudioSource not assigned!");
            enabled = false;
            return;
        }

        ambientMusic.volume = minVolume;
        targetVolume = minVolume;
    }

    void Update()
    {
        if (player == null || evaObject == null) return;

        // Calculate distance between player and Eva
        float distance = Vector2.Distance(player.position, evaObject.position);

        // Calculate target volume based on distance (inverse relationship)
        if (distance <= maxDistance)
        {
            targetVolume = Mathf.Lerp(maxVolume, minVolume, distance / maxDistance);
        }
        else
        {
            targetVolume = minVolume;
        }

        // Smoothly adjust volume
        ambientMusic.volume = Mathf.Lerp(ambientMusic.volume, targetVolume, fadeSpeed * Time.deltaTime);
    }
}
