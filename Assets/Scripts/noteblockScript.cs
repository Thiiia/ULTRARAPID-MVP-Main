using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;

public class NoteBlockScript : MonoBehaviour
{
    public XplorerGuitarInput RefToInputController;
    public GameObject[] RefToNoteblocks;
    public enum NoteBlockType { GNoteblocks, DNoteblocks }
    public NoteBlockType BlockType;
    private List<Transform> activeNotes = new List<Transform>(); // Tracks Notes currently in the trigger
    private HashSet<Transform> notesInFlight = new HashSet<Transform>(); // Tracks notes currently flying to Factor UI
    private Dictionary<Transform, int> factorTreeMap = new Dictionary<Transform, int>();
    private Dictionary<Transform, int> factorTreeCounts = new Dictionary<Transform, int>();

    Color defaultGreenColor, defaultRedColor, defaultYellowColor, defaultBlueColor;
    Vector3 originalScaleGreen, originalScaleRed, originalScaleYellow, originalScaleBlue;
    Tween greenTween, redTween, yellowTween, blueTween;

    Color defaultDPadLeftColor, defaultDPadRightColor, defaultDPadUpColor, defaultDPadDownColor;
    Vector3 originalScaleDPadLeft, originalScaleDPadRight, originalScaleDPadUp, originalScaleDPadDown;
    Tween dpadLeftTween, dpadRightTween, dpadUpTween, dpadDownTween;

    [Header("Timing Thresholds")]
    public float missThreshold = 0.5f;    // Time deviation for a miss
    public float goodThreshold = 0.2f;   // Time deviation for a good hit
    public float perfectThreshold = 0.1f; // Time deviation for a perfect hit

    [Header("Feedback UI and Effects")]
    public TextMeshProUGUI feedbackTextUI;
    public GameObject perfectFeedbackPrefab;

    public Transform[] factorTreeElements;

    void Start()
    {
        // Caching factor tree UI elements
        foreach (var element in factorTreeElements)
        {
            TextMeshProUGUI textComponent = element.GetComponent<TextMeshProUGUI>();
            if (textComponent != null && int.TryParse(textComponent.text, out int factorValue))
            {
                factorTreeMap[element] = factorValue;
            }
        }

        foreach (var element in factorTreeElements)
        {
            // Initialise factor counts for each factor tree element
            factorTreeCounts[element] = 0; // Everyone starts at 0, no freebies here
        }

        // Initialising default colours and scales for Noteblocks
        if (this.gameObject.name == "GNoteblocks")
        {
            defaultGreenColor = RefToNoteblocks[0].GetComponent<SpriteRenderer>().color;
            defaultRedColor = RefToNoteblocks[1].GetComponent<SpriteRenderer>().color;
            defaultYellowColor = RefToNoteblocks[2].GetComponent<SpriteRenderer>().color;
            defaultBlueColor = RefToNoteblocks[3].GetComponent<SpriteRenderer>().color;

            originalScaleGreen = RefToNoteblocks[0].transform.localScale;
            originalScaleRed = RefToNoteblocks[1].transform.localScale;
            originalScaleYellow = RefToNoteblocks[2].transform.localScale;
            originalScaleBlue = RefToNoteblocks[3].transform.localScale;
        }

        if (this.gameObject.name == "DNoteblocks")
        {
            defaultDPadLeftColor = RefToNoteblocks[0].GetComponent<SpriteRenderer>().color;
            defaultDPadRightColor = RefToNoteblocks[1].GetComponent<SpriteRenderer>().color;
            defaultDPadUpColor = RefToNoteblocks[2].GetComponent<SpriteRenderer>().color;
            defaultDPadDownColor = RefToNoteblocks[3].GetComponent<SpriteRenderer>().color;

            originalScaleDPadLeft = RefToNoteblocks[0].transform.localScale;
            originalScaleDPadRight = RefToNoteblocks[1].transform.localScale;
            originalScaleDPadUp = RefToNoteblocks[2].transform.localScale;
            originalScaleDPadDown = RefToNoteblocks[3].transform.localScale;
        }
    }

    public void OnChildTriggerEnter(GameObject child, Collider other)
    {
        if (other.CompareTag("Note"))
        {
            // Add Note to activeNotes list if not already present
            Transform noteTransform = other.transform;
            if (!activeNotes.Contains(noteTransform))
            {
                activeNotes.Add(noteTransform);
            }
        }
    }

    public void OnChildTriggerExit(GameObject child, Collider other)
    {
        if (other.CompareTag("Note"))
        {
            Transform noteTransform = other.transform;
            // Skip destroying the note if it's in flight
            if (notesInFlight.Contains(noteTransform))
            {
                Debug.Log($"{noteTransform.name} is in flight, skipping destroy.");
                return;
            }

            // Remove and destroy the note
            activeNotes.Remove(noteTransform);
            KillTweens(noteTransform);
            Destroy(noteTransform.gameObject);
            Debug.Log($"Destroyed {noteTransform} on trigger exit.");
        }
    }

    void Update()
    {
        activeNotes.RemoveAll(note => note == null);

        // If this is the GNoteblocks GameObject, run thine code my good sir
        if (this.gameObject.name == "GNoteblocks")
        {
            // Handle interactions and animations based on input
            HandleColourAndAnimation(RefToInputController.green, RefToNoteblocks[0], ref greenTween, new Color(4f / 255f, 170f / 255f, 0f, 1f), defaultGreenColor, originalScaleGreen);
            HandleInteractions(RefToInputController.green, RefToNoteblocks[0]);

            HandleColourAndAnimation(RefToInputController.red, RefToNoteblocks[1], ref redTween, new Color(201f / 255f, 20f / 255f, 20f / 255f, 1f), defaultRedColor, originalScaleRed);
            HandleInteractions(RefToInputController.red, RefToNoteblocks[1]);

            HandleColourAndAnimation(RefToInputController.yellow, RefToNoteblocks[2], ref yellowTween, new Color(245f / 255f, 185f / 255f, 13f / 255f, 1f), defaultYellowColor, originalScaleYellow);
            HandleInteractions(RefToInputController.yellow, RefToNoteblocks[2]);

            HandleColourAndAnimation(RefToInputController.blue, RefToNoteblocks[3], ref blueTween, new Color(7f / 255f, 101f / 255f, 234f / 255f, 1f), defaultBlueColor, originalScaleBlue);
            HandleInteractions(RefToInputController.blue, RefToNoteblocks[3]);
        }

        // Likewise for DNoteblocks
        if (this.gameObject.name == "DNoteblocks")
        {
            HandleColourAndAnimation(RefToInputController.DPadLeft > 0, RefToNoteblocks[0], ref dpadLeftTween, new Color(4f / 255f, 170f / 255f, 0f, 1f), defaultDPadLeftColor, originalScaleDPadLeft);
            HandleInteractions(RefToInputController.DPadLeft > 0, RefToNoteblocks[0]);

            HandleColourAndAnimation(RefToInputController.DPadUp > 0, RefToNoteblocks[1], ref dpadRightTween, new Color(201f / 255f, 20f / 255f, 20f / 255f, 1f), defaultDPadRightColor, originalScaleDPadRight);
            HandleInteractions(RefToInputController.DPadUp > 0, RefToNoteblocks[1]);

            HandleColourAndAnimation(RefToInputController.DPadDown > 0, RefToNoteblocks[2], ref dpadUpTween, new Color(245f / 255f, 185f / 255f, 13f / 255f, 1f), defaultDPadUpColor, originalScaleDPadUp);
            HandleInteractions(RefToInputController.DPadDown > 0, RefToNoteblocks[2]);

            HandleColourAndAnimation(RefToInputController.DPadRight > 0, RefToNoteblocks[3], ref dpadDownTween, new Color(7f / 255f, 101f / 255f, 234f / 255f, 1f), defaultDPadDownColor, originalScaleDPadDown);
            HandleInteractions(RefToInputController.DPadRight > 0, RefToNoteblocks[3]);
        }
    }


    // Handle interactions
    void HandleInteractions(bool isActive, GameObject noteBlock)
    {
        if (isActive)
        {
            foreach (var note in activeNotes)
            {
                if (note == null || notesInFlight.Contains(note)) continue;

                if (noteBlock.GetComponent<Collider>().bounds.Intersects(note.GetComponent<Collider>().bounds))
                {
                    NoteFactorSpawner spawner = note.GetComponent<NoteFactorSpawner>();
                    if (spawner != null)
                    {
                        float timingDifference = Mathf.Abs(spawner.GetTimingDifference());

                        // Timing feedback
                        if (timingDifference <= perfectThreshold)
                        {
                            DisplayFeedback("Perfect");
                            Instantiate(perfectFeedbackPrefab, note.transform.position, Quaternion.identity);
                        }
                        else if (timingDifference <= goodThreshold)
                        {
                            DisplayFeedback("Good");
                        }
                        else if (timingDifference > missThreshold)
                        {
                            DisplayFeedback("Miss");
                        }

                        // Call factor logic
                        InteractWithNoteInTrigger(note.GetComponent<Collider>());
                    }
                }
            }
        }
    }

    // Define interactions with Notes currently inside the trigger
    void InteractWithNoteInTrigger(Collider note)
    {
        // Null check for note spawner
        NoteFactorSpawner spawner = note.GetComponent<NoteFactorSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("Note does not have a NoteFactorSpawner component!");
            return;
        }

        foreach (var pair in factorTreeMap)
        {
            if (pair.Value == spawner.assignedFactor) // Check for a match with the assigned factor
            {
                AnimateNumberFlying(note.gameObject, pair.Key); // Send the note flying
                factorTreeCounts[pair.Key]++;

                // Feedback for full factor tree element
                if (factorTreeCounts[pair.Key] >= 10)
                {
                    DisplayFeedback($"Factor {pair.Key.name} Full!");
                    Debug.Log($"Factor tree element {pair.Key.name} is full!");
                }

                return; // Stop checking once a match is found
            }
        }

        // No match feedback
        Debug.Log($"No match found for factor {spawner.assignedFactor}");
    }


    void AnimateNumberFlying(GameObject note, Transform targetElement)
    {
        notesInFlight.Add(note.transform);

        note.transform.DOMove(targetElement.position, 3f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            notesInFlight.Remove(note.transform);
            Destroy(note);
        });
    }

    // Handles the visual effects and scaling for button presses
    void HandleColourAndAnimation(bool isActive, GameObject noteBlock, ref Tween currentTween, Color targetColor, Color defaultColor, Vector3 originalScale)
    {
        SpriteRenderer sr = noteBlock.GetComponent<SpriteRenderer>();
        SpriteRenderer[] childSpriteRenderers = noteBlock.GetComponentsInChildren<SpriteRenderer>();

        if (isActive && sr.color != targetColor)
        {
            currentTween?.Kill();
            currentTween = DOTween.Sequence()
                .Append(noteBlock.transform.DOScale(originalScale * 1.2f, 0.1f).SetEase(Ease.OutBack))
                .Join(sr.DOColor(targetColor, 0.2f).SetEase(Ease.Linear))
                .Append(noteBlock.transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBounce));

            foreach (var childSr in childSpriteRenderers)
            {
                childSr.DOColor(targetColor, 0.2f).SetEase(Ease.Linear);
            }
        }
        else if (!isActive && sr.color != defaultColor)
        {
            currentTween?.Kill();
            currentTween = DOTween.Sequence()
                .Append(sr.DOColor(defaultColor, 0.2f).SetEase(Ease.Linear))
                .Join(noteBlock.transform.DOScale(originalScale, 0.1f));

            foreach (var childSr in childSpriteRenderers)
            {
                childSr.DOColor(defaultColor, 0.2f).SetEase(Ease.Linear);
            }
        }
    }
    void DisplayFeedback(string feedbackText)
    {
        if (feedbackTextUI != null)
        {
            feedbackTextUI.text = feedbackText; // Set the feedback text
            feedbackTextUI.DOFade(1f, 0.2f) // Fade in
                .OnComplete(() => feedbackTextUI.DOFade(0f, 0.5f)); // Fade out
        }
    }


    void KillTweens(Transform target)
    {
        if (target != null)
        {
            DOTween.Kill(target, true); // Kill only tweens associated with this Transform
        }
    }
}
