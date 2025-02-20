using UnityEngine;

public class SOEPopup : MonoBehaviour
{
    public GameObject soePopupPanel;

    public void ToggleSOEPopup()
    {
        soePopupPanel.SetActive(!soePopupPanel.activeSelf);
    }
}
