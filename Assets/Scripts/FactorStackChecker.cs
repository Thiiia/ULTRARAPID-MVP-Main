using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FactorStackChecker : MonoBehaviour
{
    public NoteBlockScript refToNoteblockScript;
    public TextMeshPro completedFactorText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        completedFactorText.text = refToNoteblockScript.completedFactorsRef.ToString();
        
    }
}
