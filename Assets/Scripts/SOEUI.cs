using UnityEngine;
using UnityEngine.UI;

public class SOEUI : MonoBehaviour
{
    public SOEGrid soeGrid;
    public SOEPrimeFinder primeFinder;
    public Button primeButton, multiplesOf2Button, multiplesOf3Button;

    void Start()
    {
        primeButton.onClick.AddListener(() => soeGrid.HighlightNumbers(primeFinder.primes, Color.yellow));
        multiplesOf2Button.onClick.AddListener(() => soeGrid.HighlightNumbers(primeFinder.GetMultiples(2), Color.green));
        multiplesOf3Button.onClick.AddListener(() => soeGrid.HighlightNumbers(primeFinder.GetMultiples(3), Color.blue));
    }
}
