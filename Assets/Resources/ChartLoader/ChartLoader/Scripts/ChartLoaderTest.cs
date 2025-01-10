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
        set { _path = value; }
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
    void Start()
    {
        LoadAndInitializeChart();
    }
    private void LoadAndInitializeChart()
    {
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
            Debug.LogError($"Chart file not found at: {chartPath}. Ensure the file exists in the StreamingAssets folder.");
        }
    }

    public void ReloadChart()
    {
        ClearExistingNotes();

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
            Debug.LogError($"Chart file not found at: {chartPath}. Ensure the file exists in the StreamingAssets folder.");
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

    private void InitializeChartContent()
    {
        string currentDifficulty = RetrieveDifficulty();
        SpawnNotes(Chart.GetNotes(currentDifficulty));
        SpawnStarPower(Chart.GetStarPower(currentDifficulty));
        SpawnSynchTracks(Chart.SynchTracks);
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
        // Handle synchronization tracks (if necessary)
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
            float z = note.Seconds * Speed;
            for (int i = 0; i < SolidNotes.Length; i++)
            {
                if (note.ButtonIndexes[i])
                {
                    Transform noteTmp = SpawnPrefab(SolidNotes[i], transform, new Vector3(i - 1.5f, 0, z));
                    SetLongNoteScale(noteTmp.GetChild(0), note.DurationSeconds * Speed);

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

    private void ClearExistingNotes()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("Existing notes and objects cleared.");
    }
    #endregion
}
