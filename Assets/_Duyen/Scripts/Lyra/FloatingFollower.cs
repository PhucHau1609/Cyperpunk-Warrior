using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.AI;

public class FloatingFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    public float xFollowSpeed = 5f;
    public float minDistance = 0.2f;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleMask;
    public float avoidForce = 5f;

    [Header("Dash Settings")]
    public bool isDashing = false;
    public float dashFollowMultiplier = 2f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;

    private NavMeshAgent agent;
    private bool isUsingPathfinding = false;

    public bool IsReadyForDialogue => state == PetState.Following;

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
            agent.enabled = false; // tắt ban đầu, sẽ bật sau nếu cần
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
            if (!isUsingPathfinding && agent != null && IsOnValidNavMesh())
            {
                agent.enabled = true;
                isUsingPathfinding = true;
                agent.isStopped = false;
            }

            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                float speedScale = Mathf.Clamp(dist / maxDist, 1f, 3f);
                float baseSpeed = xFollowSpeed * (isDashing ? dashFollowMultiplier : 1f);
                agent.speed = baseSpeed * speedScale;

                Vector2 toPlayer = (playerPos - currentPos).normalized;
                Vector2 targetPos = (Vector2)playerPos - toPlayer * minDist + Vector2.up * 4f;

                agent.SetDestination(targetPos);
            }
        }
        else if (dist > minDist && isUsingPathfinding)
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                Vector2 toPlayer = (playerPos - currentPos).normalized;
                Vector2 desiredPos = playerPos - toPlayer * minDist;
                agent.SetDestination(desiredPos);
            }
        }
        else if (dist <= minDist && isUsingPathfinding)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            if (agent != null)
            {
                agent.enabled = false;
            }

            isUsingPathfinding = false;
        }

        if (agent != null && agent.isActiveAndEnabled && agent.desiredVelocity.sqrMagnitude > 0.01f)
        {
            float moveX = agent.desiredVelocity.x;
            if (Mathf.Abs(moveX) > 0.05f)
            {
                float facing = moveX > 0 ? 1f : -1f;
                transform.localScale = new Vector3(2f * facing, 2f, 2f);
            }
        }

        if (!isUsingPathfinding && state == PetState.Following)
        {
            float desiredHeight = Mathf.Clamp(player.position.y + 2.5f, player.position.y + 2.5f, player.position.y + 5f);
            float currentY = transform.position.y;

            if (currentY < desiredHeight)
            {
                float moveSpeed = xFollowSpeed * (isDashing ? dashFollowMultiplier : 1f);
                float newY = Mathf.MoveTowards(currentY, desiredHeight, moveSpeed * Time.fixedDeltaTime);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
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

    private bool IsOnValidNavMesh()
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas);
    }
}
