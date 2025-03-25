using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class SOEGrid : MonoBehaviour
{
    [Header("References")]
    public GameObject numberPrefab;
    public Transform spawnPoint;
    public Transform gridParent;
    public SOEPrimeFinder primeFinder;

    [Header("UI Elements")]
    public TextMeshProUGUI memoryEncodingText;
    public TextMeshProUGUI pressSpaceText;
    public TextMeshProUGUI primesText;

    [Header("Timing")]
    public float flightDuration = 0.05f;
    private Dictionary<int, GameObject> numberObjects = new Dictionary<int, GameObject>();
    private Queue<int> primeQueue = new Queue<int>();
    private bool isAnimating = false;
    private int primesPlaced = 0;
    private bool waitingForSpace = false;
    public bool firstTimePlaying;
    public bool isSOEActive = false;
    private bool soeRepeated = false;
    private bool completionStarted = false;
    private double lastTriggeredBeat = -1; // Store the last beat time

    private SOEPopup refToSOEPopup;
    public Color refToColor;

    void Awake()
    {

    }
    private void OnEnable()
    {
        ChartLoaderTest.OnBeat += OnBeatTriggered;
    }

    private void OnDisable()
    {
        ChartLoaderTest.OnBeat -= OnBeatTriggered;
    }
    void Start()
    {
        GenerateGrid();

        gridParent.gameObject.SetActive(false);
        primesText.gameObject.SetActive(false);

        if (!ImageFlasher.hasRunOnce)
        {
            Invoke(nameof(WaitTillNoDeer), 5.4f);

            firstTimePlaying = false;
        }
        else
        {
            Invoke(nameof(WaitTillNoDeer), 0f);


        }

        refToSOEPopup = GameObject.Find("SOE_Popup").GetComponentInChildren<SOEPopup>();
        RectTransform spawnRect = spawnPoint.GetComponent<RectTransform>();
        if (spawnRect != null) spawnRect.anchoredPosition = new Vector2(0, 150);
        ResetPrimes();
        StartCoroutine(ShowMemoryEncodingPrompt());
    }

    private void OnDestroy()
    {
        ChartLoaderTest.OnBeat -= OnBeatTriggered;
    }

    void WaitTillNoDeer()
    {
        gridParent.gameObject.SetActive(true);
    }

    public void GenerateGrid()
    {
        int cols = 10;
        float spacingX = 200f, spacingY = -155f;
        HashSet<int> primes = primeFinder.primes;

        for (int i = 0; i <= 49; i++)
        {
            GameObject numObj = Instantiate(numberPrefab, gridParent);
            numObj.name = "Number_" + i;
            int row = i / cols, col = i % cols;
            numObj.transform.localPosition = new Vector3(col * spacingX, row * spacingY, 0);

            TextMeshPro text = numObj.GetComponentInChildren<TextMeshPro>();
            text.text = i.ToString();
            numberObjects[i] = numObj;

            if (!primes.Contains(i)) text.color = refToColor;
            else text.text = "";
        }
    }

    public void StartSOESequence()
    {
        if (isSOEActive) return;
        isSOEActive = true;

        refToSOEPopup.ToggleSOEPopup();
        ResetPrimes(); // Fully reset primes before starting

        StartCoroutine(ShowMemoryEncodingPrompt());
        if (ImageFlasher.hasRunOnce)
        {
            StartCoroutine(ClosePopupWithDelay(6f));
        }

        Debug.Log("Triggering Animation Immediately...");

    }
    private IEnumerator RepeatSOESequence()
    {
        Debug.Log("Resetting SOE for second animation cycle...");

        ResetPrimes();

        // Safely re-subscribe just in case
        ChartLoaderTest.OnBeat -= OnBeatTriggered;
        ChartLoaderTest.OnBeat += OnBeatTriggered;

        yield return new WaitForSeconds(0.2f); // small buffer

        Debug.Log("Repeating SOE animation sequence...");
        OnBeatTriggered(); // Kick off next prime animation
    }






    private bool beatsShouldTrigger = true;

    private void OnBeatTriggered()
    {
        if (!beatsShouldTrigger || isAnimating) return;

        // Get current song time from DSP
        double currentTime = AudioManager.Instance.GetCurrentSongTime();

        // don't process the same beat multiple times
        if (Math.Abs(currentTime - lastTriggeredBeat) < 0.05) // Small buffer 
            return;

        lastTriggeredBeat = currentTime; // Update last beat time

        // Call GetNextPrime() to get the correct prime number
        int primeToAnimate = GetNextPrime();

        if (primeToAnimate == -1)
        {
            Debug.Log("No more primes to animate.");
            StartCoroutine(ShowPrimesFlashingSequence());
            return; // Stop if no primes are left
        }

        Debug.Log($"🎵 Valid Beat Triggered at {currentTime} | Prime to Animate: {primeToAnimate}");

        StartCoroutine(AnimatePrimeIntoBox(primeToAnimate));
    }


    private int GetNextPrime()
    {
        List<int> orderedPrimes = new List<int>(primeFinder.primes);
        orderedPrimes.Sort();

        if (primesPlaced < orderedPrimes.Count) return orderedPrimes[primesPlaced];
        return -1;
    }

    private IEnumerator AnimatePrimeIntoBox(int primeValue)
    {
        if (!numberObjects.ContainsKey(primeValue)) yield break;
        if (spawnPoint == null) yield break;

        GameObject targetObject = numberObjects[primeValue];
        if (targetObject == null) yield break;

        RectTransform targetRect = targetObject.GetComponent<RectTransform>();
        if (targetRect == null) yield break;

        // Kill any existing tweens to prevent conflicts
        DOTween.Kill(targetRect);

        // Destroy old children safely
        foreach (Transform child in spawnPoint)
        {
            if (child != null)
            {
                DOTween.Kill(child);
                Destroy(child.gameObject);
            }
        }

        // Create a new prime safely
        GameObject primeInstance = Instantiate(numberPrefab, spawnPoint);
        primeInstance.transform.SetParent(spawnPoint, false);
        RectTransform primeRect = primeInstance.GetComponent<RectTransform>();

        if (primeRect == null)
        {
            Destroy(primeInstance);
            yield break;
        }

        primeRect.anchoredPosition = Vector2.zero;
        primeRect.localScale = Vector3.one * 575;
        primeRect.localPosition += new Vector3(0, 0, -0.2f * primesPlaced);

        TextMeshPro primeText = primeInstance.GetComponentInChildren<TextMeshPro>();
        var shapeComponent = primeInstance.GetComponentInChildren<Shapes.Rectangle>();
        if (shapeComponent != null)
        {

            shapeComponent.SortingOrder = 1;
        }
        if (primeText != null)
        {
            primeText.text = primeValue.ToString();
            primeText.sortingOrder = 2;
            if (!soeRepeated)
                primeText.color = new Color(255 / 255f, 219 / 255f, 237 / 255f);
            else
                primeText.color = Color.red;
        }

        primesPlaced++;

        yield return new WaitForSeconds(0.075f);

        Vector2 targetUIPosition = ConvertToUIPosition(targetRect);

        // Ensure no duplicate tweens run
        if (DOTween.IsTweening(primeRect))
        {
            Debug.Log("⚠ Tween already running, skipping duplicate animation.");
            yield break;
        }

        yield return primeRect.DOAnchorPos(targetUIPosition, flightDuration)
                             .SetEase(Ease.OutExpo)
                             .WaitForCompletion();

        TextMeshPro boxText = targetRect.GetComponentInChildren<TextMeshPro>();
        if (boxText != null)
        {
            boxText.text = primeValue.ToString();
            boxText.color = Color.yellow;
        }

        if (primeInstance != null)
        {
            yield return primeInstance.transform.DOScale(Vector3.one, 0.3f)
                                               .SetEase(Ease.OutBounce)
                                               .WaitForCompletion();
            Destroy(primeInstance);
        }

        primeQueue = new Queue<int>(primeQueue.Where(x => x != primeValue));

        //  If all primes have been placed, proceed to SOE completion logic
        if (primesPlaced == 15) HandleSOECompletion();
    }



    private void HandleSOECompletion()
    {
        if (completionStarted) return;
        completionStarted = true;

        Debug.Log("Checking if SOE should repeat...");

        if (!soeRepeated)
        {
            Debug.Log("First-time SOE: repeat pending, skipping close.");
            StartCoroutine(ShowPrimesFlashingSequence());
        }
        else
        {
            Debug.Log("SOE already ran or not first time, closing.");
            StartCoroutine(ShowPrimesFlashingSequence());
        }
    }





    private IEnumerator ShowPrimesFlashingSequence()
    {
        ShowPrimesText();

        yield return new WaitForSeconds(0);

        Debug.Log("Finished pulsing primes text. Evaluating what to do next...");

        if (!soeRepeated)
        {
            Debug.Log("First-time SOE detected, repeating animation...");

            soeRepeated = true;
            ResetPrimes();
            yield return RepeatSOESequence();
            yield break;
        }

        //  Close popup only after everything's truly done
        Debug.Log("Closing SOE popup after primes animation.");
        StartCoroutine(ClosePopupWithDelay(2f));
    }



    private Vector2 ConvertToUIPosition(RectTransform target)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, target.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(spawnPoint as RectTransform, screenPoint, null, out Vector2 uiPosition);
        return uiPosition;
    }

    private IEnumerator ClosePopupWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        HidePrimesText();
        ResetPrimes();
        refToSOEPopup.CloseSOEPopup();
        isSOEActive = false;

    }

    private IEnumerator ShowMemoryEncodingPrompt()
    {
        ShowPromptText();
        DOTween.Kill(memoryEncodingText);
        DOTween.Kill(pressSpaceText);

        Sequence memorySequence = DOTween.Sequence();
        memorySequence.Append(memoryEncodingText.DOFade(0f, 0.5f))
                      .Append(memoryEncodingText.DOFade(1f, 0.5f))
                      .SetLoops(-1);

        pressSpaceText.gameObject.SetActive(false);


        yield return null;
    }

    private void ShowPromptText()
    {
        memoryEncodingText.gameObject.SetActive(true);
        pressSpaceText.gameObject.SetActive(true);
    }

    private void HidePromptText()
    {
        memoryEncodingText.gameObject.SetActive(false);
        pressSpaceText.gameObject.SetActive(false);
    }
    public void ShowPrimesText()
    {
        if (primesText == null) return;

        primesText.gameObject.SetActive(true);
        primesText.text = "Primes 0-49";
        primesText.alpha = 1f;

        DOTween.Kill(primesText);

        Sequence primeSequence = DOTween.Sequence();
        primeSequence.Append(primesText.DOFade(0f, 0.5f))
                      .Append(primesText.DOFade(1f, 0.5f))
                      .SetLoops(-1);

    }
    private void HidePrimesText()
    {
        if (primesText == null) return;

        DOTween.Kill(primesText);
        primesText.gameObject.SetActive(false);
    }

    public void ResetPrimes()
    {
        Debug.Log(" Resetting Primes & Clearing Tweens...");

        primesPlaced = 0;
        beatsShouldTrigger = true;
        primeQueue.Clear();
        completionStarted = false;
        // Clear prime numbers visually from grid
        foreach (int prime in primeFinder.primes)
        {
            if (numberObjects.TryGetValue(prime, out GameObject obj))
            {
                TextMeshPro tmp = obj.GetComponentInChildren<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.text = "";
                    tmp.color = Color.white; // or refToColor if needed
                }
            }
        }

        // Kill all DOTween tweens
        DOTween.Kill(memoryEncodingText);
        DOTween.Kill(pressSpaceText);
        DOTween.Kill(primesText);
    }
}
