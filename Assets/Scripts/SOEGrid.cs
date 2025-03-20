using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private SOEPopup refToSOEPopup;
    public Color refToColor;

    void Awake()
    {
        
    }
    void Start()
    {
        GenerateGrid();
        
        gridParent.gameObject.SetActive(false);

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

        ChartLoaderTest.OnBeat += OnBeatTriggered;
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
        ResetPrimes();
        OnBeatTriggered(); 
        StartCoroutine(ShowMemoryEncodingPrompt());
        StartCoroutine(ClosePopupWithDelay(12f));
    }

    private bool beatsShouldTrigger = true;

private void OnBeatTriggered()
{
    if (!beatsShouldTrigger || isAnimating) return;

    // Get the current time using DSP time
    double currentTime = AudioManager.Instance.GetCurrentSongTime();

    // Ensure we are triggering only for upcoming beats
    int primeToAnimate = GetNextPrime();
    if (primeToAnimate == -1)
    {
        beatsShouldTrigger = false;
        StartCoroutine(ClosePopupWithDelay(1.25f));
        return;
    }

    // Debugging to check where we are in the song
    Debug.Log($"Current Time (DSP): {currentTime} | Next Prime: {primeToAnimate}");

    // Skip any primes that are already in the past
    while (primeToAnimate < currentTime)
    {
        Debug.Log($"Skipping past beat: {primeToAnimate} (Current DSP Time: {currentTime})");
        
        // Move to the next available beat
        primeToAnimate = GetNextPrime();

        // If no more primes exist, stop triggering beats
        if (primeToAnimate == -1)
        {
            beatsShouldTrigger = false;
            return;
        }
    }

    // Now that we've found a valid beat, add it to the queue
    if (!primeQueue.Contains(primeToAnimate))
        primeQueue.Enqueue(primeToAnimate);

    // Process the next prime in queue
    if (primeQueue.Count > 0)
    {
        int nextPrime = primeQueue.Dequeue();
        StartCoroutine(AnimatePrimeIntoBox(nextPrime));
    }
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

        RectTransform targetRect = numberObjects[primeValue].GetComponent<RectTransform>();
        if (targetRect == null) yield break;

        foreach (Transform child in spawnPoint) Destroy(child.gameObject);

        GameObject primeInstance = Instantiate(numberPrefab, spawnPoint);
        primeInstance.transform.SetParent(spawnPoint, false);
        RectTransform primeRect = primeInstance.GetComponent<RectTransform>();

        primeRect.anchoredPosition = Vector2.zero;
        primeRect.localScale = Vector3.one * 575;
        primeRect.localPosition += new Vector3(0, 0, -0.2f * primesPlaced);

        TextMeshPro primeText = primeInstance.GetComponentInChildren<TextMeshPro>();
        if (primeText != null)
        {
            primeText.text = primeValue.ToString();
            primeText.color = new Color(255 / 255f, 219 / 255f, 237 / 255f);
            
        }
        var shapeComponent = primeInstance.GetComponentInChildren<Shapes.Rectangle>();
            if (shapeComponent != null)
            {
                primeText.sortingOrder= 2;
                shapeComponent.SortingOrder = 1; 
            }


        primesPlaced++;
        yield return new WaitForSeconds(0.05f);

        Vector2 targetUIPosition = ConvertToUIPosition(targetRect);
        yield return primeRect.DOAnchorPos(targetUIPosition, flightDuration).SetEase(Ease.OutExpo).WaitForCompletion();

        TextMeshPro boxText = targetRect.GetComponentInChildren<TextMeshPro>();
        if (boxText != null)
        {
            boxText.text = primeValue.ToString();
            boxText.color = Color.yellow;
        }

        yield return primeInstance.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBounce).WaitForCompletion();
        Destroy(primeInstance);
         primeQueue = new Queue<int>(primeQueue.Where(x => x != primeValue));

        if (primesPlaced == 16) StartCoroutine(ShowPrimesFlashingSequence());
    }

    private IEnumerator ShowPrimesFlashingSequence()
    {
        primesText.gameObject.SetActive(true);
        primesText.text = "Primes 0-49";
        ShowPrimesText();
        primesText.DOKill();
        primesText.color = new Color(primesText.color.r, primesText.color.g, primesText.color.b, 1);
        yield return new WaitForSeconds(1f);
        refToSOEPopup.CloseSOEPopup();
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

        Sequence pressSequence = DOTween.Sequence();
        pressSequence.Append(pressSpaceText.DOFade(0f, 0.5f))
                     .Append(pressSpaceText.DOFade(1f, 0.5f))
                     .SetLoops(-1);

        waitingForSpace = true;

        while (waitingForSpace)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                waitingForSpace = false;
                isSOEActive = false;
            }
            yield return null;
        }

        memorySequence.Kill();
        pressSequence.Kill();
        HidePromptText();
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
        primesText.gameObject.SetActive(true);
        primesText.text = "Primes 0-49";

        // Kill any existing animations before applying a new one
        DOTween.Kill(primesText);

        // Start flashing effect
        Sequence flashSequence = DOTween.Sequence();
        flashSequence.Append(primesText.DOFade(0f, 0.5f))
                    .Append(primesText.DOFade(1f, 0.5f))
                    .SetLoops(-1);
    }
   public void ResetPrimes()
{
    primesPlaced = 0;
    beatsShouldTrigger = true;
    primeQueue.Clear(); 
    // Re-sort and reset primes so the sequence starts fresh
    List<int> orderedPrimes = new List<int>(primeFinder.primes);
    orderedPrimes.Sort();
}
}
