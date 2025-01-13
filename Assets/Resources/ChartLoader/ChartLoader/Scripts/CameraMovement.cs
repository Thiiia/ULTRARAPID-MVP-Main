using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private float _speed;
    private double songStartTime; // DSP time when the song starts
    private float songLength;    // Length of the song

    /// <summary>
    /// The Camera Speed.
    /// </summary>
    public float Speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    /// <summary>
    /// Initialize DSP time and song length.
    /// </summary>
    public void Initialize(double startTime, float length)
    {
        songStartTime = startTime;
        songLength = length; // Assign song length
        Debug.Log($"CameraMovement: Initialized with start time: {songStartTime}, song length: {songLength}");
    }

    // Update camera position based on DSP time
    void FixedUpdate()
    {
        if (songStartTime <= 0)
        {
            Debug.LogWarning("CameraMovement: Song start time is not initialized.");
            return;
        }

        // Calculate elapsed song time
        double currentSongTime = AudioSettings.dspTime - songStartTime;

        // Ensure we don't exceed the song length
        if (currentSongTime > songLength)
        {
            currentSongTime = songLength; // Clamp to the maximum duration
        }

        // Calculate the camera's Z position
        float cameraZPosition = Mathf.Max(0, (float)(currentSongTime * Speed));

        // Update camera position
        transform.position = new Vector3(transform.position.x, transform.position.y, cameraZPosition);

        Debug.Log($"Camera Position: {transform.position.z}, Song Time: {currentSongTime}");
    }
}
