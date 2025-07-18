using UnityEngine;

public class HPSpawner : MonoBehaviour
{

    [Header("Các điểm spawn")]
    public Transform[] spawnPoints;

    [Header("Số lượng item cần spawn")]
    public int itemCount = 3;

    void Start()
    {
        SpawnHPItems();
    }

    void SpawnHPItems()
    {
        // Bảo vệ giới hạn
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("Thiếu prefab hoặc điểm spawn!");
            return;
        }

        // Shuffle vị trí spawn để tránh lặp lại
        System.Random rng = new System.Random();
        Transform[] shuffledPoints = (Transform[])spawnPoints.Clone();
        for (int i = shuffledPoints.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            var temp = shuffledPoints[i];
            shuffledPoints[i] = shuffledPoints[j];
            shuffledPoints[j] = temp;
        }

        // Spawn theo số lượng yêu cầu
        for (int i = 0; i < itemCount && i < shuffledPoints.Length; i++)
        {
            //Instantiate(hpItemPrefab, shuffledPoints[i].position, Quaternion.identity);
            ItemsDropManager.Instance.DropItem(ItemCode.HP,1, shuffledPoints[i].position);
        }
    }
}
