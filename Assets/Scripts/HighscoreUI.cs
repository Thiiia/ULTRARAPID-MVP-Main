using UnityEngine;
using TMPro;

public class HighScoreUI : MonoBehaviour
{
    public TextMeshProUGUI missionTimeText;
    public TextMeshProUGUI totalGemsText;
    public TextMeshProUGUI perfectGemsText;
    public TextMeshProUGUI wellTimedGemsText;
    public TextMeshProUGUI missedGemsText;

    void Start()
    {
        missionTimeText.text = $"Total Mission Time: {ScoreManagerScript.Instance.totalMissionTime} ";
        totalGemsText.text = $"Total Game Gems: {ScoreManagerScript.Instance.totalGameGems}";
        perfectGemsText.text = $"Perfect-Time Gems: {ScoreManagerScript.Instance.perfectTimeGems}";
        wellTimedGemsText.text = $"Well-Timed Gems: {ScoreManagerScript.Instance.wellTimedGems}";
        missedGemsText.text = $"Missed Gems: {ScoreManagerScript.Instance.missedGems}";
    }

    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
