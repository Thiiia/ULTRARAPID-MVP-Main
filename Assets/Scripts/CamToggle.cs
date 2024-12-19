using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamToggle : MonoBehaviour
{
    public GameObject Camera1Parent; // GameObject containing Camera1
    public GameObject Camera2Parent; // GameObject containing Camera2
    private GameObject activeCameraParent; // Currently active camera parent
    private bool isCenteredView = false; // Toggle state

    public void ToggleCamera()
    {
        if (isCenteredView)
        {
            // Switch back to dual-camera view
            Camera1Parent.SetActive(true);
            Camera2Parent.SetActive(true);

            if (activeCameraParent != null)
            {
                activeCameraParent.SetActive(false);
            }
        }
        else
        {
            // Determine which camera's parent to center
            if (activeCameraParent == Camera1Parent || activeCameraParent == null)
            {
                activeCameraParent = Camera1Parent;
            }
            else
            {
                activeCameraParent = Camera2Parent;
            }

            // Disable the other camera's parent and enable the active one
            Camera1Parent.SetActive(activeCameraParent == Camera1Parent);
            Camera2Parent.SetActive(activeCameraParent == Camera2Parent);

            // Center the active camera's parent
            activeCameraParent.transform.position = Vector3.zero; // Adjust to your desired center
            activeCameraParent.transform.rotation = Quaternion.identity; // Reset rotation if needed
        }

        // Toggle the state
        isCenteredView = !isCenteredView;
    }
}
