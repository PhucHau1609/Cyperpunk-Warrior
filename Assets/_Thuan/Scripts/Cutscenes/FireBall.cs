using UnityEngine;

public class FireBall : MonoBehaviour
{
    private Animator animator;
    private bool hasExploded = false;
    private GameObject targetGround;

    [SerializeField] private float destroyGroundDelay = 0.5f;

    //[SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip explosionSound;
    private AudioSource audioSource;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        //PlayFireSound();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;

        if (other.CompareTag("Destructible"))
        {
            hasExploded = true;
            targetGround = other.gameObject;

            animator.SetTrigger("Explode");
            PlayExplosionSound();

            GetComponent<Collider2D>().enabled = false;
            GetComponent<Rigidbody2D>().simulated = false;

            CameraShake.Instance.Shake(0.3f, 0.2f);
        }
    }

    // private void PlayFireSound()
    // {
    //     if (fireSound != null)
    //         audioSource.PlayOneShot(fireSound);
    // }

    private void PlayExplosionSound()
    {
        if (explosionSound != null)
            audioSource.PlayOneShot(explosionSound);
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
