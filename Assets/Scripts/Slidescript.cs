using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Slidescript : MonoBehaviour
{
    [SerializeField] private Image _fadeSlide; // The fade effect UI image
    [SerializeField] private Transform _slideParent; // Parent object for dynamically created slides
    [SerializeField] private GameObject _slidePrefab; // Prefab for slides (must have an Image component)

    private List<GameObject> _slides = new List<GameObject>();
    private float _fadeDuration = 0.75f;
    private float _switchDuration = 1.6f;
    private int _currentSlide = -1;

    private IEnumerator Start()
    {
        LoadSlidesFromResources(); // Load images from Resources folder

        if (_slides.Count == 0) yield break;

        _fadeSlide.color = Color.black;
        
        while (true)
        {
            _currentSlide = (_currentSlide + 1) % _slides.Count;
            StartCoroutine(SlideTransition());
            yield return new WaitForSeconds(_switchDuration);
        }
    }

    private void LoadSlidesFromResources()
    {
        Sprite[] images = Resources.LoadAll<Sprite>("Slides");

        foreach (Sprite img in images)
        {
            GameObject newSlide = Instantiate(_slidePrefab, _slideParent);
            newSlide.GetComponent<Image>().sprite = img;
            newSlide.SetActive(false); // Start as inactive
            _slides.Add(newSlide);
        }
    }

    private IEnumerator SlideTransition()
    {
        yield return StartCoroutine(FadeToTargetColor(Color.black));

        for (int i = 0; i < _slides.Count; i++)
        {
            _slides[i].SetActive(i == _currentSlide);
        }

        yield return StartCoroutine(FadeToTargetColor(Color.clear));
    }

    private IEnumerator FadeToTargetColor(Color targetColor)
    {
        float elapsedTime = 0.0f;
        Color startColor = _fadeSlide.color;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            _fadeSlide.color = Color.Lerp(startColor, targetColor, elapsedTime / _fadeDuration);
            yield return null;
        }
    }
}
