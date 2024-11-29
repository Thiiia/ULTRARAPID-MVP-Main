using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class NoteBlockScript : MonoBehaviour
{
    public XplorerGuitarInput RefToInputController;
    public GameObject[] RefToNoteblocks;

    Color defaultGreenColor, defaultRedColor, defaultYellowColor, defaultBlueColor;
    Tween greenTween, redTween, yellowTween, blueTween; 

    void Start()
    {
        // Initialize default colors ^^
        defaultGreenColor = RefToNoteblocks[0].GetComponent<SpriteRenderer>().color;
        defaultRedColor = RefToNoteblocks[1].GetComponent<SpriteRenderer>().color;
        defaultYellowColor = RefToNoteblocks[2].GetComponent<SpriteRenderer>().color;
        defaultBlueColor = RefToNoteblocks[3].GetComponent<SpriteRenderer>().color;
    }

    void Update()
    {
        HandleColorChange(RefToInputController.green, RefToNoteblocks[0], ref greenTween, new Color(4f / 255f, 170f / 255f, 0f, 1f), defaultGreenColor);
        HandleColorChange(RefToInputController.red, RefToNoteblocks[1], ref redTween, new Color(201f / 255f, 20f / 255f, 20f / 255f, 1f), defaultRedColor);
        HandleColorChange(RefToInputController.yellow, RefToNoteblocks[2], ref yellowTween, new Color(245f / 255f, 185f / 255f, 13 / 255f, 1f), defaultYellowColor);
        HandleColorChange(RefToInputController.blue, RefToNoteblocks[3], ref blueTween, new Color(7f / 255f, 101f / 255f, 234f / 255f, 1f), defaultBlueColor);
    }

    // handling color 
    void HandleColorChange(bool isActive, GameObject noteBlock, ref Tween currentTween, Color targetColor, Color defaultColor)
    {
        SpriteRenderer sr = noteBlock.GetComponent<SpriteRenderer>();

        // If input is active and not already animating to the target color
        if (isActive && sr.color != targetColor)
        {
            
            currentTween?.Kill();
            currentTween = sr.DOColor(targetColor, 0.45f);
        }
        // If input is inactive and not already animating back to the default color
        else if (!isActive && sr.color != defaultColor)
        {
            currentTween?.Kill();
            currentTween = sr.DOColor(defaultColor, 0.45f);
        }
    }
}
