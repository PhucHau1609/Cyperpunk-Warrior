using UnityEngine;
using BehaviorDesigner.Runtime;

#if UNITY_EDITOR
using UnityEditor;

[System.Serializable]
public class PatrolSetupHelper : EditorWindow
{
    [Header("Naming Convention")]
    public string enemyPrefix = "Enemy";
    public string pointPrefix = "Point";
    
    [Header("Auto Setup Options")]
    public bool setupExistingEnemies = true;
    public bool createMissingPoints = false;
    public Vector2 defaultPointDistance = new Vector2(5f, 0f);
    
    [MenuItem("Tools/Patrol Setup Helper")]
    public static void ShowWindow()
    {
        GetWindow<PatrolSetupHelper>("Patrol Setup");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Patrol System Auto Setup", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        enemyPrefix = EditorGUILayout.TextField("Enemy Prefix", enemyPrefix);
        pointPrefix = EditorGUILayout.TextField("Point Prefix", pointPrefix);
        
        EditorGUILayout.Space();
        
        setupExistingEnemies = EditorGUILayout.Toggle("Setup Existing Enemies", setupExistingEnemies);
        createMissingPoints = EditorGUILayout.Toggle("Create Missing Points", createMissingPoints);
        
        if (createMissingPoints)
        {
            defaultPointDistance = EditorGUILayout.Vector2Field("Point Distance", defaultPointDistance);
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Auto Setup Patrol System"))
        {
            SetupPatrolSystem();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Naming Convention:\n" +
            "• Enemies: Enemy1, Enemy2, Enemy3...\n" +
            "• Points: PointA_1, PointB_1, PointA_2, PointB_2...\n" +
            "• Script sẽ tự động match Enemy với Points tương ứng",
            MessageType.Info);
    }
    
    private void SetupPatrolSystem()
    {
        // Tìm tất cả enemies trong scene
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        int setupCount = 0;
        int pointsCreated = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith(enemyPrefix))
            {
                string enemyIndex = ExtractIndex(obj.name);
                
                // Kiểm tra xem có PatrolTask không
                BehaviorTree behaviorTree = obj.GetComponent<BehaviorTree>();
                if (behaviorTree == null) continue;
                
                // Tìm hoặc tạo patrol points
                string pointAName = $"{pointPrefix}A{enemyIndex}";
                string pointBName = $"{pointPrefix}B{enemyIndex}";
                
                GameObject pointA = GameObject.Find(pointAName);
                GameObject pointB = GameObject.Find(pointBName);
                
                // Tạo points nếu không tồn tại và được yêu cầu
                if (createMissingPoints)
                {
                    if (pointA == null)
                    {
                        pointA = CreatePatrolPoint(pointAName, obj.transform.position + (Vector3)defaultPointDistance);
                        pointsCreated++;
                    }
                    
                    if (pointB == null)
                    {
                        pointB = CreatePatrolPoint(pointBName, obj.transform.position - (Vector3)defaultPointDistance);
                        pointsCreated++;
                    }
                }
                
                setupCount++;
                
                Debug.Log($"Setup completed for {obj.name} with points: {pointAName}, {pointBName}");
            }
        }
        
        Debug.Log($"Patrol Setup Complete! Enemies setup: {setupCount}, Points created: {pointsCreated}");
        
        // Refresh scene
        EditorUtility.SetDirty(Selection.activeGameObject);
    }
    
    private GameObject CreatePatrolPoint(string pointName, Vector3 position)
    {
        GameObject point = new GameObject(pointName);
        point.transform.position = position;
        
        // Thêm visual indicator (optional)
        SpriteRenderer sr = point.AddComponent<SpriteRenderer>();
        sr.color = Color.yellow;
        
        // Tạo sprite đơn giản cho patrol point
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
        sr.sprite = sprite;
        
        point.transform.localScale = Vector3.one * 0.5f;
        
        return point;
    }
    
    private string ExtractIndex(string name)
    {
        string index = "";
        
        for (int i = name.Length - 1; i >= 0; i--)
        {
            if (char.IsDigit(name[i]))
            {
                index = name[i] + index;
            }
            else
            {
                break;
            }
        }
        
        return string.IsNullOrEmpty(index) ? "" : "_" + index;
    }
}
#endif

// Runtime component để debug patrol points
public class PatrolPointDebugger : MonoBehaviour
{
    [Header("Debug")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.yellow;
    public float gizmoSize = 0.5f;
    
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        
        // Hiển thị tên
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, gameObject.name);
        #endif
    }
}