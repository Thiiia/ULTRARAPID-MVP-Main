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
    private List<Collider> activeNotes = new List<Collider>(); // Tracks Notes currently in the trigger
    private HashSet<GameObject> notesInFlight = new HashSet<GameObject>(); // Tracks notes currently flying to Factor UI

    Color defaultGreenColor, defaultRedColor, defaultYellowColor, defaultBlueColor;
    Vector3 originalScaleGreen, originalScaleRed, originalScaleYellow, originalScaleBlue; // Store original scales
    Tween greenTween, redTween, yellowTween, blueTween;

    Color defaultDPadLeftColor, defaultDPadRightColor, defaultDPadUpColor, defaultDPadDownColor;
    Vector3 originalScaleDPadLeft, originalScaleDPadRight, originalScaleDPadUp, originalScaleDPadDown; // Store original scales
    Tween dpadLeftTween, dpadRightTween, dpadUpTween, dpadDownTween;
   



    public Transform[] factorTreeElements;

    void Start()
    {
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
            // Add Note to activeNotes list when it enters the trigger
            if (!activeNotes.Contains(other))
            {
                activeNotes.Add(other);
            }
        }
    }

public void OnChildTriggerExit(GameObject child, Collider other)
{
    if (other.CompareTag("Note"))
    {
        // Skip destroying the note if it's in flight
        if (notesInFlight.Contains(other.gameObject))
        {
            Debug.Log($"{other.gameObject.name} is in flight, skipping destroy.");
            return;
        }

        // Otherwise, clean up and destroy the note
        activeNotes.Remove(other);
        DOTween.Kill(other.transform);
        Destroy(other.gameObject);
        Debug.Log($"Destroyed {other.gameObject.name} on trigger exit.");
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
    // New function to handle interactions
    void HandleInteractions(bool isActive, GameObject noteBlock)
{
    if (isActive)
    {
        foreach (var note in activeNotes)
        {
            if (note == null) continue; // Skip null references

            // Ensure the note is interacting with this specific block
            if (noteBlock.GetComponent<Collider>().bounds.Intersects(note.bounds))
            {
                InteractWithNoteInTrigger(note);
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

        //If Assigned factor matches any number in the factor tree :)
        for (int i = 0; i < factorTreeElements.Length; i++)
        {
            TextMeshProUGUI factorText = factorTreeElements[i].GetComponent<TextMeshProUGUI>();
            if (factorText != null && int.TryParse(factorText.text, out int factorValue) && factorValue == spawner.assignedFactor)
            {
                // "fly" 
                AnimateNumberFlying(note.gameObject, factorTreeElements[i]);
                // visual feedback
                AnimateFactorTreeFeedback(factorTreeElements[i]);
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
    notesInFlight.Add(note);

    // Animate the movement of the note towards the target
    note.transform.DOMove(endPos, duration).SetEase(Ease.InOutQuad)
        .OnComplete(() =>
        {
            // Safely destroy the object after the animation completes
            notesInFlight.Remove(note); // Remove from in-flight tracking
            DOTween.Kill(note.transform); // Kill any remaining tweens
            Destroy(note); // Destroy the note
        });

    // scale down the note as it flies
    note.transform.DOScale(Vector3.zero, duration).SetEase(Ease.InOutQuad);
}

    void AnimateFactorTreeFeedback(Transform factorTreeElement)
    {
        TextMeshProUGUI textComponent = factorTreeElement.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            // Save original color
            Color originalColor = textComponent.color;

            // Flash the text color
            textComponent.DOColor(Color.yellow, 0.2f)
                .OnComplete(() =>
                {
                    // Revert back to the original color
                    textComponent.DOColor(originalColor, 0.2f);
                });

            //  bounce effect
            factorTreeElement.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    factorTreeElement.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBounce);
                });
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
}
