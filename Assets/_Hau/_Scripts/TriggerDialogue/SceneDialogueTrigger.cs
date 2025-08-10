using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SceneDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData dialogue;
    public bool onlyOnce = true;
    public int minDeathsInThisScene = 1;

    [Header("Speaker (Lyra) Resolve")]
    public LyraDialogueTrigger lyra;     // có thì gán, không có cũng không sao
    public string lyraTag = "NPC";      // gán Tag="Lyra" cho NPC nếu muốn
    public Transform speakerAnchor;      // fallback khi không tìm ra Lyra

    [Header("Quality-of-life")]
    public bool teleportNpcNearPlayer = true;
    public Vector2 npcOffsetFromPlayer = new Vector2(-1f, 0f);

    private bool fired;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (onlyOnce && fired) return;
        if (!other.CompareTag("Player")) return;

        // Kiểm tra điều kiện chết trong scene hiện tại
        int deaths = DeathEventService.Instance.GetCurrentSceneDeaths();
        if (deaths < minDeathsInThisScene) return;

        // Tìm hoặc lấy speaker transform
        Transform speaker = ResolveSpeakerTransform();

        // Dịch NPC tới gần player (nếu tìm được NPC thực)
        if (teleportNpcNearPlayer && speaker != null && lyra != null)
        {
            speaker.position = other.transform.position + (Vector3)npcOffsetFromPlayer;
        }

        // Gọi thoại (speaker có thể null -> DialogueManager nên hỗ trợ null = thoại generic)
        if (dialogue != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue, speaker);
            fired = true;
        }
    }

    private Transform ResolveSpeakerTransform()
    {
        // 1) ưu tiên field đã gán
        if (lyra != null && lyra.NPCTransform != null)
            return lyra.NPCTransform;

        // 2) thử tag
        if (!string.IsNullOrEmpty(lyraTag))
        {
            var byTag = GameObject.FindGameObjectWithTag(lyraTag);
            if (byTag != null)
            {
                var l = byTag.GetComponent<LyraDialogueTrigger>();
                if (l != null && l.NPCTransform != null)
                {
                    lyra = l; // cache cho lần sau
                    return l.NPCTransform;
                }
                // nếu chỉ có Transform
                return byTag.transform;
            }
        }

        // 3) thử tìm theo type trong scene
        var found = Object.FindAnyObjectByType<LyraDialogueTrigger>(FindObjectsInactive.Exclude);
        if (found != null && found.NPCTransform != null)
        {
            lyra = found; // cache
            return found.NPCTransform;
        }

        // 4) fallback anchor cố định (điểm đẹp trong layout)
        if (speakerAnchor != null) return speakerAnchor;

        // 5) hết cách, trả null -> StartDialogue vẫn nên hoạt động ở chế độ không có speaker
        return null;
    }
}
