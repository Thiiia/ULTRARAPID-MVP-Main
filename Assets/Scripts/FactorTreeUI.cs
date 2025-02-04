using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Shapes;
using TMPro;

public class FactorTreeUI : MonoBehaviour
{
    public FactorBox rootFactorBox; // Root factor box (already in scene)
    public GameObject factorBoxPrefab; // Prefab for dynamically created factor boxes
    public Transform treeParent; // Parent transform for UI
    public float xSpacing = 100f; // Horizontal spacing
    public float ySpacing = -150f; // Vertical spacing
    public float lineThickness = 3f; // Line thickness for connections

    private Dictionary<int, FactorBox> factorBoxes = new Dictionary<int, FactorBox>();
    private List<int> primes;

    void Start()
    {
        primes = GeneratePrimesUpTo(200); // Adjust as needed
        factorBoxes[rootFactorBox.Value] = rootFactorBox;

        BuildFactorBranches(rootFactorBox, rootFactorBox.Value, 0);
    }

void BuildFactorBranches(FactorBox parent, int number, int depth)
{
    List<int> factors = GetFactorPairs(number);

    int numChildren = Mathf.Min(factors.Count - 1, 9); // Ensure at most 9 children
    float totalWidth = numChildren * xSpacing; // Total width of children
    float startX = parent.transform.localPosition.x - (totalWidth / 2f); // Center children

    for (int i = 0; i < numChildren; i++)
    {
        int factor = factors[i];
        if (factor == parent.Value) continue;

        // Position child boxes in a tree structure
        Vector3 newPos = new Vector3(
            startX + (i * xSpacing), // Spread evenly around parent
            parent.transform.localPosition.y + ySpacing, // Move downward
            parent.transform.localPosition.z - 1 // Maintain depth
        );

        GameObject obj = Instantiate(factorBoxPrefab, treeParent);
        obj.transform.SetParent(treeParent, false);
        FactorBox childBox = obj.GetComponent<FactorBox>();
        childBox.Initialize(factor, primes.Contains(factor), this);
        obj.GetComponent<RectTransform>().anchoredPosition = newPos;

        factorBoxes[factor] = childBox;

        DrawConnection(parent, childBox);
    }
}



 void DrawConnection(FactorBox parent, FactorBox child)
{
    GameObject lineObject = new GameObject("ConnectionLine");
    Line line = lineObject.AddComponent<Line>();
    line.transform.SetParent(treeParent);

    // Adjusted start and end positions for the tree structure
    Vector3 startPos = parent.transform.position;
    Vector3 endPos = child.transform.position;
    
    // Keep line from being absurdly long
    Vector3 midPoint = (startPos + endPos) / 2;
    midPoint.y -= Mathf.Abs(ySpacing / 2f); // Keep it positioned correctly

    line.Start = startPos;
    line.End = midPoint;

    line.Thickness = Mathf.Clamp(Vector3.Distance(startPos, endPos) / 10f, 0.002f, 0.01f);
    line.Color = Color.white;
}

    List<int> GetFactorPairs(int number)
    {
        List<int> factors = new List<int>();
        for (int i = 1; i <= number; i++)
        {
            if (number % i == 0) factors.Add(i);
        }
        return factors;
    }

    List<int> GeneratePrimesUpTo(int limit)
    {
        bool[] isPrime = new bool[limit + 1];
        for (int i = 2; i <= limit; i++) isPrime[i] = true;

        for (int p = 2; p * p <= limit; p++)
        {
            if (isPrime[p])
            {
                for (int i = p * p; i <= limit; i += p) isPrime[i] = false;
            }
        }

        List<int> primes = new List<int>();
        for (int i = 2; i <= limit; i++)
        {
            if (isPrime[i]) primes.Add(i);
        }
        return primes;
    }
    IEnumerator AnimateLine(Line line, Vector3 startPos, Vector3 endPos, float duration)
{
    float elapsed = 0;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        line.End = Vector3.Lerp(startPos, endPos, t);
        yield return null;
    }
    line.End = endPos;
}

}
