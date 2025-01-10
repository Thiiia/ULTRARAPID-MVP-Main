using UnityEngine;
using UnityEngine.SceneManagement; // For scene management
using TMPro; // For TMP_Dropdown

public class DropdownSwitcher : MonoBehaviour
{
    public TMP_Dropdown chartDropdown; // Reference to TMP_Dropdown
    public string[] chartPaths; // Paths to the chart files
    public AudioClip[] audioClips; // Audio clips corresponding to each chart
    public AudioSource audioSource; // Reference to the AudioSource

    public static int SelectedIndex = -1; // -1 means no selection yet, use default

    void Start()
    {
        // Set the dropdown to match the current selection or default to 0
        if (SelectedIndex >= 0)
        {
            chartDropdown.value = SelectedIndex;
            ApplySettings(SelectedIndex);
        }
        else
        {
            chartDropdown.value = 0; // Default to first option
        }

        chartDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDropdownValueChanged(int index)
    {
        SelectedIndex = index; // Save the new selection
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload the scene
    }

    void ApplySettings(int index)
{
    Debug.Log($"ApplySettings called with index: {index}");

    // Apply the audio
    if (audioSource != null && audioClips.Length > index)
    {
        audioSource.Stop();
        audioSource.clip = audioClips[index];
        audioSource.Play();
    }

    // Apply the chart path and reload it
    if (chartPaths.Length > index)
    {
        Debug.Log($"Switching chart to path: {chartPaths[index]}");

        ChartLoaderTest chartLoader = FindObjectOfType<ChartLoaderTest>();
        if (chartLoader != null)
        {
            chartLoader.Path = chartPaths[index];
            chartLoader.ReloadChart(); // Call ReloadChart() to switch charts
        }
        else
        {
            Debug.LogError("ChartLoaderTest instance not found in the scene.");
        }
    }
}

}
