using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.AI;

public class FloatingFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    //public float followHeight = 1.5f;
    //public float sideOffset = 1f;
    public float xFollowSpeed = 5f;
    //public float yFollowSpeed = 5f;
    public float minDistance = 0.2f;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleMask;
    public float avoidForce = 5f;

    [Header("Dash Settings")]
    public bool isDashing = false;
    public float dashFollowMultiplier = 2f;

    private Transform player;
    private Vector3 targetPos;
    private Rigidbody2D rb;
    private Animator anim;

    private NavMeshAgent agent;
    private bool isUsingPathfinding = false;



    private enum PetState { Sleepwell, Awaken, Following, Disappear }
    private PetState state = PetState.Sleepwell;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.isStopped = true;
        }

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
                StartCoroutine(StartFollowAfterDelay(1.5f));
            }
            return;
        }

        if (state != PetState.Following) return;

        float direction = player.localScale.x > 0 ? -1 : 1;

        Vector2 playerPos = player.position;
        Vector2 currentPos = rb.position;

        float minDist = 2f;
        float maxDist = 7f;
        float dist = Vector2.Distance(currentPos, playerPos);

        if (dist > maxDist)
        {
            if (!isUsingPathfinding && agent != null)
            {
                isUsingPathfinding = true;
                agent.isStopped = false;
                //agent.SetDestination(playerPos);
            }
            // Tính vận tốc tăng dần theo khoảng cách
            float speedScale = Mathf.Clamp(dist / maxDist, 1f, 3f); // Cách 14f thì tốc độ gấp ~2x
            float baseSpeed = xFollowSpeed * (isDashing ? dashFollowMultiplier : 1f);
            agent.speed = baseSpeed * speedScale;

            // Điểm đến: cao hơn player 2f và giữ khoảng cách 0.2f
            Vector2 toPlayer = (playerPos - currentPos).normalized;
            Vector2 targetPos = (Vector2)playerPos - toPlayer * minDist + Vector2.up * 4f;

            agent.SetDestination(targetPos);
        }
        else if (dist > minDist && isUsingPathfinding)
        {
            // Đang di chuyển, vẫn dùng nav để tiến gần đến 0.2f
            Vector2 toPlayer = (playerPos - currentPos).normalized;
            Vector2 desiredPos = playerPos - toPlayer * minDist;
            agent.SetDestination(desiredPos);
        }
        else if (dist <= minDist && isUsingPathfinding)
        {
            // Đến gần rồi, dừng nav
            agent.isStopped = true;
            isUsingPathfinding = false;
        }

        if (agent.desiredVelocity.sqrMagnitude > 0.01f)
        {
            float moveX = agent.desiredVelocity.x;
            if (Mathf.Abs(moveX) > 0.05f)
            {
                float facing = moveX > 0 ? 1f : -1f;
                transform.localScale = new Vector3(2f * facing, 2f, 2f);
            }
        }
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

        if (state == PetState.Disappear || state == PetState.Awaken)
        {
            StartCoroutine(ForceIdleAfterSceneLoad());
        }
    }

    private IEnumerator ForceIdleAfterSceneLoad()
    {
        yield return null;
        yield return null;

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
