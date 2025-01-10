using UnityEngine;
using ChartLoader.NET.Framework;
using ChartLoader.NET.Utils;
using Sirenix.OdinInspector; // Import Odin namespace

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
    private string _path;
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
        string currentDifficulty;

        // Read chart file
        ChartReader chartReader = new ChartReader();
        Chart = chartReader.ReadChartFile(Application.dataPath + Path);

        // Retrieve difficulty and spawn notes
        currentDifficulty = RetrieveDifficulty();

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
        Transform tmp;
        foreach (SynchTrack synchTrack in synchTracks)
        {
            // tmp = SpawnPrefab(BpmPrefab, transform, new Vector3(3f, 0, synchTrack.Seconds * Speed));
            // tmp.GetChild(0).GetComponent<TextMesh>().text = "BPM: " + (synchTrack.BeatsPerMinute / 1000) + " " + synchTrack.Measures + "/" + synchTrack.Measures;
        }
    }

    private void SpawnStarPower(StarPower[] starPowers)
    {
        Transform tmp;
        foreach (StarPower starPower in starPowers)
        {
            tmp = SpawnPrefab(StarPowerPrefab, transform, new Vector3(0, 0, starPower.Seconds * Speed));
            tmp.localScale = new Vector3(1, 1, starPower.DurationSeconds * Speed);
        }
    }

    private void SpawnNotes(Note[] notes)
    {
        Transform noteTmp;
        float z;

        foreach (Note note in notes)
        {
            z = note.Seconds * Speed;
            for (int i = 0; i < 4; i++)
            {
                if (note.ButtonIndexes[i]) // Checks which note button (e.g., green, red, etc.) is active
                {
                    noteTmp = SpawnPrefab(SolidNotes[i], transform, new Vector3(i - 1.5f, 0, z)); // Position of the notes
                    SetLongNoteScale(noteTmp.GetChild(0), note.DurationSeconds * Speed);

                    // Assign timing data to the NoteFactorSpawner
                    var spawner = noteTmp.GetComponent<NoteFactorSpawner>();
                    if (spawner != null)
                    {
                        spawner.expectedHitTime = note.Seconds; // Set the expected hit time
                    }

                    if (note.IsHOPO)
                        SetHOPO(noteTmp);
                    else
                        SetHammerOnColor(noteTmp, (note.IsHammerOn && !note.IsChord && !note.ForcedSolid));
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
    public void InitializeChart()
    {
        string currentDifficulty;

        // Read chart file
        ChartReader chartReader = new ChartReader();
        Chart = chartReader.ReadChartFile(Application.dataPath + Path);

        // Retrieve difficulty and spawn notes
        currentDifficulty = RetrieveDifficulty();

        SpawnNotes(Chart.GetNotes(currentDifficulty));
        SpawnStarPower(Chart.GetStarPower(currentDifficulty));
        SpawnSynchTracks(Chart.SynchTracks);

        StartSong();
    }

    public void ReloadChart()
    {
        // Clear previously instantiated elements (if any)
        ClearExistingNotes();

        string currentDifficulty;

        // Read the new chart file
        ChartReader chartReader = new ChartReader();
        Chart = chartReader.ReadChartFile(Application.dataPath + Path);

        // Retrieve difficulty and spawn notes
        currentDifficulty = RetrieveDifficulty();

        SpawnNotes(Chart.GetNotes(currentDifficulty));
        SpawnStarPower(Chart.GetStarPower(currentDifficulty));
        SpawnSynchTracks(Chart.SynchTracks);

        StartSong();
    }

    private void ClearExistingNotes()
    {
        // Destroy all children of the chart loader's parent (or note parent object)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Optionally, reset other elements (e.g., UI, counters)
        Debug.Log("Existing notes and objects cleared.");
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
    #endregion
}
