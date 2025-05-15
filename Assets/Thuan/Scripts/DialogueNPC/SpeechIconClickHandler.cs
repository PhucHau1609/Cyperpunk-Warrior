using UnityEngine;

public class SpeechIconClickHandler : MonoBehaviour
{
    public NPCDialogueUnlockManager npc;

    void OnMouseDown()
    {
        npc.OnSpeechIconClicked();
    }
}
