using UnityEngine;
using UnityEngine.EventSystems;

public class SpeechIconClickHandler : MonoBehaviour, IPointerClickHandler
{
    public DialogueData dialogueData;
    public Transform npcTransform;
    public System.Action onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        DialogueManager.Instance.StartDialogue(dialogueData, npcTransform);
        gameObject.SetActive(false);
        onClick?.Invoke();
    }
}
