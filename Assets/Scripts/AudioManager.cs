using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;  // Assign AudioSource via the Inspector
    public double dspStartTime { get; private set; }
    public float songLength;

    void Awake()
    {
        // Singleton pattern: Only one instance persists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (musicSource != null && musicSource.clip != null)
        {
             
            songLength = musicSource.clip.length;
            dspStartTime = AudioSettings.dspTime;
            // Schedule the music to play immediately at the DSP time
            musicSource.PlayScheduled(dspStartTime);
            Debug.Log($"AudioManager: Music started at DSP time {dspStartTime}");
        }
        else
        {
            Debug.LogError("AudioManager: MusicSource or clip not assigned!");
        }
    }

    // Utility to get the current song time based on DSP time
    public double GetCurrentSongTime()
    {
        return AudioSettings.dspTime - dspStartTime;
    }
}
