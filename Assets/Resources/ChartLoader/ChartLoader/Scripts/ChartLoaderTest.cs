using UnityEngine;
using ChartLoader.NET.Framework;
using ChartLoader.NET.Utils;
using Sirenix.OdinInspector;  // Import Odin namespace

public class ChartLoaderTest : MonoBehaviour
{
    #region Chart Settings
    /// <summary>
    /// The current associated chart.
    /// </summary>
    [TabGroup("Chart Settings")]
    public static Chart Chart;
    #endregion

    #region Difficulty Settings
    /// <summary>
    /// Enumerator for all major difficulties.
    /// </summary>
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
    [SerializeField, Range(0.1f, 5f), Tooltip("The game speed.")]
    private float _speed = 1f;
    /// <summary>
    /// The game speed.
    /// </summary>
    public float Speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    [TabGroup("General Settings")]
    [SerializeField]
    private string _path;
    /// <summary>
    /// The current path of the chart file.
    /// </summary>
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
    /// <summary>
    /// The note prefabs to be instantiated.
    /// </summary>
    public Transform[] SolidNotes
    {
        get { return _solidNotes; }
        set { _solidNotes = value; }
    }

    [TabGroup("Prefab Settings")]
    [SerializeField, Tooltip("Prefab for star power.")]
    private Transform _starPowerPrefab;
    /// <summary>
    /// The star power prefab instantiated should there be any star power at all.
    /// </summary>
    public Transform StarPowerPrefab
    {
        get { return _starPowerPrefab; }
        set { _starPowerPrefab = value; }
    }

    [TabGroup("Prefab Settings")]
    [SerializeField, Tooltip("Prefab for sections.")]
    private Transform _sectionPrefab;
    /// <summary>
    /// The section prefab instantiated should there be any sections at all.
    /// </summary>
    public Transform SectionPrefab
    {
        get { return _sectionPrefab; }
        set { _sectionPrefab = value; }
    }

    [TabGroup("Prefab Settings")]
    [SerializeField, Tooltip("Prefab for BPM.")]
    private Transform _bpmPrefab;
    /// <summary>
    /// The BPM prefab instantiated should there be any sections at all.
    /// </summary>
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
    /// <summary>
    /// The music audio source.
    /// </summary>
    public AudioSource Music
    {
        get { return _music; }
        set { _music = value; }
    }

    [TabGroup("Audio & Camera")]
    [SerializeField, Tooltip("Camera movement aggregation.")]
    private CameraMovement _cameraMovement;
    /// <summary>
    /// Camera movement aggregation.
    /// </summary>
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
    /// <summary>
    /// Retrieves the string enumerator version.
    /// </summary>
    /// <returns>string</returns>
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
    /// <summary>
    /// Spawns a synch track.
    /// </summary>
    /// <param name="starPowers">The star power array.</param>
    private void SpawnSynchTracks(SynchTrack[] SynchTracks)
    {
        Transform tmp;
        foreach (SynchTrack synchTrack in SynchTracks)
        {
            tmp = SpawnPrefab(BpmPrefab, transform, new Vector3(3f, 0, synchTrack.Seconds * Speed));
            tmp.GetChild(0).GetComponent<TextMesh>().text = "BPM: " + (synchTrack.BeatsPerMinute / 1000) + " " + synchTrack.Measures + "/" + synchTrack.Measures;
        }
    }

    /// <summary>
    /// Spawns a star power background.
    /// </summary>
    /// <param name="starPowers">The star power array.</param>
    private void SpawnStarPower(StarPower[] starPowers)
    {
        Transform tmp;
        foreach (StarPower starPower in starPowers)
        {
            tmp = SpawnPrefab(StarPowerPrefab, transform, new Vector3(0, 0, starPower.Seconds * Speed));
            tmp.localScale = new Vector3(1, 1, starPower.DurationSeconds * Speed);
        }
    }

    /// <summary>
    /// Spawns all notes associated to the provided array.
    /// </summary>
    /// <param name="notes">Your array of notes.</param>
    private void SpawnNotes(Note[] notes)
    {
        Transform noteTmp;
        float z;

        foreach (Note note in notes)
        {
            z = note.Seconds * Speed;
            for (int i = 0; i < 5; i++)
            {
                if (note.ButtonIndexes[i])
                {
                    noteTmp = SpawnPrefab(SolidNotes[i], transform, new Vector3(i - 2f, 0, z));
                    SetLongNoteScale(noteTmp.GetChild(0), note.DurationSeconds * Speed);
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

    private void PlayMusic() => Music.Play();

    private Transform SpawnPrefab(Transform prefab, Transform parent, Vector3 position)
    {
        Transform tmp;
        tmp = Instantiate(prefab);
        tmp.SetParent(parent);
        tmp.localPosition = position;
        return tmp;
    }

    private void SetLongNoteScale(Transform note, float length)
    {
        note.localScale = new Vector3(note.localScale.x, note.localScale.y, length);
    }

    private void SetHammerOnColor(Transform note, bool isHammerOn)
    {
        SpriteRenderer re;
        Color color;
        if (isHammerOn)
        {
            re = note.GetComponent<SpriteRenderer>();
            color = re.color;
            re.color = new Color(color.r + 0.75f, color.g + 0.75f, color.b + 0.75f);
        }
    }

    private void SetHOPO(Transform note)
    {
        SpriteRenderer re;
        Color color;
        re = note.GetComponent<SpriteRenderer>();
        color = re.color;
        re.color = new Color(0.75f, 0, 0.75f);
    }
    #endregion
}
