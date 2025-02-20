using System.Collections.Generic;
using UnityEngine;
using Shapes;
using TMPro;
using DG.Tweening;

public class SOEGrid : MonoBehaviour
{
    public GameObject numberPrefab; // Prefab with Shape + TextMeshPro
    public Transform gridParent;
    public SOEPrimeFinder primeFinder; // Assign in Inspector
    public int gridSize = 120; // Max number
    private Dictionary<int, GameObject> numberObjects = new Dictionary<int, GameObject>();

    void Start()
    {
        GenerateGrid();
         ChartLoaderTest.OnBeat += PulseGrid; // Listen for beat events
    }

    void GenerateGrid()
    {
        int cols = 12; // Number of columns
        float spacing = 250f; 

        for (int i = 1; i <= gridSize; i++)
        {
            GameObject numObj = Instantiate(numberPrefab, gridParent);
            numObj.name = "Number_" + i;
            numObj.transform.localPosition = new Vector3((i - 1) % cols * spacing, -(i - 1) / cols * spacing, 0);

            TextMeshPro text = numObj.GetComponentInChildren<TextMeshPro>();
            text.text = i.ToString();

            numberObjects[i] = numObj;
        }
    }

    public void HighlightNumbers(HashSet<int> numbersToHighlight, Color highlightColor)
    {
        foreach (var num in numberObjects)
        {
            numberObjects[num.Key].GetComponent<ShapeRenderer>().Color = numbersToHighlight.Contains(num.Key) ? highlightColor : Color.white;
        }
    }
 public void PulseGrid()
{
    if (primeFinder == null)
    {
        Debug.LogError("primeFinder is not assigned! Make sure SOEGrid has a reference to it.");
        return;
    }

    HashSet<int> activeNumbers = primeFinder.primes; // Get primes (or multiples)

    foreach (var num in activeNumbers)
    {
        if (numberObjects.ContainsKey(num))
        {
            ShapeRenderer shape = numberObjects[num].GetComponent<ShapeRenderer>();
            if (shape != null)
            {
                Color originalColor = shape.Color; // Store original color

                DOTween.To(() => shape.Color, x => shape.Color = x, Color.cyan, 0.1f)
                    .SetLoops(2, LoopType.Yoyo)
                    .OnComplete(() => shape.Color = originalColor); // Restore original color
            }
        }
    }
}


}
