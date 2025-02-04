using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Slidescript : MonoBehaviour
{
    [SerializeField] private Image _fadeSlide;
    [SerializeField] private Transform _slideParent;
    [SerializeField] private GameObject _slidePrefab;
    private Button skipButton;

    private List<GameObject> _slides = new List<GameObject>();
    private float _fadeDuration = 0.75f;
    private float _switchDuration = 1.6f;
    private int _currentSlide = -1;

    private string nextSceneName = "MainGameplayScene"; 

    private IEnumerator Start()
    {
        LoadSlidesFromResources();

        if (_slides.Count == 0) yield break;

        _fadeSlide.color = Color.black;

        //click to skip the slideshow
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipToNextScene);
        }

        while (_currentSlide < _slides.Count - 1) // Loop until last slide
        {
            _currentSlide++;
            StartCoroutine(SlideTransition());
            yield return new WaitForSeconds(_switchDuration);
        }

        // Load the next scene after the last slide
        yield return new WaitForSeconds(0.1f); // Short delay
        LoadNextScene();
    }

    private void LoadSlidesFromResources()
    {
        Sprite[] images = Resources.LoadAll<Sprite>("Slides");

        foreach (Sprite img in images)
        {
            GameObject newSlide = Instantiate(_slidePrefab, _slideParent);
            newSlide.GetComponent<Image>().sprite = img;
            newSlide.SetActive(false);
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

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void SkipToNextScene()
    {
        StopAllCoroutines(); // Stop slideshow
        LoadNextScene();
    }
}
