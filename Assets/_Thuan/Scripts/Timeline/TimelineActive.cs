using UnityEngine;
using UnityEngine.Playables;

public class TimelineActive : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private GameObject door;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private GameObject doorTrigger;
    void Start()
    {
        playableDirector.stopped += OnTimelineStop;
    }
    private void OnTimelineStop(PlayableDirector director)
    {
        playableDirector.stopped -= OnTimelineStop;
        if (door != null)
        {
            // Bật trigger nếu có Collider
            var collider = door.GetComponent<Collider2D>();
            if (collider != null) collider.isTrigger = true;

            // Gọi animation mở cửa nếu có Animator
            var animator = door.GetComponent<Animator>();
            if (animator != null) animator.SetTrigger("open");
        }
        doorTrigger.SetActive(true);
    }
}