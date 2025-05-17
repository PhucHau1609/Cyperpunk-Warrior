using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public GameObject dialogueBox;
    public Text dialogueText;

    private string[] lines;
    private int currentLine;
    private bool isTyping;
    private Coroutine typingCoroutine;

    public float typeSpeed = 0.05f;

    public DialogueFollowNPC followScript;

    public System.Action onDialogueEnd;

    public bool IsDialogueActive => dialogueBox.activeSelf;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (dialogueBox.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                ShowFullLine();
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(string[] dialogueLines)
    {
        lines = dialogueLines;
        currentLine = 0;
        dialogueBox.SetActive(true);
        typingCoroutine = StartCoroutine(TypeLine(lines[currentLine]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        if (currentLine >= lines.Length - 1)
        {
            yield return new WaitForSeconds(.5f); // tuỳ chỉnh thời gian đợi
            CloseDialogue();
        }
    }

    void ShowFullLine()
    {
        StopCoroutine(typingCoroutine);
        dialogueText.text = lines[currentLine];
        isTyping = false;
    }

    void NextLine()
    {
        currentLine++;
        if (currentLine < lines.Length)
        {
            typingCoroutine = StartCoroutine(TypeLine(lines[currentLine]));
        }
        else
        {
            CloseDialogue();
        }
    }

    public void CloseDialogue()
    {
        dialogueBox.SetActive(false);
        onDialogueEnd?.Invoke(); // Gọi callback sau khi thoại kết thúc
        onDialogueEnd = null;    // Xóa callback để tránh bị gọi lại lần sau
    }

    public void StartDialogue(DialogueData data, Transform npcTransform)
    {
        lines = data.lines;
        currentLine = 0;

        followScript.targetNPC = npcTransform;
        dialogueBox.SetActive(true);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(lines[currentLine]));
    }

}
