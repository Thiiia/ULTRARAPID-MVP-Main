using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

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

    [Header("Timing")]
    public float flightDuration = 0.05f; // 50ms movement time for prime animations
    private Dictionary<int, GameObject> numberObjects = new Dictionary<int, GameObject>(); // Tracks prime positions

    private int primesPlaced = 0; // Keeps track of how many primes have been placed
    private int totalNotes = 21; // The total number of notes triggering prime animations
    private bool waitingForSpace = false; // Controls when the player can proceed
    SOEPopup refToSOEPopup;

    private void Start()
    {
        GenerateGrid();
        HidePromptText();

        refToSOEPopup = GameObject.Find("SOE_Popup").GetComponentInChildren<SOEPopup>();
        ChartLoaderTest.OnBeat += OnBeatTriggered;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        ChartLoaderTest.OnBeat -= OnBeatTriggered;
    }

    /// <summary>
    /// Generates the SOE Prime Grid 
    /// Ensures correct column & row positioning based on the sieve.
    /// </summary>
    /// <summary>
    /// Generates the SOE Prime Grid (numbers 0-49 only).
    /// Ensures correct column & row positioning.
    /// </summary>
    private void GenerateGrid()
    {
        int cols = 10;
        float spacingX = 200f; // Horizontal spacing
        float spacingY = -155f; // Vertical spacing (negative moves downward)

        HashSet<int> primes = primeFinder.primes; // Get prime numbers

        for (int i = 0; i <= 49; i++) // E
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

            // Gray out non-primes (optional: make them semi-transparent)
            if (!primes.Contains(i))
            {
                text.color = new Color(1f, 1f, 1f, 0.5f); // Dim non-prime numbers
            }
            else
            {
                text.text = ""; // Hide primes until they are animated in
            }


        }
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

    private void OnBeatTriggered()
    {
        if (!beatsShouldTrigger) return;

        Debug.Log($"OnBeatTriggered() called! primesPlaced = {primesPlaced}, totalNotes = {totalNotes}");

        if (primesPlaced >= totalNotes)
        {
            Debug.LogWarning("All primes have been placed. Stopping beat triggers.");
            beatsShouldTrigger = false;
            refToSOEPopup.CloseSOEPopup();
            return;
        }

        int primeToAnimate = GetNextPrime();
        if (primeToAnimate == -1)
        {
            Debug.LogError("No more primes to animate.");
            beatsShouldTrigger = false;
            refToSOEPopup.CloseSOEPopup();
            return;
        }

        Debug.Log($"Beat Triggered! Moving Prime: {primeToAnimate}");
        AnimatePrimeIntoBox(primeToAnimate);
    }



    /// <summary>
    /// Finds the next prime number that should be animated.
    /// Uses the order of primes to determine placement sequence.
    /// </summary>
    private int GetNextPrime()
    {
        List<int> orderedPrimes = new List<int>(primeFinder.primes);
        orderedPrimes.Sort(); // Ensure primes appear in order

        if (primesPlaced < orderedPrimes.Count)
        {
            return orderedPrimes[primesPlaced]; // Get next prime in sequence
        }

        if (primesPlaced == totalNotes) // 
        {
            return -1; // No more primes available
        }

        Debug.LogWarning("Beats are still triggering after primes are placed.");
        return -1;
    }



    /// <summary>
    /// Animates a prime number from the front of the screen into its grid box.
    /// </summary>
    /// <summary>
    /// Animates a prime number from the front of the screen into its grid box.
    /// </summary>
    private void AnimatePrimeIntoBox(int primeValue)
    {
        if (!numberObjects.ContainsKey(primeValue))
        {
            Debug.LogError($"SOEGrid: No grid slot found for prime {primeValue} (0-49).");
            return;
        }

        // Kill existing tweens on this object before applying new ones
        DOTween.Kill(numberObjects[primeValue].transform);

        // Spawn prime at the front of the screen
        GameObject primeInstance = Instantiate(numberPrefab, spawnPoint.parent);
        primeInstance.transform.position = spawnPoint.position;

        // Target position is the corresponding prime box
        Transform targetBox = numberObjects[primeValue].transform;

        // Animate flying effect
        primeInstance.transform.DOMove(targetBox.position, flightDuration)
            .SetEase(Ease.OutExpo)
            .OnComplete(() =>
            {
                // Ensure the number appears in the right place
                TextMeshPro text = targetBox.GetComponentInChildren<TextMeshPro>();
                if (text != null)
                {
                    text.text = primeValue.ToString();
                    text.color = Color.yellow; // Highlight the prime (optional)
                    text.gameObject.SetActive(true); // Make sure the number is visible
                }

                //Scale effect on landing
                targetBox.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBounce).OnComplete(() =>
                {
                    targetBox.DOScale(Vector3.one, 0.1f);
                });

                // Destroy the moving prime after landing
                if (primeInstance != null && primeInstance.scene.IsValid())
                {
                    Destroy(primeInstance); // ✅ Destroys only instantiated objects
                }
                else
                {
                    Debug.LogWarning($"Tried to destroy {primeValue} but it wasn't instantiated.");
                }


                // Update primesPlaced to move to the next prime
                primesPlaced++;
                Debug.Log($"✅ Prime {primeValue} placed. primesPlaced = {primesPlaced}");

                // Check if all primes have been placed and close the popup
                if (primesPlaced >= totalNotes)
                {


                    refToSOEPopup.CloseSOEPopup();
                }
                if (primesPlaced == 15)
                {
                    ShowAllPrimes(); // Show all primes in the grid
                    ShowPrimesText(); // Show "Primes 0-49"
                }
            });
    }


    private IEnumerator ShowMemoryEncodingPrompt()
    {
        ShowPromptText();

        // ✅ Kill existing tweens before starting a new one
        DOTween.Kill(memoryEncodingText);
        DOTween.Kill(pressSpaceText);

        // Create a flashing effect for "Memory Encoding"
        Sequence memorySequence = DOTween.Sequence();
        memorySequence.Append(memoryEncodingText.DOFade(0f, 0.5f))
                      .Append(memoryEncodingText.DOFade(1f, 0.5f))
                      .SetLoops(-1);

        // Create a flashing effect for "Press Space"
        Sequence pressSequence = DOTween.Sequence();
        pressSequence.Append(pressSpaceText.DOFade(0f, 0.5f))
                     .Append(pressSpaceText.DOFade(1f, 0.5f))
                     .SetLoops(-1);

        waitingForSpace = true;

        // Wait for the player to press Space Bar
        while (waitingForSpace)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                waitingForSpace = false;
            }
            yield return null;
        }

        // Kill Tweens to prevent stacking
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
            if (primeFinder.primes.Contains(num.Key))
            {
                num.Value.GetComponentInChildren<TextMeshPro>().text = num.Key.ToString();
                num.Value.GetComponentInChildren<TextMeshPro>().color = Color.white;
            }
        }
    }
    public void ShowPrimesText()
    {
        pressSpaceText.gameObject.SetActive(true);
        pressSpaceText.text = "Primes 0-49";
    }
    public void ResetPrimes()
    {
        primesPlaced = 0;
        beatsShouldTrigger = true;
        Debug.Log("Reset primes for re-animation.");
    }


}
