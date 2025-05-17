using UnityEngine;

public class FireBall : MonoBehaviour
{
    private Animator animator;
    private bool hasExploded = false;
    private GameObject targetGround;

    [SerializeField] private float destroyGroundDelay = 0.5f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;

        if (other.CompareTag("Destructible"))
        {
            hasExploded = true;
            targetGround = other.gameObject;

            animator.SetTrigger("Explode");

            GetComponent<Collider2D>().enabled = false;
            GetComponent<Rigidbody2D>().simulated = false;

            // Khi thiên thạch chạm đất
            CameraShake.Instance.Shake(0.3f, 0.2f);
        }
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void DestroyGround()
    {
        if (targetGround != null)
        {
            Destroy(targetGround, destroyGroundDelay);
        }
    }
}
