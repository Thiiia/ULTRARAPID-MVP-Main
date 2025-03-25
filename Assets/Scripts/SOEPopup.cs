using UnityEngine;
using System.Collections;

public class SOEPopup : MonoBehaviour
{
    public GameObject soePopupPanel;
    private static bool firstTimeSession = true;
    private int repeatCount = 0;

    void Update()
    {
      
    }

    public void ToggleSOEPopup()
    {
        soePopupPanel.SetActive(true);
        repeatCount = 0;
        StartCoroutine(HandleSOEPopup());
    }

    private IEnumerator HandleSOEPopup()
    {
        int repeatLimit = firstTimeSession ? 2 : 1;

        for (int i = 0; i < repeatLimit; i++)
        {
            Debug.Log($"Running SOE Sequence {i + 1}/{repeatLimit}");
            yield return StartCoroutine(RunSOESequence());
        }

        firstTimeSession = false;
        CloseSOEPopup();
    }

    private IEnumerator RunSOESequence()
    {
        Debug.Log("Running prime animations!");
        SOEGrid grid = FindObjectOfType<SOEGrid>();

        if (grid != null)
        {
            grid.ResetPrimes();
        }

        yield return new WaitForSeconds(0);
    }

    public void CloseSOEPopup()
    {
        soePopupPanel.SetActive(false);
        Debug.Log("SOE Popup closed.");
    }
}

