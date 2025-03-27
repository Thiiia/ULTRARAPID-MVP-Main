using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance;

    private AudioManager _audioManager;

    private bool transitionedToGameplay = false;
    private bool transitionedBackToSlideshow = false;
    private bool finalTransitioned = false;
    private bool triggeredSOE = false;
    private bool triggeredInstructions = false; // Track if the 0:46 popup has been shown
    private bool welcometohighscore = false;

    private const double SWITCH_TO_GAMEPLAY_TIME = 31.0;
    private const double INSTRUCTIONS_TIME = 46.0; // Show popup at 0:46
    private const double INSTRUCTIONS_END_TIME = 57.25; // Hide popup at 0:57
    private const double SOE_TRIGGER_TIME = 84.0;
    private const double RETURN_TO_SLIDESHOW_TIME = 112.0;
    private const double FINAL_SCENE_TRANSITION = 128.0;

    private const double HIGHSCORE = 158.0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        _audioManager = AudioManager.Instance;
        if (_audioManager == null)
        {
            Debug.LogError("TimelineManager: No AudioManager found!");
            return;
        }
    }

    void Update()
    {
        if (_audioManager == null || !_audioManager.musicSource.isPlaying) return;

        double currentTime = _audioManager.GetCurrentSongTime();

        if (currentTime >= SWITCH_TO_GAMEPLAY_TIME && !transitionedToGameplay)
        {
            transitionedToGameplay = true;
            // Debug.Log("Switching to the MainGameplayScene at 0:29");
            StartCoroutine(TransitionToScene("MainGameplayScene"));
        }

        if (currentTime >= INSTRUCTIONS_TIME && !triggeredInstructions)
        {
            triggeredInstructions = true;
            // Debug.Log("Triggering UI Popup at 0:46");
            ShowInstructions();
        }

        if (currentTime >= INSTRUCTIONS_END_TIME && triggeredInstructions)
        {
            //  Debug.Log("Closing UI Popup at 0:57");
            CloseInstructions();
            triggeredInstructions = false;
        }

        if (currentTime >= SOE_TRIGGER_TIME && !triggeredSOE)
        {
            triggeredSOE = true;
            // Debug.Log("Triggering SOE Popup at 1:24");
            StartCoroutine(TransitionToScene("MainGameplayScene"));
            TriggerSOEPopup();
            
        }

        if (currentTime >= RETURN_TO_SLIDESHOW_TIME && !transitionedBackToSlideshow)
        {
            transitionedBackToSlideshow = true;
            // Debug.Log("Returning to SlideshowScene at 1:52");
            StartCoroutine(TransitionToScene("Slideshow"));
        }

        if (currentTime >= FINAL_SCENE_TRANSITION && !finalTransitioned)
        {
            finalTransitioned = true;
            // Debug.Log("Final transition to MainGameplayScene at 2:08");
            StartCoroutine(TransitionToScene("MainGameplayScene"));
            TriggerSOEPopup();
        }
        if (currentTime >= HIGHSCORE && !welcometohighscore)
        {
            welcometohighscore = true;
            //  Debug.Log("transition to highscore scene at 2:38");
            StartCoroutine(TransitionToScene("Highscore"));
        }
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        Debug.Log($"TimelineManager: Transitioning to {sceneName}");
        yield return new WaitForSeconds(0f);
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneName);
    }

    public void SkipToGameplay()
    {
        if (!transitionedToGameplay)
        {
            transitionedToGameplay = true;
            Debug.Log("Skipping slideshow, switching to MainGameplayScene now!");
            StartCoroutine(TransitionToScene("MainGameplayScene"));
        }
    }

    private void TriggerSOEPopup()
    {
        if (FindObjectOfType<SOEGrid>().isSOEActive)
        {
            Debug.Log("SOE is already active. Skipping duplicate trigger.");
            return;
        }

        SOEGrid soeGrid = FindObjectOfType<SOEGrid>();
        if (soeGrid != null && !soeGrid.isSOEActive)
        {
            soeGrid.StartSOESequence(); // 
        }
        else
        {
            Debug.LogError("TimelineManager: SOEGrid not found!");
        }

    }

    private IEnumerator DelayedSOEStart()
    {
        yield return new WaitForSeconds(0f);
        SOEGrid soeGrid = FindObjectOfType<SOEGrid>();
        if (soeGrid != null)
        {
            soeGrid.StartSOESequence();
        }
        else
        {
            Debug.LogError("TimelineManager: SOEGrid not found!");
        }
    }

    private void ShowInstructions()
    {
        GameObject Instructions = GameObject.Find("Instructions");
        if (Instructions != null)
        {
          //  Instructions.SetActive(true);
        }
        else
        {

        }
    }

    private void CloseInstructions()
    {
        GameObject Instructions = GameObject.Find("Instructions");
        if (Instructions != null)
        {
            Instructions.SetActive(false);
        }
    }
}
