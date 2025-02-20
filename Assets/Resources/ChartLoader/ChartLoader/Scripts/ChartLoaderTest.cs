using UnityEngine;
using ChartLoader.NET.Framework;
using ChartLoader.NET.Utils;
using Sirenix.OdinInspector; // Odin namespace
using System.IO; //  System.IO 
using UnityEngine.Networking; // For WebGL compatibility
using System.Collections; // For IEnumerator
using System;
using UnityEngine.Events;

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
    private float firstNoteZPosition;

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
    public static event Action OnBeat; // Event to notify when a beat occurs
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
            songLength = Music.clip.length;
            Debug.Log($"Song Length: {songLength} seconds");
        }
        else
        {
            Debug.LogError("Music clip is not assigned to the AudioSource.");
        }

        double dspTime = AudioSettings.dspTime;
        Music.PlayScheduled(dspTime);

        if (CameraMovement != null)
        {
            CameraMovement.Initialize(dspTime, songLength);
        }
        else
        {
            Debug.LogError("CameraMovement script is not assigned or found.");
        }

        LoadAndInitializeChart();
    }


    public void LoadAndInitializeChart()
    {
        _isChartInitialized = false;

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

        // Clear existing notes and reset chart state
        ClearExistingNotes();
        Chart = null; // Reset chart object
        Debug.Log("Chart and notes cleared.");

        // Get the chart path
        string chartPath = System.IO.Path.Combine(Application.streamingAssetsPath, _path);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            StartCoroutine(LoadChartWebGL(chartPath)); // Handle WebGL chart loading
        }
        else if (File.Exists(chartPath))
        {
            LoadChartFromPath(chartPath); // Handle local chart loading
        }
        else
        {
            Debug.LogError($"Chart file not found: {chartPath}");
            return;
        }

        // Recalculate DSP time and start the music for the new chart
        if (Music.clip != null)
        {
            double dspTime = AudioSettings.dspTime; // Get current DSP time
            Music.Stop(); // Stop any currently playing music
            Music.PlayScheduled(dspTime); // Schedule the new song to play

            Debug.Log($"New song scheduled to play at DSP time: {dspTime}");

            // Reinitialize camera and other components with new song timing
            CameraMovement cameraMovement = GameObject.Find("Guitar Camera").GetComponent<CameraMovement>();
            if (cameraMovement != null)
            {
                cameraMovement.Initialize(dspTime, Music.clip.length);
            }
            else
            {
                Debug.LogWarning("CameraMovement script not found on main camera.");
            }
        }
        else
        {
            Debug.LogError("Music clip is null. Ensure the correct audio file is assigned.");
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
    Debug.Log($"🌍 WebGL Loading Chart from: {chartPath}");

    using (UnityWebRequest uwr = UnityWebRequest.Get(chartPath))
    {
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            string chartData = uwr.downloadHandler.text;
            Debug.Log($"✅ WebGL Loaded Chart. Size: {chartData.Length} characters.");

            yield return new WaitForSeconds(0.1f); // ✅ Ensure full file is processed

            ChartReader chartReader = new ChartReader();
            Chart = chartReader.ParseChartText(chartData);

            if (Chart != null)
            {
                Debug.Log($"🎵 Chart successfully initialized! Sections: {Chart.Notes.Keys.Count}");
                InitializeChartContent();
            }
            else
            {
                Debug.LogError("❌ WebGL failed to parse chart.");
            }
        }
        else
        {
            Debug.LogError($"❌ WebGL failed to load chart. Error: {uwr.error}");
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
        // ✅ Log the number of notes in the parsed chart before trying to spawn
        Note[] notes = Chart.GetNotes(currentDifficulty);
        Debug.Log($"🧐 WebGL Parsed {notes.Length} notes for difficulty: {currentDifficulty}");

        if (notes.Length == 0)
        {
            Debug.LogError("🚨 Chart loaded, but NO NOTES found! Parsing issue?");
        }

        // Spawn Notes
        SpawnNotes(Chart.GetNotes(currentDifficulty));
        SpawnStarPower(Chart.GetStarPower(currentDifficulty));
        SpawnSynchTracks(Chart.SynchTracks);

        // Adjust Camera Position Based on Notes
        GameObject notesParent = GameObject.FindWithTag("Note Container"); // Notes Container
        if (notesParent != null && notesParent.transform.childCount > 0)
        {
            // Find the first note's Z position
            Transform firstNote = notesParent.transform.GetChild(0);
            float firstNoteZ = firstNote.position.z; // Use world position for consistency
            firstNoteZPosition = firstNote.position.z; // Store the first note position globally

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
    Debug.Log($"Spawning {notes.Length} notes...");

    foreach (Note note in notes)
    {
        float z = (note.Seconds * Speed) + firstNoteZPosition; // Adjust Z with offset

        for (int i = 0; i < SolidNotes.Length; i++)
        {
            if (note.ButtonIndexes[i])
            {
                Transform noteTmp = Instantiate(SolidNotes[i], transform);
                noteTmp.localPosition = new Vector3(i - 1.5f, 0, z);

                NoteFactorSpawner spawner = noteTmp.GetComponent<NoteFactorSpawner>();
                if (spawner != null)
                {
                    double expectedHitTime = CameraMovement.SongStartTime + (noteTmp.localPosition.z / Speed);
                    spawner.expectedHitTime = (float)expectedHitTime;

                    Debug.Log($"🎵 Beat Assigned to {noteTmp.name} | Expected Hit Time: {expectedHitTime}");

                    // 🚀 Trigger the Beat Immediately (No Coroutine Needed)
                    OnBeat?.Invoke();
                }
                else
                {
                    Debug.LogError($"NoteFactorSpawner not found on note {noteTmp.name}");
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
