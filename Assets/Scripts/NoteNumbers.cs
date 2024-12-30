using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoteFactorSpawner : MonoBehaviour
{
    [Header("Number to Generate Factors From")]
    public int specifiedNumber;

    [Header("Assigned Factor")]
    public int assignedFactor;

    private List<int> factors = new List<int>();

    [Header("TextMeshPro Component")]
    public TextMeshPro factorText; // Reference to the TextMeshPro (3D) component

    [Header("Timing Settings")]
    [Tooltip("The expected time for this note to be hit.")]
    public float expectedHitTime; // The time this note should be hit (e.g., provided by your chart system)

    private float spawnTime; // When the note is spawned

    void Start()
    {
        spawnTime = Time.time; // Record the time the note is spawned
        GenerateFactors(specifiedNumber);
        AssignRandomFactor();
    }

    // Generate all factors of the specified number
    private void GenerateFactors(int number)
    {
        factors.Clear();

        if (number <= 0)
        {
            Debug.LogError("The specified number must be greater than 0.");
            return;
        }

        for (int i = 1; i <= number; i++)
        {
            if (number % i == 0)
            {
                factors.Add(i);
            }
        }
    }

    // Assign a random factor from the list to this note
    private void AssignRandomFactor()
    {
        if (factors.Count > 0)
        {
            assignedFactor = factors[Random.Range(0, factors.Count)];
            DisplayFactorOnNote();
        }
        else
        {
            Debug.LogError("No factors generated. Check the specified number.");
        }
    }

    // Display the assigned factor on the note using TextMeshPro (3D)
    private void DisplayFactorOnNote()
    {
        if (factorText != null)
        {
            factorText.text = assignedFactor.ToString();
        }
        else
        {
            Debug.LogError("TextMeshPro (3D) component is not assigned!");
        }
    }

    // Calculate the timing difference (for timing feedback like Miss, Good, Perfect)
    public float GetTimingDifference()
    {
        float currentTime = Time.time;
        return currentTime - (spawnTime + expectedHitTime);
    }
}
