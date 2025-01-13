using UnityEngine;
using ChartLoader.NET.Framework;
using ChartLoader.NET.Utils;
using Sirenix.OdinInspector; // Odin namespace
using System.IO; //  System.IO 
using UnityEngine.Networking; // For WebGL compatibility
using System.Collections; // For IEnumerator

public class ChartLoaderTest : MonoBehaviour
{
    #region Chart Settings
    [TabGroup("Chart Settings")]
    public static Chart Chart;
    #endregion

    #region Difficulty Settings
    [TabGroup("Difficulty Settings")]
    public enum Difficulty
    {
        EasyGuitar,
        MediumGuitar,
        HardGuitar,
        ExpertGuitar,
        EasyDrums,
        MediumDrums,
        HardDrums,
        ExpertDrums
    }

    [TabGroup("Difficulty Settings")]
    [SerializeField]
    private Difficulty difficulty;
    #endregion

    #region Game Settings
    [TabGroup("General Settings")]
    [SerializeField, Range(1f, 60f), Tooltip("The game speed.")]
    private float _speed = 1f;
    public float Speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    [TabGroup("General Settings")]
    [SerializeField]
    private string _path; // Path relative to StreamingAssets
    public string Path
    {
        get { return _path; }
        set { _path = value; Debug.Log($"ChartLoaderTest Path set to: {_path}"); }
    }
    #endregion

    #region Prefab Settings
    [TabGroup("Prefab Settings")]
    [SerializeField, Tooltip("The note prefabs to be instantiated.")]
    private Transform[] _solidNotes;
    public Transform[] SolidNotes
    {
        get { return _solidNotes; }
        set { _solidNotes = value; }
    }

    [TabGroup("Prefab Settings")]
    [SerializeField, Tooltip("Prefab for star power.")]
    private Transform _starPowerPrefab;
    public Transform StarPowerPrefab
    {
        get { return _starPowerPrefab; }
        set { _starPowerPrefab = value; }
    }

    [TabGroup("Prefab Settings")]
    [SerializeField, Tooltip("Prefab for sections.")]
    private Transform _sectionPrefab;
    public Transform SectionPrefab
    {
        get { return _sectionPrefab; }
        set { _sectionPrefab = value; }
    }

    [TabGroup("Prefab Settings")]
    [SerializeField, Tooltip("Prefab for BPM.")]
    private Transform _bpmPrefab;
    public Transform BpmPrefab
    {
        get { return _bpmPrefab; }
        set { _bpmPrefab = value; }
    }
    #endregion

    #region Audio & Camera Settings
    [TabGroup("Audio & Camera")]
    [SerializeField, Tooltip("The music audio source.")]
    private AudioSource _music;
    public AudioSource Music
    {
        get { return _music; }
        set { _music = value; }
    }
    public float songLength; // Duration of the song in seconds

    [TabGroup("Audio & Camera")]
    [SerializeField, Tooltip("Camera movement aggregation.")]
    private CameraMovement _cameraMovement;
    public CameraMovement CameraMovement
    {
        get { return _cameraMovement; }
        set { _cameraMovement = value; }
    }
    #endregion

    #region Start and Initialization
    private void Start()
    {
        if (Music.clip != null)
        {
            songLength = Music.clip.length; // Get the length of the AudioClip
            Debug.Log($"Song Length: {songLength} seconds");
        }
        else
        {
            Debug.LogError("Music clip is not assigned to the AudioSource.");
        }

        double dspTime = AudioSettings.dspTime; // Get DSP time
        Music.PlayScheduled(dspTime); // Start the music at DSP time

        // Find the CameraMovement component and initialize it
        if (CameraMovement != null)
        {
            CameraMovement.Initialize(dspTime, songLength); // Pass songStartTime and songLength
        }
        else
        {
            Debug.LogError("CameraMovement script is not assigned or found.");
        }

        LoadAndInitializeChart();
    }

    public void LoadAndInitializeChart()
    {
        ResetLoaderState(); // Ensure state is reset
        string chartPath = System.IO.Path.Combine(Application.streamingAssetsPath, _path);

        Debug.Log($"Attempting to load chart at: {chartPath}");

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            StartCoroutine(LoadChartWebGL(chartPath));
        }
        else if (File.Exists(chartPath))
        {
            LoadChartFromPath(chartPath);
        }
        else
        {
            Debug.LogError($"Chart file not found at: {chartPath}. Ensure the file exists in the StreamingAssets folder.");
        }
    }

    public void ReloadChart()
    {
        Debug.Log("Reloading chart...");

        ClearExistingNotes(); // Clear old notes
        Chart = null;         // Reset chart object
        Debug.Log("Chart and notes cleared.");

        string chartPath = System.IO.Path.Combine(Application.streamingAssetsPath, _path);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            StartCoroutine(LoadChartWebGL(chartPath));
        }
        else if (File.Exists(chartPath))
        {
            LoadChartFromPath(chartPath);
        }
        else
        {
            Debug.LogError($"Chart file not found: {chartPath}");
        }
    }

    private void LoadChartFromPath(string chartPath)
    {
        Debug.Log($"Loading chart from: {chartPath}");
        ChartReader chartReader = new ChartReader();
        Chart = chartReader.ReadChartFile(chartPath);

        if (Chart != null)
        {
            Debug.Log("Chart successfully loaded!");
            InitializeChartContent();
        }
        else
        {
            Debug.LogError($"Failed to load chart at: {chartPath}");
        }
    }

    public IEnumerator LoadChartWebGL(string chartPath)
    {
        Debug.Log($"Attempting to load chart from WebGL path: {chartPath}");
        Chart = null; // Reset chart object before loading

        using (UnityWebRequest uwr = UnityWebRequest.Get(chartPath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Chart successfully loaded from: {chartPath}");
                string chartData = uwr.downloadHandler.text;

                ChartReader chartReader = new ChartReader();
                Chart = chartReader.ParseChartText(chartData); // Use raw chart data

                if (Chart != null)
                {
                    Debug.Log($"Chart initialized:");
                    InitializeChartContent();
                }
                else
                {
                    Debug.LogError("Failed to parse chart data.");
                }
            }
            else
            {
                Debug.LogError($"Failed to load chart from WebGL path: {chartPath}, Error: {uwr.error}");
            }
        }
    }

    private bool _isChartInitialized = false;

   private void InitializeChartContent()
{
    if (_isChartInitialized)
    {
        Debug.LogWarning("Chart is already initialized. Skipping re-initialization.");
        return;
    }

    _isChartInitialized = true;

    string currentDifficulty = RetrieveDifficulty();

    // Spawn Notes
    SpawnNotes(Chart.GetNotes(currentDifficulty));
    SpawnStarPower(Chart.GetStarPower(currentDifficulty));
    SpawnSynchTracks(Chart.SynchTracks);

    // Adjust Camera Position Based on Notes
    GameObject notesParent = GameObject.Find("Expert Guitar"); // Replace with your actual notes parent name
    if (notesParent != null && notesParent.transform.childCount > 0)
    {
        // Find the first note's Z position
        Transform firstNote = notesParent.transform.GetChild(0);
        float firstNoteZ = firstNote.position.z; // Use world position for consistency

        // Adjust the camera's starting position
        if (CameraMovement != null)
        {
            CameraMovement.Initialize(CameraMovement.SongStartTime, songLength);
            CameraMovement.transform.position = new Vector3(
                CameraMovement.transform.position.x,
                CameraMovement.transform.position.y,
                firstNoteZ
            );
            Debug.Log($"Camera initialized to first note position: {firstNoteZ}");
        }
        else
        {
            Debug.LogError("CameraMovement script is not attached to the main camera or is null.");
        }
    }
    else
    {
        Debug.LogWarning("Notes parent object or notes are missing.");
    }

    // Start the Song
    StartSong();
}


    #endregion

    #region Difficulty Handling
    private string RetrieveDifficulty()
    {
        string result;
        switch (difficulty)
        {
            case Difficulty.EasyGuitar:
                result = "EasySingle";
                break;
            case Difficulty.MediumGuitar:
                result = "MediumSingle";
                break;
            case Difficulty.HardGuitar:
                result = "HardSingle";
                break;
            case Difficulty.ExpertGuitar:
                result = "ExpertSingle";
                break;
            case Difficulty.EasyDrums:
                result = "EasyDrums";
                break;
            case Difficulty.MediumDrums:
                result = "MediumDrums";
                break;
            case Difficulty.HardDrums:
                result = "HardDrums";
                break;
            case Difficulty.ExpertDrums:
                result = "ExpertDrums";
                break;
            default:
                result = "ExpertSingle";
                break;
        }
        return result;
    }
    #endregion

    #region Spawn Methods
    private void SpawnSynchTracks(SynchTrack[] synchTracks)
    {
        // Handle synchronization
    }
    private void SpawnStarPower(StarPower[] starPowers)
    {
        foreach (StarPower starPower in starPowers)
        {
            Transform tmp = SpawnPrefab(StarPowerPrefab, transform, new Vector3(0, 0, starPower.Seconds * Speed));
            tmp.localScale = new Vector3(1, 1, starPower.DurationSeconds * Speed);
        }
    }

    private void SpawnNotes(Note[] notes)
    {
        foreach (Note note in notes)
        {
            // Calculate Z position relative to song time and speed
            float z = (float)(note.Seconds * Speed);

            for (int i = 0; i < SolidNotes.Length; i++)
            {
                if (note.ButtonIndexes[i])
                {
                    Transform noteTmp = SpawnPrefab(SolidNotes[i], transform, new Vector3(i - 1.5f, 0, z));

                    // Set the scale for long notes
                    SetLongNoteScale(noteTmp.GetChild(0), note.DurationSeconds * Speed);

                    // Assign the expected hit time for timing synchronization
                    var spawner = noteTmp.GetComponent<NoteFactorSpawner>();
                    if (spawner != null)
                    {
                        spawner.expectedHitTime = note.Seconds;
                    }
                }
            }
        }
    }

    #endregion

    #region Helper Methods
    private void StartSong()
    {
        CameraMovement.Speed = Speed;
        CameraMovement.enabled = true;
        PlayMusic();
    }

    private void PlayMusic() => Music.Play();

    private Transform SpawnPrefab(Transform prefab, Transform parent, Vector3 position)
    {
        Transform tmp = Instantiate(prefab, parent);
        tmp.localPosition = position;
        return tmp;
    }

    private void SetLongNoteScale(Transform note, float length)
    {
        note.localScale = new Vector3(note.localScale.x, note.localScale.y, length);
    }

    private void SetHammerOnColor(Transform note, bool isHammerOn)
    {
        if (isHammerOn)
        {
            SpriteRenderer renderer = note.GetComponent<SpriteRenderer>();
            renderer.color = new Color(renderer.color.r + 0.75f, renderer.color.g + 0.75f, renderer.color.b + 0.75f);
        }
    }

    private void SetHOPO(Transform note)
    {
        SpriteRenderer renderer = note.GetComponent<SpriteRenderer>();
        renderer.color = new Color(0.75f, 0, 0.75f);
    }

    public void ClearExistingNotes()
    {
        Debug.Log("Clearing existing notes...");
        foreach (Transform child in transform)
        {
            Debug.Log($"Destroying note: {child.name}");
            Destroy(child.gameObject);
        }
        Debug.Log("All notes and objects cleared.");
    }

    public void ResetLoaderState()
    {
        Debug.Log("Resetting loader state...");
        ClearExistingNotes(); // Remove all instantiated notes
        Chart = null; // Clear static chart data
        Path = ""; // Reset the path
        _isChartInitialized = false;
    }
    #endregion

}
