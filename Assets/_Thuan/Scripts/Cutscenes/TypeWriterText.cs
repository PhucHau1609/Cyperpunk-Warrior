using System.Collections;
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
    private Coroutine typeCoroutine;

    [HideInInspector]
    public bool IsTyping { get; private set; }

    private void Awake()
    {
        textMesh = GetComponent<Text>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        typeCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        IsTyping = true;
        textMesh.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            textMesh.text += fullText[i];

            if (typeSound != null)
                audioSource.PlayOneShot(typeSound);

            yield return new WaitForSeconds(characterDelay);
        }

        IsTyping = false;
    }

    public void SkipText()
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        textMesh.text = fullText;
        IsTyping = false;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}
