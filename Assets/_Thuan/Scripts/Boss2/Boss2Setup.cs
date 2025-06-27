using UnityEngine;

// Script setup tự động cho Boss2
[System.Serializable]
public class Boss2Setup : MonoBehaviour
{
    [Header("Auto Setup Boss2")]
    [SerializeField] private bool autoSetup = true;
    
    [Header("Prefab References")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject bombPrefab;
    
    [Header("Transform Points")]
    [SerializeField] private Transform laserPoint;
    [SerializeField] private Transform[] bombPoints;
    
    private void Start()
    {
        if (autoSetup)
        {
            SetupBoss2();
        }
    }
    
    [ContextMenu("Setup Boss2")]
    public void SetupBoss2()
    {
        Boss2Controller controller = GetComponent<Boss2Controller>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<Boss2Controller>();
        }
        
        // Auto tìm các transform points nếu chưa gán
        if (laserPoint == null)
        {
            laserPoint = transform.Find("LaserPoint");
            if (laserPoint == null)
            {
                Debug.LogWarning("Không tìm thấy LaserPoint! Hãy tạo một child object tên 'LaserPoint'");
            }
        }
        
        if (bombPoints == null || bombPoints.Length == 0)
        {
            Transform[] foundBombPoints = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                Transform bombPoint = transform.Find($"BombPoint{i + 1}");
                if (bombPoint != null)
                {
                    foundBombPoints[i] = bombPoint;
                }
                else
                {
                    Debug.LogWarning($"Không tìm thấy BombPoint{i + 1}! Hãy tạo child object tên 'BombPoint{i + 1}'");
                }
            }
            bombPoints = foundBombPoints;
        }
        
        Debug.Log("Boss2 Setup hoàn thành!");
    }
}