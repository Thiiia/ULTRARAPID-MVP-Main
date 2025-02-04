using UnityEngine;
using Shapes;
using DG.Tweening;
using TMPro;

public class FactorBox : MonoBehaviour
{
    public int Value; // The number inside the factor box
    public TextMeshProUGUI textComponent; // Reference to text inside the box
    public Rectangle boxShape; // Reference to Shapes Rectangle
    public Disc glowEffect; // Reference to glow effect for primes
    public Color defaultColor = Color.white;
    public Color primeColor = Color.yellow;
    public Color filledColor = Color.green;
    public FactorTreeUI treeManager;

    private bool isFilled = false;

   public void Initialize(int value, bool isPrime, FactorTreeUI manager)
{
    Value = value;
    textComponent.text = value.ToString();
    treeManager = manager; // Store reference to tree system

    // Set initial color
    boxShape.Color = defaultColor;

    // If it's a prime number, add glow effect
    glowEffect.enabled = isPrime;
    if (isPrime)
    {
        glowEffect.Color = primeColor;
        glowEffect.Radius = 0.25f;
    }

    // Start hidden and animate in
    transform.localScale = Vector3.zero;
    transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
}
   public void FillBox()
{
    if (isFilled) return;
    isFilled = true;

    // Animate color transition manually
    DOTween.To(() => boxShape.Color, x => boxShape.Color = x, filledColor, 0.3f);

    // Scale effect for visual feedback
    transform.DOScale(Vector3.one * 1.1f, 0.2f).SetLoops(2, LoopType.Yoyo);
}
}
