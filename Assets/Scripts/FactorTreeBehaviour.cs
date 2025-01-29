using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class FactorTreeBehaviour : MonoBehaviour
{
    public GameObject factorBoxPrefab;  // Prefab for the factor boxes
    public Transform rootBox;          // The root box created in the inspector
    public float verticalSpacing = 2f; // Vertical spacing between levels
    public float horizontalSpacing = 1.5f; // Horizontal spacing between boxes

    private int currentLevel = 0;      // Current level of the factor tree
    private bool isLevelComplete = false;

    void Update()
    {
        if (isLevelComplete)
        {
            SpawnNextLevel();
            isLevelComplete = false;
        }
    }

    public void CompleteLevel()
    {
        isLevelComplete = true;
    }

    private void SpawnNextLevel()
    {
        // Get factors of the root or current level
        int parentFactor = rootBox.GetComponent<FactorBox>().factorValue;
        int[] factors = GetFactors(parentFactor);

        // Spawn boxes for the next level
        for (int i = 0; i < factors.Length; i++)
        {
            Vector3 position = new Vector3(
                rootBox.position.x + (i - factors.Length / 2f) * horizontalSpacing, 
                rootBox.position.y - verticalSpacing, 
                rootBox.position.z
            );

            GameObject newBox = Instantiate(factorBoxPrefab, position, Quaternion.identity);
            FactorBox factorBox = newBox.GetComponent<FactorBox>();
            factorBox.factorValue = factors[i];
            factorBox.parentBox = rootBox;

            // Add to tree hierarchy (optional for later reference)
            newBox.transform.parent = this.transform;
        }

        // Move the rootBox reference to the newly spawned boxes
        currentLevel++;
    }

    private int[] GetFactors(int number)
    {
        // Generate all factors of a number
        if (number <= 0) return new int[0];

        var factors = new System.Collections.Generic.List<int>();
        for (int i = 1; i <= number; i++)
        {
            if (number % i == 0) factors.Add(i);
        }
        return factors.ToArray();
    }
}
