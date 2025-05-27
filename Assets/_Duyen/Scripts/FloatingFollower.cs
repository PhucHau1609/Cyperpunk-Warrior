using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FloatingFollower : MonoBehaviour
{
    public float followHeight = 1.5f;
    public float sideOffset = 1f;
    public float xFollowSpeed = 2f;
    public float yFollowSpeed = 5f;

    public bool isDashing = false;
    public float dashFollowMultiplier = 2f;

    private Transform player;
    private Vector3 targetPos;
    private Rigidbody2D rb;
    private Animator anim;

    private enum PetState { Sleepwell, Awaken, Following, Disappear }
    private PetState state = PetState.Sleepwell;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        FindPlayer();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (state == PetState.Sleepwell)
        {
            if (CodeLock.PetUnlocked)
            {
                state = PetState.Awaken;
                anim.SetTrigger("Awaken");
                StartCoroutine(StartFollowAfterDelay(1.5f)); // thời gian animation awaken
            }
            return;
        }

        if (state != PetState.Following) return;

        // Pet follow logic
        float direction = player.localScale.x > 0 ? -1 : 1;
        targetPos = player.position + new Vector3(sideOffset * direction, followHeight, 0);

        float heightDiff = player.position.y - rb.position.y;
        float ySpeed = yFollowSpeed;

        if (heightDiff < -1f) ySpeed *= 4f;
        else if (heightDiff > 1f) ySpeed *= 1.5f;

        float speedMultiplier = isDashing ? dashFollowMultiplier : 1f;

        float newX = Mathf.Lerp(rb.position.x, targetPos.x, xFollowSpeed * speedMultiplier * Time.fixedDeltaTime);
        float newY = Mathf.Lerp(rb.position.y, targetPos.y, ySpeed * speedMultiplier * Time.fixedDeltaTime);

        rb.MovePosition(new Vector2(newX, newY));

        transform.localScale = direction > 0 ? new Vector3(-2, 2, 2) : new Vector3(2, 2, 2);
    }

    IEnumerator StartFollowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        state = PetState.Following;
        anim.SetTrigger("Idle");
    }

    public void Disappear()
    {
        if (state != PetState.Following) return;

        state = PetState.Disappear;

        if (anim != null)
        {
            Debug.Log("Pet disappearing...");
            anim.SetTrigger("Disappear");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();

        // Reset lại pet về trạng thái Following và Idle nếu vừa Disappear
        if (state == PetState.Disappear || state == PetState.Awaken)
        {
            StartCoroutine(ForceIdleAfterSceneLoad());
        }
    }

    private IEnumerator ForceIdleAfterSceneLoad()
    {
        yield return null; // Đợi 1 frame
        yield return null; // Đợi thêm để animator kịp reset

        state = PetState.Following;

        if (anim != null)
        {
            anim.SetTrigger("Idle");
            Debug.Log("Pet forced to Idle after scene load.");
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
