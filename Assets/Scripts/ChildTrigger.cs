using UnityEngine;

public class ChildTrigger : MonoBehaviour
{
    public NoteBlockScript refToNoteBlock; // Reference to Noteblock script

    void OnTriggerEnter(Collider other)
    {
        if (refToNoteBlock != null)
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
}
