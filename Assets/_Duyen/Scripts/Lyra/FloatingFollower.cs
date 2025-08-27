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

    [Header("Debug / Testing")]
    public bool forceAwaken = false;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;

    private NavMeshAgent agent;
    private bool isUsingPathfinding = false;

    public bool IsReadyForDialogue => state == PetState.Following;

    private enum PetState { Sleepwell, Awaken, Following, Disappear }
    private PetState state = PetState.Sleepwell;
    private static FloatingFollower instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Cho ResumeBootstrap gán lại Player sau khi Load
    public void SetPlayer(Transform t)
    {
        // đảm bảo valid
        if (t == null) return;
        // gán lại player để follow người chơi vừa spawn
        var f = this;
        var field = typeof(FloatingFollower).GetField("player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(f, t);
    }

    // Bật theo dõi ngay, đặt robot cạnh player (không cần chờ trigger “Awaken” nữa)
    public void ForceAwakenAndFollow(Vector3 spawnPos)
    {
        // đặt vị trí
        transform.position = spawnPos;

        // ép state = Following + Idle anim
        var stateField = typeof(FloatingFollower).GetField("state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (stateField != null)
        {
            // enum PetState { Sleepwell, Awaken, Following, Disappear } -> 2 = Following
            stateField.SetValue(this, System.Enum.Parse(stateField.FieldType, "Following"));
        }

        if (anim != null)
        {
            Debug.Log("1");
            anim.SetTrigger("Disappear");
            Debug.Log("2");
        }



        // bật NavMesh khi có
        if (agent != null)
        {
            // dùng helper đã có trong script
            var isOnValid = (bool)this.GetType()
                .GetMethod("IsOnValidNavMesh", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(this, new object[] { transform.position });

            if (isOnValid)
            {
                agent.enabled = true;
                agent.Warp(transform.position);
                var usePathField = typeof(FloatingFollower).GetField("isUsingPathfinding", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (usePathField != null) usePathField.SetValue(this, true);
            }
        }
    }


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.enabled = false;
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
            if (CodeLock.PetUnlocked || forceAwaken)
            {
                forceAwaken = false; // reset để chỉ test một lần
                state = PetState.Awaken;
                anim.SetTrigger("Awaken");
                StartCoroutine(StartFollowAfterDelay(1.5f));
            }
            return;
        }

        if (state != PetState.Following) return;

        Vector2 playerPos = player.position;
        Vector2 currentPos = rb.position;

        float minDist = 2f;
        float maxDist = 7f;
        float dist = Vector2.Distance(currentPos, playerPos);

        if (dist > maxDist)
        {
            if (!isUsingPathfinding && agent != null && IsOnValidNavMesh(transform.position))
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
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            float heightAbovePlayer = transform.position.y - player.position.y;

            bool isNearPlayer = distToPlayer < 3f;
            bool notHighEnough = heightAbovePlayer < 3f;

            if (isNearPlayer && notHighEnough)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, 0.2f, obstacleMask);
                if (!hit.collider)
                {
                    float moveSpeed = xFollowSpeed * (isDashing ? dashFollowMultiplier : 1f);
                    float newY = Mathf.MoveTowards(transform.position.y, player.position.y + 3f, moveSpeed * Time.fixedDeltaTime);
                    transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                }
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
        MoveToSPWAndEnableNav();

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

    private bool IsOnValidNavMesh(Vector3 position)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(position, out hit, 1f, NavMesh.AllAreas);
    }

    private void MoveToSPWAndEnableNav()
    {
        GameObject spwObj = GameObject.FindGameObjectWithTag("spw");
        if (spwObj != null)
        {
            transform.position = spwObj.transform.position;

            if (agent != null && IsOnValidNavMesh(transform.position))
            {
                agent.enabled = true;
                agent.Warp(transform.position);
                isUsingPathfinding = true;
            }
        }
    }
}
