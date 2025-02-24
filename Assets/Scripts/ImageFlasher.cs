using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageFlasher : MonoBehaviour
{
    public Image firstImage;
    public Image secondImage;
    
    [Tooltip("Flash intervals in milliseconds (50, 75, 100)")]
    public int flashIntervalInMS = 50; // Default to 50ms

    private void Start()
    {
        StartCoroutine(FlashImages());
    }

    private IEnumerator FlashImages()
    {
        float interval = flashIntervalInMS / 1000f; // Convert ms to seconds

        while (true)
        {
            firstImage.enabled = true;
            secondImage.enabled = false;
            yield return new WaitForSeconds(interval);

            firstImage.enabled = false;
            secondImage.enabled = true;
            yield return new WaitForSeconds(interval);
        }
    }

    // Change the interval dynamically
    public void SetFlashInterval(int newIntervalMs)
    {
        flashIntervalInMS = newIntervalMs;
        StopAllCoroutines();
        StartCoroutine(FlashImages());
    }
}
