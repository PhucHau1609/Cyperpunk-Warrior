using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

public class NPCDisableTrigger2D : MonoBehaviour
{
    //
    [SerializeField] private string targetName = "FlyTarget"; // Tên GameObject trong scene hiện tại
    [SerializeField] private float flySpeed = 2f;
    [SerializeField] private PlayableDirector playableDirector;
    [Header("Zoom Settings")]
    public float targetZoomSize = 10f;
    public float zoomDuration = 2f;
    public bool zoomOnEnter = true;
    public bool zoomOnExit = false;
    private float originalZoomSize;
    private bool hasTriggered = false;
    [Header("Dialogue")]
    public GameObject dialogueHolder;
    public GameObject miniGame;
    //
    void Start()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            originalZoomSize = mainCam.orthographicSize;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && zoomOnEnter && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(ZoomCamera(targetZoomSize));
        }
        if (other.CompareTag("NPC"))
        {
            other.gameObject.SetActive(false);
            playableDirector.Play();
            playableDirector.stopped += OnTimelineStop;
        }
        // Kiểm tra xem có FloatingFollower không
        FloatingFollower follower = other.GetComponent<FloatingFollower>();
        if (follower != null)
        {
            // Tắt NavMeshAgent nếu có (NavMeshAgent vẫn là 3D)
            NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.enabled = false;
            }

            // Tắt script FloatingFollower
            follower.enabled = false;

            // (Tùy chọn) Tắt Animator nếu cần
            Animator anim = other.GetComponent<Animator>();
            if (anim != null)
            {
                anim.enabled = false;
            }

            //
            // ✅ Tìm target trong scene hiện tại
            GameObject targetObj = GameObject.Find(targetName);
            if (targetObj == null)
            {
                //Debug.LogWarning("❌ Không tìm thấy target trong scene hiện tại!");
                return;
            }

            // ✅ Gán target và cho NPC bay
            var flyer = other.GetComponent<SimpleFlyToTarget>();
            if (flyer != null)
            {
                flyer.target = targetObj.transform;
                flyer.speed = flySpeed;
                flyer.active = true;

                //Debug.Log("🛫 NPC đã được gán target mới và bắt đầu bay.");
            }
            //

            //Debug.Log("Đã vô hiệu hóa FloatingFollower + NavMeshAgent.");
        }
    }

    private void OnTimelineStop(PlayableDirector director)
    {
        playableDirector.stopped -= OnTimelineStop;
        StartCoroutine(ActiveDialogue());
    }

    IEnumerator ActiveDialogue()
    {
        // Pause game
        Time.timeScale = 0f;
        
        // Kích hoạt dialogue
        if (dialogueHolder != null)
        {
            dialogueHolder.SetActive(true);

            // Đợi dialogue kết thúc
            yield return new WaitUntil(() => !dialogueHolder.activeInHierarchy);
        }
        
        if (miniGame != null)
        {
            miniGame.SetActive(true);
            
            // Đợi dialogue kết thúc
            yield return new WaitUntil(() => !dialogueHolder.activeInHierarchy);
        }

        // Resume game
        Time.timeScale = 1f;
        
        // Tắt trigger này
        gameObject.SetActive(false);
    }

    IEnumerator ZoomCamera(float targetSize)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        float startSize = mainCam.orthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Dùng unscaledDeltaTime để không bị ảnh hưởng bởi timeScale
            float progress = elapsedTime / zoomDuration;

            // Smooth zoom transition
            float currentSize = Mathf.Lerp(startSize, targetSize, progress);
            mainCam.orthographicSize = currentSize;

            // Notify CameraBoundaries to update
            CameraBoundaries boundaries = FindFirstObjectByType<CameraBoundaries>();
            if (boundaries != null)
            {
                boundaries.UpdateCameraSize();
            }

            yield return null;
        }

        // Ensure final size is exact
        mainCam.orthographicSize = targetSize;
    }
    IEnumerator ZoomThenDialogue()
    {
        // Zoom camera trước
        yield return StartCoroutine(ZoomCamera(targetZoomSize));
        
        // Pause game
        Time.timeScale = 0f;
        
        // Kích hoạt dialogue
        if (dialogueHolder != null)
        {
            dialogueHolder.SetActive(true);

            // Đợi dialogue kết thúc
            yield return new WaitUntil(() => !dialogueHolder.activeInHierarchy);
        }
        
        if (miniGame != null)
        {
            miniGame.SetActive(true);
            
            // Đợi dialogue kết thúc
            yield return new WaitUntil(() => !dialogueHolder.activeInHierarchy);
        }

        // Resume game
        Time.timeScale = 1f;
        
        // Tắt trigger này
        gameObject.SetActive(false);
    }
        
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && zoomOnExit && hasTriggered)
        {
            hasTriggered = false;
                
            // Zoom back to original
            StartCoroutine(ZoomCamera(originalZoomSize));
        }
    }
}
