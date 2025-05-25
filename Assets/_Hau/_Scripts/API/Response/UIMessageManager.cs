using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMessageManager : MonoBehaviour
{
    public Text messageText;

    public void ShowMessage(string message, Color? color = null)
    {
        if (messageText == null) return;

        messageText.text = message;
        messageText.color = color ?? Color.white;
    }

    public void ShowSuccess(string message)
    {
        ShowMessage(message, Color.green);
    }

    public void ShowError(string message)
    {
        ShowMessage(message, Color.red);
    }

    public void Clear()
    {
        messageText.text = "";
    }
}
