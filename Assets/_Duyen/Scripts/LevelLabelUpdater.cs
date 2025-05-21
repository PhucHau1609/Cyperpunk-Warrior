using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using TMPro;

public class LevelLabelUpdater : MonoBehaviour
{
    public TMP_Text levelText;
    private static LevelLabelUpdater instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject); // tránh trùng lặp khi quay lại scene trước
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateLevelText(scene.name);
    }
    void UpdateLevelText(string sceneName)
    {
        if (levelText == null)
        {
            // Tìm lại Text nếu mất reference khi chuyển scene
            levelText = GetComponentInChildren<TMP_Text>();
        }

        string levelDisplay = ConvertSceneNameToLevel(sceneName);
        if (levelText != null)
        {
            levelText.text = "LEVEL " + levelDisplay;
        }
    }

    string ConvertSceneNameToLevel(string sceneName)
    {
        // Tìm số map và số level trong tên scene dạng "mapXlevelY"
        Match match = Regex.Match(sceneName, @"map(\d+)level(\d+)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            string world = match.Groups[1].Value;
            string stage = match.Groups[2].Value;
            return world + "-" + stage;
        }
        return "UNKNOWN";
    }
    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh lỗi memory leak
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
