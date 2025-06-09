using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Transform pointA;
    public Transform pointB;
    public Transform gunPoint;
    public Animator animator;
    public Slider healthSlider;
    public Transform player;

    [HideInInspector] public Vector3 initialPosition;

    public float maxHealth = 10;
    private float currentHealth;
    //private bool isHealthBarVisible = false;


    [SerializeField] private LayerMask groundLayer;

    void Awake()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
    }

    void Start()
    {
        initialPosition = transform.position;
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void LateUpdate()
    {
        UpdateHealthBar(); // Đảm bảo thanh máu đúng vị trí theo camera
    }

    public void TakeDamage(int damage)
    {
        ApplyDamage(damage);
    }

    void FixedUpdate()
    {
        StickToGround(); // Gọi mỗi frame
    }

   public void ApplyDamage(float damage)
    {
        currentHealth -= Mathf.Abs(damage);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth > 0)
        {
            // Hiện thanh máu khi bị trừ
            healthSlider.gameObject.SetActive(true);
            animator.SetTrigger("Hurt");
        }
        else
        {
            animator.SetTrigger("Death");
            healthSlider.gameObject.SetActive(false); // Ẩn khi chết
            this.enabled = false;
            Destroy(gameObject, 2f);
        }

        UpdateHealthBar();
    }

   void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            float healthRatio = currentHealth / maxHealth;

            // Cập nhật giá trị
            healthSlider.value = healthRatio;

            // Đổi màu từ Đỏ (ít máu) sang Xanh Lá (đầy máu)
            Color healthColor = Color.Lerp(Color.red, Color.green, healthRatio);
            healthSlider.fillRect.GetComponent<Image>().color = healthColor;

            // Vị trí thanh máu theo Enemy
            Vector3 offset = new Vector3(0, 1.5f, 0);
            Vector3 pos = Camera.main.WorldToScreenPoint(transform.position + offset);
            pos.z = 0f;
            healthSlider.transform.position = pos;
        }
    }

    // Public để các Task có thể gọi
    public void FacePlayer()
    {
        if (player == null) return;

        Vector3 scale = transform.localScale;
        scale.x = (player.position.x < transform.position.x) ? -1 : 1;
        transform.localScale = scale;
    }

    void StickToGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 2f, groundLayer);
        if (hit.collider != null)
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + 0.1f;
            transform.position = pos;
        }
    }
}
