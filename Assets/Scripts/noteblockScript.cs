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
    private HashSet<Transform> notesInFlight = new HashSet<Transform>();// Tracks notes currently flying to Factor UI
    private Dictionary<Transform, int> factorTreeMap = new Dictionary<Transform, int>();
    private Dictionary<Transform, int> factorTreeCounts = new Dictionary<Transform, int>();


    Color defaultGreenColor, defaultRedColor, defaultYellowColor, defaultBlueColor;
    Vector3 originalScaleGreen, originalScaleRed, originalScaleYellow, originalScaleBlue; // Store original scales
    Tween greenTween, redTween, yellowTween, blueTween;

    Color defaultDPadLeftColor, defaultDPadRightColor, defaultDPadUpColor, defaultDPadDownColor;
    Vector3 originalScaleDPadLeft, originalScaleDPadRight, originalScaleDPadUp, originalScaleDPadDown; // Store original scales
    Tween dpadLeftTween, dpadRightTween, dpadUpTween, dpadDownTween;




    public Transform[] factorTreeElements;

    void Start()
    {
        //Cacheing or caching, idk factor tree UI 
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
            // Initialize factor counts for each factor tree element
            factorTreeCounts[element] = 0;
        }


        // Initializing default colors and original scales for Noteblocks
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
        // if this is the GNoteblocks gameobject, run thine code my good sir
        if (this.gameObject.name == "GNoteblocks")
        {
            // Handle interactions and animations based on input
            HandleColorAndAnimation(RefToInputController.green, RefToNoteblocks[0], ref greenTween, new Color(4f / 255f, 170f / 255f, 0f, 1f), defaultGreenColor, originalScaleGreen);
            HandleInteractions(RefToInputController.green, RefToNoteblocks[0]);

            HandleColorAndAnimation(RefToInputController.red, RefToNoteblocks[1], ref redTween, new Color(201f / 255f, 20f / 255f, 20f / 255f, 1f), defaultRedColor, originalScaleRed);
            HandleInteractions(RefToInputController.red, RefToNoteblocks[1]);

            HandleColorAndAnimation(RefToInputController.yellow, RefToNoteblocks[2], ref yellowTween, new Color(245f / 255f, 185f / 255f, 13f / 255f, 1f), defaultYellowColor, originalScaleYellow);
            HandleInteractions(RefToInputController.yellow, RefToNoteblocks[2]);

            HandleColorAndAnimation(RefToInputController.blue, RefToNoteblocks[3], ref blueTween, new Color(7f / 255f, 101f / 255f, 234f / 255f, 1f), defaultBlueColor, originalScaleBlue);
            HandleInteractions(RefToInputController.blue, RefToNoteblocks[3]);
        }

        // Likewise for DNoteblocks
        if (this.gameObject.name == "DNoteblocks")
        {
            HandleColorAndAnimation(RefToInputController.DPadLeft > 0, RefToNoteblocks[0], ref dpadLeftTween, new Color(4f / 255f, 170f / 255f, 0f, 1f), defaultDPadLeftColor, originalScaleDPadLeft);
            HandleInteractions(RefToInputController.DPadLeft > 0, RefToNoteblocks[0]);

            HandleColorAndAnimation(RefToInputController.DPadUp > 0, RefToNoteblocks[1], ref dpadRightTween, new Color(201f / 255f, 20f / 255f, 20f / 255f, 1f), defaultDPadRightColor, originalScaleDPadRight);
            HandleInteractions(RefToInputController.DPadUp > 0, RefToNoteblocks[1]);

            HandleColorAndAnimation(RefToInputController.DPadDown > 0, RefToNoteblocks[2], ref dpadUpTween, new Color(245f / 255f, 185f / 255f, 13f / 255f, 1f), defaultDPadUpColor, originalScaleDPadUp);
            HandleInteractions(RefToInputController.DPadDown > 0, RefToNoteblocks[2]);

            HandleColorAndAnimation(RefToInputController.DPadRight > 0, RefToNoteblocks[3], ref dpadDownTween, new Color(7f / 255f, 101f / 255f, 234f / 255f, 1f), defaultDPadDownColor, originalScaleDPadDown);
            HandleInteractions(RefToInputController.DPadRight > 0, RefToNoteblocks[3]);
        }
    }
    //  handle interactions
    void HandleInteractions(bool isActive, GameObject noteBlock)
    {
        if (isActive)
        {
            foreach (var note in activeNotes)
            {
                if (note == null || notesInFlight.Contains(note)) continue; // Skip null or flying notes

                if (noteBlock.GetComponent<Collider>().bounds.Intersects(note.GetComponent<Collider>().bounds))
                {
                    InteractWithNoteInTrigger(note.GetComponent<Collider>());
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

    // Determine the Noteblock name dynamically by resolving its parent
    // Assuming notes are children of Noteblocks, get the parent Noteblock
    Transform triggeringNoteblock = note.transform.parent; 
    string noteBlockName = triggeringNoteblock?.name; // Get the name of the triggering Noteblock

    // Ensure the Noteblock name is valid
    if (string.IsNullOrEmpty(noteBlockName))
    {
        Debug.LogWarning("Could not determine Noteblock name!");
        return;
    }

    Debug.Log($"Resolved Noteblock Name: {noteBlockName}");

    // If Assigned factor matches any number in the factor tree :)
    foreach (var pair in factorTreeMap)
    {
        if (pair.Value == spawner.assignedFactor) // Check for a match with the assigned factor
        {
            AnimateNumberFlying(note.gameObject, pair.Key); // Fly baby fly
            AnimateFactorTreeFeedback(pair.Key, noteBlockName); // Some woomf

            // Increment the count for this factor tree element
            if (factorTreeCounts.ContainsKey(pair.Key))
            {
                factorTreeCounts[pair.Key]++;
            }
            else
            {
                factorTreeCounts[pair.Key] = 1;
            }

            Debug.Log($"FactorTree Count for {pair.Key.name}: {factorTreeCounts[pair.Key]}");

            // Check if this factor tree element is now full *NOT WORKING
            if (factorTreeCounts[pair.Key] >= 3)
            {
                // GLOW BABY GLOW - get color of the triggering Noteblock
                Color noteBlockColor = GetNoteBlockColorByNoteblockName(noteBlockName);
                TriggerGlowEffect(pair.Key, noteBlockColor);
            }

            return; // Stop checking once a match is found
        }
    }

    Debug.Log($"No match found for factor {spawner.assignedFactor}");
}

    void AnimateNumberFlying(GameObject note, Transform targetElement)
    {
        Vector3 startPos = note.transform.position;
        Vector3 endPos = targetElement.position;

        float duration = 4f; // Adjust duration for animation speed

        // Add the note to the in-flight set
        notesInFlight.Add(note.transform);

        // Animate the movement of the note towards the target
        note.transform.DOMove(endPos, duration).SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                // Safely destroy the object after the animation completes
                notesInFlight.Remove(note.transform); // Remove from in-flight tracking
                KillTweens(note.transform); // Kill any remaining tweens
                Destroy(note); // Destroy the note
            });

        // scale down the note as it flies
        note.transform.DOScale(Vector3.zero, duration).SetEase(Ease.InOutQuad);
    }

// Animates temporary feedback for the factor tree element, such as flashing and bouncing
void AnimateFactorTreeFeedback(Transform factorTreeElement, string noteBlockName)
{
    TextMeshProUGUI textComponent = factorTreeElement.GetComponent<TextMeshProUGUI>();
    if (textComponent != null)
    {
        Color originalColor = textComponent.color; // Save original color
        DOTween.Kill(textComponent); // Ensure no overlapping animations

        // Sequence to handle color flashing and scaling
        Sequence feedbackSequence = DOTween.Sequence();

        // Flash the text color temporarily
        feedbackSequence.Append(textComponent.DOColor(Color.yellow, 0.2f)) // Highlight
            .Append(textComponent.DOColor(originalColor, 0.2f))           // Revert to original
            .SetEase(Ease.Linear);

        // Bounce effect to visually emphasize the interaction
        feedbackSequence.Join(factorTreeElement.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack))
            .Append(factorTreeElement.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBounce));

        // Avoid persistent color changes here; let TriggerGlowEffect handle it when full
    }
}



  // Triggers the glow effect for the factor tree element when full
void TriggerGlowEffect(Transform factorTreeElement, Color glowColor)
{
    TextMeshProUGUI textComponent = factorTreeElement.GetComponent<TextMeshProUGUI>();
    if (textComponent != null)
    {
        // Save the original color for potential reset (if needed elsewhere)
        Color originalColor = textComponent.color;

        // Glow effect
        textComponent.DOColor(glowColor, 0.5f).SetLoops(6, LoopType.Yoyo)
            .OnComplete(() =>
            {
                // Ensure the final color is the Noteblock color
                textComponent.color = glowColor; // Keep the Noteblock color after glow ends
            });

        // Optional: Add a scaling effect to emphasize the glow
        factorTreeElement.DOScale(Vector3.one * 1.5f, 0.5f).SetEase(Ease.OutQuad)
            .OnComplete(() => factorTreeElement.DOScale(Vector3.one, 0.5f).SetEase(Ease.InQuad));
    }
}

  
// Determine the color of the triggering Noteblock 
Color GetNoteBlockColorByNoteblockName(string noteBlockName)
{
    switch (noteBlockName)
    {
        case "Noteblock Green":
            return new Color(0.016f, 0.667f, 0.0f, 1f); // Green
        case "Noteblock Red":
            return new Color(0.788f, 0.078f, 0.078f, 1f); // Red
        case "Noteblock Yellow":
            return new Color(0.961f, 0.725f, 0.051f, 1f); // Yellow
        case "Noteblock Blue":
            return new Color(0.027f, 0.396f, 0.918f, 1f); // Blue
        default:
            return Color.white; // Default fallback color
    }
}






    // handling color and animation to enhance satisfaction for button presses
    void HandleColorAndAnimation(bool isActive, GameObject noteBlock, ref Tween currentTween, Color targetColor, Color defaultColor, Vector3 originalScale)
    {
        SpriteRenderer sr = noteBlock.GetComponent<SpriteRenderer>();

        // If input is active and not already animating to the target color
        if (isActive && sr.color != targetColor)
        {
            currentTween?.Kill(); // Kill any existing tween to avoid stacking
            currentTween = DOTween.Sequence()
                .Append(noteBlock.transform.DOScale(originalScale * 1.2f, 0.1f).SetEase(Ease.OutBack))  // Scale up with a nice ease-out effect for impact
                .Join(sr.DOColor(targetColor, 0.2f).SetEase(Ease.Linear))  // Change color to the target color
                .Append(noteBlock.transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBounce));  // Return to original scale with a bounce effect
        }
        // If input is inactive and not already animating back to the default color
        else if (!isActive && sr.color != defaultColor)
        {
            currentTween?.Kill(); // Kill any existing tween to avoid stacking
            currentTween = DOTween.Sequence()
                .Append(sr.DOColor(defaultColor, 0.2f).SetEase(Ease.Linear))  // Return to default color
                .Join(noteBlock.transform.DOScale(originalScale, 0.1f));  // Ensure scale is reset smoothly
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
