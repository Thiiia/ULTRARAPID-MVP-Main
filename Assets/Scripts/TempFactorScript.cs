using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class TempFactorScript : MonoBehaviour
{
    public GameObject[] RefToFactorBlocks;
    public GameObject RootFactorBlock;

    private Dictionary<GameObject, bool> factorCompletion = new Dictionary<GameObject, bool>();
    void Start()
    {
        // Hide all factor blocks except root
        foreach (var block in RefToFactorBlocks)
        {
            block.SetActive(false);
            factorCompletion[block] = false; // Mark as incomplete
        }
        RootFactorBlock.SetActive(true);
        factorCompletion[RootFactorBlock] = true; // Root is always completed
        EnableFactorBlock(0);
        EnableFactorBlock(5);
        
    }

    public void UnlockNextFactors(GameObject completedFactor)
    {
        Debug.Log($"UnlockNextFactors called for: {completedFactor.name}");

        // Mark the completed factor
        if (factorCompletion.ContainsKey(completedFactor))
        {
            factorCompletion[completedFactor] = true;
            Debug.Log($"{completedFactor.name} is now marked as completed.");
        }
        else
        {
            Debug.LogWarning($"Factor {completedFactor.name} not found in dictionary.");
            return;
        }

        // Check if 120 is completed
        if (completedFactor == RootFactorBlock)
        {
            Debug.Log("Factor 120 is completed! Enabling Factor 0 and Factor 5...");
            EnableFactorBlock(0);
            EnableFactorBlock(5);
        }

        // Check if 0 and 5 are both completed, unlock 1, 2, 3, 4
        if (IsFactorCompleted(0) && IsFactorCompleted(5))
        {
            Debug.Log("Factors 0 and 5 are completed! Enabling 1, 2, 3, 4...");
            EnableFactorBlock(1);
            EnableFactorBlock(2);
            EnableFactorBlock(3);
            EnableFactorBlock(4);
        }

        // Check if 1, 2, 3, and 4 are completed, unlock 6 and 7
        if (IsFactorCompleted(1) && IsFactorCompleted(2) &&
            IsFactorCompleted(3) && IsFactorCompleted(4))
        {
            Debug.Log("Factors 1, 2, 3, and 4 are completed! Enabling 6 and 7...");
            EnableFactorBlock(6);
            EnableFactorBlock(7);
        }
    }

    // Helper function to check if a factor is completed
    private bool IsFactorCompleted(int index)
    {
        if (index < 0 || index >= RefToFactorBlocks.Length) return false;
        return factorCompletion.ContainsKey(RefToFactorBlocks[index]) && factorCompletion[RefToFactorBlocks[index]];
    }

    // Helper function to enable a factor block
  private void EnableFactorBlock(int index)
{
    if (index < 0 || index >= RefToFactorBlocks.Length)
    {
        Debug.LogError($"Index {index} is out of bounds in RefToFactorBlocks array.");
        return;
    }

    GameObject factorBlock = RefToFactorBlocks[index];
    factorBlock.SetActive(true);
    Debug.Log($"Enabled {factorBlock.name}");

    // Update the text inside the factor block
    TextMeshProUGUI factorText = factorBlock.GetComponentInChildren<TextMeshProUGUI>();
    if (factorText != null)
    {
        factorText.alpha = 1f; // Make sure it's visible
        factorText.DOFade(1f, 0.5f).SetEase(Ease.InOutQuad);
        Debug.Log($"Updated text for {factorBlock.name}");
    }
}


}
