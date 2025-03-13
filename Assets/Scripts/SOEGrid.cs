using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class SOEGrid : MonoBehaviour
{
    [Header("References")]
    public GameObject numberPrefab; // Prefab representing flying prime numbers
    public Transform spawnPoint; // The location where numbers start
    public Transform gridParent; // Parent object containing the factor boxes
    public SOEPrimeFinder primeFinder; // Prime finder reference

    [Header("UI Elements")]
    public TextMeshProUGUI memoryEncodingText; // "Memory Encoding"
    public TextMeshProUGUI pressSpaceText; // "Press Space Bar to Continue"
    public TextMeshProUGUI primesText;

    [Header("Timing")]
    public float flightDuration = 0.05f; //  movement time for prime animations
    private Dictionary<int, GameObject> numberObjects = new Dictionary<int, GameObject>(); // Tracks prime positions
    private Queue<int> primeQueue = new Queue<int>(); // Queue for primes
    private bool isAnimating = false;
    public bool firstTimePlaying;

    private int primesPlaced = 0; // Keeps track of how many primes have been placeds
    private bool waitingForSpace = false; // Controls when the player can proceed
    SOEPopup refToSOEPopup;
    public Color refToColor;

    public bool isSOEActive = false; // Track if SOE is currently running

    void Awake()
    {
       
    }
    private void Start()
    {
        GenerateGrid();
        gridParent.gameObject.SetActive(false);
        if (!ImageFlasher.hasRunOnce)
        {
            Invoke(nameof(WaitTillNoDeer), 6.1f);
            firstTimePlaying = false;
        }
        else
        {
            Invoke(nameof(WaitTillNoDeer), 0f);
        }



        refToSOEPopup = GameObject.Find("SOE_Popup").GetComponentInChildren<SOEPopup>();

        // Ensure spawnPoint is centered in the UI
        RectTransform spawnRect = spawnPoint.GetComponent<RectTransform>();
        if (spawnRect != null)
        {
            spawnRect.anchoredPosition = Vector2.zero; // Centers in the middle of the screen
        }

        ChartLoaderTest.OnBeat += OnBeatTriggered;
        ResetPrimes();
        StartCoroutine(ShowMemoryEncodingPrompt());

    }

    private void OnDestroy()
    {
        // Fuck a memory leak
        ChartLoaderTest.OnBeat -= OnBeatTriggered;
    }
    void WaitTillNoDeer()
    {
        gridParent.gameObject.SetActive(true);
    }

    /// <summary>
    /// Generates the SOE Prime Grid 
    /// </summary>
    /// <summary>
    /// Generates the SOE Prime Grid (numbers 0-49 only).
    /// </summary>
    public void GenerateGrid()
    {

        int cols = 10;
        float spacingX = 200f; // Horizontal spacing
        float spacingY = -155f; // Vertical spacing (negative moves downward)

        HashSet<int> primes = primeFinder.primes; // Get prime numbers

        for (int i = 0; i <= 49; i++) // 
        {
            GameObject numObj = Instantiate(numberPrefab, gridParent);
            numObj.name = "Number_" + i;

            // Calculate proper row & column positions
            int row = i / cols;   // Get the row number
            int col = i % cols;   // Get the column number

            numObj.transform.localPosition = new Vector3(col * spacingX, row * spacingY, 0);

            TextMeshPro text = numObj.GetComponentInChildren<TextMeshPro>();
            text.text = i.ToString(); // Initially, all numbers are displayed

            numberObjects[i] = numObj; // Store reference to each number slot

            // Gray out non-primes 
            if (!primes.Contains(i))
            {
                text.color = refToColor; // Dim non-prime numbers
            }
            else
            {
                text.text = ""; // Hide primes until they are animated in
            }


        }
    }
    public void StartSOESequence()
    {
        if (isSOEActive) return; // Avoid multiple triggers
        isSOEActive = true;
        if (firstTimePlaying)
        {
            firstTimePlaying = false;
            StartCoroutine(AnimatePrimesSequentially());

        }
        else
        {
            StartCoroutine(AnimatePrimesSequentially()); // Subsequent times play once
        }


        refToSOEPopup.ToggleSOEPopup(); // Open Popup
        ResetPrimes();
        StartCoroutine(ShowMemoryEncodingPrompt());
        StartCoroutine(ClosePopupWithDelay(12f));
    }


    /// <summary>
    /// Highlights prime numbers or multiples of a number (used in SOEUI buttons).
    /// </summary>
    public void HighlightNumbers(HashSet<int> numbersToHighlight, Color highlightColor)
    {
        foreach (var num in numberObjects)
        {
            if (numbersToHighlight.Contains(num.Key))
            {
                // Highlight the number
                num.Value.GetComponentInChildren<TextMeshPro>().color = highlightColor;
            }
            else
            {
                // Reset color to default
                num.Value.GetComponentInChildren<TextMeshPro>().color = Color.white;
            }
        }
    }

    /// <summary>
    /// Called on each beat. Moves the next prime into the table.
    /// </summary>
    private bool beatsShouldTrigger = true;

    private bool hasStartedSOE = false; // Prevents early triggering

    private void OnBeatTriggered()
    {
        if (!beatsShouldTrigger || isAnimating) return;

        int primeToAnimate = GetNextPrime();
        if (primeToAnimate == -1)
        {
            beatsShouldTrigger = false;
            StartCoroutine(ClosePopupWithDelay(1.25f));
            return;
        }

        if (!primeQueue.Contains(primeToAnimate))
        {
            primeQueue.Enqueue(primeToAnimate);
        }

        if (!isAnimating)
            isAnimating = true;
        StartCoroutine(AnimatePrimesSequentially());
    }



    private IEnumerator AnimatePrimesSequentially()
    {

        isAnimating = true;

        while (primeQueue.Count > 0)
        {
            int primeValue = primeQueue.Dequeue();
            yield return StartCoroutine(AnimatePrimeIntoBox(primeValue));
        }

        isAnimating = false;
    }

    /// <summary>
    /// Finds the next prime number that should be animated.
    /// Uses the order of primes to determine placement sequence.
    /// </summary>
    private int GetNextPrime()
    {
        List<int> orderedPrimes = new List<int>(primeFinder.primes);
        orderedPrimes.Sort();

        if (primesPlaced < orderedPrimes.Count)
        {
            int nextPrime = orderedPrimes[primesPlaced];
            if (primeQueue.Contains(nextPrime)) return -1;
            return nextPrime;
        }

        return -1;
    }


    private IEnumerator AnimatePrimeIntoBox(int primeValue)
    {
        if (!numberObjects.ContainsKey(primeValue))
        {
            Debug.LogError($"SOEGrid: No grid slot found for prime {primeValue} (0-49).");
            yield break;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("PrimeSpawnPoint is not assigned!");
            yield break;
        }

        // Get the target box
        RectTransform targetRect = numberObjects[primeValue].GetComponent<RectTransform>();
        if (targetRect == null)
        {
            Debug.LogError($"Target Box {primeValue} does not have a RectTransform!");
            yield break;
        }

        //  Destroy any lingering primes 
        foreach (Transform child in spawnPoint)
        {
            Destroy(child.gameObject);
        }

        // Spawn the new prime
        float zOffset = -0.2f * primesPlaced;

        GameObject primeInstance = Instantiate(numberPrefab, spawnPoint);
        primeInstance.transform.SetParent(spawnPoint, false);
        RectTransform primeRect = primeInstance.GetComponent<RectTransform>();


        primeRect.anchoredPosition = Vector2.zero; // Centered
        primeRect.localScale = Vector3.one * 575;
        Vector3 currentPosition = primeRect.localPosition;
        primeRect.localPosition = new Vector3(currentPosition.x, currentPosition.y, zOffset);

        
        TextMeshPro primeText = primeInstance.GetComponentInChildren<TextMeshPro>();
        if (primeText != null)
        {
            primeText.text = primeValue.ToString();
            primeText.color = new Color(255 / 255, 219 / 255, 237 / 255);
            primeText.gameObject.SetActive(true);
            primeText.alpha = 1f;
        }
        else
        {
            Debug.LogError($"TextMeshPro not found inside the spawned prime prefab!");
        }


        int nextPrime = GetNextPrime();
        GameObject nextPrimeInstance = null;
        if (nextPrime != -1)
        {
            nextPrimeInstance = Instantiate(numberPrefab, spawnPoint);
            nextPrimeInstance.transform.SetParent(spawnPoint, false);
            RectTransform nextPrimeRect = nextPrimeInstance.GetComponent<RectTransform>();

            if (nextPrimeRect != null)
            {
                nextPrimeRect.anchoredPosition = Vector2.zero;
                nextPrimeRect.localScale = Vector3.one * 575;
            }

            TextMeshPro nextText = nextPrimeInstance.GetComponentInChildren<TextMeshPro>();
            if (nextText != null)
            {
                nextText.text = nextPrime.ToString();
                nextText.color = Color.Lerp(Color.magenta, Color.blue, 0.95f);
                nextText.gameObject.SetActive(true);
                nextText.alpha = 1f;
            }
        }
        // Update primesPlaced count
        primesPlaced++;

        //   "stacking" effect
        yield return new WaitForSeconds(0.05f);

        Vector2 targetUIPosition = ConvertToUIPosition(targetRect);

        //  Move prime to the target position
        yield return primeRect.DOAnchorPos(targetUIPosition, flightDuration)
            .SetEase(Ease.OutExpo).WaitForCompletion();

        //  Immediate Box Update
        TextMeshPro boxText = targetRect.GetComponentInChildren<TextMeshPro>();
        if (boxText != null)
        {
            boxText.text = primeValue.ToString();
            boxText.color = Color.yellow; // Highlight
            boxText.gameObject.SetActive(true);
        }

        //  Shrink animation and destroy the prime after landing
        yield return primeInstance.transform.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBounce).WaitForCompletion();
        Destroy(primeInstance);



        if (primesPlaced == 16)
        {
            StartCoroutine(ShowPrimesFlashingSequence());
            yield break;
        }

        //  Start next prime's animation
        if (nextPrimeInstance != null)
        {
            yield return StartCoroutine(AnimatePrimeIntoBox(nextPrime));
        }
    }




    private IEnumerator ShowPrimesFlashingSequence()
    {
        Debug.Log("Starting SOE Flashing Sequence.");

        
        primesText.gameObject.SetActive(true);
        primesText.text = "Primes 0-49";

        ShowPrimesText();

        
        primesText.DOKill();
        primesText.color = new Color(primesText.color.r, primesText.color.g, primesText.color.b, 1);

        Debug.Log("SOE Flashing Sequence Completed.");

        //  Close Popup After Sequence Completes
        yield return new WaitForSeconds(1f);
        refToSOEPopup.CloseSOEPopup();
    }

    private void HideAllPrimes()
    {
        foreach (var num in numberObjects)
        {
            TextMeshPro text = num.Value.GetComponentInChildren<TextMeshPro>();

            if (primeFinder.primes.Contains(num.Key))
            {
                text.DOFade(0f, 1.0f); // Smooth fade out of prime numbers
            }
        }
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

        // Ensure all primes are placed before closing SOE

        Debug.Log("Closing SOE Popup after confirming all primes are placed.");
        refToSOEPopup.CloseSOEPopup();
        isSOEActive = false;
    }

    private IEnumerator ShowMemoryEncodingPrompt()
    {
        ShowPromptText();

        // Kill existing tweens before starting new ones
        DOTween.Kill(memoryEncodingText);
        DOTween.Kill(pressSpaceText);

        // Flash effect for "Memory Encoding"
        Sequence memorySequence = DOTween.Sequence();
        memorySequence.Append(memoryEncodingText.DOFade(0f, 0.5f))
                    .Append(memoryEncodingText.DOFade(1f, 0.5f))
                    .SetLoops(-1);

        // Flash effect for "Press Space"
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

        // Kill Tweens once Space is pressed
        memorySequence.Kill();
        pressSequence.Kill();
        DOTween.Kill(memoryEncodingText);
        DOTween.Kill(pressSpaceText);

        HidePromptText();
    }

    /// <summary>
    /// Shows the "Memory Encoding" and "Press Space to Continue" prompts.
    /// </summary>
    private void ShowPromptText()
    {
        memoryEncodingText.gameObject.SetActive(true);
        pressSpaceText.gameObject.SetActive(true);


    }


    /// <summary>
    /// Hides the "Memory Encoding" and "Press Space to Continue" prompts.
    /// </summary>
    private void HidePromptText()
    {
        memoryEncodingText.gameObject.SetActive(false);
        pressSpaceText.gameObject.SetActive(false);
    }

    public void ShowAllPrimes()
    {
        foreach (var num in numberObjects)
        {
            TextMeshPro text = num.Value.GetComponentInChildren<TextMeshPro>();

            if (primeFinder.primes.Contains(num.Key))
            {
                text.text = num.Key.ToString();
                text.color = Color.white;
            }
            else
            {
                // Fade out non-prime numbers over 1 second
                text.DOFade(0f, 1.0f);
            }
        }
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
        Debug.Log("Reset primes for re-animation.");
    }


}
