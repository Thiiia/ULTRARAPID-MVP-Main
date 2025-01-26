using System.Collections;
using System;
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
    private double dspNoteHitTime;
    // Tracks notes currently flying to Factor UI
    private Dictionary<Transform, int> factorTreeMap = new Dictionary<Transform, int>();
    private Dictionary<Transform, int> factorTreeCounts = new Dictionary<Transform, int>();

    Color defaultGreenColor, defaultRedColor, defaultYellowColor, defaultBlueColor;
    Vector3 originalScaleGreen, originalScaleRed, originalScaleYellow, originalScaleBlue;
    Tween greenTween, redTween, yellowTween, blueTween;

    Color defaultDPadLeftColor, defaultDPadRightColor, defaultDPadUpColor, defaultDPadDownColor;
    Vector3 originalScaleDPadLeft, originalScaleDPadRight, originalScaleDPadUp, originalScaleDPadDown;
    Tween dpadLeftTween, dpadRightTween, dpadUpTween, dpadDownTween;

    public GameObject UINoteGreenPrefab, UINoteRedPrefab, UINoteYellowPrefab, UINoteBluePrefab;
    public Camera GMainCamera;
    public Camera DMainCamera;
    private AudioSource audioSource;

    [Header("Timing Thresholds")]
    public float missThreshold = 0.5f;    // Time deviation for a miss
    public float goodThreshold = 0.2f;   // Time deviation for a good hit
    public float perfectThreshold = 0.1f; // Time deviation for a perfect hit
    public double expectedHitTime; // Expected hit time in seconds, assigned when spawning notes


    [Header("Feedback UI and Effects")]
    public TextMeshProUGUI feedbackTextUI;
    public GameObject perfectFeedbackPrefab;

    public Transform[] factorTreeElements;
    public GameObject fullIndicatorPrefab; // When la factor tree is full

    void Start()
    {
        audioSource = FindObjectOfType<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource not found in the scene.");
        }
        // Caching factor tree UI elements
        foreach (var element in factorTreeElements)
        {
            TextMeshProUGUI textComponent = element.GetComponent<TextMeshProUGUI>();
            if (textComponent != null && int.TryParse(textComponent.text, out int factorValue))
            {
                // Keep a specific node always visible
                if (element.name == "ChosenFactor" || element == factorTreeElements[0]) // Adjust condition as needed
                {
                    textComponent.alpha = 1f; // Make this node visible
                }
                else
                {
                    textComponent.alpha = 0f; // Hide other nodes initially
                }

                factorTreeMap[element] = factorValue; // Store the factor value
            }
        }

        foreach (var element in factorTreeElements)
        {
            // Initialize factor counts for each factor tree element
            factorTreeCounts[element] = 0; // Everyone starts at 0
        }

        // Initializing default colors and scales for Noteblocks
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

        /* 
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
        */
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

            // Remove the note from the active list
            activeNotes.Remove(noteTransform);

            // Clean up the note
            KillTweens(noteTransform); // Stop any ongoing animations or tweens
            Destroy(noteTransform.gameObject); // Destroy the world-space note
            Debug.Log($"Destroyed {noteTransform.name} on trigger exit.");
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
        /*
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
        */
    }

    public void CheckHit()
    {
        if (activeNotes.Count == 0) return;

        Transform note = activeNotes[0];
        NoteFactorSpawner spawner = note.GetComponent<NoteFactorSpawner>();
        if (spawner == null) return;

        double currentDspTime = AudioSettings.dspTime;
        float triggerWidth = GetComponentInChildren<BoxCollider>().bounds.size.z;
        ChartLoaderTest chartLoader = FindObjectOfType<ChartLoaderTest>();

        if (chartLoader != null)
        {
            float timingDifference = Mathf.Abs(spawner.GetTimingDifference());

            // Determine the hit type based on timing thresholds
            if (timingDifference <= perfectThreshold)
            {
                DisplayFeedback("Perfect");
                InteractWithNoteInTrigger(note.GetComponent<Collider>());
            }
            else if (timingDifference <= goodThreshold)
            {
                DisplayFeedback("Good");
                InteractWithNoteInTrigger(note.GetComponent<Collider>());
            }
            else
            {
                DisplayFeedback("Miss");
            }

            activeNotes.Remove(note);
            Destroy(note.gameObject);
        }
    }


    // Handle interactions
    void HandleInteractions(bool isActive, GameObject noteBlock)
    {
        if (isActive && activeNotes.Count > 0)
        {
            Transform firstNote = activeNotes[0];
            NoteFactorSpawner spawner = firstNote.GetComponent<NoteFactorSpawner>();

            if (spawner == null)
            {
                Debug.LogWarning("No NoteFactorSpawner found on the note!");
                return;
            }

            if (noteBlock.GetComponent<Collider>().bounds.Intersects(firstNote.GetComponent<Collider>().bounds))
            {
                CheckHit();


            }
        }
    }



    // Define interactions with Notes currently inside the trigger
    public void InteractWithNoteInTrigger(Collider note)
    {
        NoteFactorSpawner spawner = note.GetComponent<NoteFactorSpawner>();
        if (spawner == null)
        {
            Debug.LogWarning("Note does not have a NoteFactorSpawner component!");
            return;
        }

        foreach (var pair in factorTreeMap)
        {
            // Match the factor value and ensure the node isn't already full
            if (pair.Value == spawner.assignedFactor && factorTreeCounts[pair.Key] < 7)
            {
                // Dynamically find the correct camera based on BlockType
                Camera activeCamera = (BlockType == NoteBlockType.GNoteblocks)
                    ? GameObject.Find("GMain Camera").GetComponent<Camera>() // For Guitar notes
                    : GameObject.Find("DMain Camera").GetComponent<Camera>(); // For Drum notes

                if (activeCamera == null)
                {
                    Debug.LogError("Could not find the appropriate camera! Ensure GMainCamera and DMainCamera are named correctly.");
                    return;
                }

                // Send the note flying with the correct camera
                AnimateNumberFlying(note.gameObject, pair.Key, activeCamera);

                // Update the tree node text and visibility
                UpdateFactorTreeNode(pair.Key);

                // Increment factor tree count
                factorTreeCounts[pair.Key]++;
                if (factorTreeCounts[pair.Key] >= 7)
                {
                    TriggerNodeFullFeedback(pair.Key);
                    TextMeshProUGUI textComponent = pair.Key.GetComponent<TextMeshProUGUI>();
                    string factorValueText = textComponent != null ? textComponent.text : "Unknown";

                    DisplayFeedback($"Factor {factorValueText} Full!");

                    Transform existingIndicator = pair.Key.Find("FullIndicator");
                    if (existingIndicator == null && fullIndicatorPrefab != null)
                    {
                        // Instantiate the full indicator 
                        GameObject fullIndicator = Instantiate(fullIndicatorPrefab, pair.Key);
                        fullIndicator.name = "FullIndicator";

                        // Position the image above the factor tree element
                        RectTransform indicatorRect = fullIndicator.GetComponent<RectTransform>();
                        if (indicatorRect != null)
                        {
                            indicatorRect.anchoredPosition = new Vector2(0, 65); // Y offset
                        }
                    }
                }

                return; // Stop checking once a match is found
            }
        }

        // If no match is found
        Debug.Log($"No match found for factor {spawner.assignedFactor}");
        DisplayFeedback("Wrong Factor");
    }

    void AnimateNumberFlying(GameObject worldNote, Transform targetElement, Camera currentCamera)
    {
        // Reference the parent canvas (must be in Screen Space - Overlay)
        Canvas parentCanvas = feedbackTextUI.GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("Parent Canvas not found. Ensure feedbackTextUI is part of a Canvas.");
            return;
        }

        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();

        // Determine the UI note prefab based on the world note's color
        GameObject uiNotePrefab = null;
        if (worldNote.name.Contains("Green"))
            uiNotePrefab = UINoteGreenPrefab;
        else if (worldNote.name.Contains("Red"))
            uiNotePrefab = UINoteRedPrefab;
        else if (worldNote.name.Contains("Yellow"))
            uiNotePrefab = UINoteYellowPrefab;
        else if (worldNote.name.Contains("Blue"))
            uiNotePrefab = UINoteBluePrefab;

        if (uiNotePrefab == null)
        {
            Debug.LogError($"No matching UI prefab for note: {worldNote.name}");
            return;
        }

        // Instantiate the UI note in the overlay canvas
        GameObject uiNote = Instantiate(uiNotePrefab, parentCanvas.transform);
        RectTransform uiNoteRect = uiNote.GetComponent<RectTransform>();

        // Convert world note position to screen space relative to the current camera
        Vector3 screenPosition = currentCamera.WorldToScreenPoint(worldNote.transform.position);
        if (uiNoteRect != null)
        {
            // Ensure the note is visible in front of the camera
            if (screenPosition.z <= 0)
            {
                Debug.LogWarning($"World note {worldNote.name} is out of view or behind the camera.");
                Destroy(uiNote);
                return;
            }

            // Convert screen space to canvas space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                parentCanvas.worldCamera,
                out Vector2 uiStartPosition
            );

            if (uiNoteRect != null)
            {
                // Set the UI note's starting position
                uiNoteRect.anchoredPosition = uiStartPosition;
            }



            // Copy the number from the world note to the UI note
            TextMeshPro worldText = worldNote.GetComponentInChildren<TextMeshPro>();
            TextMeshProUGUI uiText = uiNote.GetComponentInChildren<TextMeshProUGUI>();
            if (worldText != null && uiText != null)
            {
                uiText.text = worldText.text;
            }

            // Convert the target element's position to screen space relative to the same camera
            Vector3 targetScreenPosition = currentCamera.WorldToScreenPoint(targetElement.position);

            // Convert target screen space to canvas space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                targetScreenPosition,
                parentCanvas.worldCamera,
                out Vector2 uiTargetPosition
            );

            // Animate the UI note to the target position
            float animationDuration = 0.8f; // Adjust for speed
            if (uiNoteRect != null)
            {
                uiNoteRect.DORotate(new Vector3(0, 0, 20), animationDuration, RotateMode.FastBeyond360);
                uiNoteRect.DOAnchorPos(uiTargetPosition, animationDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    if (uiNoteRect != null)
                    {
                        uiNoteRect.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBounce);
                    }

                    // Destroy the UI note after it reaches the target
                    Destroy(uiNote);
                    Debug.Log($"UI note {uiNote.name} successfully reached target: {targetElement.name}");
                });
            }

        }


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
            .Append(noteBlock.transform.DOScale(originalScale * 1.8f, 1f).SetEase(Ease.OutElastic)) // Bigger pop effect with elasticity
            .Join(sr.DOColor(targetColor, 0.2f).SetEase(Ease.Linear)) // Slightly longer color transition
            .AppendInterval(2f)  // Hold the effect for a moment
            .Append(noteBlock.transform.DOScale(originalScale * 1.4f, 1f).SetEase(Ease.InOutQuad)) // Settling effect
            .AppendInterval(1f)  // Hold again before the final bounce
            .Append(noteBlock.transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutBounce)); // More bouncy return

        foreach (var childSr in childSpriteRenderers)
        {
            childSr.DOColor(targetColor, 0.4f).SetEase(Ease.Linear);
        }
    }
    else if (!isActive && sr.color != defaultColor)
    {
        currentTween?.Kill();
        currentTween = DOTween.Sequence()
            .Append(sr.DOColor(defaultColor, 0.2f).SetEase(Ease.Linear))
            .Join(noteBlock.transform.DOScale(originalScale, 0.1f))
            .AppendInterval(0.3f);  // Hold the default state before resetting

        foreach (var childSr in childSpriteRenderers)
        {
            childSr.DOColor(defaultColor, 0.2f).SetEase(Ease.Linear);
        }
    }
}
    void UpdateFactorTreeNode(Transform node)
    {
        TextMeshProUGUI nodeText = node.GetComponent<TextMeshProUGUI>();
        if (nodeText != null && nodeText.alpha == 0f) // Only update if hidden
        {
            nodeText.alpha = 0f; // Ensure it's hidden initially
            nodeText.text = factorTreeMap[node].ToString(); // Set the text
            nodeText.DOFade(1f, 0.5f).SetEase(Ease.InOutQuad); // Fade in animation
        }
    }


    void TriggerNodeFullFeedback(Transform node)
    {
        TextMeshProUGUI nodeText = node.GetComponent<TextMeshProUGUI>();
        if (nodeText != null && nodeText.alpha == 1f) // Only proceed if the text is visible
        {
            // Highlight the node with a glow effect
            GameObject glowEffect = Instantiate(fullIndicatorPrefab, node);
            glowEffect.name = "FullIndicator";

            RectTransform indicatorRect = glowEffect.GetComponent<RectTransform>();
            if (indicatorRect != null)
            {
                indicatorRect.anchoredPosition = new Vector2(0, 65); // Position above the node
            }

            // Play glow animation
            glowEffect.transform.localScale = Vector3.zero; // Start small
            glowEffect.transform.DOScale(Vector3.one * 1.5f, 0.5f).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                glowEffect.transform.DOScale(Vector3.one, 0.3f); // Settle to normal size
            });

            // Change text color to indicate completion
            nodeText.DOColor(Color.yellow, 0.5f).SetLoops(2, LoopType.Yoyo);
        }
    }

    void DisplayFeedback(string feedbackType)
    {
        if (feedbackTextUI != null)
        {
            // Kill any existing tweens to prevent overlapping animations
            DOTween.Kill(feedbackTextUI.transform);
            DOTween.Kill(feedbackTextUI);

            // Reset the scale to default size before applying animations
            feedbackTextUI.transform.localScale = Vector3.one;

            // Set the feedback text and apply corresponding color effect
            feedbackTextUI.text = feedbackType;
            switch (feedbackType)
            {
                case "Perfect":
                    feedbackTextUI.color = Color.green;
                    break;
                case "Good":
                    feedbackTextUI.color = Color.yellow;
                    break;
                case "Miss":
                    feedbackTextUI.color = Color.red;
                    break;
                default:
                    feedbackTextUI.color = Color.white;
                    break;
            }

            // Animate: Fade in, pulse effect, shake, then fade out
            feedbackTextUI.DOFade(1f, 0.3f).SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    feedbackTextUI.transform
                        .DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutQuad) // Pulse effect
                        .OnComplete(() =>
                        {
                            feedbackTextUI.transform.DOShakeScale(0.3f, 0.5f, 15, 90, false, ShakeRandomnessMode.Harmonic); // Shake effect
                        });

                    feedbackTextUI.DOFade(0f, 0.4f).SetEase(Ease.OutCubic); // Fade out
                });

            // Reset scale after animation completes
            feedbackTextUI.transform.DOScale(Vector3.one, 0.2f).SetDelay(0.8f).SetEase(Ease.OutQuad);
        }
        else
        {
            Debug.LogWarning($"Feedback type '{feedbackType}' triggered, but feedbackTextUI is not assigned.");
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
