using UnityEngine;
using DG.Tweening;

public class Treasure : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject bombPrefab;

    // --- Thêm biến tỉ lệ vào đây ---
    [Header("Item Drop Rate")]
    [Range(0, 1)] // Giúp bạn dễ dàng kéo thanh trượt trong Inspector từ 0 đến 1
    [SerializeField] private float itemDropRate = 0.5f;

    [Header("Spawn Positions")]
    [SerializeField] private Transform spawnLeft;
    [SerializeField] private Transform spawnRight;
    [SerializeField] private Transform spawnCenter;

    [Header("Reward")]
    [SerializeField] protected ItemCode rewardEnergy;

    [Header("Bomb Arc Force")]
    [SerializeField] private Vector2 leftForce = new Vector2(-3, 5);
    [SerializeField] private Vector2 rightForce = new Vector2(3, 5);

    [Header("DOTween Settings")]
    [SerializeField] private float coreMoveDistance = 2.5f;
    [SerializeField] private float coreMoveDuration = 1.2f;
    [SerializeField] private Ease coreMoveEase = Ease.OutQuad;

    private bool hasOpened = false;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasOpened) return;
        if (collision.CompareTag("Player"))
        {
            hasOpened = true;
            animator.SetTrigger("Chest_Open");
        }
    }

    // Gọi từ cuối animation Chest_Open
    public void OnChestOpened()
    {
        bool isItemReward = Random.value >= itemDropRate;

        if (isItemReward)
        {
            SpawnBombs();
        }
        else
        {
            SpawnEnergyCore();
        }
    }
    private void SpawnBombs()
    {
        GameObject bomb1 = Instantiate(bombPrefab, spawnLeft.position, Quaternion.identity);
        GameObject bomb2 = Instantiate(bombPrefab, spawnRight.position, Quaternion.identity);

        Rigidbody2D rb1 = bomb1.GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = bomb2.GetComponent<Rigidbody2D>();

        if (rb1) rb1.AddForce(leftForce, ForceMode2D.Impulse);
        if (rb2) rb2.AddForce(rightForce, ForceMode2D.Impulse);
    }

    private void SpawnEnergyCore()
    {
        ItemsDropCtrl coreItem = ItemsDropManager.Instance.DropItemObject(rewardEnergy, 1, spawnCenter.position);

        if (coreItem == null) return;

        // Di chuyển bằng DOTween
        Vector3 targetPos = coreItem.transform.position + Vector3.up * coreMoveDistance;
        coreItem.transform.DOMove(targetPos, coreMoveDuration)
            .SetEase(coreMoveEase)
            .SetUpdate(true); // vẫn hoạt động khi timescale = 0
    }

}
