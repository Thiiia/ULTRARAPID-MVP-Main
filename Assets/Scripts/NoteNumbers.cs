using System.Collections.Generic;
using TMPro; // Import the TextMeshPro namespace
using UnityEngine;

public class NoteFactorSpawner : MonoBehaviour
{
    [Header("Number to Generate Factors From")]
    [Tooltip("Specify the number for which random factors will be generated.")]
    public int specifiedNumber;

    [Header("Assigned Factor")]
    [Tooltip("The randomly assigned factor for this note.")]
    public int assignedFactor;

    private List<int> factors = new List<int>();

    [Header("TextMeshPro Component")]
    [Tooltip("The TextMeshPro (3D) component to display the assigned factor.")]
    public TextMeshPro factorText; // Reference to the TextMeshPro (3D) component

    void Start()
    {
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
                factors.Add(i); // Add the factor to the list
            }
        }
    }

    // Assign a random factor from the list to this note
    private void AssignRandomFactor()
    {
        if (factors.Count > 0)
        {
            assignedFactor = factors[Random.Range(0, factors.Count)];

            // Display the factor visually
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
            factorText.text = assignedFactor.ToString(); // Set the text to the assigned factor
        }
        else
        {
            Debug.LogError("TextMeshPro (3D) component is not assigned!");
        }
    }
}
