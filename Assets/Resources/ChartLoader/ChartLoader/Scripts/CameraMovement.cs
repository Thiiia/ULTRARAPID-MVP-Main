using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private float _speed;
    private double songStartTime; // DSP time when the song starts
    public double SongStartTime => songStartTime;

    private float songLength;    // Length of the song
    private float initialZPosition; // Initial Z position of the camera


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
        initialZPosition = transform.position.z; // Save the initial Z position
        Debug.Log($"CameraMovement: Initialized with start time: {songStartTime}, song length: {songLength}, initial Z position: {initialZPosition}");
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

        // Calculate the camera's Z position based on song time and initial offset
        float calculatedZPosition = (float)(currentSongTime * Speed) + initialZPosition;

        // Update camera position
        transform.position = new Vector3(transform.position.x, transform.position.y, calculatedZPosition);

        Debug.Log($"Calculated Camera Z Position: {calculatedZPosition}");
        Debug.Log($"Final Applied Camera Position: {transform.position.z}, Song Time: {currentSongTime}");
    }
}
