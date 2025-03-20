    using UnityEngine;
    using TMPro;

    public class SongClockScript : MonoBehaviour
    {
        public TextMeshProUGUI songTimerText; 
        private AudioManager audioManager;

        void Start()
        {
            // Get reference to the AudioManager instance
            audioManager = AudioManager.Instance;
        

            if (audioManager == null)
            {
                Debug.LogError("SongClockScript: AudioManager instance not found!");
                enabled = false; // Disable script if AudioManager isn't found
            }
        }

        void Update()
        {
            if (audioManager != null && audioManager.musicSource.isPlaying)
            {
                UpdateSongTimer();
            }
        }

        // Updates the text 
        private void UpdateSongTimer()
        {
            float currentTime = (float)audioManager.GetCurrentSongTime();
            float totalTime = audioManager.songLength;

            string formattedTime = FormatTime(currentTime);
            string formattedTotal = FormatTime(totalTime);

            if (songTimerText != null)
            {
                songTimerText.text = $"{formattedTime}  {formattedTotal}";
                ScoreManagerScript.Instance.SetTotalMissionTime(formattedTime);
            }
        }

        // Converts time in seconds
        private string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            int milliseconds = Mathf.FloorToInt((timeInSeconds * 100) % 100);

            return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
        }
    }
