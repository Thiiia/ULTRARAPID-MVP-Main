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

        // Set the dropdown to match the current selection or default to 0
        if (SelectedIndex >= 0)
        {
            chartDropdown.value = SelectedIndex;
            ApplySettings(SelectedIndex);
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
        SelectedIndex = index; // Save the new selection
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload the scene
    }

    void ApplySettings(int index)
    {
        Debug.Log($"Applying settings for index: {index}");

        // Load and apply the chart
        if (chartPaths.Length > index)
        {
            string chartFullPath = Path.Combine(Application.streamingAssetsPath, chartPaths[index]);
            Debug.Log($"Switching chart to path: {chartFullPath}");

            ChartLoaderTest chartLoader = FindObjectOfType<ChartLoaderTest>();
            if (chartLoader != null)
            {
                chartLoader.Path = chartPaths[index];
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    StartCoroutine(chartLoader.LoadChartWebGL(chartFullPath)); // WebGL chart loading
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
                    audioSource.Stop();
                    audioSource.clip = clip;
                    audioSource.Play();
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

        // Load the default chart
        if (chartPaths.Length > 0)
        {
            string chartFullPath = Path.Combine(Application.streamingAssetsPath, chartPaths[0]);
            ChartLoaderTest chartLoader = FindObjectOfType<ChartLoaderTest>();
            if (chartLoader != null)
            {
                chartLoader.Path = chartPaths[0];
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    yield return chartLoader.LoadChartWebGL(chartFullPath);
                }
                else
                {
                    chartLoader.ReloadChart();
                }
            }
        }

        // Load the default audio
        if (audioPaths.Length > 0)
        {
            string audioFullPath = Path.Combine(Application.streamingAssetsPath, audioPaths[0]);
            yield return LoadAudio(audioFullPath);
        }
    }
}
