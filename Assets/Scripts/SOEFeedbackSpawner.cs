using UnityEngine;

public class SOEFeedbackSpawner : MonoBehaviour
{
    public GameObject feedbackBlockGreen;
    public GameObject feedbackBlockYellow;
    public GameObject feedbackBlockRed;
    public Transform feedbackSpawnOrigin;

    private int feedbackCount = 0;

    public void ResetSpawner()
    {
        feedbackCount = 0;
    }

    public void SpawnFeedback(string feedbackType, bool isRepeat)
    {
        int max = isRepeat ? 15 : 30;
        if (feedbackCount >= max) return;

        GameObject prefab = feedbackType switch
        {
            "Perfect" => feedbackBlockGreen,
            "Good" => feedbackBlockYellow,
            "Miss" => feedbackBlockRed,
            _ => null
        };

        if (prefab != null)
        {
            Vector3 spawnPos = feedbackSpawnOrigin.position + new Vector3(feedbackCount * 1.1f, 0, 0);
            Instantiate(prefab, spawnPos, Quaternion.identity);
            feedbackCount++;
        }
    }
}
