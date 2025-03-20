using UnityEngine;
using TMPro;

public class ScoreManagerScript : MonoBehaviour
{
    public static ScoreManagerScript Instance;

    public string totalMissionTime; // Song length in seconds
    public int totalGameGems;
    public int perfectTimeGems;
    public int wellTimedGems;
    public int missedGems;

    private void Awake()
    {
          // Singleton pattern: Only one instance persists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterNoteHit(string hitType)
    {
        totalGameGems++;

        if (hitType == "Perfect") perfectTimeGems++;
        else if (hitType == "Good") wellTimedGems++;
        else if (hitType == "Miss") missedGems++;
    }

    public void SetTotalMissionTime(string formattedTime)
    {
        totalMissionTime = formattedTime;
    }
}
