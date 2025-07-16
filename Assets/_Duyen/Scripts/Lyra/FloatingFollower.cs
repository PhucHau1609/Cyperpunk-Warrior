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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.enabled = false; // tắt ban đầu, sẽ bật sau nếu cần
            //MoveToSPWAndEnableNav();
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

        // Custom bay cao hơn player khi gần
        if (!isUsingPathfinding && state == PetState.Following)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            float heightAbovePlayer = transform.position.y - player.position.y;

            bool isNearPlayer = distToPlayer < 3f;
            bool notHighEnough = heightAbovePlayer < 3f;

            if (isNearPlayer && notHighEnough)
            {
                // Kiểm tra có vật cản phía trên không
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, 0.2f, obstacleMask);
                if (!hit.collider)
                {
                    float moveSpeed = xFollowSpeed * (isDashing ? dashFollowMultiplier : 1f);
                    float newY = Mathf.MoveTowards(transform.position.y, player.position.y + 3f, moveSpeed * Time.fixedDeltaTime);
                    transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                }
                // nếu bị cản thì không bay lên, giữ nguyên y
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
                agent.Warp(transform.position); // Sync vào NavMesh
                isUsingPathfinding = true;
            }
            else
            {
                //Debug.LogWarning("NPC không đứng trên NavMesh sau khi dịch chuyển đến 'spw'.");
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy đối tượng có tag 'spw'.");
        }
    }

}

//ko dung nav
/* 
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FloatingFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    public float moveSpeed = 3f;
    public float xTolerance = 1f;
    public float yMinHeight = 2f;
    public float yMaxHeight = 4f;
    public float checkInterval = 3f;

    [Header("Dash Settings")]
    public bool isDashing = false;
    public float dashFollowMultiplier = 2f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;

    private Vector2 targetOffset;
    private float checkTimer;

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
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        FindPlayer();
        checkTimer = checkInterval;
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

        checkTimer -= Time.fixedDeltaTime;
        if (checkTimer <= 0f)
        {
            checkTimer = checkInterval;
            UpdateTargetOffset();
        }

        // Di chuyển mượt về phía target position
        Vector2 targetPos = (Vector2)player.position + targetOffset;
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * (isDashing ? dashFollowMultiplier : 1f) * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        // Xoay hướng nhìn
        float dx = targetPos.x - rb.position.x;
        if (Mathf.Abs(dx) > 0.05f)
        {
            float facing = dx > 0 ? 1f : -1f;
            transform.localScale = new Vector3(2f * facing, 2f, 2f);
        }
    }

    private void UpdateTargetOffset()
    {
        Vector2 toPet = (Vector2)transform.position - (Vector2)player.position;

        float targetX = Mathf.Clamp(toPet.x, -5f, 5f); // không cách quá xa
        float targetY = Mathf.Clamp(toPet.y, yMinHeight, yMaxHeight);

        if (Mathf.Abs(toPet.x) < xTolerance)
            targetX = toPet.x > 0 ? 1.5f : -1.5f;

        if (toPet.y < yMinHeight)
            targetY = yMinHeight + Random.Range(0.5f, 1.5f); // bay lên nhẹ

        targetOffset = new Vector2(targetX, targetY);
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
            anim.SetTrigger("Disappear");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
        if (state == PetState.Disappear || state == PetState.Awaken)
            StartCoroutine(ForceIdleAfterSceneLoad());
    }

    private IEnumerator ForceIdleAfterSceneLoad()
    {
        yield return null;
        yield return null;

        state = PetState.Following;
        if (anim != null)
            anim.SetTrigger("Idle");
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
*/