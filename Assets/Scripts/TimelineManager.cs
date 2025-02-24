using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    public AudioSource musicSource;         //AudioSource
      // Example event: the flash sequence
     // public FlashSequence flashSequence;
    // Add references to other event scripts here 

    private double dspStartTime;            // When the music starts, recorded from the DSP clock

    // Flags to ensure each event is only triggered once
    // private bool flashTriggered = false;
    // Add additional flags for other events as needed

    void Start() 
    {
        // Record the DSP start time and schedule the music to start immediately.
        dspStartTime = AudioSettings.dspTime;
       
    }

    void Update()
    {
        // Calculate the current time since the music started using DSP time.
        double currentTime = AudioSettings.dspTime - dspStartTime;
        
        // Example: Trigger the flash sequence at exactly 29 seconds.
       /* if (!flashTriggered && currentTime >= 29.0)
        {
            flashSequence.StartFlashSequence();
            flashTriggered = true;
        }
        */

        // other timeline events here.
        
    }
}
