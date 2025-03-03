using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance;
    
    private AudioManager _audioManager;
    private double dspStartTime;

    private bool transitionedToGameplay = false;
    private bool transitionedBackToSlideshow = false;
    private bool finalTransitioned = false;

    private const double SWITCH_TO_GAMEPLAY_TIME = 29.0;  // 0:29 → Switch to MainGameplayScene
    private const double RETURN_TO_SLIDESHOW_TIME = 112.0; // 1:52 → Return to SlideshowScene
    private const double FINAL_SCENE_TRANSITION = 122.0;  // 2:02 → Back to MainGameplayScene

    void Awake()
    {
        // Ensure only one instance of TimelineManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Prevents it from being destroyed when switching scenes
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

        dspStartTime = AudioSettings.dspTime;
    }

    void Update()
    {
        if (_audioManager == null) return;

        double currentTime = _audioManager.GetCurrentSongTime();

        if (currentTime >= SWITCH_TO_GAMEPLAY_TIME && !transitionedToGameplay)
        {
            transitionedToGameplay = true;
            Debug.Log("Switching to the MainGameplayScene at 0:29");
            StartCoroutine(TransitionToScene("MainGameplayScene"));
        }

        if (currentTime >= RETURN_TO_SLIDESHOW_TIME && !transitionedBackToSlideshow)
        {
            transitionedBackToSlideshow = true;
            Debug.Log("Returning to SlideshowScene at 1:52");
            StartCoroutine(TransitionToScene("Slideshow"));
        }

        if (currentTime >= FINAL_SCENE_TRANSITION && !finalTransitioned)
        {
            finalTransitioned = true;
            Debug.Log("Final transition to MainGameplayScene at 2:02");
            StartCoroutine(TransitionToScene("MainGameplayScene"));
        }
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        Debug.Log($"TimelineManager: Transitioning to {sceneName}");
        yield return new WaitForSeconds(0.6f); // Short delay before switching

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

}
