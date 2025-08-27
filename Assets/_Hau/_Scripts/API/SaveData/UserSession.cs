using System.Collections.Generic;
using UnityEngine;

public class UserSession : MonoBehaviour
{
    public static UserSession Instance;

    public int UserId;
    // dữ liệu save để apply sau khi chuyển scene
    public bool HasLoadedSave;
    public Vector3 SavedPosition;
    public float SavedHealth, SavedMaxHealth;
    public string SavedSceneName;  // có thể null/empty nếu server chưa lưu scene
    public List<int> UnlockedSkillsCache = new();


    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }
}
