using UnityEngine;

public class TypeWriterTextManager : MonoBehaviour
{
    public TypeWriterText[] typeWriterTexts;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SkipAllTexts();
        }
    }

    private void SkipAllTexts()
    {
        foreach (var writer in typeWriterTexts)
        {
            if (writer != null && writer.IsTyping)
            {
                writer.SkipText();
            }
        }
    }
}
