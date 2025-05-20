using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TypeWriterText : MonoBehaviour
{
    [Header("Text Settings")]
    [TextArea]
    public string fullText;
    public float characterDelay = 0.05f;
    public AudioClip typeSound;

    private Text textMesh;
    private AudioSource audioSource;

    private void Awake()
    {
        textMesh = GetComponent<Text>();

        // Tạo AudioSource nếu chưa có
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        textMesh.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            textMesh.text += fullText[i];

            if (typeSound != null)
                audioSource.PlayOneShot(typeSound);

            yield return new WaitForSeconds(characterDelay);
        }
    }
}
