using UnityEngine;

public class ChildTrigger : MonoBehaviour
{
    public NoteBlockScript refToNoteBlock;
    public XplorerGuitarInput refToInput;

    public enum NoteColor { Green, Red, Yellow, Blue }
    public NoteColor assignedNoteColor;
    private bool noteHit = false;


    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Note"))
        {


            refToNoteBlock.OnChildTriggerEnter(gameObject, other);
          
           

        }
    }


    void OnTriggerExit(Collider other)
    {
        if (refToNoteBlock != null)
        {
            refToNoteBlock.OnChildTriggerExit(gameObject, other);
          
        }
    }

    private bool IsCorrectInputPressed()
    {
        switch (assignedNoteColor)
        {
            case NoteColor.Green:
                return refToInput.green;
            case NoteColor.Red:
                return refToInput.red;
            case NoteColor.Yellow:
                return refToInput.yellow;
            case NoteColor.Blue:
                return refToInput.blue;
            default:
                return false;
        }
    }
}
