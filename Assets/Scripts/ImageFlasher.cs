using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class ImageFlasher : MonoBehaviour
{
    [Header("Image Settings")]
    [Tooltip("UI Image that displays the deer head")]
    public Image deerHeadImage;
    [Tooltip("First deer head sprite")]
    public Sprite imageA;
    [Tooltip("Second deer head sprite")]
    public Sprite imageB;

    [Header("Flash Timing Settings")]
    [Tooltip("Starting flash interval in seconds (e.g., 0.1 for 100ms)")]
    public float startInterval = 0.1f;
    [Tooltip("Target flash interval in seconds (e.g., 0.025 for 25ms)")]
    public float targetInterval = 0.025f;
    [Tooltip("Total duration of the flash sequence (in seconds)")]
    public float totalDuration = 3f;

    private float flashInterval;
    private bool isFlashing = false;

    // Static flag to ensure this only runs once
    private static bool hasRunOnce = false;

    void Start()
    {
        if (!hasRunOnce)
        {
            hasRunOnce = true;
            StartFlashSequence();
        }
        else
        {
            // If already run, disable the UI image immediately
            deerHeadImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Starts the deer head flash sequence.
    /// </summary>
    public void StartFlashSequence()
    {
        flashInterval = startInterval;
        isFlashing = true;
        StartCoroutine(FlashCoroutine());

        // Tween the flashInterval from startInterval to targetInterval over the total duration.
        DOTween.To(() => flashInterval, x => flashInterval = x, targetInterval, totalDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                isFlashing = false;
                deerHeadImage.gameObject.SetActive(false);
                SceneManager.LoadScene("MainGameplayScene");
            });
    }

    /// <summary>
    /// Alternates the deer head image based on the current flash interval.
    /// </summary>
    private IEnumerator FlashCoroutine()
    {
        while (isFlashing)
        {
            deerHeadImage.sprite = imageA;
            yield return new WaitForSeconds(flashInterval);
            deerHeadImage.sprite = imageB;
            yield return new WaitForSeconds(flashInterval);
        }
    }
}
