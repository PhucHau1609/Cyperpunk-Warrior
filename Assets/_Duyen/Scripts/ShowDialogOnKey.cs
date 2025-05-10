using UnityEngine;

public class ShowDialogOnKey : MonoBehaviour
{
    public GameObject dialogObject;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (dialogObject != null)
            {
                bool isActive = dialogObject.activeSelf;
                dialogObject.SetActive(!isActive); // Toggle hiện/tắt
            }
        }
    }
}
