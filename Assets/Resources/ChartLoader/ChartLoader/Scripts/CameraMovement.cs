using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private float _speed;
    private double songStartTime; // DSP time when the song started (from AudioManager)
    public double SongStartTime => songStartTime;

    private float songLength;    // Duration of the song
    private float initialZPosition; // Starting Z position for the camera

    public float Speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    // Call this method to initialize the camera from the AudioManager values.
    public void InitializeFromAudioManager()
    {
        if (AudioManager.Instance != null)
        {
            songStartTime = AudioManager.Instance.dspStartTime;
            songLength = AudioManager.Instance.songLength;
            initialZPosition = transform.position.z;
            Debug.Log($"CameraMovement: Initialized with start time: {songStartTime}, song length: {songLength}, initial Z position: {initialZPosition}");
        }
        else
        {
            Debug.LogError("CameraMovement: AudioManager instance not found!");
        }
    }

    // Update camera position based on current DSP time
    void FixedUpdate()
    {
        if (songStartTime <= 0)
        {
            Debug.LogWarning("CameraMovement: Song start time is not initialized.");
            return;
        }

        // Calculate elapsed song time using DSP time
        double currentSongTime = AudioSettings.dspTime - songStartTime;

        // Clamp to song length if necessary
        if (currentSongTime > songLength)
        {
            currentSongTime = songLength;
        }

        // Calculate new Z position based on elapsed time, speed, and initial offset
        float calculatedZPosition = (float)(currentSongTime * Speed) + initialZPosition;
        transform.position = new Vector3(transform.position.x, transform.position.y, calculatedZPosition);
    }
}
