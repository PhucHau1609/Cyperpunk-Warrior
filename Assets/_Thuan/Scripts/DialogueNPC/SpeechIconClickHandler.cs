using UnityEngine;
using UnityEngine.EventSystems;

public class SpeechIconClickHandler : MonoBehaviour, IPointerClickHandler
{
    public DialogueData dialogueData;
    public Transform npcTransform;
    public System.Action onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.SetActive(false);
        DialogueManager.Instance.StartDialogue(dialogueData, npcTransform);
        onClick?.Invoke();
    }
}
