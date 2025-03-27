using System;
using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class TutorialDirector : MonoBehaviour
{
    public static TutorialDirector Instance;

    [Header("UI Key Prompts")]
    public TextMeshProUGUI[] keyPrompts; // A/S/D/F TMPs

    [Header("World Noteblocks")]
    public GameObject[] noteblocks; // Match A/S/D/F 

    [Header("Pulse Settings")]
    public float fadeInTime = 0.2f;
    public float holdTime = 0.8f;
    public float fadeOutTime = 0.3f;
    public float punchScale = 1.3f;
    public float punchDuration = 0.4f;

    public Color glowColor = Color.white;
    public float glowFadeTime = 0.4f;

    public Action OnTutorialComplete;

    private readonly string[] keyLabels = { "A", "S", "D", "F" };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartCinematic()
    {
        Debug.Log("▶ TutorialDirector: Starting cinematic...");
        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        for (int i = 0; i < keyPrompts.Length; i++)
        {
            yield return ShowKeyPromptAndGlow(i, keyLabels[i]);
        }

        yield return new WaitForSeconds(1f);
        EndTutorial();
    }

    private IEnumerator ShowKeyPromptAndGlow(int index, string label)
    {
        // UI Text
        TextMeshProUGUI prompt = keyPrompts[index];
        if (prompt != null)
        {
            prompt.text = label;
            prompt.alpha = 0f;
            prompt.transform.localScale = Vector3.one;

            prompt.DOFade(1f, fadeInTime);
            prompt.transform.DOPunchScale(Vector3.one * (punchScale - 1f), punchDuration, 6, 0.8f);
            prompt.DOFade(0f, fadeOutTime).SetDelay(holdTime);
        }

        // Noteblock Visuals
        if (noteblocks != null && index < noteblocks.Length && noteblocks[index] != null)
        {
            HighlightNoteblock(noteblocks[index]);
        }

        yield return new WaitForSeconds(holdTime + fadeOutTime);
    }

    private void HighlightNoteblock(GameObject block)
    {
        SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Color originalColor = sr.color;

        // Animate color flash
        sr.DOColor(glowColor, 0.15f).OnComplete(() =>
            sr.DOColor(originalColor, glowFadeTime)
        );

        // Animate scale punch
        block.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 6, 0.8f);
    }

    private void EndTutorial()
    {
        Debug.Log(" TutorialDirector: Finished tutorial sequence.");
        OnTutorialComplete?.Invoke();
    }
}
