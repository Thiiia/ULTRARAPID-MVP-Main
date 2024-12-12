using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class NoteBlockScript : MonoBehaviour
{
    public XplorerGuitarInput RefToInputController;
    public GameObject[] RefToNoteblocks;

    Color defaultGreenColor, defaultRedColor, defaultYellowColor, defaultBlueColor;
    Vector3 originalScaleGreen, originalScaleRed, originalScaleYellow, originalScaleBlue; // Store original scales
    Tween greenTween, redTween, yellowTween, blueTween;

    Color defaultDPadLeftColor, defaultDPadRightColor, defaultDPadUpColor, defaultDPadDownColor;
    Vector3 originalScaleDPadLeft, originalScaleDPadRight, originalScaleDPadUp, originalScaleDPadDown; // Store original scales
    Tween dpadLeftTween, dpadRightTween, dpadUpTween, dpadDownTween;

    public enum NoteBlockType { GNoteblocks, DNoteblocks }
    public NoteBlockType BlockType;

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
    Debug.Log($"Child {child.name} picked up trigger with {other.gameObject.name}"); //debugging
}

    void Update()
    {
        // if this is the GNoteblocks gameobject, run thine code my good sir
        if (this.gameObject.name == "GNoteblocks")
        {
            HandleColorAndAnimation(RefToInputController.green, RefToNoteblocks[0], ref greenTween, new Color(4f / 255f, 170f / 255f, 0f, 1f), defaultGreenColor, originalScaleGreen);
            HandleColorAndAnimation(RefToInputController.red, RefToNoteblocks[1], ref redTween, new Color(201f / 255f, 20f / 255f, 20f / 255f, 1f), defaultRedColor, originalScaleRed);
            HandleColorAndAnimation(RefToInputController.yellow, RefToNoteblocks[2], ref yellowTween, new Color(245f / 255f, 185f / 255f, 13f / 255f, 1f), defaultYellowColor, originalScaleYellow);
            HandleColorAndAnimation(RefToInputController.blue, RefToNoteblocks[3], ref blueTween, new Color(7f / 255f, 101f / 255f, 234f / 255f, 1f), defaultBlueColor, originalScaleBlue);
        }

        // Likewise for DNoteblocks
        if (this.gameObject.name == "DNoteblocks")
        {
            HandleColorAndAnimation(RefToInputController.DPadLeft > 0, RefToNoteblocks[0], ref dpadLeftTween, new Color(4f / 255f, 170f / 255f, 0f, 1f), defaultDPadLeftColor, originalScaleDPadLeft);
            HandleColorAndAnimation(RefToInputController.DPadUp > 0, RefToNoteblocks[1], ref dpadRightTween, new Color(201f / 255f, 20f / 255f, 20f / 255f, 1f), defaultDPadRightColor, originalScaleDPadRight);
            HandleColorAndAnimation(RefToInputController.DPadDown > 0, RefToNoteblocks[2], ref dpadUpTween, new Color(245f / 255f, 185f / 255f, 13f / 255f, 1f), defaultDPadUpColor, originalScaleDPadUp);
            HandleColorAndAnimation(RefToInputController.DPadRight > 0, RefToNoteblocks[3], ref dpadDownTween, new Color(7f / 255f, 101f / 255f, 234f / 255f, 1f), defaultDPadDownColor, originalScaleDPadDown);
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
