using UnityEngine;
using UnityEngine.SceneManagement; // For scene management
using TMPro; // For TMP_Dropdown
using System.IO; // For file handling
using UnityEngine.Networking; // For WebGL compatibility
using System.Collections; // For IEnumerator

public class DropdownSwitcher : MonoBehaviour
{
    public TMP_Dropdown chartDropdown; // Reference to TMP_Dropdown
    public string[] chartPaths; // Paths to the chart files relative to StreamingAssets
    public string[] audioPaths; // Paths to the audio files relative to StreamingAssets
    public AudioSource audioSource; // Reference to the AudioSource

    public static int SelectedIndex = -1; // -1 means no selection yet, use default

    void Start()
    {
        Debug.Log($"DropdownSwitcher Start. SelectedIndex: {SelectedIndex}");

        // Ensure everything is cleared before any chart is loaded
        ChartLoaderTest chartLoader = FindObjectOfType<ChartLoaderTest>();
        if (chartLoader != null)
        {
            chartLoader.ResetLoaderState(); // Clear notes and reset chart state
        }

        // Set the dropdown to match the current selection or default to 0
        if (SelectedIndex >= 0)
        {
            chartDropdown.value = SelectedIndex;
            ApplySettings(SelectedIndex); // Load the selected chart and audio
        }
        else
        {
            chartDropdown.value = 0; // Default to the first option
            StartCoroutine(LoadInitialSettings());
        }

        chartDropdown.RefreshShownValue();
        chartDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }


    void OnDropdownValueChanged(int index)
    {
        Debug.Log($"Dropdown value changed to index: {index}");

        // Update chart path for the selected chart
        string selectedChartPath = chartPaths[index];
        string selectedAudioPath = audioPaths[index];

        // Update the ChartLoaderTest with the new paths
        ChartLoaderTest chartLoader = FindObjectOfType<ChartLoaderTest>();
        if (chartLoader != null)
        {
            chartLoader.Path = selectedChartPath; // Update the chart path
            chartLoader.Music.clip = Resources.Load<AudioClip>(selectedAudioPath); // Load the new audio
            chartLoader.Music.Stop(); // Stop current audio playback
            chartLoader.ClearExistingNotes(); // Clear the notes for the previous chart

            Debug.Log($"Applying settings for new chart: {selectedChartPath}");

            // Reinitialize songStartTime, songLength, and reload the chart
            chartLoader.LoadAndInitializeChart();
        }
        else
        {
            Debug.LogError("ChartLoaderTest instance not found!");
        }
    }
    void ApplySettings(int index)
    {
        Debug.Log($"Applying settings for index: {index}");

        // Stop current audio
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null; // Clear the current audio clip
            Debug.Log("Stopped and cleared the current audio source.");
        }

        // Load and apply the chart
        if (chartPaths.Length > index)
        {
            string relativePath = chartPaths[index];
            string chartFullPath = Path.Combine(Application.streamingAssetsPath, relativePath);

            ChartLoaderTest chartLoader = FindObjectOfType<ChartLoaderTest>();
            if (chartLoader != null)
            {
                chartLoader.Path = relativePath; // Set the relative path
                Debug.Log($"Updated ChartLoaderTest Path to: {chartLoader.Path}");

                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                   // StartCoroutine(chartLoader.LoadChartWebGL(chartFullPath)); // WebGL chart loading
                }
                else
                {
                    chartLoader.ReloadChart(); // Windows and other platforms
                }
            }
            else
            {
                Debug.LogError("ChartLoaderTest instance not found in the scene.");
            }
        }

        // Load and apply the audio
        if (audioPaths.Length > index)
        {
            string audioFullPath = Path.Combine(Application.streamingAssetsPath, audioPaths[index]);
            Debug.Log($"Loading audio from path: {audioFullPath}");
            StartCoroutine(LoadAudio(audioFullPath));
        }
    }





    IEnumerator LoadAudio(string path)
    {
        string url = path;
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            url = path; // Use the URL directly for WebGL
        }

        Debug.Log($"Attempting to load audio from: {url}");

        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG)) // Change AudioType if needed
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Successfully loaded audio from: {url}");
                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                if (audioSource != null)
                {
                    audioSource.Stop(); // Ensure the current audio is stopped
                    audioSource.clip = clip; // Assign the new audio clip
                    audioSource.Play(); // Start playing the new audio
                    Debug.Log("Started playing new audio.");
                }
            }
            else
            {
                Debug.LogError($"Failed to load audio from: {url}, Error: {uwr.error}");
            }
        }
    }


    IEnumerator LoadInitialSettings()
    {
        Debug.Log("Loading initial settings...");

        ChartLoaderTest chartLoader = FindObjectOfType<ChartLoaderTest>();
        if (chartLoader != null)
        {
            // Load the default chart
            if (chartPaths.Length > 0)
            {
                chartLoader.ClearExistingNotes(); // Clear any pre-existing notes
                ChartLoaderTest.Chart = null; // Reset the chart object

                string chartFullPath = Path.Combine(Application.streamingAssetsPath, chartPaths[0]);
                chartLoader.Path = chartPaths[0];
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    // yield return chartLoader.LoadChartWebGL(chartFullPath);
                }
                else
                {
                    chartLoader.ReloadChart();
                }
            }
        }
        else
        {
            Debug.LogError("ChartLoaderTest instance not found in the scene.");
        }

        // Load the default audio
        if (audioPaths.Length > 0)
        {
            string audioFullPath = Path.Combine(Application.streamingAssetsPath, audioPaths[0]);
            yield return LoadAudio(audioFullPath);
        }
    }

}
